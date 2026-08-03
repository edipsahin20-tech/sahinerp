using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// Aynı adisyona farklı zamanlarda gönderilen her "sipariş turu" — örn. önce ana yemek,
// sonra tatlı ayrı bir RestaurantOrder olarak eklenir.
public sealed class RestaurantOrder : EntityBase
{
    public DateTime OrderedAtUtc { get; set; } = DateTime.UtcNow;
    public string OrderedByUserId { get; set; } = string.Empty;

    // Aynı "gönder" tıklamasının iki kez sipariş oluşturmaması için.
    public Guid? SubmissionKey { get; set; }

    public int RestaurantCheckId { get; set; }
    public RestaurantCheck RestaurantCheck { get; set; } = null!;

    public ICollection<RestaurantOrderLine> Lines { get; set; } = new List<RestaurantOrderLine>();
    public ICollection<KitchenTicket> KitchenTickets { get; set; } = new List<KitchenTicket>();
}
