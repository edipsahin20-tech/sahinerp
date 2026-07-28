using Microsoft.AspNetCore.Identity;
using SahinSoft.Domain.Entities;

namespace SahinSoft.Web.Data;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? PersonnelCode { get; set; }
    public string? JobTitle { get; set; }
    public string? Address { get; set; }
    public DateTime? HireDateUtc { get; set; }
    public DateTime? TerminationDateUtc { get; set; }
    public decimal? Salary { get; set; }
    public decimal? Deduction { get; set; }
    public string? DeductionNote { get; set; }
    public string? Iban { get; set; }
    public string? BankAccountNumber { get; set; }
    public decimal? CommissionRate { get; set; }
    public int? BreakDurationMinutes { get; set; }
    public string? LicensePlate { get; set; }
    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public int? DefaultFinancialAccountId { get; set; }
    public FinancialAccount? DefaultFinancialAccount { get; set; }
    public int? DefaultPriceListId { get; set; }
    public PriceList? DefaultPriceList { get; set; }
}
