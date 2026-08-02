using SahinSoft.Domain.Common;

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

    public bool OrderToDispatchPurchaseAutoApprove { get; set; }
    public bool OrderToDispatchSalesAutoApprove { get; set; }
    public bool OrderToInvoicePurchaseAutoApprove { get; set; }
    public bool OrderToInvoiceSalesAutoApprove { get; set; }
    public bool DispatchToInvoicePurchaseAutoApprove { get; set; }
    public bool DispatchToInvoiceSalesAutoApprove { get; set; }
}
