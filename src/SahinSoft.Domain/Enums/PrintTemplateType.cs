using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

// Çıktı Tasarımcısı (Edip'in "YAZDIRMA / RAPOR TASARIM ALTYAPISI" talimatı) - 4 belge tipi,
// her biri kendi PrintFieldRegistry alan listesine ve varsayılan şablonuna sahip.
public enum PrintTemplateType
{
    [Display(Name = "Adisyon Fişi")]
    Adisyon = 1,
    [Display(Name = "Mutfak Fişi")]
    MutfakFisi = 2,
    [Display(Name = "X Raporu")]
    XRaporu = 3,
    [Display(Name = "Z Raporu")]
    ZRaporu = 4,
    [Display(Name = "Tahsilat/Tediye Makbuzu")]
    CariMakbuz = 5
}
