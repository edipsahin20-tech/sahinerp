using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum InstrumentStatus
{
    [Display(Name = "Portföyde")]
    Portfolio = 1,
    [Display(Name = "Ciro Edildi")]
    Endorsed = 2,
    [Display(Name = "Tahsil Edildi")]
    Collected = 3,
    [Display(Name = "Ödendi")]
    Paid = 4,
    [Display(Name = "İade Edildi")]
    Returned = 5,
    [Display(Name = "Karşılıksız")]
    Protested = 6,
    [Display(Name = "İptal")]
    Cancelled = 7
}
