using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ProductRecipeLine : EntityBase
{
    public decimal Quantity { get; set; }
    public decimal WastagePercent { get; set; }

    public int ProductRecipeHeaderId { get; set; }
    public ProductRecipeHeader ProductRecipeHeader { get; set; } = null!;

    // Hammadde de bir Product kaydıdır — yeni bir "Ingredient" tablosu yok.
    public int IngredientProductId { get; set; }
    public Product IngredientProduct { get; set; } = null!;

    public int? UnitOfMeasureId { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }
}
