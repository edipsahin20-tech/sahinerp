using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum RestaurantTableSessionStatus
{
    [Display(Name = "Açık")]
    Open = 1,
    [Display(Name = "Kapalı")]
    Closed = 2
}
