using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class StockSlipDetailsViewModel
{
    public int Id { get; set; }
    public StockSlipType SlipType { get; set; }
    public StockSlipStatus Status { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public DateTime SlipDateUtc { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public IReadOnlyList<StockSlipDetailsLineViewModel> Lines { get; set; } = [];
}

public sealed class StockSlipDetailsLineViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Description { get; set; }
}
