using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum PackageOrderChannel
{
    [Display(Name = "Telefon")]
    Phone = 1,
    [Display(Name = "Web")]
    Web = 2,
    [Display(Name = "Gel-Al")]
    PickupInStore = 3
}
