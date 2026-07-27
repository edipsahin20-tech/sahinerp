using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class Warehouse : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
