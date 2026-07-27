using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ProductVariant : EntityBase
{
    public string VariantCode { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public decimal AdditionalPurchasePrice { get; set; }
    public decimal AdditionalSalePrice { get; set; }
    public bool IsActive { get; set; } = true;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ColorId { get; set; }
    public ProductColor? Color { get; set; }
    public ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
