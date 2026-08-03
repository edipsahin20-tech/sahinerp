using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class RestaurantSection : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public ICollection<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();
}
