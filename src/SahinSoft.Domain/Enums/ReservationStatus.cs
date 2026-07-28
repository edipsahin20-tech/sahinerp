using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum ReservationStatus
{
    [Display(Name = "Aktif")]
    Active = 1,
    [Display(Name = "Tamamlandı")]
    Fulfilled = 2,
    [Display(Name = "İptal")]
    Cancelled = 3,
    [Display(Name = "Süresi Doldu")]
    Expired = 4
}
