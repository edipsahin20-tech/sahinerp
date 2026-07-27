using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ProductUnitConversion : EntityBase
{
    public decimal MultiplierToBaseUnit { get; set; } = 1;
    public bool IsPurchaseUnit { get; set; }
    public bool IsSalesUnit { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int UnitOfMeasureId { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
}
