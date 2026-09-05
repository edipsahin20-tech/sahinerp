using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum PackageOrderChannel
{
    [Display(Name = "Telefon")]
    Phone = 1,
    [Display(Name = "Web")]
    Web = 2,
    [Display(Name = "Gel-Al")]
    PickupInStore = 3,
    // Onaylı Paket Operasyon Merkezi mockup'ı bu 3 platformu ayrı kanal olarak istiyor (madde
    // birebir-uygulama, 2026-09-05) - GERÇEK API entegrasyonu YOK (spec madde 35: "Paket
    // Operasyon Merkezi'nin ayrıntılı geliştirmesi sonraki çalışma paketinde"), sipariş yine
    // kasiyer tarafından manuel girilir, sadece kaynağı bu kanallardan biri olarak İŞARETLENİR.
    [Display(Name = "Yemeksepeti")]
    Yemeksepeti = 4,
    [Display(Name = "Trendyol Yemek")]
    TrendyolYemek = 5,
    [Display(Name = "GetirYemek")]
    GetirYemek = 6
}
