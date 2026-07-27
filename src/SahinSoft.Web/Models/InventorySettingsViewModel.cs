using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class InventorySettingsViewModel
{
    [Display(Name = "Barkod zorunlu")]
    public bool RequireBarcode { get; set; }

    [Display(Name = "Boş barkodu otomatik üret")]
    public bool AutoGenerateBarcode { get; set; }

    [Required, Display(Name = "Varsayılan barkod tipi")]
    public string DefaultBarcodeType { get; set; } = "EAN13";

    [Required, Display(Name = "Varsayılan terazi ön eki")]
    public string DefaultScalePrefix { get; set; } = "27";

    [Display(Name = "Stok seviyesini kontrol et")]
    public bool EnforceStockLevel { get; set; }

    [Display(Name = "Negatif stoğa izin ver")]
    public bool AllowNegativeStock { get; set; }

    [Display(Name = "Stok yokken satışa izin ver")]
    public bool AllowSaleWhenOutOfStock { get; set; }

    [Display(Name = "Minimum stok uyarısı")]
    public bool EnableMinimumStockWarning { get; set; }

    [Display(Name = "Depo transferi onay gerektirsin")]
    public bool RequireTransferApproval { get; set; }

    [Display(Name = "Varyant bazlı stok takibi")]
    public bool TrackStockByVariant { get; set; }

    [Display(Name = "Varyantlı ürünlerde seçim zorunlu")]
    public bool RequireProductVariant { get; set; }

    [Display(Name = "Maliyet altı satışa izin ver")]
    public bool AllowSaleBelowCost { get; set; }
}
