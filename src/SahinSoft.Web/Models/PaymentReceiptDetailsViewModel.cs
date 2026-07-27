using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class PaymentReceiptDetailsViewModel
{
    public int Id { get; set; }
    public ReceiptType ReceiptType { get; set; }
    public PaymentReceiptStatus Status { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceiptDateUtc { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? CancelledByUserId { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }
    public IReadOnlyList<PaymentReceiptDetailsLineViewModel> Lines { get; set; } = [];
}

public sealed class PaymentReceiptDetailsLineViewModel
{
    public int LineNumber { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public decimal Amount { get; set; }
    public string FinancialAccountName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
