namespace SahinSoft.Web.Models;

public sealed class CustomerStatementViewModel
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public IReadOnlyList<CustomerStatementLineViewModel> Lines { get; set; } = [];
}

public sealed class CustomerStatementLineViewModel
{
    public DateTime TransactionDateUtc { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}
