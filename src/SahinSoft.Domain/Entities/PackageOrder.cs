using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

// Paket/Gel-Al siparişine özel METADATA - fiyat/ürün/ödeme/mutfak verisi burada TEKRAR
// TUTULMAZ, mevcut RestaurantCheck/RestaurantOrder/RestaurantOrderLine/RestaurantPayment/
// KitchenTicket zincirinden okunur (bkz. Edip'in onayı, 2026-08-09). Her paket sipariş kendi
// gizli sanal RestaurantTable/Session/Check zincirine 1:1 bağlıdır (bkz. RestaurantPostingService.
// CreatePackageOrderAsync) - bu yüzden RestaurantCheckId burada TEKİL (unique).
public sealed class PackageOrder : EntityBase
{
    public string PackageNumber { get; set; } = string.Empty;
    public PackageOrderChannel Channel { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }

    public PackageOrderStatus Status { get; set; } = PackageOrderStatus.Preparing;
    public DateTime? ReadyAtUtc { get; set; }
    public DateTime? DispatchedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }

    // Çift tıklama/mükerrer POST koruması (durum ilerletme) - bkz. StockSlip.SubmissionKey.
    public Guid? SubmissionKey { get; set; }

    public int RestaurantCheckId { get; set; }
    public RestaurantCheck RestaurantCheck { get; set; } = null!;

    // Onaylı Paket Operasyon Merkezi mockup'ı (madde birebir-uygulama, 2026-09-05) - kurye ataması
    // ve platform komisyonu ("Platform İndirimi" / "Restoran Hakedişi" satırları). Komisyon gerçek
    // bir platform API'sinden GELMİYOR (entegrasyon yok, spec madde 35) - kasiyer siparişi
    // Yemeksepeti/Trendyol/GetirYemek olarak işaretlerken elle girer, muhasebeye YANSIMAZ (sadece
    // bu ekranda bilgi amaçlı gösterilir - gerçek satış tutarı zaten RestaurantCheck üzerinden
    // aynı yerden geçiyor).
    public int? AssignedCourierId { get; set; }
    public RestaurantCourier? AssignedCourier { get; set; }
    public decimal? PlatformCommissionAmount { get; set; }
    public string? CancellationReason { get; set; }
}
