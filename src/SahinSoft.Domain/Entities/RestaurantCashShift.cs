using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class RestaurantCashShift : EntityBase
{
    public string CashierUserId { get; set; } = string.Empty;
    public RestaurantCashShiftStatus Status { get; set; } = RestaurantCashShiftStatus.Open;
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public decimal OpeningBalance { get; set; }
    public DateTime? ClosedAtUtc { get; set; }

    // Sistem hesaplar.
    public decimal? ClosingBalanceExpected { get; set; }
    // Kasiyer girer.
    public decimal? ClosingBalanceCounted { get; set; }

    public Guid? SubmissionKey { get; set; }

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public int FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
}
