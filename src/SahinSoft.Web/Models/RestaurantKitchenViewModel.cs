using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class RestaurantKitchenViewModel
{
    public IReadOnlyList<RestaurantKitchenStationFilter> Stations { get; set; } = [];
    public int? SelectedStationId { get; set; }
    public IReadOnlyList<RestaurantKitchenTicketViewModel> Tickets { get; set; } = [];
}

public sealed record RestaurantKitchenStationFilter(int Id, string Name, int PendingCount);

public sealed class RestaurantKitchenTicketViewModel
{
    public int TicketId { get; set; }
    public string? TicketNumber { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string CheckNumber { get; set; } = string.Empty;
    public KitchenTicketStatus Status { get; set; }
    public DateTime SentAtUtc { get; set; }
    public IReadOnlyList<RestaurantKitchenTicketLineViewModel> Lines { get; set; } = [];
}

public sealed record RestaurantKitchenTicketLineViewModel(
    string ProductName,
    string? PortionName,
    decimal Quantity,
    string? KitchenNote,
    IReadOnlyList<string> Modifiers,
    bool IsCancelled);
