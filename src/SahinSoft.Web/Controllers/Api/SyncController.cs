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
                TaxRateRecordId = x.TaxRate.RecordId,
                CategoryCode = x.Category.Code,
                TaxRateCode = x.TaxRate.Code
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
            if (evt.EventType is "CollectionReceiptApproved" or "PaymentReceiptApproved" or "CollectionReceiptCancelled" or "PaymentReceiptCancelled")
            {
                await HandleReceiptEventAsync(evt, branchCode, result, cancellationToken);
                continue;
            }

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

            // Belge numarası SATIŞIN şubesiyle kurulur (yük kodu yoksa bağlantı kodu) - böylece iki
            // şubenin aynı numaralı fişi merkezde ayrı kalır.
            // Şube kodu olmayan (eski) olay tahmin edilerek bir şubeye atanmaz; merkezde kayıt oluşturulmaz.
            if (string.IsNullOrWhiteSpace(payload.BranchCode))
            {
                result.SkippedCount++;
                continue;
            }

            var saleBranchCode = payload.BranchCode;
            var documentNumber = $"{saleBranchCode}-{payload.DocumentNumber}";
            var centralBranchId = await dbContext.Branches.AsNoTracking()
                .Where(x => x.Code == saleBranchCode).Select(x => (int?)x.Id).FirstOrDefaultAsync(cancellationToken);

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
                    OriginBranchId = centralBranchId,
                    Description = $"[{saleBranchCode}] Restoran satışı - {payload.CheckNumber}{zNoSuffix}"
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
                    OriginBranchId = centralBranchId,
                    Description = $"[{saleBranchCode}] Restoran tahsilatı - {payload.CheckNumber}{zNoSuffix}"
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


    // Tahsilat/tediye onayı: müşteri ve hesaplar KODLA eşlenir; biri merkezde yoksa olay atlanır (tahmin yok).
    // Onay cari (alacak/borç) ve kasa/banka hareketi yazar; iptal, orijinal kaydın ters hareketini üretir.
    private async Task HandleReceiptEventAsync(TransactionSyncEvent evt, string branchCode, TransactionSyncResult result, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<PaymentReceiptSyncPayload>(evt.PayloadJson);
        if (payload is null)
        {
            result.SkippedCount++;
            return;
        }

        var isCancel = evt.EventType.EndsWith("Cancelled", StringComparison.Ordinal);
        var entityType = isCancel ? "PaymentReceiptCancelled" : "PaymentReceiptApproved";
        var externalId = payload.ReceiptRecordId.ToString();
        var alreadyProcessed = await dbContext.ExternalRecordMappings.AnyAsync(
            x => x.SourceSystem == branchCode && x.EntityType == entityType && x.ExternalId == externalId, cancellationToken);
        if (alreadyProcessed)
        {
            result.SkippedCount++;
            return;
        }

        if (isCancel)
        {
            var original = await dbContext.ExternalRecordMappings.AsNoTracking().SingleOrDefaultAsync(
                x => x.SourceSystem == branchCode && x.EntityType == "PaymentReceiptApproved" && x.ExternalId == externalId, cancellationToken);
            if (original is null)
            {
                result.SkippedCount++;
                return;
            }

            var docNumber = original.InternalId;
            var cariOriginals = await dbContext.CurrentAccountTransactions
                .Where(x => x.DocumentNumber == docNumber && x.ReversalOfId == null).ToListAsync(cancellationToken);
            foreach (var orig in cariOriginals)
            {
                dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
                {
                    TransactionDateUtc = DateTime.UtcNow,
                    TransactionType = orig.TransactionType == CurrentAccountTransactionType.Collection
                        ? CurrentAccountTransactionType.DebitNote
                        : CurrentAccountTransactionType.CreditNote,
                    DocumentNumber = $"IPTAL-{docNumber}",
                    CurrencyCode = orig.CurrencyCode,
                    ExchangeRate = orig.ExchangeRate,
                    Debit = orig.Credit,
                    Credit = orig.Debit,
                    CustomerId = orig.CustomerId,
                    OriginBranchId = orig.OriginBranchId,
                    Description = $"[{branchCode}] Tahsilat/tediye iptali - {payload.ReceiptNumber} - {payload.Reason}",
                    ReversalOfId = orig.Id
                });
            }

            var finOriginals = await dbContext.FinancialTransactions
                .Where(x => x.DocumentNumber == docNumber && x.ReversalOfId == null).ToListAsync(cancellationToken);
            foreach (var orig in finOriginals)
            {
                dbContext.FinancialTransactions.Add(new FinancialTransaction
                {
                    TransactionDateUtc = DateTime.UtcNow,
                    TransactionType = orig.TransactionType,
                    DocumentNumber = $"IPTAL-{docNumber}",
                    Amount = orig.Amount,
                    ExchangeRate = orig.ExchangeRate,
                    Description = $"[{branchCode}] Tahsilat/tediye iptali - {payload.ReceiptNumber} - {payload.Reason}",
                    FinancialAccountId = orig.FinancialAccountId,
                    CustomerId = orig.CustomerId,
                    OriginBranchId = orig.OriginBranchId,
                    ReversalOfId = orig.Id
                });
            }

            dbContext.ExternalRecordMappings.Add(new ExternalRecordMapping
            {
                SourceSystem = branchCode,
                EntityType = entityType,
                ExternalId = externalId,
                InternalId = $"IPTAL-{docNumber}",
                ExternalCode = payload.ReceiptNumber,
                LastSynchronizedAtUtc = DateTime.UtcNow
            });
            result.AcceptedCount++;
            return;
        }

        if (string.IsNullOrWhiteSpace(payload.BranchCode) || string.IsNullOrWhiteSpace(payload.CustomerCode) || payload.Lines.Count == 0)
        {
            result.SkippedCount++;
            return;
        }

        var customer = await dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Code == payload.CustomerCode, cancellationToken);
        var originBranch = await dbContext.Branches.AsNoTracking().FirstOrDefaultAsync(x => x.Code == payload.BranchCode, cancellationToken);
        var accountCodes = payload.Lines.Select(x => x.AccountCode).Distinct().ToList();
        var accounts = await dbContext.FinancialAccounts.AsNoTracking()
            .Where(x => accountCodes.Contains(x.Code)).ToDictionaryAsync(x => x.Code, x => x.Id, cancellationToken);
        if (customer is null || originBranch is null || accounts.Count != accountCodes.Count)
        {
            result.SkippedCount++;
            return;
        }

        var isCollection = payload.ReceiptType == (int)ReceiptType.Collection;
        var docNo = $"{payload.BranchCode}-{payload.ReceiptNumber}";
        var cari = new CurrentAccountTransaction
        {
            TransactionDateUtc = payload.ReceiptDateUtc,
            TransactionType = isCollection ? CurrentAccountTransactionType.Collection : CurrentAccountTransactionType.Payment,
            DocumentNumber = docNo,
            CurrencyCode = "TRY",
            ExchangeRate = 1,
            Debit = isCollection ? 0 : payload.TotalAmount,
            Credit = isCollection ? payload.TotalAmount : 0,
            CustomerId = customer.Id,
            OriginBranchId = originBranch.Id,
            Description = $"[{payload.BranchCode}] Tahsilat/tediye - {payload.ReceiptNumber}"
        };
        dbContext.CurrentAccountTransactions.Add(cari);

        foreach (var line in payload.Lines)
        {
            dbContext.FinancialTransactions.Add(new FinancialTransaction
            {
                TransactionDateUtc = payload.ReceiptDateUtc,
                TransactionType = isCollection ? FinancialTransactionType.Collection : FinancialTransactionType.Payment,
                DocumentNumber = docNo,
                Amount = line.Amount,
                ExchangeRate = 1,
                Description = line.Description ?? $"[{payload.BranchCode}] {payload.ReceiptNumber}",
                FinancialAccountId = accounts[line.AccountCode],
                CustomerId = customer.Id,
                OriginBranchId = originBranch.Id,
                CurrentAccountTransaction = cari
            });
        }

        dbContext.ExternalRecordMappings.Add(new ExternalRecordMapping
        {
            SourceSystem = branchCode,
            EntityType = entityType,
            ExternalId = externalId,
            InternalId = docNo,
            ExternalCode = payload.ReceiptNumber,
            LastSynchronizedAtUtc = DateTime.UtcNow
        });
        result.AcceptedCount++;
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
