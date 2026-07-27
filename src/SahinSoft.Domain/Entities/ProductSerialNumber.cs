using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ProductSerialNumber : EntityBase
{
    public string SerialNumber { get; set; } = string.Empty;
    public string? LotNumber { get; set; }
    public DateTime? ExpirationDateUtc { get; set; }
    public bool IsInStock { get; set; } = true;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
}
