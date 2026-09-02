namespace SahinSoft.Web.Models;

public sealed class RestaurantFloorViewModel
{
    public List<RestaurantFloorSectionViewModel> Sections { get; set; } = [];
}

public sealed class RestaurantFloorSectionViewModel
{
    public string Name { get; set; } = string.Empty;
    public List<RestaurantFloorTableViewModel> Tables { get; set; } = [];
}

public sealed class RestaurantFloorTableViewModel
{
    public int TableId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsOccupied { get; set; }
    public int? SessionId { get; set; }
    public int? CheckId { get; set; }
    public int? GuestCount { get; set; }
    public DateTime? OpenedAtUtc { get; set; }
    public decimal RunningTotal { get; set; }
}
