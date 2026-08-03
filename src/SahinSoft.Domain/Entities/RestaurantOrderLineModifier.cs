using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class RestaurantOrderLineModifier : EntityBase
{
    public string NameSnapshot { get; set; } = string.Empty;
    public decimal PriceSnapshot { get; set; }
    public decimal Quantity { get; set; } = 1;

    public int RestaurantOrderLineId { get; set; }
    public RestaurantOrderLine RestaurantOrderLine { get; set; } = null!;
}
