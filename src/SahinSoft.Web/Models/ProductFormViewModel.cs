using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class ProductFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [StringLength(40)]
    [Display(Name = "Stok kodu")]
    public string StockCode { get; set; } = string.Empty;

    [Required, StringLength(200)]
    [Display(Name = "Stok adı")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    [Required]
    [Display(Name = "KDV")]
    public int TaxRateId { get; set; }

    [StringLength(100)]
    [Display(Name = "Marka")]
    public string? Brand { get; set; }

    [StringLength(100)]
    [Display(Name = "Model")]
    public string? Model { get; set; }

    [StringLength(50)]
    [Display(Name = "Barkod")]
    public string? Barcode { get; set; }

    [Required]
    [Display(Name = "Birim")]
    public int? UnitOfMeasureId { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Stok cinsi")]
    public string ProductType { get; set; } = "Donanım";

    [StringLength(200)]
    [Display(Name = "Ek Ad")]
    public string? AlternateName { get; set; }

    [Range(0, 3650)]
    [Display(Name = "Raf Ömrü (gün)")]
    public int? ShelfLifeDays { get; set; }

    [StringLength(100)]
    [Display(Name = "Ülke")]
    public string? CountryOfOrigin { get; set; }

    [Display(Name = "Seri No Zorunlu mu?")]
    public bool TrackSerialNumbers { get; set; }

    [Display(Name = "Alış fiyatı")]
    [Range(typeof(decimal), "0", "999999999999")]
    public decimal PurchasePrice { get; set; }

    [Display(Name = "Satış fiyatı")]
    [Range(typeof(decimal), "0", "999999999999")]
    public decimal SalePrice { get; set; }

    [Display(Name = "Stok miktarı")]
    [Range(typeof(decimal), "0", "999999999999")]
    public decimal StockQuantity { get; set; }

    [Display(Name = "Minimum stok")]
    [Range(typeof(decimal), "0", "999999999999")]
    public decimal MinimumStockQuantity { get; set; }

    [Display(Name = "Stok takip edilsin")]
    public bool TrackStock { get; set; } = true;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? ImagePath { get; set; }

    // Stok Tanıtım Kartı'ndan yüklenen fotoğraf - ImagePath'e kaydedilir, kendisi DB'de tutulmaz.
    [Display(Name = "Ürün Fotoğrafı")]
    public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }

    [Display(Name = "Fotoğrafı kaldır")]
    public bool RemoveImage { get; set; }

    [StringLength(1000)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    // Sadece Restoran Modülü açıkken Stok Tanıtım Kartı'nda gösterilir.
    [Display(Name = "Puan")]
    [Range(0, 999999)]
    public int LoyaltyPoints { get; set; }

    [Display(Name = "Kısayol Görünsün mü?")]
    public bool ShowAsShortcut { get; set; } = true;

    [Display(Name = "Mobilde Görünsün mü?")]
    public bool ShowInMobile { get; set; } = true;

    [Display(Name = "Online Siparişte Görünsün mü?")]
    public bool ShowInOnlineOrder { get; set; } = true;

    [Display(Name = "Şubelerde Görünsün mü?")]
    public bool VisibleInBranches { get; set; } = true;

    [Display(Name = "İndirim Uygulanmaz")]
    public bool DiscountNotApplicable { get; set; }

    [Display(Name = "Promosyon Uygulanmaz")]
    public bool PromotionNotApplicable { get; set; }

    [Display(Name = "Mutfak Yazıcı Adı")]
    public string? KitchenPrinterName { get; set; }

    [Display(Name = "Mutfak İstasyonu (Yazıcı)")]
    public int? DefaultKitchenStationId { get; set; }

    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> TaxRateOptions { get; set; } = [];
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> CategoryOptions { get; set; } = [];
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> UnitOfMeasureOptions { get; set; } = [];
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> KitchenStationOptions { get; set; } = [];
    public IEnumerable<string> BrandOptions { get; set; } = [];
    public IEnumerable<string> ModelOptions { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CategoryId <= 0)
        {
            yield return new ValidationResult("Kategori seçilmelidir.", [nameof(CategoryId)]);
        }

        if (TaxRateId <= 0)
        {
            yield return new ValidationResult("KDV seçilmelidir.", [nameof(TaxRateId)]);
        }

        if (UnitOfMeasureId is null or <= 0)
        {
            yield return new ValidationResult("Birim seçilmelidir.", [nameof(UnitOfMeasureId)]);
        }
    }
}
