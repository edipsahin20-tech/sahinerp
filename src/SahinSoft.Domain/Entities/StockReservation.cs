using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class StockReservation : EntityBase
{
    public decimal Quantity { get; set; }
    public DateTime ReservedUntilUtc { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int? QuoteLineId { get; set; }
    public QuoteLine? QuoteLine { get; set; }
}
