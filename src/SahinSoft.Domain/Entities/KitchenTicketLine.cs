using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class KitchenTicketLine : EntityBase
{
    // Kendi bağımsız mutfak durumu — RestaurantOrderLine.Status'tan OKUNMAZ, çünkü aynı sipariş
    // kalemi farklı istasyonlara veya tekrar mutfağa gönderilebilir. RestaurantOrderLine'ın genel
    // hazırlık durumu BUNDAN hesaplanır (ters yön) — bkz. §11 Karar 4.
    public KitchenTicketLineStatus Status { get; set; } = KitchenTicketLineStatus.Sent;

    public int KitchenTicketId { get; set; }
    public KitchenTicket KitchenTicket { get; set; } = null!;
    public int RestaurantOrderLineId { get; set; }
    public RestaurantOrderLine RestaurantOrderLine { get; set; } = null!;
}
