using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class Expense : EntityBase
{
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime ExpenseDateUtc { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public decimal NetAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int ExpenseCategoryId { get; set; }
    public ExpenseCategory ExpenseCategory { get; set; } = null!;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? TaxRateId { get; set; }
    public TaxRate? TaxRate { get; set; }
    public int? FinancialAccountId { get; set; }
    public FinancialAccount? FinancialAccount { get; set; }
    public int? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public int? BusinessProjectId { get; set; }
    public BusinessProject? BusinessProject { get; set; }
}
