using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class CustomerContact : EntityBase
{
    public string FullName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}
