using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

// Bu durum artık RestaurantOrderLine üzerinde elle değiştirilmez — KitchenTicketLine'ların
// durumlarından hesaplanır (bkz. CLEAN_ROOM_DEVELOPMENT.md §11 Karar 4). Sütun, ekranlarda hızlı
// filtreleme/gösterim için denormalize edilmiş bir önbellek olarak tutulur.
public enum RestaurantOrderLineStatus
{
    [Display(Name = "Sipariş Alındı")]
    Ordered = 1,
    [Display(Name = "Hazırlanıyor")]
    Preparing = 2,
    [Display(Name = "Hazır")]
    Ready = 3,
    [Display(Name = "Servis Edildi")]
    Served = 4,
    [Display(Name = "İptal")]
    Cancelled = 5
}
