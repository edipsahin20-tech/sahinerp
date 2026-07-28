using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class StockTransferDetailsViewModel
{
    public int Id { get; set; }
    public StockTransferStatus Status { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDateUtc { get; set; }
    public string FromWarehouseName { get; set; } = string.Empty;
    public string ToWarehouseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public IReadOnlyList<StockTransferDetailsLineViewModel> Lines { get; set; } = [];
}

public sealed class StockTransferDetailsLineViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
