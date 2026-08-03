using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum RestaurantCheckStatus
{
    [Display(Name = "Açık")]
    Open = 1,
    [Display(Name = "Kapalı")]
    Closed = 2,
    [Display(Name = "İptal")]
    Cancelled = 3
}
