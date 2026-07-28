using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class DispatchNote : EntityBase
{
    public InvoiceType DispatchType { get; set; }
    public BusinessDocumentStatus Status { get; set; } = BusinessDocumentStatus.Draft;
    public string DispatchNumber { get; set; } = string.Empty;
    public DateTime DispatchDateUtc { get; set; } = DateTime.UtcNow;
    public string? VehiclePlate { get; set; }
    public string? CarrierName { get; set; }
    public string? Notes { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int? BusinessOrderId { get; set; }
    public BusinessOrder? BusinessOrder { get; set; }
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public ICollection<DispatchNoteLine> Lines { get; set; } = new List<DispatchNoteLine>();
}
