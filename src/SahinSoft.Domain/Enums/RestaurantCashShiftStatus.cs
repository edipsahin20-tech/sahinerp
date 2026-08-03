using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum RestaurantCashShiftStatus
{
    [Display(Name = "Açık")]
    Open = 1,
    [Display(Name = "Kapalı")]
    Closed = 2
}
