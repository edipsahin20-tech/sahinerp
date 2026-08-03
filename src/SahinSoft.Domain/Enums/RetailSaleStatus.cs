using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum RetailSaleStatus
{
    [Display(Name = "Kesildi")]
    Issued = 1,
    [Display(Name = "İptal")]
    Cancelled = 2
}
