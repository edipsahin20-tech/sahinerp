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

    // MASTER tasarımdaki HESAP İSTENDİ rozeti - dolu masa için.
    public bool BillRequested { get; set; }

    // MASTER tasarımdaki REZERVE rozeti - boş masa için.
    public bool IsReserved { get; set; }
    public int? ReservationId { get; set; }
    public DateTime? ReservedForUtc { get; set; }
    public int? ReservationGuestCount { get; set; }
    public string? ReservationNote { get; set; }
}
