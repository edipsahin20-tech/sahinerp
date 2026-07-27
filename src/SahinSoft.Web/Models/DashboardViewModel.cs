namespace SahinSoft.Web.Models;

public sealed class DashboardViewModel
{
    public int ActiveCustomerCount { get; set; }
    public int ActiveProductCount { get; set; }
    public int DraftInvoiceCount { get; set; }
    public int ApprovedInvoiceCount { get; set; }
}
