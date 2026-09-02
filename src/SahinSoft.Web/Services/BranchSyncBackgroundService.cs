using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models.Api;

namespace SahinSoft.Web.Services;

/// <summary>
/// Hibrit mimarinin şube tarafı: MerkezSync:Enabled açıksa periyodik olarak merkezden
/// katalog (kategori/KDV/stok-fiyat) çeker ve yerel veritabanına RecordId üzerinden
/// upsert eder. Merkez bağlantısı yoksa/koparsa hiçbir şeyi durdurmaz, sadece bir
/// sonraki turda tekrar dener - şube bu servisten bağımsız çalışmaya devam eder.
/// </summary>
public sealed class BranchSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<MerkezSyncOptions> options,
    ILogger<BranchSyncBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var current = options.CurrentValue;
            if (!current.Enabled || string.IsNullOrWhiteSpace(current.BaseUrl) || string.IsNullOrWhiteSpace(current.BranchCode))
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                continue;
            }

            try
            {
                await RunCatalogSyncAsync(current, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Merkez senkronu (katalog) başarısız oldu, bir sonraki turda tekrar denenecek.");
            }

            try
            {
                await RunOutboxPushAsync(current, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Merkez senkronu (satış push) başarısız oldu, bir sonraki turda tekrar denenecek.");
            }

            var delaySeconds = Math.Max(30, current.PollIntervalSeconds);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }
    }

    private async Task RunCatalogSyncAsync(MerkezSyncOptions current, CancellationToken cancellationToken)
    {
        var state = BranchSyncState.Load();
        var since = state.LastCatalogSyncUtc ?? DateTime.MinValue;

        var client = httpClientFactory.CreateClient("MerkezSync");
        client.BaseAddress = new Uri(current.BaseUrl);
        client.DefaultRequestHeaders.Remove("X-Branch-ApiKey");
        client.DefaultRequestHeaders.Add("X-Branch-ApiKey", current.ApiKey);

        var sinceQuery = since.ToString("O");
        var requestUri = $"api/sync/catalog?branchCode={Uri.EscapeDataString(current.BranchCode)}&since={Uri.EscapeDataString(sinceQuery)}";

        var response = await client.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Merkez senkron isteği başarısız: {StatusCode}", response.StatusCode);
            return;
        }

        var payload = await response.Content.ReadFromJsonAsync<CatalogSyncResponse>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var appliedCount = 0;

        foreach (var item in payload.Categories)
        {
            var local = await dbContext.ProductCategories.FirstOrDefaultAsync(x => x.RecordId == item.RecordId, cancellationToken);
            if (local is null)
            {
                dbContext.ProductCategories.Add(new ProductCategory
                {
                    RecordId = item.RecordId,
                    Code = item.Code,
                    Name = item.Name,
                    IsActive = item.IsActive
                });
            }
            else
            {
                local.Code = item.Code;
                local.Name = item.Name;
                local.IsActive = item.IsActive;
            }
            appliedCount++;
        }

        foreach (var item in payload.TaxRates)
        {
            var local = await dbContext.TaxRates.FirstOrDefaultAsync(x => x.RecordId == item.RecordId, cancellationToken);
            if (local is null)
            {
                dbContext.TaxRates.Add(new TaxRate
                {
                    RecordId = item.RecordId,
                    Code = item.Code,
                    Name = item.Name,
                    Rate = item.Rate,
                    IsExempt = item.IsExempt,
                    IsActive = item.IsActive
                });
            }
            else
            {
                local.Code = item.Code;
                local.Name = item.Name;
                local.Rate = item.Rate;
                local.IsExempt = item.IsExempt;
                local.IsActive = item.IsActive;
            }
            appliedCount++;
        }

        // Kategori/KDV eklemelerini önce kaydet ki ürünler onları FK ile referans edebilsin.
        if (payload.Categories.Count > 0 || payload.TaxRates.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var item in payload.Products)
        {
            var category = await dbContext.ProductCategories.FirstOrDefaultAsync(x => x.RecordId == item.CategoryRecordId, cancellationToken);
            var taxRate = await dbContext.TaxRates.FirstOrDefaultAsync(x => x.RecordId == item.TaxRateRecordId, cancellationToken);
            if (category is null || taxRate is null)
            {
                logger.LogWarning("Ürün {StockCode} için kategori/KDV yerelde bulunamadı, bu turda atlandı.", item.StockCode);
                continue;
            }

            var local = await dbContext.Products.FirstOrDefaultAsync(x => x.RecordId == item.RecordId, cancellationToken);
            if (local is null)
            {
                dbContext.Products.Add(new Product
                {
                    RecordId = item.RecordId,
                    StockCode = item.StockCode,
                    Name = item.Name,
                    Barcode = item.Barcode,
                    Unit = item.Unit,
                    PurchasePrice = item.PurchasePrice,
                    SalePrice = item.SalePrice,
                    TrackStock = item.TrackStock,
                    IsActive = item.IsActive,
                    CategoryId = category.Id,
                    TaxRateId = taxRate.Id
                });
            }
            else
            {
                local.StockCode = item.StockCode;
                local.Name = item.Name;
                local.Barcode = item.Barcode;
                local.Unit = item.Unit;
                local.PurchasePrice = item.PurchasePrice;
                local.SalePrice = item.SalePrice;
                local.TrackStock = item.TrackStock;
                local.IsActive = item.IsActive;
                local.CategoryId = category.Id;
                local.TaxRateId = taxRate.Id;
            }
            appliedCount++;
        }

        if (payload.Products.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        state.LastCatalogSyncUtc = payload.ServerTimeUtc;
        state.Save();

        if (appliedCount > 0)
        {
            logger.LogInformation("Merkez senkronu: {Count} kayıt güncellendi.", appliedCount);
        }
    }

    // Faz C: bekleyen (ProcessedAtUtc IS NULL) IntegrationOutboxMessage kayıtlarını merkeze
    // gönderir. Merkez tarafı ExternalRecordMapping ile idempotent olduğu için burada da
    // "gönderdim ama yanıtı alamadım" senaryosunda güvenle tekrar denenebilir - iki kez
    // postalanma riski yok.
    private async Task RunOutboxPushAsync(MerkezSyncOptions current, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var pending = await dbContext.IntegrationOutboxMessages
            .Where(x => x.EventType == "RestaurantCheckClosed" && x.ProcessedAtUtc == null)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        var client = httpClientFactory.CreateClient("MerkezSync");
        client.BaseAddress = new Uri(current.BaseUrl);
        client.DefaultRequestHeaders.Remove("X-Branch-ApiKey");
        client.DefaultRequestHeaders.Add("X-Branch-ApiKey", current.ApiKey);

        var request = new TransactionSyncRequest
        {
            Events = pending.Select(x => new TransactionSyncEvent
            {
                RecordId = x.RecordId,
                EventType = x.EventType,
                PayloadJson = x.PayloadJson
            }).ToList()
        };

        var requestUri = $"api/sync/transactions?branchCode={Uri.EscapeDataString(current.BranchCode)}";
        var response = await client.PostAsJsonAsync(requestUri, request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Merkeze satış push isteği başarısız: {StatusCode}", response.StatusCode);
            foreach (var message in pending)
            {
                message.RetryCount++;
                message.LastError = $"HTTP {(int)response.StatusCode}";
            }
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        // Merkez tüm event'leri tek transaction'da (ya da idempotent tekrar) işlediği için burada
        // hepsini "işlendi" say - kısmi başarı senaryosu yok (bkz. SyncController.PostTransactions,
        // her event kendi ExternalRecordMapping kaydıyla ayrı ayrı idempotent).
        foreach (var message in pending)
        {
            message.ProcessedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Merkeze {Count} satış kaydı gönderildi.", pending.Count);
    }
}
