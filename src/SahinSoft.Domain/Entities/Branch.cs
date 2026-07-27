using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class Branch : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsHeadOffice { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
