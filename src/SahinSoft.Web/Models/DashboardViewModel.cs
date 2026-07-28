namespace SahinSoft.Web.Models;

public sealed class DashboardViewModel
{
    public decimal PurchaseInvoiceTotal { get; set; }
    public decimal SalesInvoiceTotal { get; set; }
    public decimal TotalDebt { get; set; }
    public decimal TotalReceivable { get; set; }
    public DateTime? FilterFrom { get; set; }
    public DateTime? FilterTo { get; set; }
    public IReadOnlyList<DailyInvoiceStat> DailyInvoiceStats { get; set; } = [];
    public IReadOnlyList<TopCustomerStat> TopCustomers { get; set; } = [];
}

public sealed class DailyInvoiceStat
{
    public DateTime DateUtc { get; set; }
    public int SalesCount { get; set; }
    public decimal SalesTotal { get; set; }
    public int PurchaseCount { get; set; }
    public decimal PurchaseTotal { get; set; }
}

public sealed class TopCustomerStat
{
    public string CustomerName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
