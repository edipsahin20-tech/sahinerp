using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class BusinessProject : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
