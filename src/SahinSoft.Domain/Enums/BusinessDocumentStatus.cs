using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum BusinessDocumentStatus
{
    [Display(Name = "Taslak")]
    Draft = 1,
    [Display(Name = "Onaylı")]
    Approved = 2,
    [Display(Name = "Kısmen Karşılandı")]
    PartiallyFulfilled = 3,
    [Display(Name = "Tamamlandı")]
    Fulfilled = 4,
    [Display(Name = "İptal")]
    Cancelled = 5
}
