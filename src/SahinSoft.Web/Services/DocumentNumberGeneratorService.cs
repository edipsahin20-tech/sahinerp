using System.Data;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// Yoğun eşzamanlı yük altında (ör. çok sayıda kullanıcının aynı anda aynı belge türünü kaydetmesi)
// ExecuteWithConcurrencyRetryAsync'in tüm tekrar deneme haklarını tüketip vazgeçtiğini işaret eder.
// Controller'lar bunu özel olarak yakalayıp kullanıcıya "tekrar deneyin" mesajı gösterip
// (araç çubuğunu korumuş şekilde) formu yeniden gösterebilir — ham DbUpdateConcurrencyException
// veya 500 hatası kullanıcıya asla sızmamalı.
public sealed class ConcurrencyRetryExhaustedException()
    : Exception("İşlem yoğun eşzamanlı istek nedeniyle tamamlanamadı, lütfen tekrar deneyin.");

public sealed class DocumentNumberGeneratorService(ApplicationDbContext dbContext)
{
    public Task<string> GenerateAsync(string sequenceKey, CancellationToken cancellationToken = default) =>
        ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
        {
            // EnableRetryOnFailure() (Program.cs) sets a retrying execution strategy; elle açılan
            // transaction'lar bununla uyumlu değil, tüm bloğun CreateExecutionStrategy() üzerinden
            // "tekrar denenebilir birim" olarak sarılması gerekiyor (aksi halde InvalidOperationException).
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var documentNumber = await GenerateWithinTransactionAsync(sequenceKey, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return documentNumber;
            });
        }, cancellationToken);

    // NumberSequence (EntityBase'ten gelen RowVersion aracılığıyla) iyimser eşzamanlılık kontrollü.
    // Serializable transaction izolasyonu, iki eşzamanlı isteğin aynı sayacı okuyup her ikisinin de
    // güncelleme denemesi durumunda birini SqlException (transient, EnableRetryOnFailure zaten
    // yakalar) yerine DbUpdateConcurrencyException ile başarısız kılabiliyor — bu EF Core'un
    // varsayılan "transient hata" listesinde OLMADIĞI için EnableRetryOnFailure bunu tekrar
    // denemiyor. Bu yüzden ayrı bir dış tekrar deneme katmanı gerekiyor: kaybeden istek DbContext'in
    // eski takip durumunu temizleyip sıfırdan (yeni transaction, taze okuma) tekrar dener. Yoğun
    // eşzamanlı kayıt denemesinde (örn. iki kullanıcının aynı anda fatura kaydetmesi) numaranın
    // rastgele bir hatayla başarısız olmaması için gerekli — canlıda 8 eşzamanlı istekle doğrulandı.
    internal static async Task<T> ExecuteWithConcurrencyRetryAsync<T>(
        ApplicationDbContext dbContext,
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default,
        int maxAttempts = 5)
    {
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Son deneme de çakışırsa buradaki "when (attempt < maxAttempts)" olmadan hepsini
                // yakalamak şart: aksi halde son denemenin ham DbUpdateConcurrencyException'ı
                // döngünün sonundaki dostane hataya hiç uğramadan doğrudan çağırana (ve oradan da
                // muhtemelen kullanıcıya çirkin bir 500 olarak) sızıyordu — bu satırın altındaki
                // throw hiçbir zaman çalışmıyordu (ölü kod). Şimdi son denemede de ChangeTracker
                // temizlenip aynı dostane mesajla ConcurrencyRetryExhaustedException fırlatılıyor.
                foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
                {
                    entry.State = EntityState.Detached;
                }

                if (attempt == maxAttempts)
                {
                    throw new ConcurrencyRetryExhaustedException();
                }

                await Task.Delay(Random.Shared.Next(10, 50) * attempt, cancellationToken);
            }
        }

        throw new ConcurrencyRetryExhaustedException();
    }

    // GenerateAsync'in transaction/SaveChanges yönetmeyen çekirdeği — zaten açık bir transaction
    // içinde çalışan çağıranlar için (ör. InvoicePostingService.PostClosedInvoiceSettlementAsync,
    // Onayla'nın kendi Serializable transaction'ı içinden çağırır). Aynı bağlantıda ikinci bir
    // BeginTransactionAsync açmaya çalışmak EF Core'da hataya sebep olur, bu yüzden bu çekirdek
    // sayaç güncellemesini yapar ama SaveChanges/commit'i çağırana bırakır.
    internal async Task<string> GenerateWithinTransactionAsync(string sequenceKey, CancellationToken cancellationToken = default)
    {
        var sequence = await dbContext.NumberSequences
            .SingleAsync(x => x.Key == sequenceKey, cancellationToken);

        var documentNumber = $"{sequence.Prefix}{sequence.NextNumber.ToString($"D{sequence.Padding}")}";
        sequence.NextNumber++;
        sequence.UpdatedAtUtc = DateTime.UtcNow;

        return documentNumber;
    }

    // Formu doldururken önerilen sıra numarasını göstermek için — sayaç TÜKETİLMEZ (NextNumber artmaz).
    // Varsayılan seriyi (ör. "SF.") temsil eden asıl NumberSequence satırına bakar.
    public async Task<(string Prefix, long NextNumber, int Padding)> PeekAsync(string sequenceKey, CancellationToken cancellationToken = default)
    {
        var sequence = await dbContext.NumberSequences
            .AsNoTracking()
            .SingleAsync(x => x.Key == sequenceKey, cancellationToken);
        return (sequence.Prefix, sequence.NextNumber, sequence.Padding);
    }

    // Kullanıcının elle girdiği seriye (ör. "EDP") göre önerilen sıra numarasını gösterir — sayaç
    // TÜKETİLMEZ. Seri boşsa veya varsayılan seriyle (ör. "SF.") aynıysa varsayılan sayaç döner;
    // farklı bir seri daha önce hiç kullanılmadıysa 1'den başlayan "sanal" bir öneri döner (satır
    // henüz veritabanında oluşturulmaz — o ancak EnsureAtLeastForSeriesAsync ile, gerçekten
    // kaydedilince oluşur).
    public async Task<(string Prefix, long NextNumber, int Padding)> PeekForSeriesAsync(string sequenceKey, string series, CancellationToken cancellationToken = default)
    {
        var defaultSequence = await dbContext.NumberSequences
            .AsNoTracking()
            .SingleAsync(x => x.Key == sequenceKey, cancellationToken);

        if (string.IsNullOrEmpty(series) || series == defaultSequence.Prefix)
        {
            return (defaultSequence.Prefix, defaultSequence.NextNumber, defaultSequence.Padding);
        }

        var composedKey = ComposeKey(sequenceKey, series);
        var sequence = await dbContext.NumberSequences
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == composedKey, cancellationToken);

        return sequence is null
            ? (series, 1, defaultSequence.Padding)
            : (sequence.Prefix, sequence.NextNumber, sequence.Padding);
    }

    // Kullanıcı belge sırasını elle değiştirip mevcut sayaçtan daha ileri bir numara girdiğinde,
    // bundan sonraki otomatik numaraların çakışmadan bu elle girilen numaranın devamından gelmesi
    // için o SERİYE ait sayacı ileri alır (geriye doğru asla almaz). Seri varsayılan seriyle
    // aynıysa asıl sayaç güncellenir; farklı, daha önce hiç kullanılmamış bir seriyse (ör. "EDP")
    // o seri için yeni, 1'den başlayan kendi sayacı otomatik oluşturulur — böylece her seri kendi
    // bağımsız sırasını takip eder, varsayılan serinin sayacı bundan etkilenmez.
    public Task EnsureAtLeastForSeriesAsync(string sequenceKey, string series, long minimumNextNumber, CancellationToken cancellationToken = default) =>
        ExecuteWithConcurrencyRetryAsync<object?>(dbContext, async () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                await EnsureAtLeastForSeriesWithinTransactionAsync(sequenceKey, series, minimumNextNumber, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            });
            return null;
        }, cancellationToken);

    // EnsureAtLeastForSeriesAsync'in transaction/SaveChanges yönetmeyen çekirdeği — GenerateWithinTransactionAsync
    // ile aynı gerekçeyle: zaten açık bir transaction içinde çalışan çağıranlar (ör.
    // InvoicesController'ın fatura kaydını da aynı transaction'a saran akışı) için.
    internal async Task EnsureAtLeastForSeriesWithinTransactionAsync(string sequenceKey, string series, long minimumNextNumber, CancellationToken cancellationToken = default)
    {
        var defaultSequence = await dbContext.NumberSequences
            .SingleAsync(x => x.Key == sequenceKey, cancellationToken);

        NumberSequence target;
        if (string.IsNullOrEmpty(series) || series == defaultSequence.Prefix)
        {
            target = defaultSequence;
        }
        else
        {
            var composedKey = ComposeKey(sequenceKey, series);
            target = await dbContext.NumberSequences.SingleOrDefaultAsync(x => x.Key == composedKey, cancellationToken)
                ?? new NumberSequence
                {
                    Key = composedKey,
                    Prefix = series,
                    NextNumber = 1,
                    Padding = defaultSequence.Padding
                };
            if (target.Id == 0)
            {
                dbContext.NumberSequences.Add(target);
            }
        }

        if (minimumNextNumber > target.NextNumber)
        {
            target.NextNumber = minimumNextNumber;
            target.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    private static string ComposeKey(string sequenceKey, string series) => $"{sequenceKey}:{series}";
}
