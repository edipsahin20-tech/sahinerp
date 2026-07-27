using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class StockMovement : EntityBase
{
    public DateTime MovementDateUtc { get; set; } = DateTime.UtcNow;
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Description { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int? InvoiceLineId { get; set; }
    public InvoiceLine? InvoiceLine { get; set; }
    public int? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public int? BusinessProjectId { get; set; }
    public BusinessProject? BusinessProject { get; set; }
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int? StockTransferLineId { get; set; }
    public StockTransferLine? StockTransferLine { get; set; }
    public int? StockSlipLineId { get; set; }
    public StockSlipLine? StockSlipLine { get; set; }
    public int? InventoryCountLineId { get; set; }
    public InventoryCountLine? InventoryCountLine { get; set; }
    public int? ReversalOfId { get; set; }
    public StockMovement? ReversalOf { get; set; }
}
