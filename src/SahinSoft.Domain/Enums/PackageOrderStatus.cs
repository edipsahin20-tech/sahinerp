using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

// Sıradaki durum her zaman sunucuda MEVCUT durumdan hesaplanır (bkz. RestaurantPostingService.
// AdvancePackageOrderAsync) - istemciden "hedef durum" parametresi asla kabul edilmez, bu yüzden
// "Hazırlanıyor" iken doğrudan "Yolda"ya sıçrama yapılması yapısal olarak mümkün değildir.
public enum PackageOrderStatus
{
    [Display(Name = "Hazırlanıyor")]
    Preparing = 1,
    [Display(Name = "Hazır")]
    Ready = 2,
    [Display(Name = "Kurye Bekliyor")]
    CourierWaiting = 3,
    [Display(Name = "Yolda")]
    OnTheWay = 4,
    [Display(Name = "Teslim Edildi")]
    Delivered = 5,
    [Display(Name = "İptal")]
    Cancelled = 6,
    // Onaylı Paket Operasyon Merkezi mockup'ı (madde birebir-uygulama, 2026-09-05) siparişin
    // Hazırlanıyor'dan ÖNCE 2 aşamadan geçmesini istiyor - GERİYE DÖNÜK UYUMLULUK için var olan
    // 1-6 numaraları DEĞİŞTİRİLMEDİ (DB'de zaten kayıtlı satırlar var), yeni durumlar sona eklendi.
    [Display(Name = "Yeni")]
    New = 7,
    [Display(Name = "Onay Bekliyor")]
    PendingApproval = 8
}
