namespace SahinSoft.Web.Models;

public sealed class CustomerPriceListViewModel
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public IReadOnlyList<CustomerPriceListItemViewModel> Items { get; set; } = [];
}

public sealed class CustomerPriceListItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
