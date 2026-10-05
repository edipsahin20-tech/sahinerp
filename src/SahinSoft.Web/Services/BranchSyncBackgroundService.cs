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
            // Eşleşme RecordId veya kod (benzersiz) ile: yerelde aynı kodlu kayıt varsa ikinci kopya
            // eklenmez (IX_..._Code benzersizlik hatası katalog senkronunu kilitliyordu). Merkez kimliği alınır.
            var local = await dbContext.ProductCategories.FirstOrDefaultAsync(x => x.RecordId == item.RecordId || x.Code == item.Code, cancellationToken);
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
                local.RecordId = item.RecordId;
                local.Code = item.Code;
                local.Name = item.Name;
                local.IsActive = item.IsActive;
            }
            appliedCount++;
        }

        foreach (var item in payload.TaxRates)
        {
            // Eşleşme RecordId veya kod ile (bkz. kategori notu). KDV10 gibi yerel kodlu kayıtlar çakışmaz.
            var local = await dbContext.TaxRates.FirstOrDefaultAsync(x => x.RecordId == item.RecordId || x.Code == item.Code, cancellationToken);
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
                local.RecordId = item.RecordId;
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

        // Merkez RecordId -> yerel Id eşlemesi. Yerelde kodla eşleşen kayıtların (ör. KDV10) RecordId'si merkezinkinden
        // farklı kalabilir (RecordId mevcut satırda güncellenmez); bu yüzden ürünler RecordId yerine bu eşlemeyle bağlanır.
        var categoryMap = new Dictionary<Guid, ProductCategory>();
        foreach (var item in payload.Categories)
        {
            var match = await dbContext.ProductCategories.FirstOrDefaultAsync(x => x.RecordId == item.RecordId || x.Code == item.Code, cancellationToken);
            if (match is not null) { categoryMap[item.RecordId] = match; }
        }
        var taxMap = new Dictionary<Guid, TaxRate>();
        foreach (var item in payload.TaxRates)
        {
            var match = await dbContext.TaxRates.FirstOrDefaultAsync(x => x.RecordId == item.RecordId || x.Code == item.Code, cancellationToken);
            if (match is not null) { taxMap[item.RecordId] = match; }
        }

        var skippedProducts = 0;
        foreach (var item in payload.Products)
        {
            if (!categoryMap.TryGetValue(item.CategoryRecordId, out var category))
            {
                category = await dbContext.ProductCategories.FirstOrDefaultAsync(x => x.RecordId == item.CategoryRecordId || (item.CategoryCode != null && x.Code == item.CategoryCode), cancellationToken);
            }
            if (!taxMap.TryGetValue(item.TaxRateRecordId, out var taxRate))
            {
                taxRate = await dbContext.TaxRates.FirstOrDefaultAsync(x => x.RecordId == item.TaxRateRecordId || (item.TaxRateCode != null && x.Code == item.TaxRateCode), cancellationToken);
            }
            if (category is null || taxRate is null)
            {
                logger.LogWarning("Ürün {StockCode} için kategori/KDV yerelde bulunamadı, bu turda atlandı.", item.StockCode);
                skippedProducts++;
                continue;
            }

            // Eşleşme RecordId veya stok kodu ile (bkz. kategori notu).
            var local = await dbContext.Products.FirstOrDefaultAsync(x => x.RecordId == item.RecordId || x.StockCode == item.StockCode, cancellationToken);
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
                local.RecordId = item.RecordId;
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

        // Atlanan ürün varsa senkron zamanı ilerletilmez: bir sonraki turda aynı değişiklikler yeniden denenir.
        if (skippedProducts == 0)
        {
            state.LastCatalogSyncUtc = payload.ServerTimeUtc;
            state.Save();
        }

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
            .Where(x => (x.EventType == "RestaurantCheckClosed" || x.EventType == "CollectionReceiptApproved" || x.EventType == "PaymentReceiptApproved" || x.EventType == "CollectionReceiptCancelled" || x.EventType == "PaymentReceiptCancelled") && x.ProcessedAtUtc == null)
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
        // Bağlantı kesilmesi (merkez kapalı, ağ hatası, zaman aşımı) da bir DENEME sayılır: sayaç ve son
        // hata kaydedilir ki bekleyen/hatalı durum doğru görünsün (aksi halde istisna sessizce yutulurdu).
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync(requestUri, request, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Merkeze satış push isteği ulaşamadı: {Message}", ex.Message);
            foreach (var message in pending)
            {
                message.RetryCount++;
                message.LastError = "Bağlantı hatası: " + ex.Message;
            }
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

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
            message.LastError = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Merkeze {Count} satış kaydı gönderildi.", pending.Count);
    }
}
