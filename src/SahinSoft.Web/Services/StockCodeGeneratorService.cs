using System.Data;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class StockCodeGeneratorService(ApplicationDbContext dbContext)
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
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

            var sequence = await dbContext.NumberSequences
                .SingleAsync(x => x.Key == "STOCK", cancellationToken);

            // Sayaç, elle/katalog verisiyle (CatalogSeedData vb.) zaten kullanılan kodların gerisinde
            // kalabilir. Bu yüzden sistemdeki gerçek en büyük numara da bulunup sayaçla karşılaştırılır;
            // ikisinin büyüğü kullanılır. Böylece silinen kayıtların bıraktığı boşluk asla doldurulmaz —
            // her zaman o ana kadar görülen en büyük numaranın bir fazlası verilir.
            var existingNumbers = await dbContext.Products
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
    }
}
