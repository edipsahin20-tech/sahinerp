namespace SahinSoft.Web.Models;

public sealed class RestaurantDashboardViewModel
{
    public decimal NetRevenueToday { get; set; }
    public int ClosedReceiptCountToday { get; set; }
    public decimal CashCollectedToday { get; set; }
    public decimal CreditCardCollectedToday { get; set; }
    public decimal MealCardCollectedToday { get; set; }
    public int KitchenPendingCount { get; set; }
    public int KitchenLongestWaitMinutes { get; set; }
    public List<RestaurantDashboardMovement> RecentMovements { get; set; } = [];
    public List<decimal> HourlyRevenue { get; set; } = [];
    public int HourlyRevenueStartHour { get; set; }

    // MASTER tasarımdaki "Canlı Operasyon Nabzı" + "Operasyon KPI'ları" + "Servis Sağlığı" için
    // eklenen gerçek metrikler (Edip, 2026-09-03: "herşeyi ekle olması gerektiği gibi yap") -
    // hiçbiri uydurma değil, hepsi mevcut zaman damgalarından hesaplanır. Masa doluluk/açık
    // adisyon toplamı ViewBag.Shell'de zaten var (RestaurantControllerBase), burada tekrarlanmadı.
    public int BillRequestedCount { get; set; }
    public int ActivePackageCount { get; set; }

    // Ortalama masa devir süresi (bugün kapanan adisyonlar, açılıştan kapanışa).
    public int? AvgTableTurnMinutes { get; set; }
    // Bekleyen mutfak kalemlerinin ortalama bekleme süresi (en uzun değil, ortalama).
    public int? AvgKitchenWaitMinutes { get; set; }
    // Bugün teslim edilen paketlerin ortalama süresi (sipariş oluşturulmasından teslime kadar).
    public int? AvgPackageDeliveryMinutes { get; set; }
    // Hesap istenmesinden adisyon kapanışına kadar geçen ortalama süre (bugün kapananlar).
    public int? AvgPaymentCompletionMinutes { get; set; }

    // Ayarlar > Stok Parametreleri'nde girilirse gösterilir, girilmezse bu bölüm hiç render
    // edilmez (bkz. InventorySettings.DailyRevenueTarget).
    public decimal? DailyRevenueTarget { get; set; }

    public List<RestaurantDashboardQueueItem> PriorityQueue { get; set; } = [];
}

public sealed class RestaurantDashboardMovement
{
    public DateTime AtUtc { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// MASTER tasarımdaki "Öncelik Kuyruğu" satırları - mutfak kritik sipariş / hesap isteyen masa /
// hazır paket, hepsi tek bir listede süreye göre sıralanır.
public sealed class RestaurantDashboardQueueItem
{
    public string Kind { get; set; } = string.Empty; // "kitchen" | "bill" | "package"
    public string TimeLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public bool IsUrgent { get; set; }
}
