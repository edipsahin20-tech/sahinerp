using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ProductCategory : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? WebsitePath { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
