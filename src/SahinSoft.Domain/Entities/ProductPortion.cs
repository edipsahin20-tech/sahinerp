using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ProductPortion : EntityBase
{
    public string Name { get; set; } = string.Empty;

    // Null ise Product.SalePrice kullanılır.
    public decimal? PriceOverride { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public ICollection<ProductRecipeHeader> RecipeHeaders { get; set; } = new List<ProductRecipeHeader>();
}
