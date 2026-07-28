using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class BusinessOrderDetailsViewModel
{
    public int Id { get; set; }
    public InvoiceType OrderType { get; set; }
    public BusinessDocumentStatus Status { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
    public DateTime? RequestedDeliveryDateUtc { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<BusinessOrderDetailsLineViewModel> Lines { get; set; } = [];
}

public sealed class BusinessOrderDetailsLineViewModel
{
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string UnitSnapshot { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal FulfilledQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
