namespace SahinSoft.Web.Models;

// Restoran POS shell'inin (topbar/sidebar/statusbar) her sayfada ihtiyaç duyduğu ortak veri -
// bkz. RestaurantControllerBase. Master tasarımdaki "Altınova Şubesi", "Vardiya Açık · 08:00",
// mutfak bekleyen sayacı, kullanıcı adı alanlarının karşılığı.
public sealed class RestaurantShellViewModel
{
    public string ActivePage { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public bool IsShiftOpen { get; set; }
    public DateTime? ShiftOpenedAtUtc { get; set; }
    public int KitchenPendingCount { get; set; }
    public int ActiveTableCount { get; set; }
    public int TotalTableCount { get; set; }
    public decimal OpenCheckTotal { get; set; }
}
