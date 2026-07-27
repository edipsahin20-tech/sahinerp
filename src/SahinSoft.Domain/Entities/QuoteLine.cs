using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class QuoteLine : EntityBase
{
    public int LineNumber { get; set; }
    public string ProductCodeSnapshot { get; set; } = string.Empty;
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string UnitSnapshot { get; set; } = "Adet";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Description { get; set; }

    public int QuoteId { get; set; }
    public Quote Quote { get; set; } = null!;
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
}
