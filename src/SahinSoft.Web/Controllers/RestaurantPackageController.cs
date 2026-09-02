using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager},{AppRoles.Waiter},{AppRoles.Cashier}")]
public sealed class RestaurantPackageController(ApplicationDbContext dbContext, RestaurantPostingService postingService) : RestaurantControllerBase(dbContext)
{
    private static readonly PackageOrderStatus[] ActiveStatuses =
    [
        PackageOrderStatus.Preparing, PackageOrderStatus.Ready, PackageOrderStatus.CourierWaiting, PackageOrderStatus.OnTheWay
    ];

    public async Task<IActionResult> Index(string tab = "active", int? selected = null)
    {
        ActivePage = "package";

        var baseQuery = dbContext.PackageOrders.AsNoTracking().Where(x => x.Status != PackageOrderStatus.Cancelled);

        var activeCount = await baseQuery.CountAsync(x => ActiveStatuses.Contains(x.Status));
        var readyCount = await baseQuery.CountAsync(x => x.Status == PackageOrderStatus.Ready);
        var onTheWayCount = await baseQuery.CountAsync(x => x.Status == PackageOrderStatus.OnTheWay);

        var filtered = tab switch
        {
            "ready" => baseQuery.Where(x => x.Status == PackageOrderStatus.Ready),
            "ontheway" => baseQuery.Where(x => x.Status == PackageOrderStatus.OnTheWay),
            _ => baseQuery.Where(x => ActiveStatuses.Contains(x.Status))
        };

        var orders = await filtered
            .Include(x => x.RestaurantCheck)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new RestaurantPackageListItemViewModel(
                x.Id,
                x.RestaurantCheckId,
                x.PackageNumber,
                x.CustomerName,
                x.Channel,
                x.Status,
                x.RestaurantCheck.Orders.SelectMany(o => o.Lines).Where(l => l.Status != RestaurantOrderLineStatus.Cancelled)
                    .Sum(l => (decimal?)(l.Quantity * l.UnitPriceSnapshot - l.DiscountAmountSnapshot)) ?? 0,
                x.CreatedAtUtc))
            .ToListAsync();

        var vm = new RestaurantPackageViewModel
        {
            ActiveTab = tab,
            ActiveCount = activeCount,
            ReadyCount = readyCount,
            OnTheWayCount = onTheWayCount,
            Orders = orders
        };

        var selectedId = selected ?? orders.FirstOrDefault()?.PackageOrderId;
        if (selectedId is not null)
        {
            var packageOrder = await dbContext.PackageOrders
                .AsNoTracking()
                .Include(x => x.RestaurantCheck).ThenInclude(x => x.Orders).ThenInclude(x => x.Lines)
                .SingleOrDefaultAsync(x => x.Id == selectedId.Value);

            if (packageOrder is not null)
            {
                var lines = packageOrder.RestaurantCheck.Orders
                    .SelectMany(o => o.Lines)
                    .Where(l => l.Status != RestaurantOrderLineStatus.Cancelled)
                    .Select(l => new RestaurantPackageDetailLineViewModel(
                        l.ProductNameSnapshot, l.Quantity, l.Quantity * l.UnitPriceSnapshot - l.DiscountAmountSnapshot))
                    .ToList();

                vm.Selected = new RestaurantPackageDetailViewModel
                {
                    PackageOrderId = packageOrder.Id,
                    CheckId = packageOrder.RestaurantCheckId,
                    PackageNumber = packageOrder.PackageNumber,
                    Channel = packageOrder.Channel,
                    Status = packageOrder.Status,
                    CustomerName = packageOrder.CustomerName,
                    CustomerPhone = packageOrder.CustomerPhone,
                    DeliveryAddress = packageOrder.DeliveryAddress,
                    Lines = lines,
                    Total = lines.Sum(x => x.LineTotal)
                };
            }
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PackageOrderChannel channel, string customerName, string? customerPhone, string? deliveryAddress, Guid submissionKey)
    {
        var branchId = await dbContext.Branches.Where(x => x.IsHeadOffice).Select(x => x.Id).FirstAsync();

        try
        {
            var packageOrder = await postingService.CreatePackageOrderAsync(
                channel, customerName, customerPhone, deliveryAddress, branchId, CurrentUserId, submissionKey);
            return RedirectToAction("Check", "Restaurant", new { id = packageOrder.RestaurantCheckId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Advance(int packageOrderId, Guid submissionKey, string tab = "active")
    {
        try
        {
            await postingService.AdvancePackageOrderAsync(packageOrderId, submissionKey);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { tab, selected = packageOrderId });
    }
}
