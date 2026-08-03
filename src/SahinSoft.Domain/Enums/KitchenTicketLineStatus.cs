using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

// KitchenTicketLine kendi durumunu bağımsız tutar (RestaurantOrderLine'dan OKUMAZ) — aynı sipariş
// kalemi farklı istasyonlara veya tekrar mutfağa gönderilebildiği için her fiş satırının kendi
// yaşam döngüsü vardır. Bkz. CLEAN_ROOM_DEVELOPMENT.md §11 Karar 4.
public enum KitchenTicketLineStatus
{
    [Display(Name = "Gönderildi")]
    Sent = 1,
    [Display(Name = "Hazırlanıyor")]
    InProgress = 2,
    [Display(Name = "Hazır")]
    Ready = 3,
    [Display(Name = "Servis Edildi")]
    Served = 4,
    [Display(Name = "İptal")]
    Cancelled = 5
}
