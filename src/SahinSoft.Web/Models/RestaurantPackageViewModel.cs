using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class RestaurantPackageViewModel
{
    // Durum kuyruğu (madde birebir-uygulama, 2026-09-05) - onaylı mockup'ın 7 sekmesi:
    // Yeni/Onay Bekliyor/Hazırlanıyor/Hazır/Kurye Bekliyor/Yolda/Teslim Edildi.
    public string ActiveTab { get; set; } = "active";
    public int NewCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public int PreparingCount { get; set; }
    public int ReadyCount { get; set; }
    public int CourierWaitingCount { get; set; }
    public int OnTheWayCount { get; set; }
    public int DeliveredCount { get; set; }

    // Kanal sekmeleri (Tümü/Telefon/Web/Yemeksepeti/Trendyol Yemek/GetirYemek).
    public string ChannelFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int PhoneCount { get; set; }
    public int WebCount { get; set; }
    public int YemeksepetiCount { get; set; }
    public int TrendyolYemekCount { get; set; }
    public int GetirYemekCount { get; set; }

    public string? SearchTerm { get; set; }
    public string? CourierFilter { get; set; }

    public IReadOnlyList<RestaurantPackageListItemViewModel> Orders { get; set; } = [];
    public RestaurantPackageDetailViewModel? Selected { get; set; }
    public List<RestaurantCourierViewModel> Couriers { get; set; } = [];
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
    public int? RetailSaleId { get; set; }
    public string PackageNumber { get; set; } = string.Empty;
    public PackageOrderChannel Channel { get; set; }
    public PackageOrderStatus Status { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Note { get; set; }
    public List<RestaurantPackageDetailLineViewModel> Lines { get; set; } = [];
    public decimal Total { get; set; }
    public decimal? PlatformCommissionAmount { get; set; }
    public decimal NetPayout => Total - (PlatformCommissionAmount ?? 0);
    public int? AssignedCourierId { get; set; }
    public string? AssignedCourierName { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadyAtUtc { get; set; }
    public DateTime? DispatchedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
}

public sealed record RestaurantPackageDetailLineViewModel(string ProductName, decimal Quantity, decimal LineTotal);

public sealed record RestaurantCourierViewModel(int CourierId, string Name, string? Phone, bool IsExternal, CourierStatus Status, int ActiveOrderCount);
