using Microsoft.AspNetCore.Mvc.Rendering;

namespace SahinSoft.Web.Models;

public sealed class StockMovementReportViewModel
{
    public int? ProductId { get; set; }
    public int? WarehouseId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal NetChange { get; set; }
    public IReadOnlyList<StockMovementReportLineViewModel> Lines { get; set; } = [];
    public IReadOnlyList<SelectListItem> Products { get; set; } = [];
    public IReadOnlyList<SelectListItem> Warehouses { get; set; } = [];
}

public sealed class StockMovementReportLineViewModel
{
    public DateTime MovementDateUtc { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Description { get; set; }
}

public sealed class StockReconciliationReportViewModel
{
    public int CheckedProductCount { get; set; }
    public IReadOnlyList<StockReconciliationLineViewModel> Lines { get; set; } = [];
}

public sealed class StockReconciliationLineViewModel
{
    public int ProductId { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal RecordedQuantity { get; set; }
    public decimal MovementQuantity { get; set; }
    public decimal Difference { get; set; }
}

public sealed class LastPurchasePricesReportViewModel
{
    public int? ProductId { get; set; }
    public IReadOnlyList<LastPurchasePriceLineViewModel> Lines { get; set; } = [];
    public IReadOnlyList<SelectListItem> Products { get; set; } = [];
}

public sealed class LastPurchasePriceLineViewModel
{
    public int ProductId { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime LastPurchaseDateUtc { get; set; }
    public decimal UnitPriceInclTax { get; set; }
    public decimal Quantity { get; set; }
    public string? InvoiceNumber { get; set; }
    public int PurchaseCount { get; set; }
}
