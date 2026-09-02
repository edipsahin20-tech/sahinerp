using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Kategori kodu")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Kategori adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Ek Ad")]
    public string? AlternateName { get; set; }

    [StringLength(30)]
    [Display(Name = "Birim")]
    public string? Unit { get; set; }

    [StringLength(250)]
    [Display(Name = "Web sitesi yolu")]
    public string? WebsitePath { get; set; }

    [Required, StringLength(7)]
    [Display(Name = "Renk")]
    public string Color { get; set; } = "#6c757d";

    [Display(Name = "Kategori Sırası")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Ana Kategori")]
    public int? ParentCategoryId { get; set; }

    [Display(Name = "Aktif Mi?")]
    public bool IsActive { get; set; } = true;

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

    [Display(Name = "Varsayılan KDV")]
    public int? TaxRateId { get; set; }

    [Display(Name = "Varsayılan Mutfak İstasyonu (Yazıcı)")]
    public int? DefaultKitchenStationId { get; set; }

    [Range(0, 100)]
    [Display(Name = "İskonto Satış %")]
    public decimal DiscountPercentSale { get; set; }

    [Range(0, 100)]
    [Display(Name = "İskonto Alış %")]
    public decimal DiscountPercentPurchase { get; set; }

    [Display(Name = "Puan")]
    public int LoyaltyPoints { get; set; }

    [Range(0, 100)]
    [Display(Name = "Puan Yüzde %")]
    public decimal LoyaltyPointsPercent { get; set; }

    [Display(Name = "Resim Posta Görünsün mü?")]
    public bool ShowInReceiptImage { get; set; }

    public List<(int Id, string Name)> TaxRateOptions { get; set; } = [];
    public List<(int Id, string Name)> KitchenStationOptions { get; set; } = [];
    public List<(int Id, string Name)> ParentCategoryOptions { get; set; } = [];
}
