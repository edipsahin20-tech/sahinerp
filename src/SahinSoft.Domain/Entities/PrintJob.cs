using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// Kuyruk/outbox deseni (IntegrationOutboxMessage + BranchSyncBackgroundService ile AYNI desen,
// AYRI tablo) - satış kapanışı/mutfak fişi/X/Z tetiklendiğinde SADECE bu satır eklenir (ucuz,
// yerel INSERT), asıl HTTP çağrısı PrintDispatchBackgroundService'te ayrı döngüde denenir.
// Yazıcı erişilemez olsa bile satış akışı ASLA bloklanmaz/geri alınmaz.
public sealed class PrintJob : EntityBase
{
    public int PrinterId { get; set; }
    public Printer Printer { get; set; } = null!;

    public int? PrintTemplateId { get; set; }
    public PrintTemplate? PrintTemplate { get; set; }

    // Web app tarafında ÖNCEDEN render edilmiş ESC/POS baytları (Base64) - agent SADECE gönderir,
    // render mantığı hiç agent'a taşınmaz.
    public string RenderedEscPosBase64 { get; set; } = string.Empty;

    public string SourceDescription { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public bool IsTestPrint { get; set; }
}
