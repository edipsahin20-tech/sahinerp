using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class RestaurantTable : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }

    // Salon planı görsel konumu — opsiyonel.
    public decimal? PosX { get; set; }
    public decimal? PosY { get; set; }
    public bool IsActive { get; set; } = true;

    public int RestaurantSectionId { get; set; }
    public RestaurantSection RestaurantSection { get; set; } = null!;

    // Kasıtlı olarak burada YOK: CurrentOrderId, CurrentSessionId, kalıcı Status.
    // Masanın dolu/boş durumu her zaman RestaurantTableSessions'ta Status=Open olan bir
    // kayıt var mı diye sorgulanarak hesaplanır.
    public ICollection<RestaurantTableSession> Sessions { get; set; } = new List<RestaurantTableSession>();
    public ICollection<RestaurantTableReservation> Reservations { get; set; } = new List<RestaurantTableReservation>();
}
