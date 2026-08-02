using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class DispatchNoteDetailsViewModel
{
    public int Id { get; set; }
    public InvoiceType DispatchType { get; set; }
    public BusinessDocumentStatus Status { get; set; }
    public string DispatchNumber { get; set; } = string.Empty;
    public DateTime DispatchDateUtc { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerAddress { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public PdfCompanyInfoViewModel Company { get; set; } = new();
    public string? VehiclePlate { get; set; }
    public string? CarrierName { get; set; }
    public string? Notes { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public IReadOnlyList<DispatchNoteDetailsLineViewModel> Lines { get; set; } = [];
    public int? SourceOrderId { get; set; }
    public string? SourceOrderNumber { get; set; }
    public IReadOnlyList<LinkedDocumentViewModel> LinkedInvoices { get; set; } = [];
}

public sealed class DispatchNoteDetailsLineViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal InvoicedQuantity { get; set; }
    public decimal RemainingQuantity => Quantity - InvoicedQuantity;
}
