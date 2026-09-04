using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// Personel <-> Yetki Profili çoktan çoğa ilişkisi. ApplicationUser Domain katmanında değil (Web
// projesindeki Identity uzantısı), bu yüzden diğer restoran tablolarındaki desen izlenir
// (bkz. RestaurantCashShift.CashierUserId) - sadece string Id, navigation YOK.
public sealed class RestaurantPersonnelPermissionProfile : EntityBase
{
    public string UserId { get; set; } = string.Empty;

    public int PermissionProfileId { get; set; }
    public RestaurantPermissionProfile PermissionProfile { get; set; } = null!;
}
