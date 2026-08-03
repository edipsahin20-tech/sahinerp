using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class RestaurantOrderLine : EntityBase
{
    public decimal Quantity { get; set; }

    // Sipariş anında donar — ürün adı/fiyatı sonradan değişse bile eski sipariş bozulmaz.
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string? PortionNameSnapshot { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public decimal TaxRateSnapshot { get; set; }
    public decimal DiscountAmountSnapshot { get; set; }

    // Kapanışta hangi reçete versiyonunun kullanıldığı.
    public int? RecipeVersionUsed { get; set; }
    public string? KitchenNote { get; set; }

    // Elle değiştirilmez — ilişkili KitchenTicketLine'ların durumlarından hesaplanan bir
    // önbellektir (bkz. RestaurantOrderLineStatus açıklaması ve §11 Karar 4).
    public RestaurantOrderLineStatus Status { get; set; } = RestaurantOrderLineStatus.Ordered;

    public string? CancelledByUserId { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }

    public int RestaurantOrderId { get; set; }
    public RestaurantOrder RestaurantOrder { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductPortionId { get; set; }
    public ProductPortion? ProductPortion { get; set; }

    public ICollection<RestaurantOrderLineModifier> Modifiers { get; set; } = new List<RestaurantOrderLineModifier>();
    public ICollection<KitchenTicketLine> KitchenTicketLines { get; set; } = new List<KitchenTicketLine>();
}
