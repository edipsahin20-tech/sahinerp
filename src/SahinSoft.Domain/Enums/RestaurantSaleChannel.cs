using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

// Sistemin ana ticari işlemi ADİSYON/SATIŞ OTURUMUdur (RestaurantTableSession) - MASA bu
// oturuma bağlanabilen OPSİYONEL bir bilgidir, oturumun kendisi değildir (Edip, 2026-09-29:
// "Masa ana işlem değildir... market yüz binlerce satış yapabilir, bunların hiçbiri masa
// üretmemeli"). Bu ayrım daha önce RestaurantSection.Name == "Self Satış"/"Paket" gibi METİN
// KARŞILAŞTIRMASIYLA yapılıyordu (kırılgan, ayrıca her kanal için gizli birer RestaurantTable/
// RestaurantSection satırı gerektiriyordu) - artık RestaurantTableSession.Channel üzerinden
// AÇIK bir alan. "Gel-Al" ayrı bir üst-seviye kanal DEĞİL - PackageOrder.Channel zaten
// PackageOrderChannel.PickupInStore ile bunu karşılıyor (Paket'in bir alt-kanalı), o yüzden
// burada TEKRAR ÜRETİLMEDİ.
public enum RestaurantSaleChannel
{
    [Display(Name = "Masa Satış")]
    Masa = 1,
    [Display(Name = "Self Satış")]
    SelfSatis = 2,
    [Display(Name = "Paket")]
    Paket = 3
}
