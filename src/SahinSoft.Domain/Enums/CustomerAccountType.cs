using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum CustomerAccountType
{
    [Display(Name = "Kurumsal")]
    Corporate = 1,
    [Display(Name = "Bireysel")]
    Individual = 2
}
