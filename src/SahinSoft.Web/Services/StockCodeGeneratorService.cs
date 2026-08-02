using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class StockCodeGeneratorService(ApplicationDbContext dbContext)
{
    // Daha önce burada IsolationLevel.Serializable kullanılıyordu: 10 eşzamanlı isteğin hepsi aynı
    // NumberSequence satırını okuyup aynı anda "en büyük mevcut kod" için Products tablosunda geniş
    // bir range-scan yaptığında (aşağıdaki existingNumbers sorgusu) SQL Server'da gerçek DEADLOCK'a
    // yol açıyordu (canlı ortamda 10 eşzamanlı stok kartı testiyle doğrulandı) — EnableRetryOnFailure
    // bile bu düzeydeki çakışmayı her zaman kurtaramıyordu. Artık DocumentNumberGeneratorService'teki
    // ispatlanmış desenle aynı: normal (Serializable olmayan) transaction + NumberSequence.RowVersion
    // üzerinden iyimser eşzamanlılık + dıştaki ExecuteWithConcurrencyRetryAsync'in DbUpdateConcurrencyException'ı
    // yakalayıp taze bir transaction'la otomatik tekrar denemesi.
    public Task<string> GenerateAsync(CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                var sequence = await dbContext.NumberSequences
                    .SingleAsync(x => x.Key == "STOCK", cancellationToken);

                // Sayaç, elle/katalog verisiyle (CatalogSeedData vb.) zaten kullanılan kodların
                // gerisinde kalabilir. Bu yüzden sistemdeki gerçek en büyük numara da bulunup
                // sayaçla karşılaştırılır; ikisinin büyüğü kullanılır. Böylece silinen kayıtların
                // bıraktığı boşluk asla doldurulmaz — her zaman o ana kadar görülen en büyük
                // numaranın bir fazlası verilir.
                var existingNumbers = await dbContext.Products
                    .AsNoTracking()
                    .Where(x => x.StockCode.StartsWith(sequence.Prefix))
                    .Select(x => x.StockCode.Substring(sequence.Prefix.Length))
                    .ToListAsync(cancellationToken);

                var highestExisting = existingNumbers
                    .Select(suffix => int.TryParse(suffix, out var n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                var nextNumber = Math.Max(sequence.NextNumber, highestExisting + 1);
                var stockCode = $"{sequence.Prefix}{nextNumber.ToString($"D{sequence.Padding}")}";

                sequence.NextNumber = nextNumber + 1;
                sequence.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return stockCode;
            });
        }, cancellationToken);
}
