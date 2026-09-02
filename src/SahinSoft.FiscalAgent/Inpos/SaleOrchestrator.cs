using Inpos;
using SahinSoft.FiscalAgent.Models;

namespace SahinSoft.FiscalAgent.Inpos;

// Vendörün BENİ OKU.txt (§ "Versiyon 0.19.2" - checkPayment/compareTotals algoritması) ve
// ExtTest.cs (AddSaleItem_Click/EndSale_Click) örneklerinden damıtılmış, headless tek-atomik-
// çağrı akışı: bağlan → (gerekirse) giriş yap → satışı başlat → kalemleri ekle → ödeme(ler)i
// al → satışı sonlandır. SahinSoft.Web tarafı TEK bir POST /sale/process çağrısıyla bunun
// tamamını tetikler, ara durumlarla hiç uğraşmaz - kırılgan yeniden-bağlanma/onaylama mantığının
// tamamı burada, cihazın kendi başına bıraktığı yerde kalır.
//
// NOT (kapsam dışı bırakılanlar - vendör dokümanında var ama bu ilk sürümde YOK): faturalı satış
// (startSaleWithInvoice/endSaleWithInvoice), avans/cari hesap akışı, harici ESC/POS yazıcı,
// yemek kartı firma seçimi. Gerçek cihazla ilk testler bittikten sonra ihtiyaç olursa eklenir.
public sealed class SaleOrchestrator(InposDeviceService device, ILogger<SaleOrchestrator> logger)
{
    private const uint TimeoutMs = 5000;

    public async Task<FiscalSaleResult> ProcessSaleAsync(FiscalSaleRequest request, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
        {
            return Fail("EmptyItems", "Satışta en az bir kalem olmalıdır.");
        }
        if (request.Payments.Count == 0)
        {
            return Fail("EmptyPayments", "En az bir ödeme satırı girilmelidir.");
        }

        if (device.SimulationMode)
        {
            return SimulateSale(request);
        }

        try
        {
            await device.EnsureInitializedAsync(ct);

            var ensureError = await EnsureReadyForSaleAsync(request.CashierName, ct);
            if (ensureError != InposExtError.InposNoError)
            {
                return Fail(ensureError.ToString(), $"Cihaz satışa hazırlanamadı: {InposDeviceService.SafeErrorDetail()}");
            }

            foreach (var item in request.Items)
            {
                var addError = AddItem(item);
                if (addError != InposExtError.InposNoError)
                {
                    return Fail(addError.ToString(), $"Kalem eklenemedi ({item.Name}): {InposDeviceService.SafeErrorDetail()}");
                }
            }

            var paymentResult = request.Payments.Count == 1
                ? await EndSaleWithSinglePaymentAsync(request.Payments[0], ct)
                : await AddSplitPaymentsAsync(request.Payments, ct);

            if (!paymentResult.Success)
            {
                return paymentResult;
            }

            return await ConfirmSaleFinishedAsync(ct);
        }
        catch (InvalidOperationException ex)
        {
            return Fail("ConfigError", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Satış işlenirken beklenmeyen hata.");
            return Fail("UnexpectedError", ex.Message);
        }
    }

    public async Task<FiscalSaleResult> CancelSaleAsync(CancellationToken ct = default)
    {
        if (device.SimulationMode)
        {
            return new FiscalSaleResult { Success = true, Simulated = true };
        }

        var error = InposExt.CancelSale(TimeoutMs);
        return error == InposExtError.InposNoError
            ? new FiscalSaleResult { Success = true }
            : Fail(error.ToString(), $"Satış iptal edilemedi: {InposDeviceService.SafeErrorDetail()}");
    }

    // ecrState'e göre: Login ekranındaysa giriş yapar (zorunlu Z isteniyorsa önce onu alır),
    // sonra MainMenu'deyse StartSale çağırır. Bkz. ExtTest.cs AddSaleItem_Click.
    private async Task<InposExtError> EnsureReadyForSaleAsync(string cashierName, CancellationToken ct)
    {
        var (error, ecrState) = await device.CheckEcrStatusAsync(ct);
        if (error != InposExtError.InposNoError)
        {
            return error;
        }

        if (ecrState == InposEcrState.InposEcrLogin)
        {
            if (!string.IsNullOrWhiteSpace(cashierName))
            {
                InposExt.SetCashierName(TimeoutMs, cashierName);
            }

            error = InposExt.Login(TimeoutMs);
            if (error != InposExtError.InposNoError)
            {
                return error;
            }

            await Task.Delay(100, ct);
            (error, ecrState) = await device.CheckEcrStatusAsync(ct);
            if (error != InposExtError.InposNoError)
            {
                return error;
            }

            if (ecrState == InposEcrState.InposEcrZReportRequired)
            {
                error = InposExt.ZReport();
                if (error != InposExtError.InposNoError)
                {
                    return error;
                }
                (error, ecrState) = await device.CheckEcrStatusAsync(ct);
            }
        }

        if (ecrState == InposEcrState.InposEcrMainMenu)
        {
            error = InposExt.StartSale(TimeoutMs);
        }
        else if (ecrState != InposEcrState.InposEcrSale)
        {
            return InposExtError.InposInvalidEcrStateError;
        }

        return error;
    }

    private static InposExtError AddItem(FiscalSaleItemDto dto)
    {
        var item = new InposEcrSaleItem(dto.Name)
        {
            unitPrice = ToKurus(dto.UnitPrice),
            multiplier = (uint)Math.Max(1, dto.Quantity),
            discountRate = 0,
            discountAmount = ToKurus(dto.DiscountAmount),
            section = (byte)Math.Clamp(dto.Section, 1, 255),
            unit = Unit.Quantity
        };
        var totals = new InposEcrSaleTotals(0);
        return InposExt.AddSaleItem(TimeoutMs, ref item, ref totals);
    }

    // Tek ödeme, tam tutar: EndSale(paymentType) ödeme + sonlandırmayı TEK adımda yapar
    // (bkz. BENİ OKU.txt "Eger parcali bir satis soz konusu degilse endSale() metodu odeme
    // tipi set edildikten sonra gonderilebilir").
    private async Task<FiscalSaleResult> EndSaleWithSinglePaymentAsync(FiscalPaymentDto payment, CancellationToken ct)
    {
        var error = InposExt.EndSale(ToInposPaymentType(payment.Method));
        if (error != InposExtError.InposNoError)
        {
            return Fail(error.ToString(), $"Satış sonlandırılamadı: {InposDeviceService.SafeErrorDetail()}");
        }

        await WaitForCardTransactionIfAnyAsync(ct);
        return new FiscalSaleResult { Success = true };
    }

    // Parçalı ödeme: her satır için AddPayment + "first/second fiş toplamı" karşılaştırmasıyla
    // ödemenin gerçekten işlendiğini doğrula (bkz. BENİ OKU.txt "Versiyon 0.19.2" algoritması).
    private async Task<FiscalSaleResult> AddSplitPaymentsAsync(List<FiscalPaymentDto> payments, CancellationToken ct)
    {
        foreach (var payment in payments)
        {
            var receipt = new InposSaleReceipt(0, 0, 0, 0);
            var before = new InposEcrSaleTotalsExt();
            InposExt.ReceiptData(TimeoutMs, ref receipt, ref before);

            var addError = payment.Method switch
            {
                FiscalPaymentMethod.CreditCard or FiscalPaymentMethod.Cash or FiscalPaymentMethod.MealCard
                    => InposExt.AddPayment(ToInposPaymentType(payment.Method), ToKurus(payment.Amount)),
                _ => InposExtError.InposInvalidArgumentError
            };
            if (addError != InposExtError.InposNoError)
            {
                return Fail(addError.ToString(), $"Ödeme eklenemedi ({payment.Method} {payment.Amount:N2}): {InposDeviceService.SafeErrorDetail()}");
            }

            await WaitForCardTransactionIfAnyAsync(ct);

            var confirmed = await ConfirmPaymentRegisteredAsync(before, ct);
            if (!confirmed)
            {
                return Fail("PaymentNotConfirmed", $"Ödeme cihazda doğrulanamadı ({payment.Method} {payment.Amount:N2}). Cihaz ekranını kontrol edin.");
            }
        }

        return new FiscalSaleResult { Success = true };
    }

    // Kredi kartı ödemesinde cihaz "SaleWaitingForTransactionCompleted" durumuna geçer (kart
    // okutulması bekleniyor) - bu durum bitene kadar bekle. Bkz. ExtTest.cs EndSale_Click.
    private async Task WaitForCardTransactionIfAnyAsync(CancellationToken ct)
    {
        for (var i = 0; i < 60; i++) // ~60sn üst sınır - kart işlemi banka hızına bağlı sürebilir.
        {
            var (error, saleState, _) = await device.CheckSaleStatusAsync(ct);
            if (error != InposExtError.InposNoError || saleState != InposEcrSaleState.InposSaleWaitingForTransactionCompleted)
            {
                return;
            }
            await Task.Delay(1000, ct);
        }
    }

    private async Task<bool> ConfirmPaymentRegisteredAsync(InposEcrSaleTotalsExt before, CancellationToken ct)
    {
        await Task.Delay(200, ct);
        var receipt = new InposSaleReceipt(0, 0, 0, 0);
        var after = new InposEcrSaleTotalsExt();

        for (var i = 0; i < 10; i++)
        {
            var error = InposExt.ReceiptData(TimeoutMs, ref receipt, ref after);
            if (error == InposExtError.InposConnectionError)
            {
                await device.ReconnectAsync(ct);
            }
            if (before != after)
            {
                return true;
            }
            await Task.Delay(100, ct);
        }

        return before != after;
    }

    // Satış bittiğinde cihaz ana menüye döner - bunu doğrulamadan "başarılı" denmez (bkz.
    // ExtTest.cs checkSaleEnd).
    private async Task<FiscalSaleResult> ConfirmSaleFinishedAsync(CancellationToken ct)
    {
        for (var elapsedMs = 0; elapsedMs < 10000; elapsedMs += 1000)
        {
            var (error, ecrState) = await device.CheckEcrStatusAsync(ct);
            if (error == InposExtError.InposNoError && ecrState == InposEcrState.InposEcrMainMenu)
            {
                return await BuildSuccessResultAsync(ct);
            }
            if (error == InposExtError.InposNoError && ecrState == InposEcrState.InposEcrPrintingMerchantSlip)
            {
                return Fail("MerchantSlipPending", "Cihazda iş yeri nüshası için tuşlama gerekiyor. Cihaz ekranını kontrol edin.");
            }

            var (saleErr, saleState, _) = await device.CheckSaleStatusAsync(ct);
            if (saleErr == InposExtError.InposNoError && saleState == InposEcrSaleState.InposSaleDataFinalized)
            {
                return await BuildSuccessResultAsync(ct);
            }

            await Task.Delay(1000, ct);
        }

        return Fail("SaleNotConfirmed", "Satışın tamamlandığı cihazdan doğrulanamadı. Cihaz ekranını kontrol edin.");
    }

    private async Task<FiscalSaleResult> BuildSuccessResultAsync(CancellationToken ct)
    {
        var receipt = new InposSaleReceipt(0, 0, 0, 0);
        var totals = new InposEcrSaleTotalsExt();
        InposExt.ReceiptData(TimeoutMs, ref receipt, ref totals);
        await Task.CompletedTask;

        return new FiscalSaleResult
        {
            Success = true,
            ReceiptNo = (int)receipt.receiptNo,
            ZNo = (int)receipt.zNo,
            EruNo = (int)receipt.eruNo,
            DeviceDateTimeUtc = receipt.dateTime > 0 ? InposExt.FromUnixTime((int)receipt.dateTime) : null,
            DeviceSerialNumber = device.SerialNo
        };
    }

    private static PaymentType ToInposPaymentType(FiscalPaymentMethod method) => method switch
    {
        FiscalPaymentMethod.Cash => PaymentType.CashPayment,
        FiscalPaymentMethod.CreditCard => PaymentType.CreditCardPayment,
        FiscalPaymentMethod.MealCard => PaymentType.MealCardPayment,
        _ => throw new ArgumentOutOfRangeException(nameof(method))
    };

    // SDK amount alanları UInt64 ve kuruş (TL x100) bekliyor gibi görünüyor - vendör dokümanında
    // açıkça yazmıyor, ExtTest.cs'te de doğrudan UI'daki NumericUpDown değeri aktarılıyor.
    // GERÇEK CİHAZLA doğrulanana kadar bu bir VARSAYIM - ilk canlı testte kontrol edilmeli.
    private static ulong ToKurus(decimal tl) => (ulong)Math.Round(tl * 100m, MidpointRounding.AwayFromZero);

    private static FiscalSaleResult Fail(string code, string message) => new()
    {
        Success = false,
        ErrorCode = code,
        ErrorMessage = message,
        ErrorDetail = InposDeviceService.SafeErrorDetail()
    };

    // Gerçek cihaz yokken ("cihaz elimize ulaştığında test ederiz") Web tarafının API'yi uçtan
    // uca deneyebilmesi için - hiçbir native çağrı yapmaz, tutarlı sahte bir başarı döner.
    private FiscalSaleResult SimulateSale(FiscalSaleRequest request)
    {
        var fakeReceiptNo = Random.Shared.Next(1, 99999);
        return new FiscalSaleResult
        {
            Success = true,
            Simulated = true,
            ReceiptNo = fakeReceiptNo,
            ZNo = 1,
            EruNo = fakeReceiptNo,
            DeviceDateTimeUtc = DateTime.UtcNow,
            DeviceSerialNumber = string.IsNullOrWhiteSpace(device.SerialNo) ? "SIMULATED" : device.SerialNo
        };
    }
}
