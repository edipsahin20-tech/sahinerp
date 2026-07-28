using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class StockSlip : EntityBase
{
    public string SlipNumber { get; set; } = string.Empty;
    public DateTime SlipDateUtc { get; set; } = DateTime.UtcNow;
    public StockSlipType SlipType { get; set; }
    public StockSlipStatus Status { get; set; } = StockSlipStatus.Draft;
    public string? Description { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public int? BusinessProjectId { get; set; }
    public BusinessProject? BusinessProject { get; set; }
    public ICollection<StockSlipLine> Lines { get; set; } = new List<StockSlipLine>();
}
