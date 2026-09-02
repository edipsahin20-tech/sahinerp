using Inpos;
using Microsoft.Extensions.Options;
using SahinSoft.FiscalAgent.Config;

namespace SahinSoft.FiscalAgent.Inpos;

// InposExt.Net.dll'in ham çağrılarını sarmalayan alt katman - vendörün ExtTest.cs örnek
// projesindeki internalCheckEcrStatus/internalCheckSaleStatus/reconnect metodlarının headless
// (WinForms/Application.DoEvents olmadan, async/await ile) karşılığı. Üstteki SaleOrchestrator
// bu servisi kullanır, ham Inpos.* tiplerini dışarı hiç sızdırmaz.
public sealed class InposDeviceService(IOptionsMonitor<FiscalAgentConfig> config, ILogger<InposDeviceService> logger)
{
    private const uint DefaultTimeoutMs = 5000;
    private const uint InitTimeoutMs = 700;
    private bool _initialized;

    public bool SimulationMode => config.CurrentValue.SimulationMode;
    public string SerialNo => config.CurrentValue.Device.SerialNo;

    // Vardiya/kasiyer ekranındaki BENİ OKU.txt talimatı: "cihazin tuslarinin kilitlenmesini
    // kesinlikle tavsiye ederiz" - agent başlarken ve her reconnect sonrası çağrılır.
    public async Task EnsureInitializedAsync(CancellationToken ct = default)
    {
        if (SimulationMode)
        {
            return;
        }

        if (_initialized)
        {
            return;
        }

        await ConnectAsync(ct);
    }

    private async Task<InposExtError> ConnectAsync(CancellationToken ct)
    {
        var device = config.CurrentValue.Device;
        if (string.IsNullOrWhiteSpace(device.SerialNo) || string.IsNullOrWhiteSpace(device.Ip) || device.Port <= 0)
        {
            throw new InvalidOperationException(
                "Cihaz bilgileri (SerialNo/Ip/Port) agent.config.json'da tanımlı değil. Önce gmp3 eşlemesi yapılıp cihaz bilgileri girilmeli.");
        }

        var error = InposExt.Initialize(1, device.SerialNo.ToUpperInvariant(), device.Ip, (ushort)device.Port, InitTimeoutMs);
        if (error == InposExtError.InposNoError)
        {
            _initialized = true;
            try
            {
                InposExt.BlockEcrKeys();
            }
            catch (Exception ex)
            {
                // Tuş kilitleme başarısız olsa bile bağlantı kurulmuş sayılır - kritik değil.
                logger.LogWarning(ex, "BlockEcrKeys başarısız oldu.");
            }
        }
        else
        {
            logger.LogWarning("Initialize başarısız: {Error} ({Detail})", error, SafeErrorDetail());
        }

        await Task.CompletedTask;
        return error;
    }

    // Bağlantı hatası (InposConnectionError) alındığında en fazla 10 kere yeniden bağlanmayı
    // dener - ExtTest.cs'teki reconnect() metoduyla aynı desen, Thread.Sleep yerine Task.Delay.
    public async Task<InposExtError> ReconnectAsync(CancellationToken ct = default)
    {
        _initialized = false;
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var error = await ConnectAsync(ct);
            if (error == InposExtError.InposNoError)
            {
                return error;
            }
            if (error != InposExtError.InposConnectionError)
            {
                return error;
            }
            await Task.Delay(300, ct);
        }
        return InposExtError.InposConnectionError;
    }

    public async Task<(InposExtError Error, InposEcrState EcrState)> CheckEcrStatusAsync(CancellationToken ct = default)
    {
        var error = InposExt.EcrState(DefaultTimeoutMs, out var ecrState);
        if (error == InposExtError.InposNoError)
        {
            return (error, ecrState);
        }

        if (error == InposExtError.InposConnectionError || ecrState == InposEcrState.InposEcrInitialization)
        {
            error = await ReconnectAsync(ct);
            if (error == InposExtError.InposNoError)
            {
                error = InposExt.EcrState(DefaultTimeoutMs, out ecrState);
            }
        }

        return (error, ecrState);
    }

    public async Task<(InposExtError Error, InposEcrSaleState SaleState, InposSaleReceipt Receipt)> CheckSaleStatusAsync(CancellationToken ct = default)
    {
        var totalsDummy = new InposEcrSaleTotals();
        var receipt = new InposSaleReceipt(0, 0, 0, 0);
        var error = InposExt.SaleState(DefaultTimeoutMs, out var saleState, ref totalsDummy, ref receipt);
        if (error == InposExtError.InposConnectionError)
        {
            error = await ReconnectAsync(ct);
            if (error == InposExtError.InposNoError)
            {
                error = InposExt.SaleState(DefaultTimeoutMs, out saleState, ref totalsDummy, ref receipt);
            }
        }
        return (error, saleState, receipt);
    }

    public static string SafeErrorDetail()
    {
        try
        {
            return InposExt.ErrorDetail().ToString();
        }
        catch
        {
            return "bilinmiyor";
        }
    }
}
