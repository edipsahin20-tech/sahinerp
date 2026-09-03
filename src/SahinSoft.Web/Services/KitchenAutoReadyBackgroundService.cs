namespace SahinSoft.Web.Services;

/// <summary>
/// InventorySettings.KitchenAutoReadyMinutes doluysa (Edip, 2026-09-03), mutfağa gönderilen bir
/// fiş o süre içinde mutfak personeli hiç dokunmasa bile otomatik "Hazır" durumuna geçer - her
/// dakika RestaurantPostingService.AutoAdvanceOverdueKitchenTicketsAsync'i çağırır (asıl durum
/// geçiş kuralı orada, AdvanceKitchenTicketAsync ile AYNI kod yolu). Parametre boş/0 ise servis
/// her turda erken çıkar, hiçbir şey yapmaz.
/// </summary>
public sealed class KitchenAutoReadyBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<KitchenAutoReadyBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var postingService = scope.ServiceProvider.GetRequiredService<RestaurantPostingService>();
                var advancedCount = await postingService.AutoAdvanceOverdueKitchenTicketsAsync(stoppingToken);
                if (advancedCount > 0)
                {
                    logger.LogInformation("Mutfak otomatik Hazır: {Count} fiş ilerletildi.", advancedCount);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Mutfak otomatik Hazır turu başarısız oldu, bir sonraki turda tekrar denenecek.");
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}
