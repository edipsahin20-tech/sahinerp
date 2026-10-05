using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// Dönem kilidi (Şirket Parametreleri > "Kapalı dönem sonu"): o tarihe (dahil) kadar muhasebe evraklarında
// ekleme/değiştirme/onay/iptal/silme yapılamaz. Restoran satışları etkilenmez; kilit boşsa hiçbir şey değişmez.
public sealed class PeriodLockService(ApplicationDbContext db)
{
    public async Task<DateTime?> GetClosedUntilAsync(CancellationToken ct = default) =>
        await db.CompanySettings.AsNoTracking().Where(x => x.Id == 1).Select(x => x.ClosedPeriodUntil).SingleOrDefaultAsync(ct);

    // Kilitliyse kullanıcıya gösterilecek mesajı, değilse null döner.
    public async Task<string?> CheckAsync(DateTime documentDate, CancellationToken ct = default)
    {
        var until = await GetClosedUntilAsync(ct);
        if (until is null || documentDate.Date > until.Value.Date) { return null; }
        return $"Evrak tarihi ({documentDate:dd.MM.yyyy}) kapalı dönemde. {until.Value:dd.MM.yyyy} tarihine (dahil) kadar işlem yapılamaz; kilidi Şirket Parametreleri'nden değiştirebilirsiniz.";
    }

    public async Task EnsureOpenAsync(DateTime documentDate, CancellationToken ct = default)
    {
        var message = await CheckAsync(documentDate, ct);
        if (message is not null) { throw new InvalidOperationException(message); }
    }
}
