using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

// Vardiya, kullanıcı değil KASA (FinancialAccount) bazlıdır - aynı kasadaki 5 kasiyerden biri
// vardiyayı açar, hepsi aynı açık vardiyayı görür/kullanır, hangisi isterse kapatabilir. Bkz.
// RestaurantPostingService.OpenShiftAsync yorumu.
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager},{AppRoles.Cashier}")]
public sealed class RestaurantShiftController(ApplicationDbContext dbContext, RestaurantPostingService postingService) : RestaurantControllerBase(dbContext)
{
    public async Task<IActionResult> Index()
    {
        ActivePage = "shift";

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == CurrentUserId)
            .Select(x => new { x.BranchId, x.DefaultFinancialAccountId, AccountName = x.DefaultFinancialAccount!.Name })
            .SingleOrDefaultAsync();

        var vm = new RestaurantShiftViewModel
        {
            HasFinancialAccount = user?.DefaultFinancialAccountId is not null,
            FinancialAccountName = user?.AccountName
        };

        if (user?.DefaultFinancialAccountId is null)
        {
            return View(vm);
        }

        var financialAccountId = user.DefaultFinancialAccountId.Value;

        var openShift = await dbContext.RestaurantCashShifts
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.FinancialAccountId == financialAccountId && x.Status == RestaurantCashShiftStatus.Open);

        if (openShift is not null)
        {
            var now = DateTime.UtcNow;
            var cashIn = await dbContext.RestaurantPayments
                .Where(x => x.FinancialAccountId == financialAccountId && !x.IsReversal && x.PaidAtUtc >= openShift.OpenedAtUtc && x.PaidAtUtc <= now)
                .SumAsync(x => (decimal?)x.Amount) ?? 0m;

            var closedChecksQuery = dbContext.RestaurantChecks
                .Where(x => x.Status == RestaurantCheckStatus.Closed && x.ClosedAtUtc >= openShift.OpenedAtUtc && x.ClosedAtUtc <= now);

            vm.HasOpenShift = true;
            vm.ShiftId = openShift.Id;
            vm.OpenedAtUtc = openShift.OpenedAtUtc;
            vm.OpeningBalance = openShift.OpeningBalance;
            vm.CashInDuringShift = cashIn;
            vm.ExpectedBalance = openShift.OpeningBalance + cashIn;
            vm.ClosedCheckCount = await closedChecksQuery.CountAsync();
            vm.GrandTotalDuringShift = await closedChecksQuery.SumAsync(x => (decimal?)x.GrandTotal) ?? 0m;
        }

        vm.RecentClosedShifts = await dbContext.RestaurantCashShifts
            .AsNoTracking()
            .Where(x => x.FinancialAccountId == financialAccountId && x.Status == RestaurantCashShiftStatus.Closed)
            .OrderByDescending(x => x.ClosedAtUtc)
            .Take(10)
            .Select(x => new RestaurantShiftHistoryRow(x.Id, x.OpenedAtUtc, x.ClosedAtUtc, x.OpeningBalance, x.ClosingBalanceExpected, x.ClosingBalanceCounted))
            .ToListAsync();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(decimal openingBalance, Guid submissionKey)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == CurrentUserId)
            .Select(x => new { x.BranchId, x.DefaultFinancialAccountId })
            .SingleOrDefaultAsync();

        if (user?.DefaultFinancialAccountId is null || user.BranchId is null)
        {
            TempData["Error"] = "Personel kartınızda kasa/şube tanımlı değil. Yöneticinize başvurun.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await postingService.OpenShiftAsync(user.DefaultFinancialAccountId.Value, user.BranchId.Value, CurrentUserId, openingBalance, submissionKey);
            TempData["Success"] = "Vardiya açıldı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int shiftId, decimal closingBalanceCounted)
    {
        try
        {
            var shift = await postingService.CloseShiftAsync(shiftId, closingBalanceCounted);
            var diff = shift.ClosingBalanceCounted!.Value - shift.ClosingBalanceExpected!.Value;
            TempData["Success"] = diff == 0
                ? "Vardiya kapatıldı. Kasa tam uyuştu."
                : $"Vardiya kapatıldı. Fark: {diff:N2} ₺ ({(diff > 0 ? "fazla" : "eksik")}).";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
