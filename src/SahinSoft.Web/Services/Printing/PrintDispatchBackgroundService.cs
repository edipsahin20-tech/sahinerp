using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services.Printing;

// BranchSyncBackgroundService ile AYNI desen - bekleyen (ProcessedAtUtc IS NULL) PrintJob
// kayıtlarını periyodik dener. Yazıcı kapalı/erişilemez olsa bile satış akışı ASLA bloklanmaz;
// başarısızlıklar RetryCount/LastError'a yazılır, 10 denemeden sonra kalıcı başarısız sayılır
// (bir daha otomatik denenmez, Yazıcı Yönetimi ekranından manuel "Tekrar Dene" gerekir).
public sealed class PrintDispatchBackgroundService(IServiceScopeFactory scopeFactory, ILogger<PrintDispatchBackgroundService> logger) : BackgroundService
{
    private const int MaxRetryCount = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Yazdırma kuyruğu işlenirken hata oluştu, bir sonraki turda tekrar denenecek.");
            }

            await Task.Delay(TimeSpan.FromSeconds(12), stoppingToken);
        }
    }

    private async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dispatchService = scope.ServiceProvider.GetRequiredService<PrintDispatchService>();

        var pendingIds = await dbContext.PrintJobs
            .Where(x => x.ProcessedAtUtc == null && x.RetryCount < MaxRetryCount)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(20)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var jobId in pendingIds)
        {
            var result = await dispatchService.SendAsync(jobId, cancellationToken);
            if (!result.Success)
            {
                logger.LogWarning("PrintJob {JobId} gönderilemedi: {Error}", jobId, result.Error);
            }
        }
    }
}
