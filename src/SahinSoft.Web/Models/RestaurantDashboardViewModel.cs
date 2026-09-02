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
}

public sealed class RestaurantDashboardMovement
{
    public DateTime AtUtc { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
