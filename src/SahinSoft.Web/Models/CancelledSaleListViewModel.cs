using SahinSoft.Domain.Entities;

namespace SahinSoft.Web.Models;

// Fiş İptal Listesi (Edip, 2026-09-28: "bide fiş iptal listesi diye bir rapor yap unude ekle") -
// RetailSale.Status=Cancelled olan kayıtları listeler, RetailSalesController'ın Perakende Fiş
// Listesi ile AYNI şube/tarih/kanal süzme mantığını kullanır ama ödeme türü yerine iptal
// bilgilerini (kim, ne zaman, neden) gösterir.
public sealed class CancelledSaleListViewModel
{
    public List<CancelledSaleListItemViewModel> Items { get; set; } = [];
    public List<Branch> Branches { get; set; } = [];

    public int? BranchId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Channel { get; set; }
    public int Page { get; set; } = 1;
    public int TotalCount { get; set; }
    public int PageSize { get; set; } = 50;
}

public sealed class CancelledSaleListItemViewModel
{
    public int RetailSaleId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string SourceLabel { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string? CustomerName { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancelledByUserName { get; set; }
    public string? CancellationReason { get; set; }
}
