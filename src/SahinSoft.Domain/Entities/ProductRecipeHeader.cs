using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// Reçetenin versiyonlu başlığı — reçete değişince eski satış geriye dönük bozulmaz; yeni bir
// versiyon açılır, eskisinin ValidToUtc'si doldurulur.
public sealed class ProductRecipeHeader : EntityBase
{
    public int Version { get; set; } = 1;
    public DateTime ValidFromUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ValidToUtc { get; set; }

    // Üretim miktarı (örn. "1 tabak").
    public decimal YieldQuantity { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Null = porsiyonsuz ürün.
    public int? ProductPortionId { get; set; }
    public ProductPortion? ProductPortion { get; set; }

    // Null = tüm şubeler.
    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    // Varsayılan hammadde düşüm deposu.
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public ICollection<ProductRecipeLine> Lines { get; set; } = new List<ProductRecipeLine>();
}
