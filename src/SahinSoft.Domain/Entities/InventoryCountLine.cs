using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class InventoryCountLine : EntityBase
{
    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal DifferenceQuantity => CountedQuantity - SystemQuantity;
    public int InventoryCountId { get; set; }
    public InventoryCount InventoryCount { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
