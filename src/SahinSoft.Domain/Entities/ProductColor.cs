using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ProductColor : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? HexCode { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
