using System.Data;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class DocumentNumberGeneratorService(ApplicationDbContext dbContext)
{
    public async Task<string> GenerateAsync(string sequenceKey, CancellationToken cancellationToken = default)
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
                .SingleAsync(x => x.Key == sequenceKey, cancellationToken);

            var documentNumber = $"{sequence.Prefix}{sequence.NextNumber.ToString($"D{sequence.Padding}")}";
            sequence.NextNumber++;
            sequence.UpdatedAtUtc = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return documentNumber;
        });
    }
}
