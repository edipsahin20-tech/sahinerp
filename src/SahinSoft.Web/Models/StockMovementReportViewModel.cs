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

public sealed class FinancialTransactionReportViewModel
{
    public int? FinancialAccountId { get; set; }
    public string? AccountType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string PageTitle { get; set; } = "Kasa/Banka Hareketleri";
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal NetChange { get; set; }
    public IReadOnlyList<FinancialTransactionReportLineViewModel> Lines { get; set; } = [];
    public IReadOnlyList<SelectListItem> FinancialAccounts { get; set; } = [];
}

public sealed class FinancialTransactionReportLineViewModel
{
    public DateTime TransactionDateUtc { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public bool IsIncoming { get; set; }
    public decimal Amount { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CustomerName { get; set; }
}
