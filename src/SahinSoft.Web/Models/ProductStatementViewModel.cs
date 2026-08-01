namespace SahinSoft.Web.Models;

public sealed class ProductStatementViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public IReadOnlyList<ProductStatementLineViewModel> Lines { get; set; } = [];
    public IReadOnlyList<ProductPriceHistoryLineViewModel> PurchasePriceHistory { get; set; } = [];
    public IReadOnlyList<ProductPriceHistoryLineViewModel> SalePriceHistory { get; set; } = [];
    public IReadOnlyList<ProductWarehouseQuantityViewModel> WarehouseQuantities { get; set; } = [];
}

public sealed class ProductPriceHistoryLineViewModel
{
    public DateTime InvoiceDateUtc { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPriceInclTax { get; set; }
}

public sealed class ProductWarehouseQuantityViewModel
{
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public sealed class ProductStatementLineViewModel
{
    public DateTime MovementDateUtc { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string? CustomerName { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal RunningBalance { get; set; }
}
