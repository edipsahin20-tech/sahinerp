using System.Net.Sockets;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services.Printing;

public sealed record PrintSendResult(bool Success, string? Error);

// Kuyruk/outbox deseni (Edip'in planı: "satış kapanışı/mutfak fişi/X/Z tetiklendiğinde SADECE
// bir PrintJob satırı eklenir, asıl gönderim ayrı bir arka plan döngüsünde denenir. Böylece
// yazıcı kapalı/erişilemez olsa bile satış akışı ASLA bloklanmaz"). Test Yazdır BUNU
// ATLAMAZ - aynı PrintJob satırını oluşturup senkron olarak SendAsync çağırır (anlık geri
// bildirim için), kayıt/izlenebilirlik her durumda oluşur.
public sealed class PrintDispatchService(ApplicationDbContext dbContext, PrintRenderingService renderingService, IPrintDataProvider dataProvider, IHttpClientFactory httpClientFactory)
{
    public async Task<int?> EnqueueForRoleAsync(PrinterRole role, int branchId, PrintTemplateType templateType, int? sourceId, string sourceDescription, CancellationToken cancellationToken = default, int? kitchenStationId = null)
    {
        var candidates = await dbContext.Printers.AsNoTracking().Where(x => x.Role == role && x.BranchId == branchId && x.IsActive).ToListAsync(cancellationToken);
        // Mutfak rolünde birden fazla istasyona (Sıcak Mutfak/Bar vb.) ayrı yazıcı atanmışsa,
        // KitchenStationId eşleşen yazıcı ÖNCELİKLİDİR; eşleşme yoksa istasyona bağlı olmayan
        // genel bir Mutfak yazıcısına düşer (kademeli geçiş - tek yazıcılı şubeler hiçbir ek
        // ayar yapmadan çalışmaya devam eder).
        var printer = kitchenStationId.HasValue
            ? candidates.FirstOrDefault(x => x.KitchenStationId == kitchenStationId.Value) ?? candidates.FirstOrDefault(x => x.KitchenStationId == null)
            : candidates.FirstOrDefault();
        if (printer is null)
        {
            // Bu role atanmış aktif yazıcı yok - sessizce atla (Edip'in kademeli geçiş kararı:
            // yazıcı tanımlanana kadar eski window.print() akışları BOZULMADAN çalışmaya devam
            // eder, bu sadece EK bir yol).
            return null;
        }

        var job = await CreateJobAsync(printer, templateType, sourceId, sourceDescription, isTestPrint: false, cancellationToken);
        return job.Id;
    }

    // Kullanıcının EL İLE tetiklediği yazdırma (ör. Z Raporu detay ekranındaki "Termal Yazıcıya
    // Gönder") - satış akışının aksine burada ANLIK geri bildirim beklenir, bu yüzden arka plan
    // kuyruğunu (12sn poll) beklemeden senkron gönderilir. Kayıt/izlenebilirlik AYNI PrintJob
    // satırı üzerinden, kuyruk deseninden farklı değildir.
    public async Task<(int? JobId, PrintSendResult? Result)> EnqueueAndSendForRoleAsync(PrinterRole role, int branchId, PrintTemplateType templateType, int? sourceId, string sourceDescription, CancellationToken cancellationToken = default)
    {
        var jobId = await EnqueueForRoleAsync(role, branchId, templateType, sourceId, sourceDescription, cancellationToken);
        if (!jobId.HasValue)
        {
            return (null, null);
        }
        var result = await SendAsync(jobId.Value, cancellationToken);
        return (jobId, result);
    }

    public async Task<(int JobId, PrintSendResult Result)> TestPrintAsync(int printerId, CancellationToken cancellationToken = default)
    {
        var printer = await dbContext.Printers.AsNoTracking().SingleAsync(x => x.Id == printerId, cancellationToken);
        var templateType = await ResolveTemplateTypeAsync(printer, cancellationToken);
        var job = await CreateJobAsync(printer, templateType, sourceId: null, "Test Yazdır", isTestPrint: true, cancellationToken);
        var result = await SendAsync(job.Id, cancellationToken);
        return (job.Id, result);
    }

    // Çıktı Tasarımcısı'nda HENÜZ KAYDEDİLMEMİŞ taslak düzenle test yazdırma - kayıtlı bir
    // PrintTemplate'e ihtiyaç duymaz, tasarımcının o anki LayoutJson'ını doğrudan render eder.
    public async Task<(int JobId, PrintSendResult Result)> TestPrintDraftAsync(int printerId, PrintTemplateType templateType, int paperWidthMm, string layoutJson, CancellationToken cancellationToken = default)
    {
        var printer = await dbContext.Printers.AsNoTracking().SingleAsync(x => x.Id == printerId, cancellationToken);
        var layout = PrintRenderingService.ParseLayout(layoutJson);
        var sample = dataProvider.BuildSample(templateType);
        var escPos = await renderingService.RenderEscPosAsync(layout, paperWidthMm, sample, cancellationToken);

        var job = new PrintJob
        {
            PrinterId = printer.Id,
            PrintTemplateId = null,
            RenderedEscPosBase64 = Convert.ToBase64String(escPos),
            SourceDescription = "Tasarımcı - Test Yazdır (taslak)",
            OccurredAtUtc = DateTime.UtcNow,
            IsTestPrint = true
        };
        dbContext.PrintJobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = await SendAsync(job.Id, cancellationToken);
        return (job.Id, result);
    }

    private async Task<PrintTemplateType> ResolveTemplateTypeAsync(Printer printer, CancellationToken cancellationToken)
    {
        if (printer.PrintTemplateId.HasValue)
        {
            var t = await dbContext.PrintTemplates.AsNoTracking().Where(x => x.Id == printer.PrintTemplateId.Value).Select(x => (PrintTemplateType?)x.TemplateType).SingleOrDefaultAsync(cancellationToken);
            if (t.HasValue)
            {
                return t.Value;
            }
        }
        return printer.Role switch
        {
            PrinterRole.Adisyon => PrintTemplateType.Adisyon,
            PrinterRole.Mutfak => PrintTemplateType.MutfakFisi,
            PrinterRole.XRaporu => PrintTemplateType.XRaporu,
            PrinterRole.ZRaporu => PrintTemplateType.ZRaporu,
            PrinterRole.CariMakbuz => PrintTemplateType.CariMakbuz,
            _ => PrintTemplateType.Adisyon
        };
    }

    private async Task<PrintJob> CreateJobAsync(Printer printer, PrintTemplateType templateType, int? sourceId, string sourceDescription, bool isTestPrint, CancellationToken cancellationToken)
    {
        var template = printer.PrintTemplateId.HasValue
            ? await dbContext.PrintTemplates.AsNoTracking().SingleOrDefaultAsync(x => x.Id == printer.PrintTemplateId.Value, cancellationToken)
            : await dbContext.PrintTemplates.AsNoTracking().Where(x => x.TemplateType == templateType && x.IsDefault && x.IsActive).FirstOrDefaultAsync(cancellationToken);

        // Yazıcıya bağlı şablon istenen belge tipinden farklıysa (ör. makbuz, Adisyon yazıcısına
        // yönlendirildiğinde) o şablon kullanılmaz; istenen tipin varsayılan şablonu alınır.
        if (template != null && template.TemplateType != templateType)
        {
            template = await dbContext.PrintTemplates.AsNoTracking().Where(x => x.TemplateType == templateType && x.IsDefault && x.IsActive).FirstOrDefaultAsync(cancellationToken);
        }

        var layout = template != null ? PrintRenderingService.ParseLayout(template.LayoutJson) : [];
        var paperWidth = template?.PaperWidthMm ?? (int)printer.PaperWidth;

        var data = isTestPrint || sourceId is null
            ? dataProvider.BuildSample(templateType)
            : await BuildRealDataAsync(templateType, sourceId.Value, cancellationToken);

        var escPos = await renderingService.RenderEscPosAsync(layout, paperWidth, data, cancellationToken);

        var job = new PrintJob
        {
            PrinterId = printer.Id,
            PrintTemplateId = template?.Id,
            RenderedEscPosBase64 = Convert.ToBase64String(escPos),
            SourceDescription = sourceDescription,
            OccurredAtUtc = DateTime.UtcNow,
            IsTestPrint = isTestPrint
        };
        dbContext.PrintJobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);
        return job;
    }

    private async Task<PrintDataContext> BuildRealDataAsync(PrintTemplateType templateType, int sourceId, CancellationToken cancellationToken) => templateType switch
    {
        PrintTemplateType.Adisyon => await dataProvider.BuildForRetailSaleAsync(sourceId, cancellationToken),
        PrintTemplateType.MutfakFisi => await dataProvider.BuildForKitchenTicketAsync(sourceId, cancellationToken),
        PrintTemplateType.ZRaporu => await dataProvider.BuildForZPeriodAsync(sourceId, cancellationToken),
        PrintTemplateType.XRaporu => await dataProvider.BuildForXReportAsync(sourceId, cancellationToken),
        PrintTemplateType.CariMakbuz => await dataProvider.BuildForPaymentReceiptAsync(sourceId, cancellationToken),
        _ => new PrintDataContext()
    };

    // Bekleyen kuyruktaki bir PrintJob'u gönderir - NetworkRaw9100 için web sunucusu DOĞRUDAN
    // ham TCP soketle yazıcıya bağlanır (agent gerekmez), WindowsSpooler için o makinedeki
    // SahinSoft.PrintAgent'a HTTP POST edilir.
    public async Task<PrintSendResult> SendAsync(int jobId, CancellationToken cancellationToken = default)
    {
        var job = await dbContext.PrintJobs.Include(x => x.Printer).SingleAsync(x => x.Id == jobId, cancellationToken);
        var bytes = Convert.FromBase64String(job.RenderedEscPosBase64);

        PrintSendResult result;
        try
        {
            result = job.Printer.ConnectionType == PrinterConnectionType.NetworkRaw9100
                ? await SendRawTcpAsync(job.Printer.ConnectionAddress, bytes, cancellationToken)
                : await SendViaAgentAsync(job.Printer, bytes, cancellationToken);
        }
        catch (Exception ex)
        {
            result = new PrintSendResult(false, ex.Message);
        }

        if (result.Success)
        {
            job.ProcessedAtUtc = DateTime.UtcNow;
            job.LastError = null;
        }
        else
        {
            job.RetryCount++;
            job.LastError = result.Error;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static async Task<PrintSendResult> SendRawTcpAsync(string address, byte[] bytes, CancellationToken cancellationToken)
    {
        var parts = address.Split(':', 2);
        if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
        {
            return new PrintSendResult(false, $"Geçersiz ağ yazıcı adresi: '{address}'. 'ip:port' biçiminde olmalı (örn. 192.168.1.50:9100).");
        }

        using var client = new TcpClient();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            await client.ConnectAsync(parts[0], port, cts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new PrintSendResult(false, $"Yazıcıya bağlanılamadı (zaman aşımı): {address}");
        }
        catch (SocketException ex)
        {
            return new PrintSendResult(false, $"Yazıcıya bağlanılamadı: {ex.Message}");
        }

        await using var stream = client.GetStream();
        await stream.WriteAsync(bytes, cancellationToken);
        await stream.FlushAsync(cancellationToken);
        return new PrintSendResult(true, null);
    }

    private async Task<PrintSendResult> SendViaAgentAsync(Printer printer, byte[] bytes, CancellationToken cancellationToken)
    {
        var baseUrl = string.IsNullOrWhiteSpace(printer.AgentBaseUrl) ? "http://localhost:5058" : printer.AgentBaseUrl;
        var client = httpClientFactory.CreateClient("PrintAgent");
        client.Timeout = TimeSpan.FromSeconds(10);

        var payload = JsonSerializer.Serialize(new { printerName = printer.ConnectionAddress, dataBase64 = Convert.ToBase64String(bytes) });
        try
        {
            var response = await client.PostAsync($"{baseUrl.TrimEnd('/')}/print/job", new StringContent(payload, System.Text.Encoding.UTF8, "application/json"), cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new PrintSendResult(false, $"Agent hatası ({baseUrl}): HTTP {(int)response.StatusCode} - {body}");
            }

            // Agent HTTP 200 ile birlikte GERÇEK sonucu {"success": bool, "error": "..."} gövdesinde
            // döner (ör. yazıcı bulunamadı) - GERÇEK HATA (bulundu 2026-09-28): burada önceden
            // sadece HTTP status koduna bakılıyordu, agent'ın "success:false" gövdesi hiç
            // okunmuyordu, bu yüzden başarısız bir yazdırma bile "Başarılı" olarak raporlanıyordu.
            try
            {
                using var doc = JsonDocument.Parse(body);
                var success = doc.RootElement.TryGetProperty("success", out var successProp) && successProp.GetBoolean();
                if (!success)
                {
                    var agentError = doc.RootElement.TryGetProperty("error", out var errorProp) ? errorProp.GetString() : null;
                    return new PrintSendResult(false, $"Agent hatası ({baseUrl}): {agentError ?? "bilinmeyen hata"}");
                }
            }
            catch (JsonException)
            {
                // Gövde beklenen JSON şeklinde değilse HTTP başarı kodu yine de anlamlı bir sinyal -
                // eski/uyumsuz bir agent sürümüyle konuşuyor olabiliriz, en azından bağlantı kurulmuş.
            }

            return new PrintSendResult(true, null);
        }
        catch (Exception ex)
        {
            return new PrintSendResult(false, $"PrintAgent'a ulaşılamadı ({baseUrl}): {ex.Message}");
        }
    }
}
