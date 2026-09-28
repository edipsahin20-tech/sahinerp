using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

// Yazıcı Yönetimi - Adisyon/Mutfak/X/Z 4 rol, birbirinden bağımsız yazıcıya atanabilir (Edip:
// "Her belge hangi yazıcıdan çıkacak parametrik olsun").
public enum PrinterRole
{
    [Display(Name = "Adisyon")]
    Adisyon = 1,
    [Display(Name = "Mutfak")]
    Mutfak = 2,
    [Display(Name = "X Raporu")]
    XRaporu = 3,
    [Display(Name = "Z Raporu")]
    ZRaporu = 4
}
