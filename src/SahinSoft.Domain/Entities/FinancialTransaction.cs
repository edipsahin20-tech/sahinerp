using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class FinancialTransaction : EntityBase
{
    public DateTime TransactionDateUtc { get; set; } = DateTime.UtcNow;
    public FinancialTransactionType TransactionType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; } = 1;
    public string? Description { get; set; }

    public int FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? CurrentAccountTransactionId { get; set; }
    public CurrentAccountTransaction? CurrentAccountTransaction { get; set; }
    public int? ReversalOfId { get; set; }
    public FinancialTransaction? ReversalOf { get; set; }
    public ICollection<PaymentReceiptLine> PaymentReceiptLines { get; set; } = new List<PaymentReceiptLine>();
}
