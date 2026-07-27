using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class BusinessOrder : EntityBase
{
    public InvoiceType OrderType { get; set; }
    public BusinessDocumentStatus Status { get; set; } = BusinessDocumentStatus.Draft;
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RequestedDeliveryDateUtc { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? QuoteId { get; set; }
    public Quote? Quote { get; set; }
    public ICollection<BusinessOrderLine> Lines { get; set; } = new List<BusinessOrderLine>();
}
