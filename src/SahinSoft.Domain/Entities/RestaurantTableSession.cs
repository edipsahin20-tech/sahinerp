using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class RestaurantTableSession : EntityBase
{
    public RestaurantTableSessionStatus Status { get; set; } = RestaurantTableSessionStatus.Open;
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public string OpenedByUserId { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public string? WaiterUserId { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public string? ClosedByUserId { get; set; }

    // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
    public Guid? SubmissionKey { get; set; }

    public int RestaurantTableId { get; set; }
    public RestaurantTable RestaurantTable { get; set; } = null!;

    // Masa birleştirme — bu oturum başka bir oturuma birleştirildiyse hedef oturumu gösterir.
    // Faz 1'de yalnızca veri modeli hazırlanır; birleştirme ekranı/akışı ikinci aşamada gelir
    // (bkz. CLEAN_ROOM_DEVELOPMENT.md §11 Karar 5).
    public int? MergedIntoSessionId { get; set; }
    public RestaurantTableSession? MergedIntoSession { get; set; }

    // 1 masa oturumu → N adisyon (bkz. CLEAN_ROOM_DEVELOPMENT.md §11 Karar 2). İlk kullanımda
    // yalnızca tek adisyon açılır, ama altyapı bölme/ayrı hesap senaryolarını destekler.
    public ICollection<RestaurantCheck> Checks { get; set; } = new List<RestaurantCheck>();
    public ICollection<RestaurantTableSessionMove> Moves { get; set; } = new List<RestaurantTableSessionMove>();
}
