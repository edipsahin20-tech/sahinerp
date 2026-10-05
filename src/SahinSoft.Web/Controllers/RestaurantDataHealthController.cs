using SahinSoft.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Talimat 1 (2026-09-06) madde 6-7 - "Veri Sağlığı / Mutabakat Merkezi": kullanıcı teknik tablo
// gezmek zorunda kalmadan günlük satışların Yerel/Merkez/Muhasebe karşılıklarını tek ekranda
// görüp eksikleri güvenli şekilde onarabilsin. Yalnızca Administrator - repair aksiyonları
// senkron/muhasebe durumuna dokunuyor.
[Authorize(Roles = AppRoles.Administrator)]
public sealed class RestaurantDataHealthController(ApplicationDbContext dbContext, IConfiguration configuration, RestaurantShellService shellService) : RestaurantControllerBase(dbContext, shellService)
{
    public async Task<IActionResult> Index(DateOnly? date, string? tab, int? zId, string? q)
    {
        ActivePage = "datahealth";
        var vm = await BuildViewModelAsync(date, null, tab, zId, q);
        return View(vm);
    }

    // Fiş İnceleme - tek bir fişin Yerel/Merkez/Muhasebe kayıtları, gerçek fiş kaynağı ve
    // o fişe ait onarım/denetim logu.
    public async Task<IActionResult> Inspect(int id, DateOnly? date)
    {
        ActivePage = "datahealth";
        var vm = await BuildViewModelAsync(date, id, "fis", null, null);
        if (vm.SelectedRow is null)
        {
            return NotFound();
        }
        return View(vm);
    }

    // "Tekrar Kontrol Et" - hiçbir veri değiştirmez, ekran zaten HER YÜKLEMEDE canlı yeniden
    // hesaplanır (kayıtlı/eski bir "durum" alanı YOK) - bu buton sadece aynı sayfayı tazeler.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Recheck(DateOnly? date) => RedirectToAction(nameof(Index), new { date });

    // "Yeniden Aktar" - Merkez eksik/hatalı bir satış için outbox mesajını yeniden göndermeye
    // zorlar. Mesaj hiç yoksa (Faz C öncesi kalan çok eski bir satış gibi) yeni bir tane
    // oluşturur - RestaurantPostingService.CloseCheckAsync ile AYNI payload şekli.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetransmitToCentral(int retailSaleId, DateOnly? date)
    {
        var zNumberById = await dbContext.RestaurantZPeriods.AsNoTracking().Where(x => x.ZNumber != null).ToDictionaryAsync(x => x.Id, x => x.ZNumber);
        var sale = await dbContext.RetailSales
            .Include(x => x.RestaurantCheck)
            .SingleOrDefaultAsync(x => x.Id == retailSaleId);
        if (sale is null)
        {
            TempData["Error"] = "Fiş bulunamadı.";
            return RedirectToAction(nameof(Index), new { date });
        }

        // Outbox satırının KENDİ RecordId'si fişin RecordId'si DEĞİL (satır ayrı üretilir) - fişi
        // payload'daki RetailSaleRecordId ile eşleştiriyoruz; aksi halde her yeniden aktarım aynı
        // fiş için ikinci bir outbox satırı oluşturur (mükerrer).
        var saleRecordIdText = sale.RecordId.ToString();
        var existing = await dbContext.IntegrationOutboxMessages
            .Where(x => x.EventType == "RestaurantCheckClosed" && x.PayloadJson.Contains(saleRecordIdText))
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        // Yük HER durumda yeniden kurulur: eski (şubesiz) yük yeniden gönderilirse merkez onu atlar.
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(new SahinSoft.Web.Models.Api.RestaurantCheckClosedPayload
                {
                    RetailSaleRecordId = sale.RecordId,
                    BranchCode = await dbContext.Branches.Where(x => x.Id == sale.BranchId).Select(x => x.Code).SingleOrDefaultAsync(),
                    DocumentNumber = sale.DocumentNumber,
                    IssuedAtUtc = sale.IssuedAtUtc,
                    SubtotalAmount = sale.SubtotalAmount,
                    TaxAmount = sale.TaxAmount,
                    GrandTotal = sale.GrandTotal,
                    TradeType = sale.TradeType,
                    CheckNumber = sale.RestaurantCheck.CheckNumber,
                    RestaurantZPeriodId = sale.RestaurantZPeriodId,
                    RestaurantZNo = sale.RestaurantZPeriodId is { } zId ? RestaurantZPeriod.LabelFor(zId, zNumberById.GetValueOrDefault(zId)) : null
                });

        if (existing is not null)
        {
            // Deterministik onarım: aynı olayı YENİDEN yaz değil, sadece "işlenmedi" işaretle -
            // bir sonraki BranchSyncBackgroundService turunda güvenle tekrar denenir (merkez
            // tarafı ExternalRecordMapping ile idempotent, ikinci kez postalanmaz).
            existing.PayloadJson = payloadJson;
            existing.ProcessedAtUtc = null;
            existing.LastError = null;
        }
        else
        {
            dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
            {
                EventType = "RestaurantCheckClosed",
                PayloadJson = payloadJson,
                RecordId = sale.RecordId
            });
        }

        await dbContext.SaveChangesAsync();
        TempData["Success"] = $"{sale.DocumentNumber} merkeze yeniden gönderilmek üzere kuyruğa alındı.";
        return RedirectToAction(nameof(Index), new { date, selected = retailSaleId });
    }

    // "Eksikleri Onar" (madde 7 - "deterministik alanlar sistem tarafından güvenle
    // tamamlanabilir"). YALNIZCA Z referansı için: hangi Z dönemine ait olduğu, satışın
    // IssuedAtUtc'sinin hangi Z'nin [Açılış, Kapanış) aralığına düştüğünden KESİN olarak
    // türetilir - tutar/miktar/ürün gibi finansal bir alan asla TAHMİN EDİLMEZ (bkz. madde 7).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RepairZReference(int retailSaleId, DateOnly? date)
    {
        var sale = await dbContext.RetailSales.SingleOrDefaultAsync(x => x.Id == retailSaleId);
        if (sale is null)
        {
            TempData["Error"] = "Fiş bulunamadı.";
            return RedirectToAction(nameof(Index), new { date });
        }

        if (sale.RestaurantZPeriodId is not null)
        {
            TempData["Error"] = "Bu fişin Z referansı zaten dolu.";
            return RedirectToAction(nameof(Index), new { date, selected = retailSaleId });
        }

        var matchingPeriod = await dbContext.RestaurantZPeriods
            .Where(x => x.OpenedAtUtc <= sale.IssuedAtUtc && (x.ClosedAtUtc == null || sale.IssuedAtUtc < x.ClosedAtUtc))
            .OrderBy(x => x.OpenedAtUtc)
            .FirstOrDefaultAsync();

        if (matchingPeriod is null)
        {
            TempData["Error"] = "Bu satışın ait olduğu Z dönemi kesin olarak belirlenemedi - manuel inceleme gerekiyor.";
            return RedirectToAction(nameof(Index), new { date, selected = retailSaleId });
        }

        sale.RestaurantZPeriodId = matchingPeriod.Id;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = $"{sale.DocumentNumber} → {RestaurantZPeriod.LabelFor(matchingPeriod.Id, matchingPeriod.ZNumber)} olarak tamamlandı.";
        return RedirectToAction(nameof(Index), new { date, selected = retailSaleId });
    }

    private async Task<RestaurantDataHealthViewModel> BuildViewModelAsync(DateOnly? date, int? selected, string? tab, int? zPeriodId, string? q)
    {
        var zNumberById = await dbContext.RestaurantZPeriods.AsNoTracking().Where(x => x.ZNumber != null).ToDictionaryAsync(x => x.Id, x => x.ZNumber);
        var filterDate = date ?? DateOnly.FromDateTime(DateTime.Now);
        var dayStartUtc = filterDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var dayEndUtc = dayStartUtc.AddDays(1);

        var merkezEnabled = configuration.GetValue<bool>("MerkezSync:Enabled");

        var sales = await dbContext.RetailSales
            .AsNoTracking()
            .Where(x => x.IssuedAtUtc >= dayStartUtc && x.IssuedAtUtc < dayEndUtc && x.Status != RetailSaleStatus.Cancelled)
            .Select(x => new
            {
                x.Id,
                x.RecordId,
                x.DocumentNumber,
                x.RestaurantCheckId,
                CheckNumber = x.RestaurantCheck.CheckNumber,
                x.RestaurantZPeriodId,
                x.IssuedAtUtc,
                x.GrandTotal
            })
            .OrderByDescending(x => x.IssuedAtUtc)
            .ToListAsync();

        // GERÇEK HATA (2026-09-06, bu ekranın ilk sürümünde bulundu) - "muhasebe gerekli mi"
        // sorusu yalnızca GrandTotal>0'a bakıyordu; ama Ödenmez (madde 2.1) GrandTotal>0 olsa BİLE
        // kasıtlı olarak HİÇBİR kasa/banka/cari hareketi ÜRETMEZ - bu, tam Ödenmez fişlerini
        // yanlışlıkla "Muhasebe fişi yok" diye kırmızıya boyuyordu. Gerçek kural: bir satışın
        // muhasebe karşılığı ancak Nakit/Kredi Kartı/Açık Hesap gibi GERÇEK finansal karşılığı
        // olan bir ödeme yöntemi kullanılmışsa beklenir.
        var checkIds = sales.Select(x => x.RestaurantCheckId).ToList();
        var checksWithRealPayment = (await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => checkIds.Contains(x.RestaurantCheckId) && !x.IsReversal && x.PaymentMethod != RestaurantPaymentMethod.Unpaid)
            .Select(x => x.RestaurantCheckId)
            .Distinct()
            .ToListAsync())
            .ToHashSet();

        var recordIds = sales.Select(x => x.RecordId).ToList();
        var documentNumbers = sales.Select(x => x.DocumentNumber).ToList();

        var outboxLookup = await dbContext.IntegrationOutboxMessages
            .AsNoTracking()
            .Where(x => x.EventType == "RestaurantCheckClosed" && recordIds.Contains(x.RecordId))
            .Select(x => new { x.RecordId, x.ProcessedAtUtc, x.RetryCount, x.LastError })
            .ToDictionaryAsync(x => x.RecordId, x => x);

        var recordIdStrings = recordIds.Select(x => x.ToString()).ToHashSet();
        var mappedExternalIds = (await dbContext.ExternalRecordMappings
            .AsNoTracking()
            .Where(x => x.EntityType == "RestaurantCheckClosed")
            .Select(x => x.ExternalId)
            .ToListAsync())
            .Where(recordIdStrings.Contains)
            .ToHashSet();

        var financialDocs = await dbContext.FinancialTransactions
            .AsNoTracking()
            .Where(x => documentNumbers.Contains(x.DocumentNumber))
            .Select(x => x.DocumentNumber)
            .Distinct()
            .ToListAsync();
        var cariDocs = await dbContext.CurrentAccountTransactions
            .AsNoTracking()
            .Where(x => documentNumbers.Contains(x.DocumentNumber))
            .Select(x => x.DocumentNumber)
            .Distinct()
            .ToListAsync();
        var accountingDocSet = financialDocs.Concat(cariDocs).ToHashSet();

        // Z referansı eksikliği yalnızca RestaurantZPeriod sistemi VAR OLDUKTAN SONRAKİ satışlar
        // için gerçek bir hatadır - bu sistem kurulmadan önceki satışlar (eski flush-Z ile zaten
        // arşivlendi) kalıcı olarak Z'siz kalır, bu BEKLENEN bir tarihsel durumdur, "sessiz hata"
        // DEĞİLDİR.
        var firstZPeriodOpenedAtUtc = await dbContext.RestaurantZPeriods
            .AsNoTracking()
            .OrderBy(x => x.OpenedAtUtc)
            .Select(x => (DateTime?)x.OpenedAtUtc)
            .FirstOrDefaultAsync();

        var rows = new List<RestaurantDataHealthRow>();
        foreach (var s in sales)
        {
            var row = new RestaurantDataHealthRow
            {
                RetailSaleId = s.Id,
                DocumentNumber = s.DocumentNumber,
                CheckNumber = s.CheckNumber,
                ZPeriodId = s.RestaurantZPeriodId,
                ZNo = s.RestaurantZPeriodId is { } zId ? RestaurantZPeriod.LabelFor(zId, zNumberById.GetValueOrDefault(zId)) : null,
                IssuedAtUtc = s.IssuedAtUtc,
                GrandTotal = s.GrandTotal
            };

            // Yerel: bu satış zaten var olduğu için varlığı Tam - tek gerçek yerel eksiklik
            // Z referansının hiç atanmamış olmasıdır (mevcut mimaride olmaması beklenir, ama
            // "sessiz hata kabul edilmez" ilkesiyle yine de kontrol edilir). RestaurantZPeriod
            // sisteminden ÖNCEKİ satışlar hariç (yukarıdaki yorum).
            if (s.RestaurantZPeriodId is null && firstZPeriodOpenedAtUtc is not null && s.IssuedAtUtc >= firstZPeriodOpenedAtUtc)
            {
                row.LocalStatus = "Eksik";
                row.ErrorReasons.Add("Z referansı eksik");
                row.CanRepairZReference = true;
            }
            else
            {
                row.LocalStatus = "Tam";
            }

            if (!merkezEnabled)
            {
                row.CentralStatus = "Kapalı";
            }
            else if (mappedExternalIds.Contains(s.RecordId.ToString()))
            {
                row.CentralStatus = "Tam";
            }
            else if (outboxLookup.TryGetValue(s.RecordId, out var outbox))
            {
                if (outbox.RetryCount > 0)
                {
                    row.CentralStatus = "Hatalı";
                    row.ErrorReasons.Add($"Merkez kaydı yok ({outbox.RetryCount}. deneme başarısız)");
                }
                else
                {
                    row.CentralStatus = "Bekliyor";
                }
                row.CanRetransmitToCentral = true;
            }
            else
            {
                row.CentralStatus = "Hatalı";
                row.ErrorReasons.Add("Merkez kaydı yok (kuyrukta olay bulunamadı)");
                row.CanRetransmitToCentral = true;
            }

            // Muhasebe: yalnızca gerçek finansal karşılığı OLMASI GEREKEN satışlar için kontrol
            // edilir - tam İkram (GrandTotal=0) VE tam Ödenmez (GrandTotal>0 ama gerçek ödeme
            // yöntemi YOK) hiçbir kasa/banka/cari hareketi ÜRETMEZ (bkz. Talimat 1 madde 2.1), bu
            // satırlar için "Eksik" değil "Gerekmiyor" gösterilir.
            if (s.GrandTotal <= 0 || !checksWithRealPayment.Contains(s.RestaurantCheckId))
            {
                row.AccountingStatus = "Gerekmiyor";
            }
            else if (accountingDocSet.Contains(s.DocumentNumber))
            {
                row.AccountingStatus = "Tam";
            }
            else
            {
                row.AccountingStatus = "Eksik";
                row.ErrorReasons.Add("Muhasebe fişi yok");
            }

            row.OverallStatus = row.LocalStatus == "Eksik" || row.AccountingStatus == "Eksik" || row.CentralStatus == "Hatalı"
                ? "KIRMIZI"
                : row.CentralStatus == "Bekliyor" ? "SARI" : "YESIL";

            rows.Add(row);
        }

        // Z Bazında Mutabakat: Yerel toplam = tüm fişler; Muhasebe toplam = muhasebe karşılığı
        // doğrulanmış (Tam/Gerekmiyor) fişler. Ödenmez fişleri iki tarafta da yer alır (kasa
        // hareketi yok ama Z'de sayılır), bu yüzden Fark yalnızca eksik muhasebe kayıtlarını gösterir.
        var zPeriodMeta = await dbContext.RestaurantZPeriods
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => new { x.OpenedAtUtc, x.ClosedAtUtc });

        var zGroups = rows
            .Where(x => x.ZPeriodId is not null)
            .GroupBy(x => new { x.ZPeriodId, x.ZNo })
            .Select(g =>
            {
                var meta = zPeriodMeta.GetValueOrDefault(g.Key.ZPeriodId!.Value);
                return new RestaurantDataHealthZSummaryRow(
                    g.Key.ZPeriodId!.Value,
                    g.Key.ZNo!,
                    g.Count(),
                    g.Count(x => x.CentralStatus is "Tam" or "Kapalı"),
                    g.Count(x => x.AccountingStatus is "Tam" or "Gerekmiyor"),
                    g.Any(x => x.OverallStatus == "KIRMIZI"),
                    g.Sum(x => x.GrandTotal),
                    g.Where(x => x.AccountingStatus is "Tam" or "Gerekmiyor").Sum(x => x.GrandTotal),
                    meta?.OpenedAtUtc ?? g.Min(x => x.IssuedAtUtc),
                    meta?.ClosedAtUtc);
            })
            .OrderByDescending(x => x.ZPeriodId)
            .ToList();

        // Arama (Z no / fiş no) - Fiş ve Z sekmelerinde filtre olarak uygulanır.
        var search = q?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            rows = rows.Where(x => x.DocumentNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (x.ZNo?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            zGroups = zGroups.Where(x => x.ZNo.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        RestaurantDataHealthZDetail? zDetail = null;
        var activeZId = zPeriodId ?? zGroups.FirstOrDefault()?.ZPeriodId;
        var zSummary = zGroups.FirstOrDefault(x => x.ZPeriodId == activeZId);
        if (zSummary is not null)
        {
            zDetail = new RestaurantDataHealthZDetail
            {
                ZPeriodId = zSummary.ZPeriodId,
                ZNo = zSummary.ZNo,
                OpenedAtUtc = zSummary.OpenedAtUtc,
                ClosedAtUtc = zSummary.ClosedAtUtc,
                LocalCount = zSummary.BranchCount,
                LocalTotal = zSummary.LocalTotal,
                AccountingTotal = zSummary.AccountingTotal,
                Rows = rows.Where(x => x.ZPeriodId == zSummary.ZPeriodId).ToList()
            };
        }

        // Onarım Logu: fiş / merkez kuyruğu / Z değişikliklerinin denetim kaydı (kullanıcı adıyla).
        var logRows = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(x => x.EntityName == "RetailSale" || x.EntityName == "IntegrationOutboxMessage" || x.EntityName == "RestaurantZPeriod")
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(100)
            .Select(x => new { x.CreatedAtUtc, x.UserId, x.Action, x.EntityName, x.EntityId, x.OldValuesJson, x.NewValuesJson })
            .ToListAsync();
        var userIds = logRows.Select(x => x.UserId).Distinct().ToList();
        var userNames = await dbContext.Users
            .AsNoTracking()
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName);
        var recentRepairs = logRows
            .Select(x => new RestaurantDataHealthAuditRow(
                x.CreatedAtUtc,
                userNames.GetValueOrDefault(x.UserId, x.UserId),
                x.Action,
                x.EntityName,
                x.EntityId,
                x.OldValuesJson,
                x.NewValuesJson))
            .ToList();

        var selectedRow = selected is not null ? rows.FirstOrDefault(x => x.RetailSaleId == selected.Value) : null;
        var selectedAudits = selectedRow is null
            ? []
            : recentRepairs.Where(x => x.EntityName == "RetailSale" && x.EntityId == selectedRow.RetailSaleId.ToString()).ToList();

        return new RestaurantDataHealthViewModel
        {
            FilterDate = filterDate,
            MerkezSyncEnabled = merkezEnabled,
            ActiveTab = tab is "z" or "log" ? tab : "fis",
            Search = search,
            TotalCount = rows.Count,
            MatchedCount = rows.Count(x => x.OverallStatus == "YESIL"),
            PendingCount = rows.Count(x => x.OverallStatus == "SARI"),
            ErrorCount = rows.Count(x => x.OverallStatus == "KIRMIZI"),
            MissingCentralCount = rows.Count(x => x.CentralStatus == "Hatalı"),
            MissingAccountingCount = rows.Count(x => x.AccountingStatus == "Eksik"),
            ZSummary = zGroups,
            ZDetail = zDetail,
            Rows = rows,
            RecentRepairs = recentRepairs,
            SelectedRow = selectedRow,
            SelectedRowAudits = selectedAudits
        };
    }
}
