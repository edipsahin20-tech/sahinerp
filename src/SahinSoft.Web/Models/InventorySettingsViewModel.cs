using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

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

    [Display(Name = "Restoran Modülü aktif")]
    public bool IsRestaurantModuleEnabled { get; set; }

    [Display(Name = "Yazar Kasa")]
    public FiscalDeviceType FiscalDeviceType { get; set; }

    [Display(Name = "Fiscal Agent Adresi")]
    public string? FiscalAgentUrl { get; set; }

    [Display(Name = "Alış Siparişi → İrsaliye otomatik onay")]
    public bool OrderToDispatchPurchaseAutoApprove { get; set; }

    [Display(Name = "Satış Siparişi → İrsaliye otomatik onay")]
    public bool OrderToDispatchSalesAutoApprove { get; set; }

    [Display(Name = "Alış Siparişi → Fatura otomatik onay")]
    public bool OrderToInvoicePurchaseAutoApprove { get; set; }

    [Display(Name = "Satış Siparişi → Fatura otomatik onay")]
    public bool OrderToInvoiceSalesAutoApprove { get; set; }

    [Display(Name = "Alış İrsaliyesi → Fatura otomatik onay")]
    public bool DispatchToInvoicePurchaseAutoApprove { get; set; }

    [Display(Name = "Satış İrsaliyesi → Fatura otomatik onay")]
    public bool DispatchToInvoiceSalesAutoApprove { get; set; }
}
