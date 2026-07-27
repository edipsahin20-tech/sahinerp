using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class PurchasePriceListItem : EntityBase
{
    public decimal MinimumQuantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public string? SupplierProductCode { get; set; }
    public string? Notes { get; set; }

    public int PurchasePriceListId { get; set; }
    public PurchasePriceList PurchasePriceList { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
