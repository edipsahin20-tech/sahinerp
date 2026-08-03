using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class KitchenStation : EntityBase
{
    public string Name { get; set; } = string.Empty;

    // Gerçek yazıcı/ekran adı BURADA tutulur, sipariş satırında metin olarak yazılmaz.
    public string? PrinterName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public ICollection<KitchenTicket> Tickets { get; set; } = new List<KitchenTicket>();
}
