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

    [Display(Name = "Günlük Ciro Hedefi")]
    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? DailyRevenueTarget { get; set; }

    [Display(Name = "Satış İçin Açık Vardiya Şart")]
    public bool RequireOpenShiftForSales { get; set; }

    [Display(Name = "Mutfak KDS Takibi")]
    public bool IsKitchenTrackingEnabled { get; set; }

    [Display(Name = "Mutfak Otomatik Hazır Süresi (dk)")]
    [Range(0, 1440)]
    public int? KitchenAutoReadyMinutes { get; set; }

    [Display(Name = "Sipariş İptalinde Gerekçe Sorulsun")]
    public bool RequireCancellationReason { get; set; }

    [Display(Name = "Hazır İptal Gerekçeleri (her satıra bir tane)")]
    public string? CancellationReasonPresets { get; set; }

    [Display(Name = "Hazır Ürün Notları (her satıra bir tane)")]
    public string? QuickNotePresets { get; set; }

    // "Şifre sorulsun mu?" (madde 21) - açık olan kritik işlem, yetkili kullanıcı tarafından
    // yapılsa BİLE ikinci bir yetkilinin PIN onayını ister (bkz. RestaurantPermissionService).
    [Display(Name = "Sipariş Satırı İptalinde İkinci Yetkili Onayı")]
    public bool RequireSecondApprovalForCancelOrderLine { get; set; }

    [Display(Name = "Fiş İptalinde İkinci Yetkili Onayı")]
    public bool RequireSecondApprovalForCancelReceipt { get; set; }

    [Display(Name = "İndirimde İkinci Yetkili Onayı")]
    public bool RequireSecondApprovalForDiscount { get; set; }

    [Display(Name = "İkramda İkinci Yetkili Onayı")]
    public bool RequireSecondApprovalForComplimentary { get; set; }

    [Display(Name = "Mutfağa Gönderilmiş Ürün Düzenlemede İkinci Yetkili Onayı")]
    public bool RequireSecondApprovalForEditKitchenSentLines { get; set; }

    [Display(Name = "Not Eklemede İkinci Yetkili Onayı")]
    public bool RequireSecondApprovalForAddNote { get; set; }

    [Display(Name = "Hızlı Ödeme Sonrası Fiş Sorulsun mu?")]
    public bool RequireReceiptPromptAfterQuickPay { get; set; }

    [Display(Name = "Ödenmez Ödeme Tipi Gösterilsin mi?")]
    public bool ShowUnpaidPaymentType { get; set; }

    // Sağ İşlem Menüsü (madde 22) - sistem geneli açık/kapalı katmanı.
    [Display(Name = "Fiş Notu")]
    public bool EnableTicketNoteButton { get; set; } = true;
    [Display(Name = "Masa Transfer")]
    public bool EnableTableTransferButton { get; set; } = true;
    [Display(Name = "Mutfağa Gönder")]
    public bool EnableSendToKitchenButton { get; set; } = true;
    [Display(Name = "Fiyat Gör")]
    public bool EnablePriceCheckButton { get; set; } = true;
    [Display(Name = "Klavye")]
    public bool EnableKeyboardButton { get; set; } = true;
    [Display(Name = "Fişi Beklet")]
    public bool EnableHoldReceiptButton { get; set; } = true;
    [Display(Name = "Bekleyen Fişler")]
    public bool EnableHeldReceiptsButton { get; set; } = true;
    [Display(Name = "Ürün Listesi")]
    public bool EnableProductListButton { get; set; } = true;
    [Display(Name = "Fiş İkram")]
    public bool EnableComplimentaryReceiptButton { get; set; } = true;
    [Display(Name = "Fiş Listesi")]
    public bool EnableReceiptListButton { get; set; } = true;
    [Display(Name = "Sipariş Sil")]
    public bool EnableClearOrderButton { get; set; } = true;
    [Display(Name = "Masa açılışında kişi sayısı sorulsun mu")]
    public bool AskGuestCountOnTableOpen { get; set; } = true;

    // Madde 26 (Ayarlar) - sol ana menüde modülün TAMAMININ görünürlüğü.
    [Display(Name = "Masa Satış görünsün mü")]
    public bool ShowTableSaleNav { get; set; } = true;
    [Display(Name = "Self Satış görünsün mü")]
    public bool ShowSelfSaleNav { get; set; } = true;
    [Display(Name = "Paket görünsün mü")]
    public bool ShowPackageNav { get; set; } = true;

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
