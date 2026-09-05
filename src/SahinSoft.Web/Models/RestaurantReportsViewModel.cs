namespace SahinSoft.Web.Models;

public sealed class RestaurantReportsViewModel
{
    public string ActiveTab { get; set; } = "daily";
    public string SourceFilter { get; set; } = "all";
    public DateOnly ReportDate { get; set; }

    // Ortak dönem filtresi (madde 28, 2026-09-05) - "Günlük | Haftalık | Aylık | Yıllık". Yalnızca
    // "daily" (Fiş Hareketleri) ve Kasiyer Raporu sekmelerinde geçerli - spec'in kendisi X/Z/Z
    // Listesi'ni HARİÇ tutuyor ("Kasiyer Raporu, X Raporu, Z Raporu ve Z Listesi hariç"). Seçilen
    // döneme göre kartlar/grafik/tablo/fiş hareketleri BİRLİKTE güncellenir.
    public string PeriodFilter { get; set; } = "day";
    public string PeriodRangeLabel { get; set; } = string.Empty;

    // Günün özet KPI'ları (seçilen döneme göre, iptal hariç) - Dashboard'daki AYNI sorgu deseni.
    public decimal NetRevenue { get; set; }
    public int ReceiptCount { get; set; }
    public decimal AverageReceipt { get; set; }
    public decimal CancelRatePercent { get; set; }
    // Ödeme dağılımı (madde 30, 2026-09-05) - artık Nakit/Kredi Kartı/Yemek Çeki'ye HARD-CODE
    // değil, o dönemde GERÇEKTEN var olan tüm ödeme türlerinden (Ödenmez/Açık Hesap dahil) dinamik
    // oluşuyor - AvailablePaymentFilters ile AYNI desen.
    public List<RestaurantPaymentBreakdownItemViewModel> PaymentBreakdown { get; set; } = [];
    public decimal PaymentBreakdownTotal { get; set; }
    public string PaymentBreakdownConicGradient { get; set; } = string.Empty;

    // Saatlik/günlük/aylık ciro akışı (seçilen dönem için, iptal hariç) - dönem büyüdükçe
    // granülerlik kabalaşır (gün→saatlik, hafta/ay→günlük, yıl→aylık) - Dashboard'daki Yoğunluk
    // Haritası ile AYNI hesap deseni, sadece eksen birimi değişken.
    public List<decimal> HourlyRevenue { get; set; } = [];
    public int HourlyRevenueStartHour { get; set; }
    public List<string> HourlyRevenueLabels { get; set; } = [];

    // Vardiya/Z durumu (RestaurantCashShift'ten - yeni bir kapanış kavramı İCAT EDİLMEDİ).
    public bool IsShiftOpen { get; set; }
    public int? OpenShiftId { get; set; }
    public DateTime? ShiftOpenedAtUtc { get; set; }
    public string? FinancialAccountName { get; set; }
    public string? LastZNumber { get; set; }
    public DateTime? LastZClosedAtUtc { get; set; }

    // Günlük Fişler (madde 19, Edip 2026-09-04: Excel-vari filtrelenebilir liste)
    public List<RestaurantReceiptRowViewModel> Receipts { get; set; } = [];
    public int ListedCount { get; set; }
    public decimal ListedTotal { get; set; }
    public decimal ListedDiscount { get; set; }
    public int ListedCancelledCount { get; set; }
    public string? PaymentFilter { get; set; }
    public string? StatusFilter { get; set; }
    public string? SearchTerm { get; set; }
    // Sadece o gün/filtrede GERÇEKTEN var olan ödeme türleri - hard-code liste DEĞİL.
    public List<RestaurantPaymentFilterOptionViewModel> AvailablePaymentFilters { get; set; } = [];
    // Filtrelenmiş sonuçların ödeme türü bazında alt toplamları (Nakit/Kredi Kartı/Yemek Kartı/
    // Ödenmez/diğer tahsilat carileri) - sadece gerçekten mevcut olanlar listelenir.
    public List<RestaurantPaymentSubtotalViewModel> PaymentSubtotals { get; set; } = [];

    // X Raporu - yalnızca açık vardiyada dolu, vardiyayı KAPATMAZ.
    public RestaurantXReportViewModel? XReport { get; set; }

    // Z Listesi - geçmiş kapanışlar.
    public List<RestaurantZListRowViewModel> ZList { get; set; } = [];

    // Z Listesi'nde bir Z'ye tıklanınca o vardiyanın (Açılış→Kapanış) satış hareketleri - Edip'in
    // isteği (2026-09-03): "z listesine girer o günkü z tıklar ve görmek istediği z nın günlük
    // satış hareketleri gelir". Düzenleme/silme henüz yok (reversal muhasebe kuralına aykırı
    // olur - bkz. [[feedback_sahinsoft_conventions]] - Edip'ten ayrıca netleştirme bekleniyor).
    public int? SelectedZShiftId { get; set; }
    public string? SelectedZNumber { get; set; }
    public List<RestaurantReceiptRowViewModel> SelectedZReceipts { get; set; } = [];

    // Fişi Gör modalı için.
    public int? SelectedReceiptId { get; set; }
    public RestaurantReceiptDetailViewModel? SelectedReceipt { get; set; }

    // Kasiyer Raporu (madde 31, 2026-09-05) - "normal kasiyer yalnızca kendi işlemlerini
    // görmelidir ... yetkili yönetici ise başka kasiyerleri veya tüm kasiyerleri seçebilmelidir".
    // Ortak dönem filtresi bu sekmede de geçerli (PeriodFilter/ReportDate).
    public bool CanPickAnyKasiyer { get; set; }
    public List<RestaurantKasiyerOptionViewModel> KasiyerOptions { get; set; } = [];
    public string? SelectedKasiyerUserId { get; set; }
    public RestaurantKasiyerReportViewModel Kasiyer { get; set; } = new();

    // Onaylı Restoran Raporları mockup'ının 1. satırındaki 5 rapor (madde birebir-uygulama,
    // 2026-09-05) - hepsi ortak dönem filtresini (PeriodFilter/ReportDate) kullanır.
    public List<RestaurantBestSellerRowViewModel> BestSellers { get; set; } = [];
    public List<RestaurantCategorySalesRowViewModel> CategorySales { get; set; } = [];
    public List<RestaurantVatRowViewModel> VatBreakdown { get; set; } = [];
    public decimal DiscountTotal { get; set; }
    public decimal ComplimentaryTotal { get; set; }
    public List<RestaurantDiscountComplimentaryRowViewModel> DiscountComplimentaryRows { get; set; } = [];
}

public sealed record RestaurantBestSellerRowViewModel(string ProductName, decimal Quantity, decimal Total);
public sealed record RestaurantCategorySalesRowViewModel(string CategoryName, decimal Quantity, decimal Total, decimal Percent);
public sealed record RestaurantVatRowViewModel(decimal TaxRate, decimal Matrah, decimal VatAmount, decimal Gross);
public sealed record RestaurantDiscountComplimentaryRowViewModel(DateTime IssuedAtUtc, string DocumentNumber, string SourceLabel, bool IsComplimentary, string? Reason, decimal Amount);

public sealed record RestaurantPaymentBreakdownItemViewModel(string Label, decimal Amount, decimal Percent, string ColorVar);

public sealed record RestaurantKasiyerOptionViewModel(string UserId, string FullName);

public sealed class RestaurantKasiyerReportViewModel
{
    public string DisplayName { get; set; } = string.Empty;
    public decimal NetRevenue { get; set; }
    public int ReceiptCount { get; set; }
    public decimal Cash { get; set; }
    public decimal Card { get; set; }
    public decimal OtherCollections { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ComplimentaryAmount { get; set; }
    public int CancelledCount { get; set; }
}

public sealed record RestaurantReceiptRowViewModel(
    int RetailSaleId,
    DateTime IssuedAtUtc,
    string DocumentNumber,
    string SourceLabel,
    string SourceSubtitle,
    string SourceType,
    string PaymentSummary,
    // "cash"/"creditcard"/"mealcard"/"unpaid"/"openaccount"/"mixed"/"none" - filtreleme için.
    string PaymentFilterKey,
    bool IsCancelled,
    decimal GrandTotal);

public sealed record RestaurantPaymentFilterOptionViewModel(string Value, string Label);
public sealed record RestaurantPaymentSubtotalViewModel(string Label, decimal Total);

public sealed class RestaurantXReportViewModel
{
    public DateTime OpenedAtUtc { get; set; }
    public int ReceiptCount { get; set; }
    public decimal NetRevenue { get; set; }
    // Madde 30/32 - "Nakit, kart ve bütün diğer tahsilatlar" - hard-code 3 yöntem DEĞİL.
    public List<RestaurantPaymentBreakdownItemViewModel> PaymentBreakdown { get; set; } = [];
}

public sealed record RestaurantZListRowViewModel(
    int ShiftId,
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
