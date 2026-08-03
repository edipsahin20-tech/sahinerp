using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

// Dahili perakende satış fişi resmi mali fişle henüz eşleşmediği sürece
// "Resmî Mali Belge Değildir" ibaresiyle basılır — bkz. CLEAN_ROOM_DEVELOPMENT.md §7.
public enum RetailSaleFiscalizationStatus
{
    [Display(Name = "Mali Fişe Bağlanmadı")]
    NotFiscalized = 1,
    [Display(Name = "Mali Fişe Bağlandı")]
    Fiscalized = 2,
    [Display(Name = "Başarısız")]
    Failed = 3
}
