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
    [Range(0, 999999999999)]
    public decimal PurchasePrice { get; set; }

    [Display(Name = "Satış fiyatı")]
    [Range(0, 999999999999)]
    public decimal SalePrice { get; set; }

    [Display(Name = "Stok miktarı")]
    [Range(0, 999999999999)]
    public decimal StockQuantity { get; set; }

    [Display(Name = "Minimum stok")]
    [Range(0, 999999999999)]
    public decimal MinimumStockQuantity { get; set; }

    [Display(Name = "Stok takip edilsin")]
    public bool TrackStock { get; set; } = true;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    [Display(Name = "Görsel yolu")]
    public string? ImagePath { get; set; }

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

    [StringLength(100)]
    [Display(Name = "Yazıcı")]
    public string? KitchenPrinterName { get; set; }

    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> TaxRateOptions { get; set; } = [];
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> CategoryOptions { get; set; } = [];
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> UnitOfMeasureOptions { get; set; } = [];

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
