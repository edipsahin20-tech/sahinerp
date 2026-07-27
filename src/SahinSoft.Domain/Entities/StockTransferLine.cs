using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class StockTransferLine : EntityBase
{
    public int LineNumber { get; set; }
    public decimal Quantity { get; set; }
    public string? Description { get; set; }
    public int StockTransferId { get; set; }
    public StockTransfer StockTransfer { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
