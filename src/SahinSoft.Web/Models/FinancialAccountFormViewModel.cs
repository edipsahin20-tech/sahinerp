using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class FinancialAccountFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Hesap kodu")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Hesap adı")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Hesap türü")]
    public FinancialAccountType AccountType { get; set; } = FinancialAccountType.Cash;

    [Required, StringLength(3)]
    [Display(Name = "Para birimi")]
    public string CurrencyCode { get; set; } = "TRY";

    [StringLength(150)]
    [Display(Name = "Banka adı")]
    public string? BankName { get; set; }

    [StringLength(150)]
    [Display(Name = "Şube adı")]
    public string? BranchName { get; set; }

    [StringLength(34)]
    [Display(Name = "IBAN")]
    public string? Iban { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
