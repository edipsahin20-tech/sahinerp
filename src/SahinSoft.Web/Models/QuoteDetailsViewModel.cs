using SahinSoft.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SahinSoft.Web.Models;

public sealed class QuoteDetailsViewModel
{
    public int Id { get; set; }
    public QuoteStatus Status { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public DateTime QuoteDateUtc { get; set; }
    public DateTime? ValidUntilUtc { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal AmountDiscount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public bool HasConvertedInvoice { get; set; }
    public IReadOnlyList<QuoteDetailsLineViewModel> Lines { get; set; } = [];
    public IReadOnlyList<QuoteTaxBreakdownViewModel> TaxBreakdown { get; set; } = [];
    public IReadOnlyList<SelectListItem> Warehouses { get; set; } = [];
}

public sealed class QuoteTaxBreakdownViewModel
{
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
}

public sealed class QuoteDetailsLineViewModel
{
    public int Id { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string UnitSnapshot { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}
