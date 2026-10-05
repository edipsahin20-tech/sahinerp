using Microsoft.AspNetCore.Mvc.Rendering;

namespace SahinSoft.Web.Models;

public sealed record StockValuationRow(string StockCode, string Name, string Category, decimal Quantity, decimal AverageCost, decimal Value, decimal SaleNet, decimal SaleValue, decimal AverageCostInclTax = 0, decimal ValueInclTax = 0);

public sealed class StockValuationViewModel
{
    public List<StockValuationRow> Rows { get; set; } = [];
    public int? WarehouseId { get; set; }
    public int? CategoryId { get; set; }
    public bool OnlyInStock { get; set; } = true;
    public List<SelectListItem> Warehouses { get; set; } = [];
    public List<SelectListItem> Categories { get; set; } = [];
    public decimal TotalQuantity => Rows.Sum(x => x.Quantity);
    public decimal TotalValue => Rows.Sum(x => x.Value);
    public decimal TotalSaleValue => Rows.Sum(x => x.SaleValue);
    public decimal TotalValueInclTax => Rows.Sum(x => x.ValueInclTax);
}

public sealed record GrossProfitRow(string StockCode, string Name, decimal Quantity, decimal NetSales, decimal Cost)
{
    public decimal Profit => NetSales - Cost;
    public decimal Margin => NetSales == 0 ? 0 : Math.Round(Profit / NetSales * 100, 2);
}

public sealed class GrossProfitViewModel
{
    public List<GrossProfitRow> Rows { get; set; } = [];
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int? BranchId { get; set; }
    public bool IncludeRestaurant { get; set; } = true;
    public List<SelectListItem> Branches { get; set; } = [];
    public decimal TotalSales => Rows.Sum(x => x.NetSales);
    public decimal TotalCost => Rows.Sum(x => x.Cost);
    public decimal TotalProfit => TotalSales - TotalCost;
    public decimal TotalMargin => TotalSales == 0 ? 0 : Math.Round(TotalProfit / TotalSales * 100, 2);
}

public sealed class AgingRow
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal NotDue { get; set; }
    public decimal D1To30 { get; set; }
    public decimal D31To60 { get; set; }
    public decimal D61To90 { get; set; }
    public decimal D90Plus { get; set; }
    public decimal Total => NotDue + D1To30 + D31To60 + D61To90 + D90Plus;
}

public sealed class CustomerAgingViewModel
{
    public DateTime AsOf { get; set; }
    public List<AgingRow> Receivables { get; set; } = [];
    public List<AgingRow> Payables { get; set; } = [];
}

public sealed record SalesAnalysisRow(string Code, string Name, int DocumentCount, decimal Quantity, decimal NetSales, decimal TaxAmount);

public sealed class SalesAnalysisViewModel
{
    public List<SalesAnalysisRow> Rows { get; set; } = [];
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string By { get; set; } = "customer";
    public int? BranchId { get; set; }
    public List<SelectListItem> Branches { get; set; } = [];
    public decimal TotalNet => Rows.Sum(x => x.NetSales);
}
