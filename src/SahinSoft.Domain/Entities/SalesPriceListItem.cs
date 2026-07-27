using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class SalesPriceListItem : EntityBase
{
    public decimal MinimumQuantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public int SalesPriceListId { get; set; }
    public SalesPriceList SalesPriceList { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
