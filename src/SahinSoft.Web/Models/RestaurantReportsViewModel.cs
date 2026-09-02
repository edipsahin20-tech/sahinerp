namespace SahinSoft.Web.Models;

public sealed class RestaurantReportsViewModel
{
    public string ActiveTab { get; set; } = "daily";
    public string SourceFilter { get; set; } = "all";
    public DateOnly ReportDate { get; set; }

    // Günün özet KPI'ları (rapor tarihine göre, iptal hariç) - Dashboard'daki AYNI sorgu deseni.
    public decimal NetRevenue { get; set; }
    public int ReceiptCount { get; set; }
    public decimal AverageReceipt { get; set; }
    public decimal CancelRatePercent { get; set; }
    public decimal Cash { get; set; }
    public decimal Card { get; set; }
    public decimal MealCard { get; set; }

    // Vardiya/Z durumu (RestaurantCashShift'ten - yeni bir kapanış kavramı İCAT EDİLMEDİ).
    public bool IsShiftOpen { get; set; }
    public int? OpenShiftId { get; set; }
    public DateTime? ShiftOpenedAtUtc { get; set; }
    public string? FinancialAccountName { get; set; }
    public string? LastZNumber { get; set; }
    public DateTime? LastZClosedAtUtc { get; set; }

    // Günlük Fişler
    public List<RestaurantReceiptRowViewModel> Receipts { get; set; } = [];
    public int ListedCount { get; set; }
    public decimal ListedTotal { get; set; }
    public decimal ListedDiscount { get; set; }
    public int ListedCancelledCount { get; set; }

    // X Raporu - yalnızca açık vardiyada dolu, vardiyayı KAPATMAZ.
    public RestaurantXReportViewModel? XReport { get; set; }

    // Z Listesi - geçmiş kapanışlar.
    public List<RestaurantZListRowViewModel> ZList { get; set; } = [];

    // Fişi Gör modalı için.
    public int? SelectedReceiptId { get; set; }
    public RestaurantReceiptDetailViewModel? SelectedReceipt { get; set; }
}

public sealed record RestaurantReceiptRowViewModel(
    int RetailSaleId,
    DateTime IssuedAtUtc,
    string DocumentNumber,
    string SourceLabel,
    string SourceSubtitle,
    string SourceType,
    string PaymentSummary,
    bool IsCancelled,
    decimal GrandTotal);

public sealed class RestaurantXReportViewModel
{
    public DateTime OpenedAtUtc { get; set; }
    public int ReceiptCount { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal Cash { get; set; }
    public decimal Card { get; set; }
    public decimal MealCard { get; set; }
}

public sealed record RestaurantZListRowViewModel(
    string ZNumber,
    string FinancialAccountName,
    DateTime OpenedAtUtc,
    DateTime ClosedAtUtc,
    decimal OpeningBalance,
    decimal? ExpectedBalance,
    decimal? CountedBalance);

public sealed class RestaurantReceiptDetailViewModel
{
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public string SourceLabel { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public bool IsCancelled { get; set; }
    public List<RestaurantReceiptDetailLine> Lines { get; set; } = [];
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public List<RestaurantReceiptDetailPayment> Payments { get; set; } = [];
}

public sealed record RestaurantReceiptDetailLine(string ProductName, decimal Quantity, decimal LineTotal);
public sealed record RestaurantReceiptDetailPayment(string Method, decimal Amount);
