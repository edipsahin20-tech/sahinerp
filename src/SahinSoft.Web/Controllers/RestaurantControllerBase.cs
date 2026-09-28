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

        // Masa doluluk/aktif masa istatistiklerine Self Satış/Paket dahil edilmez (Edip,
        // 2026-09-29: "masa doluluk, açık masa gibi masa istatistiklerine Self/Paket/Gel-Al
        // dahil edilmemeli") - RestaurantSection'ı Self Satış/Paket olan tablolar zaten gizli
        // (IsActive=false) ama eski (2026-09-29 öncesi) kayıtlarda IsActive=true olarak
        // üretilmişti, bu yüzden bölüm adına göre de AYRICA filtreleniyor.
        var totalTableCount = await dbContext.RestaurantTables
            .CountAsync(x => x.IsActive && x.RestaurantSection.Name != "Self Satış" && x.RestaurantSection.Name != "Paket");
        var activeTableCount = await dbContext.RestaurantTableSessions
            .CountAsync(x => x.Status == RestaurantTableSessionStatus.Open && x.Channel == RestaurantSaleChannel.Masa);

        var openCheckTotal = await dbContext.RestaurantOrderLines
            .Where(x => x.RestaurantOrder.RestaurantCheck.Status == RestaurantCheckStatus.Open && x.Status != RestaurantOrderLineStatus.Cancelled)
            .Select(x => x.Quantity * x.UnitPriceSnapshot - x.DiscountAmountSnapshot)
            .SumAsync();

        // Madde 26 (Ayarlar) - sol ana menüde Masa Satış/Self Satış/Paket'in kendisi görünsün mü.
        var navVisibility = await dbContext.InventorySettings
            .AsNoTracking()
            .Where(x => x.Id == 1)
            .Select(x => new { x.ShowTableSaleNav, x.ShowSelfSaleNav, x.ShowPackageNav })
            .SingleOrDefaultAsync();

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
            OpenCheckTotal = openCheckTotal,
            ShowTableSaleNav = navVisibility?.ShowTableSaleNav ?? true,
            ShowSelfSaleNav = navVisibility?.ShowSelfSaleNav ?? true,
            ShowPackageNav = navVisibility?.ShowPackageNav ?? true
        };
    }
}
