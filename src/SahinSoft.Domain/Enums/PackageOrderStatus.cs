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
    Cancelled = 6
}
