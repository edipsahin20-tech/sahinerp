using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class KitchenTicket : EntityBase
{
    // Ekran/yazıcı gösterimi.
    public string? TicketNumber { get; set; }
    public KitchenTicketStatus Status { get; set; } = KitchenTicketStatus.Sent;
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? SubmissionKey { get; set; }

    public int RestaurantOrderId { get; set; }
    public RestaurantOrder RestaurantOrder { get; set; } = null!;
    public int KitchenStationId { get; set; }
    public KitchenStation KitchenStation { get; set; } = null!;

    public ICollection<KitchenTicketLine> Lines { get; set; } = new List<KitchenTicketLine>();
}
