namespace SahinSoft.Web.Models;

public sealed class CustomerListItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? District { get; set; }
    public string? TaxNumber { get; set; }
    public string? Phone { get; set; }
    public bool IsCustomer { get; set; }
    public bool IsSupplier { get; set; }
    public bool IsActive { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance => Debit - Credit;
}
