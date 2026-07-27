using System.Data;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class StockCodeGeneratorService(ApplicationDbContext dbContext)
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var sequence = await dbContext.NumberSequences
            .SingleAsync(x => x.Key == "STOCK", cancellationToken);

        var stockCode = $"{sequence.Prefix}{sequence.NextNumber.ToString($"D{sequence.Padding}")}";
        sequence.NextNumber++;
        sequence.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return stockCode;
    }
}
