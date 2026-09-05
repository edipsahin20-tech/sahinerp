using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

// Kurye durumu (madde birebir-uygulama, Paket Operasyon Merkezi, 2026-09-05) - kasiyer/kurye
// tarafından ELLE güncellenir, gerçek bir GPS/mobil uygulama entegrasyonu YOK (spec madde 35'in
// "sonraki çalışma paketi" dediği kapsamda - canlı harita/konum takibi burada İCAT EDİLMEDİ).
public enum CourierStatus
{
    [Display(Name = "Müsait")]
    Available = 1,
    [Display(Name = "Teslimatta")]
    Delivering = 2,
    [Display(Name = "Çevrimdışı")]
    Offline = 3
}
