using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class StockTransfer : EntityBase
{
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDateUtc { get; set; } = DateTime.UtcNow;
    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;
    public string? Description { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }

    public int FromWarehouseId { get; set; }
    public Warehouse FromWarehouse { get; set; } = null!;
    public int ToWarehouseId { get; set; }
    public Warehouse ToWarehouse { get; set; } = null!;
    public ICollection<StockTransferLine> Lines { get; set; } = new List<StockTransferLine>();
}
