using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ProductCategory : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AlternateName { get; set; }
    public string? Unit { get; set; }
    public string? WebsitePath { get; set; }
    public bool IsActive { get; set; } = true;
    public string Color { get; set; } = "#6c757d";
    public int DisplayOrder { get; set; }

    // Kategori bazında iskonto/puan bilgisi - salt bilgi amaçlı alanlar, herhangi bir hesaplamayı
    // otomatik tetiklemez (Edip, 2026-09-03: "sadece alan olarak eklensin").
    public decimal DiscountPercentSale { get; set; }
    public decimal DiscountPercentPurchase { get; set; }
    // Puan iki şekilde de girilebilir - sabit/TL bazlı (LoyaltyPoints) ya da satış tutarının
    // yüzdesi (LoyaltyPointsPercent). İkisi birlikte var, biri diğerini geçersiz kılmaz.
    public int LoyaltyPoints { get; set; }
    public decimal LoyaltyPointsPercent { get; set; }
    public bool ShowInReceiptImage { get; set; }

    public int? ParentCategoryId { get; set; }
    public ProductCategory? ParentCategory { get; set; }
    public ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();

    // Kategori düzeyinde varsayılan - "Stoklara Uygula" ile bu kategorideki TÜM ürünlere
    // toplu olarak yazılır (bkz. CategoriesController.ApplyToProducts). Kategorinin kendisi
    // için zorunlu değil, sadece toplu güncelleme kaynağı.
    public int? TaxRateId { get; set; }
    public TaxRate? TaxRate { get; set; }
    public int? DefaultKitchenStationId { get; set; }
    public KitchenStation? DefaultKitchenStation { get; set; }
    public bool ShowAsShortcut { get; set; } = true;
    public bool ShowInMobile { get; set; } = true;
    public bool ShowInOnlineOrder { get; set; } = true;
    public bool VisibleInBranches { get; set; } = true;
    public bool DiscountNotApplicable { get; set; }
    public bool PromotionNotApplicable { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
