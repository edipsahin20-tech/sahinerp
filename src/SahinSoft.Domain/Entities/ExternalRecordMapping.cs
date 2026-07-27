using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ExternalRecordMapping : EntityBase
{
    public string SourceSystem { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string InternalId { get; set; } = string.Empty;
    public string? ExternalCode { get; set; }
    public DateTime LastSynchronizedAtUtc { get; set; }
    public string? ContentHash { get; set; }
}
