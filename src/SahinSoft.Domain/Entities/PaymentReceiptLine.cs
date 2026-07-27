using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class PaymentReceiptLine : EntityBase
{
    public int LineNumber { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }

    public int PaymentReceiptId { get; set; }
    public PaymentReceipt PaymentReceipt { get; set; } = null!;
    public int FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
    public int? CurrentAccountTransactionId { get; set; }
    public CurrentAccountTransaction? CurrentAccountTransaction { get; set; }
    public int? FinancialTransactionId { get; set; }
    public FinancialTransaction? FinancialTransaction { get; set; }
}
