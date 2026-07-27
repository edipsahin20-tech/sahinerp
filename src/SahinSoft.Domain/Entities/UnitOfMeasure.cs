using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class UnitOfMeasure : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ProductUnitConversion> ProductConversions { get; set; } = new List<ProductUnitConversion>();
}
