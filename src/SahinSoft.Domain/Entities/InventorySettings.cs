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
