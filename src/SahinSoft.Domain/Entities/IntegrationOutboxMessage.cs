using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class IntegrationOutboxMessage : EntityBase
{
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
}
