using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class InvoiceDetailsViewModel
{
    public int Id { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public InvoiceStatus Status { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDateUtc { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string? CustomerTaxOffice { get; set; }
    public string? CustomerTaxNumber { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerAddress { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public string? ReferenceNumber { get; set; }
    public string? PaymentTerm { get; set; }
    public string? TradeType { get; set; }
    public bool IsReturn { get; set; }
    public string? SalespersonName { get; set; }
    public bool IsClosedInvoice { get; set; }
    public string? SettlementPaymentMethodName { get; set; }
    public string? SettlementFinancialAccountName { get; set; }
    public int? SettlementReceiptId { get; set; }
    public string? SettlementReceiptNumber { get; set; }
    public PdfCompanyInfoViewModel Company { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal AmountDiscount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? CancelledByUserId { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }
    public IReadOnlyList<InvoiceDetailsLineViewModel> Lines { get; set; } = [];
    public IReadOnlyList<InvoiceTaxBreakdownViewModel> TaxBreakdown { get; set; } = [];
    public IReadOnlyList<InvoiceDetailsScheduleViewModel> PaymentSchedules { get; set; } = [];
    public int ActiveLinkedReceiptCount { get; set; }
}

public sealed class InvoiceTaxBreakdownViewModel
{
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
}

public sealed class InvoiceDetailsLineViewModel
{
    public int LineNumber { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string UnitSnapshot { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
    public decimal GrossTotal { get; set; }
    public decimal NetTotal { get; set; }
    public string? Description { get; set; }
    public string? SourceDisplay { get; set; }
}

public sealed class InvoiceDetailsScheduleViewModel
{
    public int InstallmentNumber { get; set; }
    public DateTime DueDateUtc { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
}
