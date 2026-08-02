using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class InvoicePostingService(
    ApplicationDbContext dbContext,
    InventoryBalanceService inventoryBalance,
    PaymentReceiptPostingService paymentReceiptPosting,
    DocumentNumberGeneratorService documentNumberGenerator)
{
    public Task ApproveAsync(
        int invoiceId,
        string approvedByUserId,
        CancellationToken cancellationToken = default) =>
        // Onay artık ilişkili BusinessOrderLine.FulfilledQuantity / DispatchNoteLine.InvoicedQuantity'yi
        // de güncelleyebiliyor (Sipariş/İrsaliye → Fatura dönüşümü) — RowVersion'lı bu satırlarda iki
        // eşzamanlı onay çakışırsa Serializable izolasyon tek başına yetmiyor (bu oturumda İrsaliye
        // numarası üretiminde ham DbUpdateException olarak kanıtlandı).
        // DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync — sayaç üretimini koruyan aynı
        // genel amaçlı sarmalayıcı — DbUpdateConcurrencyException'ı yakalayıp taze transaction'la
        // otomatik tekrar dener.
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            // EnableRetryOnFailure() (Program.cs) sets a retrying execution strategy; elle açılan
            // transaction'lar bununla uyumlu değil, tüm bloğun CreateExecutionStrategy() üzerinden
            // "tekrar denenebilir birim" olarak sarılması gerekiyor (aksi halde InvalidOperationException).
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () => { await ApproveCoreAsync(invoiceId, approvedByUserId, cancellationToken); return true; });
        }, cancellationToken);

    private async Task ApproveCoreAsync(
        int invoiceId,
        string approvedByUserId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var invoice = await dbContext.Invoices
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .Include(x => x.Lines)
            .ThenInclude(x => x.BusinessOrderLine!)
            .ThenInclude(x => x.BusinessOrder!)
            .ThenInclude(x => x.Lines)
            .Include(x => x.Lines)
            .ThenInclude(x => x.DispatchNoteLine!)
            .ThenInclude(x => x.DispatchNote!)
            .ThenInclude(x => x.Lines)
            .Include(x => x.PaymentSchedules)
            .SingleOrDefaultAsync(x => x.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Fatura bulunamadı.");

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException("Yalnızca taslak faturalar onaylanabilir.");
        }

        var inventorySettings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1, cancellationToken);

        ApplyConversionFulfillment(invoice);

        await PostStockAndAccountAsync(invoice, inventorySettings, cancellationToken);

        if (invoice.IsClosedInvoice)
        {
            await PostClosedInvoiceSettlementAsync(invoice, approvedByUserId, cancellationToken);
        }

        invoice.Status = InvoiceStatus.Approved;
        invoice.ApprovedByUserId = approvedByUserId;
        invoice.ApprovedAtUtc = DateTime.UtcNow;
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
        {
            EventType = invoice.InvoiceType == InvoiceType.Sales
                ? "SalesInvoiceApproved"
                : "PurchaseInvoiceApproved",
            PayloadJson = JsonSerializer.Serialize(new
            {
                invoice.RecordId,
                invoice.InvoiceNumber,
                invoice.InvoiceType,
                invoice.CustomerId,
                invoice.GrandTotal
            })
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task CancelAsync(
        int invoiceId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        // İptal artık ilişkili BusinessOrderLine.FulfilledQuantity / DispatchNoteLine.InvoicedQuantity'yi
        // geri alabiliyor — bkz. ApproveAsync'teki aynı gerekçe.
        return DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () => { await CancelCoreAsync(invoiceId, cancelledByUserId, reason, cancellationToken); return true; });
        }, cancellationToken);
    }

    private async Task CancelCoreAsync(
        int invoiceId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var invoice = await dbContext.Invoices
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .Include(x => x.Lines)
            .ThenInclude(x => x.BusinessOrderLine!)
            .ThenInclude(x => x.BusinessOrder!)
            .ThenInclude(x => x.Lines)
            .Include(x => x.Lines)
            .ThenInclude(x => x.DispatchNoteLine!)
            .ThenInclude(x => x.DispatchNote!)
            .ThenInclude(x => x.Lines)
            .Include(x => x.PaymentSchedules)
            .Include(x => x.AccountTransactions)
            .SingleOrDefaultAsync(x => x.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Fatura bulunamadı.");

        if (invoice.Status != InvoiceStatus.Approved)
        {
            throw new InvalidOperationException("Yalnızca onaylanmış faturalar iptal edilebilir.");
        }

        if (invoice.PaymentSchedules.Any(x => x.PaidAmount > 0))
        {
            // Legacy mutabakat: PaymentReceiptPostingService.CancelCoreAsync'e PaidAmount reversal'ı
            // eklenmeden ÖNCE iptal edilmiş fişler yüzünden PaidAmount stale kalmış olabilir (örn.
            // AF.00054/AF.00055 — TED.00003/TED.00004 eski kodla iptal edildi). Sadece açık FK
            // (PaymentReceipt.InvoiceId) ile bağlı fişlere bakılır; hepsi Cancelled ve aktif fiş
            // yoksa gerçek aktif toplam zaten 0'dır, PaidAmount ona çekilir. Aktif bir fiş varsa
            // dokunulmaz ve aşağıdaki guard iptali doğru şekilde engellemeye devam eder.
            var linkedReceipts = await dbContext.PaymentReceipts
                .Where(x => x.InvoiceId == invoice.Id)
                .ToListAsync(cancellationToken);

            var allLinkedReceiptsCancelled = linkedReceipts.Count > 0
                && linkedReceipts.All(x => x.Status == PaymentReceiptStatus.Cancelled);

            if (allLinkedReceiptsCancelled)
            {
                foreach (var schedule in invoice.PaymentSchedules)
                {
                    schedule.PaidAmount = 0;
                }
            }
        }

        if (invoice.PaymentSchedules.Any(x => x.PaidAmount > 0))
        {
            throw new InvalidOperationException("Tahsilatı/ödemesi yapılmış fatura iptal edilemez.");
        }

        ReverseConversionFulfillment(invoice);

        var reversalDocumentNumber = $"IPTAL-{invoice.InvoiceNumber}";
        await ReversePostingsAsync(invoice, reversalDocumentNumber, $"Fatura iptali - {reason}", cancellationToken);

        dbContext.InvoicePaymentSchedules.RemoveRange(invoice.PaymentSchedules);

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.CancelledByUserId = cancelledByUserId;
        invoice.CancelledAtUtc = DateTime.UtcNow;
        invoice.CancellationReason = reason;
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
        {
            EventType = invoice.InvoiceType == InvoiceType.Sales
                ? "SalesInvoiceCancelled"
                : "PurchaseInvoiceCancelled",
            PayloadJson = JsonSerializer.Serialize(new
            {
                invoice.RecordId,
                invoice.InvoiceNumber,
                invoice.InvoiceType,
                invoice.CustomerId,
                invoice.GrandTotal,
                Reason = reason
            })
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    // Sipariş → Fatura (doğrudan) ve İrsaliye → Fatura dönüşümlerinden gelen satırlar için kalan
    // miktar kontrolü + sayaç güncellemesi — sadece onayda (taslak oluşturma hiçbir şeyi tüketmez).
    // Bir InvoiceLine ya DispatchNoteLineId ya BusinessOrderLineId taşır, asla ikisini birden
    // (irsaliyeden gelen bir fatura satırı siparişin havuzuna tekrar dokunmaz — irsaliye onayında
    // zaten tüketildi, bkz. DispatchNotePostingService.ApproveCoreAsync).
    private static void ApplyConversionFulfillment(Invoice invoice)
    {
        var affectedOrders = new HashSet<BusinessOrder>();
        var affectedDispatches = new HashSet<DispatchNote>();

        foreach (var line in invoice.Lines)
        {
            if (line.DispatchNoteLine is not null)
            {
                var remaining = line.DispatchNoteLine.Quantity - line.DispatchNoteLine.InvoicedQuantity;
                if (remaining < line.Quantity)
                {
                    throw new InvalidOperationException(
                        $"{line.ProductNameSnapshot} için irsaliye satırında kalan (faturalanacak) miktar yetersiz. Kalan: {remaining:N3}");
                }

                line.DispatchNoteLine.InvoicedQuantity += line.Quantity;
                affectedDispatches.Add(line.DispatchNoteLine.DispatchNote);
            }
            else if (line.BusinessOrderLine is not null)
            {
                var remaining = line.BusinessOrderLine.Quantity - line.BusinessOrderLine.FulfilledQuantity;
                if (remaining < line.Quantity)
                {
                    throw new InvalidOperationException(
                        $"{line.ProductNameSnapshot} için sipariş satırında kalan miktar yetersiz. Kalan: {remaining:N3}");
                }

                line.BusinessOrderLine.FulfilledQuantity += line.Quantity;
                affectedOrders.Add(line.BusinessOrderLine.BusinessOrder);
            }
        }

        foreach (var order in affectedOrders)
        {
            order.Status = FulfillmentStatusCalculator.Calculate(order.Lines.Select(x => (x.Quantity, x.FulfilledQuantity)));
            order.UpdatedAtUtc = DateTime.UtcNow;
        }

        foreach (var dispatch in affectedDispatches)
        {
            dispatch.Status = FulfillmentStatusCalculator.Calculate(dispatch.Lines.Select(x => (x.Quantity, x.InvoicedQuantity)));
            dispatch.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    // ApplyConversionFulfillment'ın tersi — iptalde kaynak belgenin karşılanan/faturalanan miktarını
    // güvenli biçimde geri alır.
    private static void ReverseConversionFulfillment(Invoice invoice)
    {
        var affectedOrders = new HashSet<BusinessOrder>();
        var affectedDispatches = new HashSet<DispatchNote>();

        foreach (var line in invoice.Lines)
        {
            if (line.DispatchNoteLine is not null)
            {
                line.DispatchNoteLine.InvoicedQuantity -= line.Quantity;
                affectedDispatches.Add(line.DispatchNoteLine.DispatchNote);
            }
            else if (line.BusinessOrderLine is not null)
            {
                line.BusinessOrderLine.FulfilledQuantity -= line.Quantity;
                affectedOrders.Add(line.BusinessOrderLine.BusinessOrder);
            }
        }

        foreach (var order in affectedOrders)
        {
            order.Status = FulfillmentStatusCalculator.Calculate(order.Lines.Select(x => (x.Quantity, x.FulfilledQuantity)));
            order.UpdatedAtUtc = DateTime.UtcNow;
        }

        foreach (var dispatch in affectedDispatches)
        {
            dispatch.Status = FulfillmentStatusCalculator.Calculate(dispatch.Lines.Select(x => (x.Quantity, x.InvoicedQuantity)));
            dispatch.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    // Onaylı bir faturanın satırlarını (ürün ekle/çıkar/miktar-fiyat değiştir) düzenlemeye izin
    // verir: mevcut stok/cari hareketlerini İptal ile aynı mantıkla ters kayıtla geri alır, formdan
    // gelen yeni satırları uygular (applyEdits — InvoicesController'daki MapHeader/MapLinesAsync'i
    // aynen kullanır, kod tekrarını önlemek için), sonra Onayla ile aynı mantıkla yeniden postalar.
    // Tamamı tek transaction'da olduğu için ara adımlardan biri hata verirse hiçbir şey kalıcı olmaz.
    public Task EditApprovedAsync(
        int invoiceId,
        string editedByUserId,
        Func<Invoice, Task> applyEdits,
        CancellationToken cancellationToken = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await EditApprovedCoreAsync(invoiceId, editedByUserId, applyEdits, cancellationToken));
    }

    private async Task EditApprovedCoreAsync(
        int invoiceId,
        string editedByUserId,
        Func<Invoice, Task> applyEdits,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var invoice = await dbContext.Invoices
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .Include(x => x.PaymentSchedules)
            .Include(x => x.AccountTransactions)
            .SingleOrDefaultAsync(x => x.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Fatura bulunamadı.");

        if (invoice.Status != InvoiceStatus.Approved)
        {
            throw new InvalidOperationException("Bu işlem yalnızca onaylanmış faturalar için geçerlidir.");
        }

        if (invoice.PaymentSchedules.Any(x => x.PaidAmount > 0))
        {
            throw new InvalidOperationException("Tahsilatı/ödemesi yapılmış fatura düzenlenemez. Önce tahsilatı iptal edin.");
        }

        var reversalDocumentNumber = $"DUZ-{invoice.InvoiceNumber}";
        await ReversePostingsAsync(invoice, reversalDocumentNumber, "Fatura düzenleme - eski stok/cari hareketi düzeltmesi", cancellationToken);

        dbContext.InvoicePaymentSchedules.RemoveRange(invoice.PaymentSchedules);

        await applyEdits(invoice);

        // Yeni satırlar gerçek Id alsın ki aşağıdaki StockMovement.InvoiceLineId onlara işaret
        // edebilsin (eski satırların silinmesiyle onlara bağlı hareketler SetNull ile korunur).
        await dbContext.SaveChangesAsync(cancellationToken);

        var inventorySettings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1, cancellationToken);
        await PostStockAndAccountAsync(invoice, inventorySettings, cancellationToken);

        invoice.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
        {
            EventType = invoice.InvoiceType == InvoiceType.Sales
                ? "SalesInvoiceEdited"
                : "PurchaseInvoiceEdited",
            PayloadJson = JsonSerializer.Serialize(new
            {
                invoice.RecordId,
                invoice.InvoiceNumber,
                invoice.InvoiceType,
                invoice.CustomerId,
                invoice.GrandTotal,
                EditedByUserId = editedByUserId
            })
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    // Bir faturanın o anki (onaylı) satırlarına karşılık gelen stok/cari hareketlerini ters kayıtla
    // geri alır — hem İptal hem de onaylı fatura düzenleme akışı tarafından kullanılır. Fatura
    // birden çok kez düzenlenmiş olabileceği için "orijinal" cari hareketi değil, en son (henüz
    // ters kaydı yapılmamış) hareketi baz alır.
    private async Task ReversePostingsAsync(
        Invoice invoice,
        string reversalDocumentNumber,
        string description,
        CancellationToken cancellationToken)
    {
        foreach (var line in invoice.Lines)
        {
            if (line.ProductId is null || line.Product is null || !line.Product.TrackStock)
            {
                continue;
            }

            var originalMovement = await dbContext.StockMovements
                .SingleOrDefaultAsync(x => x.InvoiceLineId == line.Id, cancellationToken);

            if (originalMovement is null)
            {
                continue;
            }

            dbContext.StockMovements.Add(new StockMovement
            {
                MovementDateUtc = DateTime.UtcNow,
                MovementType = originalMovement.MovementType,
                Quantity = -originalMovement.Quantity,
                UnitCost = originalMovement.UnitCost,
                DocumentNumber = reversalDocumentNumber,
                Description = description,
                ProductId = originalMovement.ProductId,
                ProductVariantId = originalMovement.ProductVariantId,
                WarehouseId = originalMovement.WarehouseId,
                InvoiceLineId = line.Id,
                ReversalOfId = originalMovement.Id
            });

            line.Product.StockQuantity -= originalMovement.Quantity;
            line.Product.UpdatedAtUtc = DateTime.UtcNow;
        }

        // DİKKAT: "invoice.AccountTransactions içindeki son kayıt" YANLIŞ bir seçim ölçütüdür —
        // Kapalı Fatura'nın otomatik ödeme fişi de InvoiceId ile bu koleksiyona dahildir
        // (PaymentReceiptPostingService.PostReceiptAsync satır ~103), ama o fişin İPTALİ kendi ters
        // kaydını InvoiceId ETİKETLEMEDEN oluşturur (PaymentReceiptPostingService.CancelCoreAsync).
        // Bu yüzden "son kayıt" heuristiği, fiş zaten iptal edilmiş/ters kaydedilmiş olsa bile hâlâ
        // fişin ORİJİNAL (henüz ters kaydedilmemiş görünen) hareketini seçip onu BİR DAHA ters
        // kaydediyordu — üretimde AF.00054/AF.00055 üzerinde mükerrer ters kayıt olarak bulundu ve
        // doğrulandı. Doğru ölçüt: faturanın KENDİ orijinal satış/alış hareketi — DocumentNumber
        // faturanın numarasıyla birebir eşleşen ve kendisi bir ters kayıt olmayan (ReversalOfId null)
        // tek satır. Kapalı Fatura'nın fiş hareketi asla bu numarayı taşımaz (kendi ReceiptNumber'ını
        // taşır), bu yüzden bu filtre onu hiçbir zaman yanlışlıkla seçmez.
        var lastAccountTransaction = invoice.AccountTransactions
            .Where(x => x.DocumentNumber == invoice.InvoiceNumber && x.ReversalOfId is null)
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();
        if (lastAccountTransaction is not null)
        {
            dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
            {
                TransactionDateUtc = DateTime.UtcNow,
                TransactionType = invoice.InvoiceType == InvoiceType.Sales
                    ? CurrentAccountTransactionType.CreditNote
                    : CurrentAccountTransactionType.DebitNote,
                DocumentNumber = reversalDocumentNumber,
                CurrencyCode = invoice.CurrencyCode,
                ExchangeRate = invoice.ExchangeRate,
                Debit = lastAccountTransaction.Credit,
                Credit = lastAccountTransaction.Debit,
                CustomerId = invoice.CustomerId,
                InvoiceId = invoice.Id,
                Description = description,
                ReversalOfId = lastAccountTransaction.Id
            });
        }
    }

    // Faturanın o anki satırlarını doğrular, toplamlarını hesaplar, stok hareketi + cari hareketi
    // yazar ve (yoksa) bir ödeme taksiti oluşturur — hem ilk Onayla hem de onaylı fatura düzenleme
    // (eski hareketler geri alındıktan sonra yeniden postalama) akışı tarafından kullanılır.
    private async Task PostStockAndAccountAsync(
        Invoice invoice,
        InventorySettings inventorySettings,
        CancellationToken cancellationToken)
    {
        if (invoice.Lines.Count == 0)
        {
            throw new InvalidOperationException("Faturada en az bir satır bulunmalıdır.");
        }

        foreach (var line in invoice.Lines)
        {
            ValidateLine(line);
        }

        InvoiceTotalsCalculator.Calculate(invoice);

        foreach (var line in invoice.Lines.OrderBy(x => x.LineNumber))
        {
            if (line.ProductId is null || line.Product is null || !line.Product.TrackStock)
            {
                continue;
            }

            // İrsaliyeden çağrılan (Planlı İşlem) satırların stoğu zaten İRSALİYE onayında işlendi —
            // burada ikinci kez StockMovement yazıp Product.StockQuantity'yi bir daha değiştirmek
            // çifte sayıma yol açar; bu yüzden stok tarafı tamamen atlanır (varyant/negatif stok
            // kontrolleri dahil — irsaliye onayında zaten doğrulandı). Alış Fiyatı snapshot'ı ise
            // stoktan bağımsız olduğu için (fiilen ödenen fiyatı yansıtır) yine de güncellenir —
            // aşağıdaki if bloğu bunun için korunuyor.
            if (line.DispatchNoteLineId is null)
            {
                if (inventorySettings.RequireProductVariant &&
                    line.ProductVariantId is null &&
                    await dbContext.ProductVariants.AnyAsync(
                        x => x.ProductId == line.ProductId && x.IsActive,
                        cancellationToken))
                {
                    throw new InvalidOperationException(
                        $"{line.ProductNameSnapshot} için renk/varyant seçilmelidir.");
                }

                var signedQuantity = invoice.InvoiceType == InvoiceType.Sales
                    ? -line.Quantity
                    : line.Quantity;

                if (invoice.InvoiceType == InvoiceType.Sales &&
                    inventorySettings.EnforceStockLevel &&
                    !inventorySettings.AllowNegativeStock &&
                    !inventorySettings.AllowSaleWhenOutOfStock)
                {
                    var available = await inventoryBalance.GetAvailableAsync(
                        line.ProductId.Value,
                        line.ProductVariantId,
                        invoice.WarehouseId,
                        cancellationToken);

                    if (available < line.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"{line.ProductNameSnapshot} için yeterli stok yok. Mevcut: {available:N3}");
                    }
                }

                dbContext.StockMovements.Add(new StockMovement
                {
                    MovementDateUtc = invoice.InvoiceDateUtc,
                    MovementType = invoice.InvoiceType == InvoiceType.Sales
                        ? StockMovementType.Sale
                        : StockMovementType.Purchase,
                    Quantity = signedQuantity,
                    UnitCost = invoice.InvoiceType == InvoiceType.Purchase ? line.UnitPrice : 0,
                    DocumentNumber = invoice.InvoiceNumber,
                    ProductId = line.ProductId.Value,
                    ProductVariantId = line.ProductVariantId,
                    WarehouseId = invoice.WarehouseId,
                    InvoiceLineId = line.Id,
                    Description = invoice.Notes
                });

                line.Product.StockQuantity += signedQuantity;
            }

            // Alış Faturası satır fiyatı kayıt sırasında hep KDV hariç tutulur (InvoicesController.
            // MapLinesAsync, PricesIncludeTax dönüşümü); Stok Kartı'ndaki Alış Fiyatı ise her zaman
            // KDV dahil tutulur (bkz. Products/Form.cshtml uyarı notu) — burada KDV hariç fiyat
            // tekrar KDV dahile çevrilerek ürünün Alış Fiyatı'na yansıtılır.
            if (invoice.InvoiceType == InvoiceType.Purchase)
            {
                line.Product.PurchasePrice = Math.Round(
                    line.UnitPrice * (1 + line.TaxRate / 100),
                    2,
                    MidpointRounding.AwayFromZero);
            }

            line.Product.UpdatedAtUtc = DateTime.UtcNow;
        }

        var accountTransaction = new CurrentAccountTransaction
        {
            TransactionDateUtc = invoice.InvoiceDateUtc,
            TransactionType = invoice.InvoiceType == InvoiceType.Sales
                ? CurrentAccountTransactionType.Sale
                : CurrentAccountTransactionType.Purchase,
            DocumentNumber = invoice.InvoiceNumber,
            CurrencyCode = invoice.CurrencyCode,
            ExchangeRate = invoice.ExchangeRate,
            Debit = invoice.InvoiceType == InvoiceType.Sales ? invoice.GrandTotal : 0,
            Credit = invoice.InvoiceType == InvoiceType.Purchase ? invoice.GrandTotal : 0,
            DueDateUtc = invoice.DueDateUtc,
            CustomerId = invoice.CustomerId,
            InvoiceId = invoice.Id,
            Description = invoice.Notes
        };
        dbContext.CurrentAccountTransactions.Add(accountTransaction);

        if (invoice.PaymentSchedules.Count == 0)
        {
            invoice.PaymentSchedules.Add(new InvoicePaymentSchedule
            {
                InstallmentNumber = 1,
                DueDateUtc = invoice.DueDateUtc ?? invoice.InvoiceDateUtc,
                Amount = invoice.GrandTotal
            });
        }
    }

    // "Kapalı Fatura" işaretliyken, faturayla aynı anda otomatik onaylı bir Tahsilat (Satış) /
    // Tediye (Alış) fişi oluşturup postalar (PaymentReceiptPostingService.PostReceiptAsync — elle
    // akışla aynı cari/finansal hareket mantığı) ve tek taksitli ödeme planını tamamen ödenmiş
    // işaretler. Bu, InvoicePaymentSchedule.PaidAmount'a yazan sistemdeki tek yerdir; bir kez
    // yazıldıktan sonra CancelCoreAsync/EditApprovedCoreAsync'teki "PaidAmount > 0" korumaları
    // devreye girer (kapalı/tahsil edilmiş fatura elle iptal/düzenleme ile bozulamaz).
    private async Task PostClosedInvoiceSettlementAsync(Invoice invoice, string approvedByUserId, CancellationToken cancellationToken)
    {
        if (invoice.SettlementFinancialAccountId is null || invoice.SettlementPaymentMethod is null)
        {
            throw new InvalidOperationException("Kapalı fatura için kapanacak kasa/banka ve ödeme şekli seçilmelidir.");
        }

        var receiptType = invoice.InvoiceType == InvoiceType.Sales ? ReceiptType.Collection : ReceiptType.Payment;
        var sequenceKey = receiptType == ReceiptType.Collection ? "COLLECTION_RECEIPT" : "PAYMENT_RECEIPT";
        var receiptNumber = await documentNumberGenerator.GenerateWithinTransactionAsync(sequenceKey, cancellationToken);
        var description = receiptType == ReceiptType.Collection
            ? $"{invoice.InvoiceNumber} kapalı fatura tahsilatı"
            : $"{invoice.InvoiceNumber} kapalı fatura tediyesi";

        var receipt = new PaymentReceipt
        {
            ReceiptType = receiptType,
            ReceiptNumber = receiptNumber,
            DocumentNumber = invoice.InvoiceNumber,
            ReceiptDateUtc = invoice.InvoiceDateUtc,
            CurrencyCode = invoice.CurrencyCode,
            ExchangeRate = invoice.ExchangeRate,
            Description = description,
            CreatedByUserId = approvedByUserId,
            CustomerId = invoice.CustomerId,
            InvoiceId = invoice.Id,
            Lines =
            {
                new PaymentReceiptLine
                {
                    LineNumber = 1,
                    PaymentMethod = invoice.SettlementPaymentMethod.Value,
                    Amount = invoice.GrandTotal,
                    FinancialAccountId = invoice.SettlementFinancialAccountId.Value,
                    Description = description
                }
            }
        };
        dbContext.PaymentReceipts.Add(receipt);

        await paymentReceiptPosting.PostReceiptAsync(receipt, cancellationToken);

        receipt.Status = PaymentReceiptStatus.Approved;
        receipt.ApprovedByUserId = approvedByUserId;
        receipt.ApprovedAtUtc = DateTime.UtcNow;

        invoice.PaymentSchedules.Single().PaidAmount = invoice.GrandTotal;
    }

    // Negatif miktara izin verilir (iade satırı: aynı fatura içinde -1, -2 gibi bir satırla iade
    // düşülebilir) — sadece sıfır miktarlı satır anlamsız olduğu için engellenir.
    private static void ValidateLine(InvoiceLine line)
    {
        if (line.Quantity == 0)
        {
            throw new InvalidOperationException("Fatura satır miktarı sıfır olamaz.");
        }

        if (line.UnitPrice < 0 || line.DiscountRate is < 0 or > 100 || line.TaxRate is < 0 or > 100)
        {
            throw new InvalidOperationException("Fatura satır fiyat, iskonto veya KDV bilgisi geçersiz.");
        }
    }
}
