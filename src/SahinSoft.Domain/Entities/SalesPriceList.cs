using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class SalesPriceList : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime ValidFromUtc { get; set; }
    public DateTime? ValidUntilUtc { get; set; }
    public bool PricesIncludeTax { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<SalesPriceListItem> Items { get; set; } = new List<SalesPriceListItem>();
}
