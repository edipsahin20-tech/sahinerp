using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Master tasarımın ortak shell'ini (topbar/sidebar/statusbar) kullanan TÜM restoran
// controller'ları bundan türer - her action'da aynı sorguları tekrarlamak yerine
// ViewBag.Shell burada bir kere dolduruluyor. Alt sınıflar sadece kendi ActivePage
// değerini SetActivePage ile belirtir (Dashboard/Masa Satış/Paket/Self Satış/Raporlar/
// Mutfak/Vardiya).
public abstract class RestaurantControllerBase(ApplicationDbContext dbContext) : Controller
{
    protected string ActivePage { get; set; } = string.Empty;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ViewBag.Shell = await BuildShellAsync();
        await next();
    }

    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private async Task<RestaurantShellViewModel> BuildShellAsync()
    {
        var userId = CurrentUserId;

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new { x.FullName, x.BranchId, x.DefaultFinancialAccountId })
            .SingleOrDefaultAsync();

        // Kullanıcının kendi şubesi yoksa (ör. Administrator hesabı) merkez şube gösterilir -
        // tek şubeli kurulumlarda (mevcut durum) zaten tek satır var.
        var branchName = await dbContext.Branches
            .AsNoTracking()
            .Where(x => user != null && user.BranchId == x.Id || x.IsHeadOffice)
            .OrderByDescending(x => user != null && user.BranchId == x.Id)
            .Select(x => x.Name)
            .FirstOrDefaultAsync() ?? "Merkez";

        // Vardiya kasa (FinancialAccount) bazlıdır, kullanıcı bazlı değil - aynı kasadaki tüm
        // kasiyerler aynı açık vardiyayı görür (bkz. RestaurantShiftController).
        var openShift = user?.DefaultFinancialAccountId is null
            ? null
            : await dbContext.RestaurantCashShifts
                .AsNoTracking()
                .Where(x => x.FinancialAccountId == user.DefaultFinancialAccountId && x.Status == RestaurantCashShiftStatus.Open)
                .Select(x => new { x.OpenedAtUtc })
                .FirstOrDefaultAsync();

        var kitchenPendingCount = await dbContext.KitchenTicketLines
            .Where(x => x.Status == KitchenTicketLineStatus.Sent || x.Status == KitchenTicketLineStatus.InProgress)
            .CountAsync();

        var totalTableCount = await dbContext.RestaurantTables.CountAsync(x => x.IsActive);
        var activeTableCount = await dbContext.RestaurantTableSessions.CountAsync(x => x.Status == RestaurantTableSessionStatus.Open);

        var openCheckTotal = await dbContext.RestaurantOrderLines
            .Where(x => x.RestaurantOrder.RestaurantCheck.Status == RestaurantCheckStatus.Open && x.Status != RestaurantOrderLineStatus.Cancelled)
            .Select(x => x.Quantity * x.UnitPriceSnapshot - x.DiscountAmountSnapshot)
            .SumAsync();

        return new RestaurantShellViewModel
        {
            ActivePage = ActivePage,
            BranchName = branchName,
            UserFullName = user?.FullName ?? "Kullanıcı",
            IsShiftOpen = openShift is not null,
            ShiftOpenedAtUtc = openShift?.OpenedAtUtc,
            KitchenPendingCount = kitchenPendingCount,
            ActiveTableCount = activeTableCount,
            TotalTableCount = totalTableCount,
            OpenCheckTotal = openCheckTotal
        };
    }
}
