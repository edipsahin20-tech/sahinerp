using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum FinancialAccountType
{
    [Display(Name = "Kasa")]
    Cash = 1,
    [Display(Name = "Banka")]
    Bank = 2
}
