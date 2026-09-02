using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class RestaurantPackageViewModel
{
    public string ActiveTab { get; set; } = "active";
    public int ActiveCount { get; set; }
    public int ReadyCount { get; set; }
    public int OnTheWayCount { get; set; }

    public IReadOnlyList<RestaurantPackageListItemViewModel> Orders { get; set; } = [];
    public RestaurantPackageDetailViewModel? Selected { get; set; }
}

public sealed record RestaurantPackageListItemViewModel(
    int PackageOrderId,
    int CheckId,
    string PackageNumber,
    string CustomerName,
    PackageOrderChannel Channel,
    PackageOrderStatus Status,
    decimal Total,
    DateTime CreatedAtUtc);

public sealed class RestaurantPackageDetailViewModel
{
    public int PackageOrderId { get; set; }
    public int CheckId { get; set; }
    public string PackageNumber { get; set; } = string.Empty;
    public PackageOrderChannel Channel { get; set; }
    public PackageOrderStatus Status { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public List<RestaurantPackageDetailLineViewModel> Lines { get; set; } = [];
    public decimal Total { get; set; }
}

public sealed record RestaurantPackageDetailLineViewModel(string ProductName, decimal Quantity, decimal LineTotal);
