using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Common;

public abstract class EntityBase
{
    public int Id { get; set; }
    public Guid RecordId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
