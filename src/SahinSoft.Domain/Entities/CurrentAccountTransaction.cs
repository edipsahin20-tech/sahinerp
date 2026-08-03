using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class CurrentAccountTransaction : EntityBase
{
    public DateTime TransactionDateUtc { get; set; } = DateTime.UtcNow;
    public CurrentAccountTransactionType TransactionType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public string? Description { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? QuoteId { get; set; }
    public Quote? Quote { get; set; }
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public int? NegotiableInstrumentId { get; set; }
    public NegotiableInstrument? NegotiableInstrument { get; set; }
    public int? ReversalOfId { get; set; }
    public CurrentAccountTransaction? ReversalOf { get; set; }
    public ICollection<PaymentReceiptLine> PaymentReceiptLines { get; set; } = new List<PaymentReceiptLine>();
}
