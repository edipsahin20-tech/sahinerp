namespace SahinSoft.Web.Models;

public sealed class RestaurantShiftViewModel
{
    // Kullanıcının kartında kasa (DefaultFinancialAccountId) tanımlı değilse vardiya hiç açılamaz -
    // bkz. RestaurantPostingService.OpenShiftAsync yorumu (kasa şube bazlı, mevcut alan yeniden kullanılır).
    public bool HasFinancialAccount { get; set; }
    public string? FinancialAccountName { get; set; }

    public bool HasOpenShift { get; set; }
    public int? ShiftId { get; set; }
    public DateTime? OpenedAtUtc { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CashInDuringShift { get; set; }
    public decimal ExpectedBalance { get; set; }
    public int ClosedCheckCount { get; set; }
    public decimal GrandTotalDuringShift { get; set; }

    public IReadOnlyList<RestaurantShiftHistoryRow> RecentClosedShifts { get; set; } = [];
}

public sealed record RestaurantShiftHistoryRow(
    int Id,
    DateTime OpenedAtUtc,
    DateTime? ClosedAtUtc,
    decimal OpeningBalance,
    decimal? ClosingBalanceExpected,
    decimal? ClosingBalanceCounted);
