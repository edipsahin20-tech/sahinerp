namespace SahinSoft.Web.Services;

/// <summary>
/// Talimat 1 (2026-09-06) - InventorySettings.AutoZEnabled açıksa her ~30 saniyede bir
/// RestaurantPostingService.RunAutomaticZCheckAsync'i çağırır; asıl karar/atomiklik orada
/// (MANUEL Z'nin kullandığı CloseActiveZPeriodAsync ile AYNI motor). Kapalıysa veya saat henüz
/// gelmediyse/güne ait karar zaten verildiyse hiçbir şey yapmadan bir sonraki tura geçer.
/// </summary>
public sealed class RestaurantAutoZBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<RestaurantAutoZBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var postingService = scope.ServiceProvider.GetRequiredService<RestaurantPostingService>();
                var closed = await postingService.RunAutomaticZCheckAsync(DateTime.UtcNow, stoppingToken);
                if (closed is not null)
                {
                    logger.LogInformation("Otomatik Z: Z-{Id:D6} oluşturuldu ({ReceiptCount} fiş, {NetTotal} TL).", closed.Id, closed.ReceiptCount, closed.NetTotal);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Otomatik Z kontrolü başarısız oldu, bir sonraki turda tekrar denenecek.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
