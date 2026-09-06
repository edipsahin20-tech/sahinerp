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

    // Talimat 1 (2026-09-06) - "muhasebe tarafından da ADS ve Z'ye geri gidilebilmeli" (bkz.
    // FinancialTransactionReportLineViewModel'deki AYNI alanların yorumu).
    public int? RestaurantZPeriodId { get; set; }
    public string? RestaurantZNo { get; set; }
    public int? RestaurantRetailSaleId { get; set; }
}
