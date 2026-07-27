using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class TaxRate : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsExempt { get; set; }
    public bool IsActive { get; set; } = true;
}
