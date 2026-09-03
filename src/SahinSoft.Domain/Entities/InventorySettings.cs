using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class InventorySettings : EntityBase
{
    public bool RequireBarcode { get; set; } = true;
    public bool AutoGenerateBarcode { get; set; } = true;
    public string DefaultBarcodeType { get; set; } = "EAN13";
    public string DefaultScalePrefix { get; set; } = "27";
    public bool EnforceStockLevel { get; set; } = true;
    public bool AllowNegativeStock { get; set; }
    public bool AllowSaleWhenOutOfStock { get; set; }
    public bool EnableMinimumStockWarning { get; set; } = true;
    public bool RequireTransferApproval { get; set; } = true;
    public bool TrackStockByVariant { get; set; }
    public bool RequireProductVariant { get; set; }
    public bool AllowSaleBelowCost { get; set; }
    public bool IsRestaurantModuleEnabled { get; set; }

    // Restoran Dashboard'daki "Günün Hedefi" halkası için - boş/0 ise o bölüm hiç gösterilmez
    // (Edip, 2026-09-03: MASTER_SahinSoft_Restoran_POS_Premium.html referansı, "hedef" uydurma
    // bir sayı değil, gerçek ayarlanabilir bir değer).
    public decimal? DailyRevenueTarget { get; set; }

    // Mutfağa gönderilen bir sipariş, bu süre (dk) dolduğunda mutfak personeli hiç dokunmasa
    // bile otomatik "Hazır" durumuna geçer (Edip, 2026-09-03) - boş/0 ise otomatik geçiş kapalı,
    // her şey elle ilerletilir (bugünkü davranış). Bkz. KitchenAutoReadyBackgroundService.
    public int? KitchenAutoReadyMinutes { get; set; }

    // Kapalıyken (varsayılan - Edip, 2026-09-03: "default yapılmasın") mutfağa gönderilen
    // siparişler hiç KitchenTicket/KDS takibine girmez, doğrudan Servis Edildi sayılır - Mutfak
    // ekranında Hazır/Servis Edildi gibi tıklamalara gerek kalmaz. Açıkken bugünkü KDS akışı
    // (Sent→InProgress→Ready→Served) aynen çalışır. Bkz. RestaurantPostingService.
    // SendOrderToKitchenCoreAsync.
    public bool IsKitchenTrackingEnabled { get; set; }

    // Kapalıyken (varsayılan - Edip, 2026-09-03: "kapalı default olsun") yeni masa/self satış/
    // paket siparişi açmak için açık bir vardiya ŞART DEĞİL, bugünkü gibi çalışır. Açıkken hiç
    // açık RestaurantCashShift yoksa yeni satış başlatılamaz - bkz. RestaurantPostingService.
    // EnsureShiftOpenIfRequiredAsync. Vardiya'nın kendi FinancialAccountId bazlı kapsam hatası
    // AYRI ve HENÜZ DÜZELTİLMEDİ (Edip'in deyimiyle "sonra yapacağız") - bu parametre sadece
    // "satış başlatmak için açık vardiya şart mı" sorusuna cevap verir, o hatayı çözmez.
    public bool RequireOpenShiftForSales { get; set; }

    // Yok (None) iken restoran Kapat/Öde bugünkü gibi hiçbir fiskal cihaz çağrısı yapmadan
    // çalışır - bkz. SahinSoft.FiscalAgent projesi. Bir cihaz seçilip adres girildiğinde nakit/
    // kredi kartı/yemek çeki ödemeleri doğrudan yazarkasaya gönderilir (fatura kesilen satışlar
    // hariç - bkz. RestaurantController.ClosePayment).
    public FiscalDeviceType FiscalDeviceType { get; set; } = FiscalDeviceType.None;
    public string? FiscalAgentUrl { get; set; }

    public bool OrderToDispatchPurchaseAutoApprove { get; set; }
    public bool OrderToDispatchSalesAutoApprove { get; set; }
    public bool OrderToInvoicePurchaseAutoApprove { get; set; }
    public bool OrderToInvoiceSalesAutoApprove { get; set; }
    public bool DispatchToInvoicePurchaseAutoApprove { get; set; }
    public bool DispatchToInvoiceSalesAutoApprove { get; set; }
}
