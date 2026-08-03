namespace SahinSoft.Web.Models;

public sealed class DashboardViewModel
{
    public decimal PurchaseInvoiceTotal { get; set; }
    public decimal SalesInvoiceTotal { get; set; }
    public decimal CollectionTotal { get; set; }
    public decimal PaymentTotal { get; set; }
    public decimal TotalDebt { get; set; }
    public decimal TotalReceivable { get; set; }
    public DateTime FilterFrom { get; set; }
    public DateTime FilterTo { get; set; }
    public string Period { get; set; } = "today";
    public string PeriodLabel { get; set; } = "Bugün";

    public int NormalStockCount { get; set; }
    public int CriticalStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int UntrackedStockCount { get; set; }

    public int DraftInvoiceCount { get; set; }
    public int PendingOrderCount { get; set; }
    public int OverdueReceivableCount { get; set; }
    public int OverduePayableCount { get; set; }

    public IReadOnlyList<DailyInvoiceStat> DailyInvoiceStats { get; set; } = [];
    public IReadOnlyList<DailyCashFlowStat> DailyCashFlowStats { get; set; } = [];
    public IReadOnlyList<UpcomingPaymentItem> UpcomingPayments { get; set; } = [];
    public IReadOnlyList<CriticalStockItem> CriticalStocks { get; set; } = [];
    public IReadOnlyList<RecentActivityItem> RecentActivities { get; set; } = [];
}

public sealed class DailyInvoiceStat
{
    public DateTime DateUtc { get; set; }
    public decimal SalesTotal { get; set; }
    public decimal PurchaseTotal { get; set; }
}

public sealed class DailyCashFlowStat
{
    public DateTime DateUtc { get; set; }
    public decimal CollectionTotal { get; set; }
    public decimal PaymentTotal { get; set; }
}

public sealed class UpcomingPaymentItem
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DueDateUtc { get; set; }
    public decimal RemainingAmount { get; set; }
    public int DaysUntilDue { get; set; }
}

public sealed class CriticalStockItem
{
    public int ProductId { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal StockQuantity { get; set; }
    public decimal MinimumStockQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsOutOfStock => StockQuantity <= 0;
}

public sealed class RecentActivityItem
{
    public DateTime DateUtc { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Controller { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Tone { get; set; } = "info";
}
