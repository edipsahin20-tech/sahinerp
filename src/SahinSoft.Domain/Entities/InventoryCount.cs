using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class InventoryCount : EntityBase
{
    public string CountNumber { get; set; } = string.Empty;
    public DateTime CountDateUtc { get; set; } = DateTime.UtcNow;
    public InventoryCountStatus Status { get; set; } = InventoryCountStatus.Draft;
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<InventoryCountLine> Lines { get; set; } = new List<InventoryCountLine>();
}
