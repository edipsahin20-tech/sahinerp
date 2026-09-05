using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
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
        PackageOrderStatus.New, PackageOrderStatus.PendingApproval, PackageOrderStatus.Preparing,
        PackageOrderStatus.Ready, PackageOrderStatus.CourierWaiting, PackageOrderStatus.OnTheWay
    ];

    private static string ChannelLabel(PackageOrderChannel channel) => channel switch
    {
        PackageOrderChannel.Phone => "Telefon",
        PackageOrderChannel.Web => "Web",
        PackageOrderChannel.PickupInStore => "Gel-Al",
        PackageOrderChannel.Yemeksepeti => "Yemeksepeti",
        PackageOrderChannel.TrendyolYemek => "Trendyol Yemek",
        PackageOrderChannel.GetirYemek => "GetirYemek",
        _ => channel.ToString()
    };

    public async Task<IActionResult> Index(string tab = "preparing", string channel = "all", string? q = null, int? courierId = null, int? selected = null)
    {
        ActivePage = "package";

        var baseQuery = dbContext.PackageOrders.AsNoTracking().Where(x => x.Status != PackageOrderStatus.Cancelled);

        var vm = new RestaurantPackageViewModel
        {
            ActiveTab = tab,
            ChannelFilter = channel,
            SearchTerm = string.IsNullOrWhiteSpace(q) ? null : q.Trim(),
            CourierFilter = courierId?.ToString(),
            NewCount = await baseQuery.CountAsync(x => x.Status == PackageOrderStatus.New),
            PendingApprovalCount = await baseQuery.CountAsync(x => x.Status == PackageOrderStatus.PendingApproval),
            PreparingCount = await baseQuery.CountAsync(x => x.Status == PackageOrderStatus.Preparing),
            ReadyCount = await baseQuery.CountAsync(x => x.Status == PackageOrderStatus.Ready),
            CourierWaitingCount = await baseQuery.CountAsync(x => x.Status == PackageOrderStatus.CourierWaiting),
            OnTheWayCount = await baseQuery.CountAsync(x => x.Status == PackageOrderStatus.OnTheWay),
            DeliveredCount = await baseQuery.CountAsync(x => x.Status == PackageOrderStatus.Delivered),
            TotalCount = await baseQuery.CountAsync(x => ActiveStatuses.Contains(x.Status)),
            PhoneCount = await baseQuery.CountAsync(x => ActiveStatuses.Contains(x.Status) && x.Channel == PackageOrderChannel.Phone),
            WebCount = await baseQuery.CountAsync(x => ActiveStatuses.Contains(x.Status) && x.Channel == PackageOrderChannel.Web),
            YemeksepetiCount = await baseQuery.CountAsync(x => ActiveStatuses.Contains(x.Status) && x.Channel == PackageOrderChannel.Yemeksepeti),
            TrendyolYemekCount = await baseQuery.CountAsync(x => ActiveStatuses.Contains(x.Status) && x.Channel == PackageOrderChannel.TrendyolYemek),
            GetirYemekCount = await baseQuery.CountAsync(x => ActiveStatuses.Contains(x.Status) && x.Channel == PackageOrderChannel.GetirYemek)
        };

        var filtered = tab switch
        {
            "new" => baseQuery.Where(x => x.Status == PackageOrderStatus.New),
            "pendingapproval" => baseQuery.Where(x => x.Status == PackageOrderStatus.PendingApproval),
            "preparing" => baseQuery.Where(x => x.Status == PackageOrderStatus.Preparing),
            "ready" => baseQuery.Where(x => x.Status == PackageOrderStatus.Ready),
            "courierwaiting" => baseQuery.Where(x => x.Status == PackageOrderStatus.CourierWaiting),
            "ontheway" => baseQuery.Where(x => x.Status == PackageOrderStatus.OnTheWay),
            "delivered" => baseQuery.Where(x => x.Status == PackageOrderStatus.Delivered),
            _ => baseQuery.Where(x => ActiveStatuses.Contains(x.Status))
        };

        if (channel != "all" && Enum.TryParse<PackageOrderChannel>(channel, true, out var channelEnum))
        {
            filtered = filtered.Where(x => x.Channel == channelEnum);
        }
        if (courierId is not null)
        {
            filtered = filtered.Where(x => x.AssignedCourierId == courierId);
        }
        if (!string.IsNullOrWhiteSpace(vm.SearchTerm))
        {
            var term = vm.SearchTerm;
            filtered = filtered.Where(x => x.PackageNumber.Contains(term) || x.CustomerName.Contains(term) || (x.CustomerPhone != null && x.CustomerPhone.Contains(term)));
        }

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
        vm.Orders = orders;

        // Kurye Takibi paneli - her kuryenin o an aktif (henüz teslim edilmemiş) kaç siparişi
        // olduğu gerçek bir sayımla gösterilir, uydurma bir sayı DEĞİL.
        var couriers = await dbContext.RestaurantCouriers
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
        var activeOrderCounts = await dbContext.PackageOrders
            .AsNoTracking()
            .Where(x => x.AssignedCourierId != null && ActiveStatuses.Contains(x.Status))
            .GroupBy(x => x.AssignedCourierId!.Value)
            .Select(g => new { CourierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourierId, x => x.Count);
        vm.Couriers = couriers
            .Select(c => new RestaurantCourierViewModel(c.Id, c.Name, c.Phone, c.IsExternal, c.Status, activeOrderCounts.GetValueOrDefault(c.Id)))
            .ToList();

        var selectedId = selected ?? orders.FirstOrDefault()?.PackageOrderId;
        if (selectedId is not null)
        {
            var packageOrder = await dbContext.PackageOrders
                .AsNoTracking()
                .Include(x => x.RestaurantCheck).ThenInclude(x => x.Orders).ThenInclude(x => x.Lines)
                .Include(x => x.AssignedCourier)
                .SingleOrDefaultAsync(x => x.Id == selectedId.Value);

            if (packageOrder is not null)
            {
                var lines = packageOrder.RestaurantCheck.Orders
                    .SelectMany(o => o.Lines)
                    .Where(l => l.Status != RestaurantOrderLineStatus.Cancelled)
                    .Select(l => new RestaurantPackageDetailLineViewModel(
                        l.ProductNameSnapshot, l.Quantity, l.Quantity * l.UnitPriceSnapshot - l.DiscountAmountSnapshot))
                    .ToList();

                var retailSaleId = await dbContext.RetailSales
                    .AsNoTracking()
                    .Where(x => x.RestaurantCheckId == packageOrder.RestaurantCheckId)
                    .Select(x => (int?)x.Id)
                    .SingleOrDefaultAsync();

                vm.Selected = new RestaurantPackageDetailViewModel
                {
                    PackageOrderId = packageOrder.Id,
                    CheckId = packageOrder.RestaurantCheckId,
                    RetailSaleId = retailSaleId,
                    PackageNumber = packageOrder.PackageNumber,
                    Channel = packageOrder.Channel,
                    Status = packageOrder.Status,
                    CustomerName = packageOrder.CustomerName,
                    CustomerPhone = packageOrder.CustomerPhone,
                    DeliveryAddress = packageOrder.DeliveryAddress,
                    Note = packageOrder.RestaurantCheck.Note,
                    Lines = lines,
                    Total = lines.Sum(x => x.LineTotal),
                    PlatformCommissionAmount = packageOrder.PlatformCommissionAmount,
                    AssignedCourierId = packageOrder.AssignedCourierId,
                    AssignedCourierName = packageOrder.AssignedCourier?.Name,
                    CancellationReason = packageOrder.CancellationReason,
                    CreatedAtUtc = packageOrder.CreatedAtUtc,
                    ReadyAtUtc = packageOrder.ReadyAtUtc,
                    DispatchedAtUtc = packageOrder.DispatchedAtUtc,
                    DeliveredAtUtc = packageOrder.DeliveredAtUtc
                };
            }
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PackageOrderChannel channel, string customerName, string? customerPhone, string? deliveryAddress, decimal? platformCommissionAmount, Guid submissionKey)
    {
        var branchId = await dbContext.Branches.Where(x => x.IsHeadOffice).Select(x => x.Id).FirstAsync();

        try
        {
            var packageOrder = await postingService.CreatePackageOrderAsync(
                channel, customerName, customerPhone, deliveryAddress, branchId, CurrentUserId, submissionKey, platformCommissionAmount);
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

    // "Adres Düzenle" (madde birebir-uygulama) - müşteri adı/telefon/adres GERÇEK düzenleme,
    // ürün/fiyat DEĞİL (o zaten "Düzenle" ile Check ekranına gidiyor - aynı mantığı burada
    // TEKRARLAMIYORUZ).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCustomerInfo(int packageOrderId, string customerName, string? customerPhone, string? deliveryAddress, string tab = "active")
    {
        var packageOrder = await dbContext.PackageOrders.SingleOrDefaultAsync(x => x.Id == packageOrderId);
        if (packageOrder is null)
        {
            TempData["Error"] = "Sipariş bulunamadı.";
            return RedirectToAction(nameof(Index), new { tab });
        }
        if (string.IsNullOrWhiteSpace(customerName))
        {
            TempData["Error"] = "Müşteri adı zorunludur.";
            return RedirectToAction(nameof(Index), new { tab, selected = packageOrderId });
        }

        packageOrder.CustomerName = customerName.Trim();
        packageOrder.CustomerPhone = string.IsNullOrWhiteSpace(customerPhone) ? null : customerPhone.Trim();
        packageOrder.DeliveryAddress = string.IsNullOrWhiteSpace(deliveryAddress) ? null : deliveryAddress.Trim();
        packageOrder.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Müşteri bilgileri güncellendi.";
        return RedirectToAction(nameof(Index), new { tab, selected = packageOrderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int packageOrderId, string reason, string tab = "active")
    {
        try
        {
            await postingService.CancelPackageOrderAsync(packageOrderId, CurrentUserId, reason);
            TempData["Success"] = "Sipariş iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { tab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignCourier(int packageOrderId, int courierId, string tab = "active")
    {
        try
        {
            await postingService.AssignCourierAsync(packageOrderId, courierId);
            TempData["Success"] = "Kurye atandı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { tab, selected = packageOrderId });
    }

    // Kurye Takibi paneli - kasiyer buradan doğrudan yeni bir kurye tanımlayabilir, ayrı bir
    // "Kurye Tanımları" ekranına gitmesine gerek yok (Edip'in "sağ tarafta kurye takibi olsun"
    // isteğiyle AYNI ekranda tutuluyor - küçük bir CRUD için ayrı sayfa AŞIRI mühendislik olurdu).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourier(string name, string? phone, bool isExternal, string tab = "active")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Kurye adı zorunludur.";
            return RedirectToAction(nameof(Index), new { tab });
        }

        dbContext.RestaurantCouriers.Add(new RestaurantCourier
        {
            Name = name.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            IsExternal = isExternal
        });
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Kurye eklendi.";
        return RedirectToAction(nameof(Index), new { tab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCourierStatus(int courierId, CourierStatus status, string tab = "active")
    {
        var courier = await dbContext.RestaurantCouriers.SingleOrDefaultAsync(x => x.Id == courierId);
        if (courier is not null)
        {
            courier.Status = status;
            courier.UpdatedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index), new { tab });
    }
}
