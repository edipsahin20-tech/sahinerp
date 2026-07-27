using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class FinancialAccount : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public FinancialAccountType AccountType { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string? Iban { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    public ICollection<PaymentReceiptLine> PaymentReceiptLines { get; set; } = new List<PaymentReceiptLine>();
}
