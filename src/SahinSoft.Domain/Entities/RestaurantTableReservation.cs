using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// MASTER tasarımdaki "REZERVE" masa rozeti (Edip, 2026-09-03: "roketlerde/rozetlerde olsun").
// Bilinçli olarak minimal: masa başına aynı anda en fazla bir AKTİF rezervasyon olur, masa
// açıldığında (OpenTableSessionAsync) otomatik olarak IsActive=false yapılır - ayrı bir "hatırlat/
// no-show" akışı yok, sadece "bu masa şu an rezerve" sinyali.
public sealed class RestaurantTableReservation : EntityBase
{
    public int RestaurantTableId { get; set; }
    public RestaurantTable RestaurantTable { get; set; } = null!;

    public DateTime ReservedForUtc { get; set; }
    public int GuestCount { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; } = true;

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? CancelledAtUtc { get; set; }
}
