using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class CostCenter : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
