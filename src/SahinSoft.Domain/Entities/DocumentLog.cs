using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// Evrak kayıt günlüğü (Mikro'daki "kayıt bilgisi" mantığı): hangi evrakı hangi kullanıcı ne zaman girdi/değiştirdi/onayladı/iptal etti/sildi.
// Evrak ekranlarında Ctrl+D ile açılır. İşlem zamanı CreatedAtUtc'dir. Evrak silinse bile kayıt kalır.
public sealed class DocumentLog : EntityBase
{
    public string EntityName { get; set; } = string.Empty;   // Invoice, PaymentReceipt, DispatchNote, BusinessOrder, Expense, Quote
    public int EntityId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;       // Created, Updated, Approved, Cancelled, Deleted
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}
