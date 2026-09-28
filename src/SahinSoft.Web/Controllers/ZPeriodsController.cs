using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Z Listesi (Edip, 2026-09-28: "raporların altında z listesi aynı mantıkta ekle ordada z
// raprounu tukladıgında z yi gorsun cıktı alabilsin") - ANA ERP'de yaşar (RetailSalesController
// ile AYNI genel erişim deseni), RestaurantZPeriod'un kapanışta donan özet alanlarını okur.
[Authorize]
public sealed class ZPeriodsController(ApplicationDbContext dbContext, SahinSoft.Web.Services.Printing.PrintDispatchService printDispatchService) : Controller
{
    private IQueryable<RestaurantZPeriod> BuildFilteredQuery(int? branchId, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = dbContext.RestaurantZPeriods
            .AsNoTracking()
            .Where(x => x.Status == RestaurantZPeriodStatus.Closed);

        if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }
        if (dateFrom.HasValue)
        {
            query = query.Where(x => x.ClosedAtUtc >= dateFrom.Value.Date.ToUniversalTime());
        }
        if (dateTo.HasValue)
        {
            var toExclusive = dateTo.Value.Date.AddDays(1).ToUniversalTime();
            query = query.Where(x => x.ClosedAtUtc < toExclusive);
        }

        return query.OrderByDescending(x => x.ClosedAtUtc);
    }

    public async Task<IActionResult> Index(int? branchId, DateTime? dateFrom, DateTime? dateTo, int page = 1)
    {
        const int pageSize = 50;
        var query = BuildFilteredQuery(branchId, dateFrom, dateTo);
        var totalCount = await query.CountAsync();

        var periods = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var branches = await dbContext.Branches.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        var branchNamesById = branches.ToDictionary(x => x.Id, x => x.Name);

        var model = new ZPeriodListViewModel
        {
            Branches = branches,
            BranchId = branchId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Page = page,
            TotalCount = totalCount,
            PageSize = pageSize,
            Items = periods.Select(x => new ZPeriodListItemViewModel
            {
                ZPeriodId = x.Id,
                BranchName = branchNamesById.GetValueOrDefault(x.BranchId, ""),
                OpenedAtUtc = x.OpenedAtUtc,
                ClosedAtUtc = x.ClosedAtUtc,
                ClosedAutomatically = x.ClosedAutomatically,
                ReceiptCount = x.ReceiptCount,
                NetTotal = x.NetTotal
            }).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var period = await dbContext.RestaurantZPeriods.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (period is null)
        {
            return NotFound();
        }

        var branch = await dbContext.Branches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == period.BranchId);
        var branchName = branch?.Name ?? "";
        var closedByName = period.ClosedAutomatically || period.ClosedByUserId is null
            ? "Otomatik"
            : (await dbContext.Users.AsNoTracking().Where(x => x.Id == period.ClosedByUserId).Select(x => x.FullName).SingleOrDefaultAsync()) ?? period.ClosedByUserId;

        // Bu Z dönemine ait fişlerin ödeme türü dökümü - RetailSale.RestaurantZPeriodId
        // üzerinden GERÇEK bağlantıdan (bkz. RestaurantZPeriod.cs yorum satırı).
        var checkIds = await dbContext.RetailSales
            .AsNoTracking()
            .Where(x => x.RestaurantZPeriodId == id)
            .Select(x => x.RestaurantCheckId)
            .ToListAsync();
        var totalsByMethod = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(p => !p.IsReversal && checkIds.Contains(p.RestaurantCheckId))
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.Method, x => x.Total);

        // İptaller (bilgi amaçlı - Edip, 2026-09-28: "sadece iptalleri ciro hesabına dahil
        // etme") - PrintDataProvider.FillBreakdownsAsync ile AYNI mantık: satır iptalleri
        // (fiş içinde tek ürün iptali) + fiş iptalleri (RetailSaleStatus.Cancelled) ayrı ayrı.
        var cancelledSaleIds = await dbContext.RetailSales
            .AsNoTracking()
            .Where(x => x.RestaurantZPeriodId == id && x.Status == RetailSaleStatus.Cancelled)
            .Select(x => new { x.RestaurantCheckId, x.GrandTotal })
            .ToListAsync();
        var allCheckIdsForCancel = checkIds.Concat(cancelledSaleIds.Select(x => x.RestaurantCheckId)).Distinct().ToList();
        var cancelledLines = await dbContext.RestaurantOrderLines
            .AsNoTracking()
            .Where(x => x.CancelledAtUtc != null && allCheckIdsForCancel.Contains(x.RestaurantOrder.RestaurantCheckId))
            .Select(x => new { x.Quantity, x.UnitPriceSnapshot })
            .ToListAsync();

        var model = new ZPeriodDetailViewModel
        {
            ZPeriodId = period.Id,
            BranchName = branchName,
            OpenedAtUtc = period.OpenedAtUtc,
            ClosedAtUtc = period.ClosedAtUtc,
            ClosedAutomatically = period.ClosedAutomatically,
            ClosedByUserName = closedByName,
            ReceiptCount = period.ReceiptCount,
            GrossTotal = period.GrossTotal,
            DiscountTotal = period.DiscountTotal,
            NetTotal = period.NetTotal,
            TaxTotal = period.TaxTotal,
            ComplimentaryTotal = period.ComplimentaryTotal,
            CashTotal = totalsByMethod.GetValueOrDefault(RestaurantPaymentMethod.Cash),
            CreditCardTotal = totalsByMethod.GetValueOrDefault(RestaurantPaymentMethod.CreditCard),
            MealCardTotal = totalsByMethod.GetValueOrDefault(RestaurantPaymentMethod.MealCard),
            UnpaidTotal = totalsByMethod.GetValueOrDefault(RestaurantPaymentMethod.Unpaid),
            OpenAccountTotal = totalsByMethod.GetValueOrDefault(RestaurantPaymentMethod.OpenAccount),
            LineCancellationCount = cancelledLines.Count,
            LineCancellationTotal = cancelledLines.Sum(x => x.Quantity * x.UnitPriceSnapshot),
            ReceiptCancellationCount = cancelledSaleIds.Count,
            ReceiptCancellationTotal = cancelledSaleIds.Sum(x => x.GrandTotal),
            BranchAddress = branch?.Address,
            BranchPhone = branch?.Phone
        };

        return View(model);
    }

    // Çıktı Tasarımcısı entegrasyonu (kademeli geçiş, adım 4d: Z Raporu) - şubeye tanımlı bir
    // Z Raporu yazıcısı yoksa kullanıcıya açıkça söyler (sessizce atlamaz, çünkü burada
    // kullanıcı EL İLE tetikliyor, otomatik değil).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendToPrinter(int id)
    {
        var period = await dbContext.RestaurantZPeriods.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (period is null)
        {
            return NotFound();
        }

        var (jobId, result) = await printDispatchService.EnqueueAndSendForRoleAsync(PrinterRole.ZRaporu, period.BranchId, PrintTemplateType.ZRaporu, id, $"Z Raporu - #{id}");
        TempData["Success"] = jobId switch
        {
            null => "Bu şube için tanımlı bir Z Raporu yazıcısı yok (Ayarlar → Yazıcılar).",
            _ when result?.Success == true => "Z Raporu termal yazıcıya gönderildi.",
            _ => $"Z Raporu yazıcıya gönderilemedi: {result?.Error}"
        };
        return RedirectToAction(nameof(Detail), new { id });
    }
}
