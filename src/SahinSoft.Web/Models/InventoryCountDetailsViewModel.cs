using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class InventoryCountDetailsViewModel
{
    public int Id { get; set; }
    public InventoryCountStatus Status { get; set; }
    public string CountNumber { get; set; } = string.Empty;
    public DateTime CountDateUtc { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public IReadOnlyList<InventoryCountDetailsLineViewModel> Lines { get; set; } = [];
}

public sealed class InventoryCountDetailsLineViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal DifferenceQuantity => CountedQuantity - SystemQuantity;
}
