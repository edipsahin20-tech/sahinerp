using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class PurchasePriceList : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime ValidFromUtc { get; set; }
    public DateTime? ValidUntilUtc { get; set; }
    public bool PricesIncludeTax { get; set; }
    public bool IsActive { get; set; } = true;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public ICollection<PurchasePriceListItem> Items { get; set; } = new List<PurchasePriceListItem>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
