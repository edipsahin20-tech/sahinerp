using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// Her oturum açmanın (PIN veya e-posta) sunucu tarafındaki kaydı. Çerezdeki "ss_sid" anahtarı bu kayda
// bağlanır. Çıkışta YALNIZCA kendi kaydı iptal edilir; aynı kullanıcının diğer terminal oturumları açık kalır.
public sealed class LoginSession : EntityBase
{
    public string SessionKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAtUtc { get; set; }
}
