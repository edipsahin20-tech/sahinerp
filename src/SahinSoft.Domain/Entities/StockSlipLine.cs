using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class StockSlipLine : EntityBase
{
    public int LineNumber { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Description { get; set; }
    public int StockSlipId { get; set; }
    public StockSlip StockSlip { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
