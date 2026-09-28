using SahinSoft.Domain.Entities;

namespace SahinSoft.Web.Models;

// Z Listesi (Edip, 2026-09-28: "raporların altında z listesi aynı mantıkta ekle ordada z
// raprounu tukladıgında z yi gorsun cıktı alabilsin") - RestaurantZPeriod'un kapanışta
// donan (frozen) özet alanlarını doğrudan okur, Restoran modülünün ayrı X/Z raporlama
// mantığı BURADA TEKRARLANMAZ.
public sealed class ZPeriodListViewModel
{
    public List<ZPeriodListItemViewModel> Items { get; set; } = [];
    public List<Branch> Branches { get; set; } = [];

    public int? BranchId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int TotalCount { get; set; }
    public int PageSize { get; set; } = 50;
}

public sealed class ZPeriodListItemViewModel
{
    public int ZPeriodId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime OpenedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public bool ClosedAutomatically { get; set; }
    public int ReceiptCount { get; set; }
    public decimal NetTotal { get; set; }
}

public sealed class ZPeriodDetailViewModel
{
    public int ZPeriodId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime OpenedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public bool ClosedAutomatically { get; set; }
    public string? ClosedByUserName { get; set; }
    public int ReceiptCount { get; set; }
    public decimal GrossTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal NetTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal ComplimentaryTotal { get; set; }
    public decimal CashTotal { get; set; }
    public decimal CreditCardTotal { get; set; }
    public decimal MealCardTotal { get; set; }
    public decimal UnpaidTotal { get; set; }
    public decimal OpenAccountTotal { get; set; }

    // İptaller (Edip, 2026-09-28: "sadece iptalleri ciro hesabına dahil etme, o bilgi amaçlı" -
    // bilgi amaçlı ayrı satır, NetTotal'a hiç dahil değil, hiçbir zaman olmadı).
    public int LineCancellationCount { get; set; }
    public decimal LineCancellationTotal { get; set; }
    public int ReceiptCancellationCount { get; set; }
    public decimal ReceiptCancellationTotal { get; set; }

    public string? BranchAddress { get; set; }
    public string? BranchPhone { get; set; }
}
