using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// Masa taşıma geçmişi — bir oturumun fiziksel olarak hangi masadan hangi masaya taşındığını izler.
// Faz 1'de yalnızca veri modeli hazırlanır; taşıma ekranı/akışı ikinci aşamada gelir
// (bkz. CLEAN_ROOM_DEVELOPMENT.md §11 Karar 5).
public sealed class RestaurantTableSessionMove : EntityBase
{
    public DateTime MovedAtUtc { get; set; } = DateTime.UtcNow;
    public string MovedByUserId { get; set; } = string.Empty;
    public string? Reason { get; set; }

    public int RestaurantTableSessionId { get; set; }
    public RestaurantTableSession RestaurantTableSession { get; set; } = null!;
    public int FromRestaurantTableId { get; set; }
    public RestaurantTable FromRestaurantTable { get; set; } = null!;
    public int ToRestaurantTableId { get; set; }
    public RestaurantTable ToRestaurantTable { get; set; } = null!;
}
