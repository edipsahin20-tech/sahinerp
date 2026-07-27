using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class CustomerAddress : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public string AddressType { get; set; } = "Fatura";
    public string AddressLine { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string CountryCode { get; set; } = "TR";
    public bool IsDefault { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}
