using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

// Yok (None) iken restoran Kapat/Öde akışı BUGÜNKÜ gibi, hiçbir fiskal cihaz çağrısı yapmadan
// çalışır - bu alan sadece bir yazar kasa seçilip Ayarlar'dan yapılandırıldığında devreye girer.
public enum FiscalDeviceType
{
    [Display(Name = "Yok")]
    None = 0,
    [Display(Name = "İnpos M530")]
    InposM530 = 1
}
