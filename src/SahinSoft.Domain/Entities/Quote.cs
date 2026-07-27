using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class Quote : EntityBase
{
    public string QuoteNumber { get; set; } = string.Empty;
    public DateTime QuoteDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntilUtc { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
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
    public ICollection<QuoteLine> Lines { get; set; } = new List<QuoteLine>();
    public ICollection<CurrentAccountTransaction> AccountTransactions { get; set; } = new List<CurrentAccountTransaction>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
