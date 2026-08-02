using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class BusinessOrderLine : EntityBase
{
    public int LineNumber { get; set; }
    public string ProductCodeSnapshot { get; set; } = string.Empty;
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string UnitSnapshot { get; set; } = "Adet";
    public decimal Quantity { get; set; }
    public decimal FulfilledQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
    public int BusinessOrderId { get; set; }
    public BusinessOrder BusinessOrder { get; set; } = null!;
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    // Bu sipariş satırından üretilmiş irsaliye/fatura satırları (dönüşüm izlenebilirliği).
    public ICollection<DispatchNoteLine> DispatchNoteLines { get; set; } = new List<DispatchNoteLine>();
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
}
