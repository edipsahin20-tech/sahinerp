using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models.Api;

namespace SahinSoft.Web.Controllers.Api;

/// <summary>
/// Hibrit yerel/bulut mimarinin merkez tarafı: şubeler buradan tanım verisini
/// (stok/kategori/KDV oranı) çeker. Kimlik doğrulama kullanıcı girişi değil,
/// X-Branch-ApiKey header'ı ile Branch.ApiKey eşleşmesi (makine-makine bağlantısı).
/// Faz B ilk kapsamı: katalog (Product+Category+TaxRate). Cari/yetkiler aynı
/// mekanizmayla ayrı bir endpoint olarak eklenecek (fast-follow).
/// </summary>
[ApiController]
[Route("api/sync")]
public sealed class SyncController(ApplicationDbContext dbContext) : ControllerBase
{
    private const string ApiKeyHeaderName = "X-Branch-ApiKey";

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog([FromQuery] string branchCode, [FromQuery] DateTime? since, CancellationToken cancellationToken)
    {
        var authResult = await TryAuthenticateBranchAsync(branchCode, cancellationToken);
        if (authResult.Error is not null)
        {
            return authResult.Error;
        }

        var sinceUtc = since ?? DateTime.MinValue;
        var serverTimeUtc = DateTime.UtcNow;

        var categories = await dbContext.ProductCategories
            .AsNoTracking()
            .Where(x => (x.UpdatedAtUtc ?? x.CreatedAtUtc) > sinceUtc)
            .Select(x => new CategorySyncItem
            {
                RecordId = x.RecordId,
                Code = x.Code,
                Name = x.Name,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var taxRates = await dbContext.TaxRates
            .AsNoTracking()
            .Where(x => (x.UpdatedAtUtc ?? x.CreatedAtUtc) > sinceUtc)
            .Select(x => new TaxRateSyncItem
            {
                RecordId = x.RecordId,
                Code = x.Code,
                Name = x.Name,
                Rate = x.Rate,
                IsExempt = x.IsExempt,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var products = await dbContext.Products
            .AsNoTracking()
            .Where(x => (x.UpdatedAtUtc ?? x.CreatedAtUtc) > sinceUtc)
            .Select(x => new ProductSyncItem
            {
                RecordId = x.RecordId,
                StockCode = x.StockCode,
                Name = x.Name,
                Barcode = x.Barcode,
                Unit = x.Unit,
                PurchasePrice = x.PurchasePrice,
                SalePrice = x.SalePrice,
                TrackStock = x.TrackStock,
                IsActive = x.IsActive,
                CategoryRecordId = x.Category.RecordId,
                TaxRateRecordId = x.TaxRate.RecordId
            })
            .ToListAsync(cancellationToken);

        return Ok(new CatalogSyncResponse
        {
            ServerTimeUtc = serverTimeUtc,
            Categories = categories,
            TaxRates = taxRates,
            Products = products
        });
    }

    // Faz C: şubede kapanan adisyonların merkeze konsolide edilmesi. Kasa/banka hareketi burada
    // YENİDEN oluşturulmaz (bkz. RestaurantCheckClosedPayload yorumu) - yalnızca cari (Perakende
    // Satışlar Carisi) tarafı postalanır. Idempotency ExternalRecordMapping ile: aynı olay iki kez
    // gelirse (ağ kesintisi sonrası tekrar deneme) ikinci kez postalanmaz.
    [HttpPost("transactions")]
    public async Task<IActionResult> PostTransactions([FromQuery] string branchCode, [FromBody] TransactionSyncRequest request, CancellationToken cancellationToken)
    {
        var authResult = await TryAuthenticateBranchAsync(branchCode, cancellationToken);
        if (authResult.Error is not null)
        {
            return authResult.Error;
        }

        var result = new TransactionSyncResult();

        foreach (var evt in request.Events)
        {
            if (evt.EventType != "RestaurantCheckClosed")
            {
                continue;
            }

            var externalId = evt.RecordId.ToString();
            var alreadyProcessed = await dbContext.ExternalRecordMappings.AnyAsync(
                x => x.SourceSystem == branchCode && x.EntityType == "RestaurantCheckClosed" && x.ExternalId == externalId,
                cancellationToken);
            if (alreadyProcessed)
            {
                result.SkippedCount++;
                continue;
            }

            var payload = JsonSerializer.Deserialize<RestaurantCheckClosedPayload>(evt.PayloadJson);
            if (payload is null)
            {
                result.SkippedCount++;
                continue;
            }

            var retailCustomer = await dbContext.Customers.FirstOrDefaultAsync(x => x.Code == "PERAKENDE-SATIS", cancellationToken);
            if (retailCustomer is null)
            {
                return StatusCode(500, "\"Perakende Satışlar Carisi\" merkezde tanımlı değil - migration uygulanmamış olabilir.");
            }

            var documentNumber = $"{branchCode}-{payload.DocumentNumber}";

            // Talimat 1 (2026-09-06) - "muhasebe satış kaydında en az Kaynak Satış ID, Adisyon No,
            // Z Period/Report ID, Z No, şube, tarih-saat izlenebilir olmalı" - hepsi tek bir
            // Description satırında, merkez tarafında ayrıca bir sütun İCAT EDİLMEDİ (bu tablo
            // genel amaçlı CurrentAccountTransaction, restorana özel kolon eklemek YANLIŞ katman
            // olurdu) - kaynak adisyon (CheckNumber), Z No ve şube kodu izlenebilir şekilde metne
            // gömülür; RecordId zaten ExternalRecordMapping.ExternalId'de kalıcı olarak saklanır.
            var zNoSuffix = payload.RestaurantZNo is not null ? $" - {payload.RestaurantZNo}" : string.Empty;

            // GERÇEK HATA (2026-09-06, senkron kabul testinde bulundu) - tam İkram edilmiş bir
            // fişin GrandTotal'ı 0,00'dır; Debit=0/Credit=0 olan bir Sale+Collection çifti
            // CK_CurrentAccountTransactions_DebitCredit (Debit>0 XOR Credit>0) kısıtına çarpıp
            // TÜM batch'i (100 olay) başarısız ediyordu - restoran tarafında zaten İkram hiçbir
            // cari hareketi OLUŞTURMUYOR (bkz. RestaurantPostingService/Talimat 1 madde 2.1), aynı
            // kural burada da uygulanır: finansal karşılığı olmayan (0,00) bir olay merkez carisine
            // hiç yansıtılmaz, ama olay yine de İŞLENMİŞ sayılır (ExternalRecordMapping + Accepted)
            // - aksi halde her poll turunda aynı olay sonsuza dek tekrar denenip aynı hatayla
            // başarısız olurdu.
            if (payload.GrandTotal > 0)
            {
                dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
                {
                    TransactionDateUtc = payload.IssuedAtUtc,
                    TransactionType = CurrentAccountTransactionType.Sale,
                    DocumentNumber = documentNumber,
                    CurrencyCode = "TRY",
                    ExchangeRate = 1,
                    Debit = payload.GrandTotal,
                    Credit = 0,
                    CustomerId = retailCustomer.Id,
                    Description = $"[{branchCode}] Restoran satışı - {payload.CheckNumber}{zNoSuffix}"
                });

                dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
                {
                    TransactionDateUtc = payload.IssuedAtUtc,
                    TransactionType = CurrentAccountTransactionType.Collection,
                    DocumentNumber = documentNumber,
                    CurrencyCode = "TRY",
                    ExchangeRate = 1,
                    Debit = 0,
                    Credit = payload.GrandTotal,
                    CustomerId = retailCustomer.Id,
                    Description = $"[{branchCode}] Restoran tahsilatı - {payload.CheckNumber}{zNoSuffix}"
                });
            }

            dbContext.ExternalRecordMappings.Add(new ExternalRecordMapping
            {
                SourceSystem = branchCode,
                EntityType = "RestaurantCheckClosed",
                ExternalId = externalId,
                InternalId = documentNumber,
                ExternalCode = payload.DocumentNumber,
                LastSynchronizedAtUtc = DateTime.UtcNow
            });

            result.AcceptedCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(result);
    }

    private async Task<(IActionResult? Error, SahinSoft.Domain.Entities.Branch? Branch)> TryAuthenticateBranchAsync(string branchCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(branchCode))
        {
            return (BadRequest("branchCode zorunludur."), null);
        }

        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeader) || string.IsNullOrWhiteSpace(apiKeyHeader))
        {
            return (Unauthorized($"{ApiKeyHeaderName} header'ı zorunludur."), null);
        }

        var branch = await dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == branchCode, cancellationToken);

        if (branch is null || !branch.IsActive)
        {
            return (Unauthorized("Şube bulunamadı veya pasif."), null);
        }

        if (string.IsNullOrEmpty(branch.ApiKey) || branch.ApiKey != apiKeyHeader.ToString())
        {
            return (Unauthorized("Geçersiz API anahtarı."), null);
        }

        return (null, branch);
    }
}
