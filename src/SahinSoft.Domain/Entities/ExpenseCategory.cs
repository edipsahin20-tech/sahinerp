using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ExpenseCategory : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
