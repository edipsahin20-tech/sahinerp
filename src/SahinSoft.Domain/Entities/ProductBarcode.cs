using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ProductBarcode : EntityBase
{
    public string Barcode { get; set; } = string.Empty;
    public string BarcodeType { get; set; } = "EAN13";
    public decimal UnitMultiplier { get; set; } = 1;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
