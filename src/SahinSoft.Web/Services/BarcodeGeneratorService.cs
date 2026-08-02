using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// Önceden bu servis "boş bir aday bulana kadar tek tek dene" yaklaşımını kullanıyordu: hiçbir
// rezervasyon/transaction koruması olmadan Products/ProductBarcodes'ta "bu değer var mı?" diye
// sorup ilk boş olanı döndürüyordu. Hiç kimse henüz commit etmemişken eşzamanlı birden fazla istek
// hep AYNI "bir sonraki boş" adaya yakınsıyordu — canlı ortamda 10 eşzamanlı stok kartı testiyle
// doğrulandı (aynı barkod art arda defalarca çakıştı, denemeler tükendi). Artık
// StockCodeGeneratorService/DocumentNumberGeneratorService'teki ispatlanmış desenle aynı: her
// barkod türü için NumberSequence üzerinden ATOMİK rezervasyon (RowVersion iyimser eşzamanlılık +
// dıştaki ExecuteWithConcurrencyRetryAsync'in taze transaction'la otomatik tekrar denemesi).
public sealed class BarcodeGeneratorService(ApplicationDbContext dbContext)
{
    public Task<string> GenerateEan13Async(CancellationToken cancellationToken = default) =>
        GenerateWithCheckDigitAsync("BARCODE_EAN13", "1989", 12, cancellationToken);

    public Task<string> GenerateEan8Async(CancellationToken cancellationToken = default) =>
        GenerateWithCheckDigitAsync("BARCODE_EAN8", "1989", 7, cancellationToken);

    public Task<string> GenerateAsciiAsync(CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                var sequence = await dbContext.NumberSequences
                    .SingleAsync(x => x.Key == "BARCODE_ASCII", cancellationToken);

                var highestExisting = await FindHighestExistingSequenceAsync(sequence.Prefix, 6, 0, cancellationToken);
                var candidateNumber = Math.Max(sequence.NextNumber, highestExisting + 1);
                if (candidateNumber > 999999)
                {
                    throw new InvalidOperationException("Kullanılabilir ASCII barkod kalmadı.");
                }

                var candidate = $"{sequence.Prefix}{candidateNumber:D6}";
                sequence.NextNumber = candidateNumber + 1;
                sequence.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return candidate;
            });
        }, cancellationToken);

    public Task<string> GenerateScaleBarcodeAsync(
        string prefix,
        CancellationToken cancellationToken = default)
    {
        if (prefix is not ("27" or "28" or "29"))
        {
            throw new ArgumentException("Terazi barkod ön eki 27, 28 veya 29 olmalıdır.", nameof(prefix));
        }

        return DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                // Terazi barkod öneki (27/28/29) kullanıcı seçimine bağlı olduğu için sabit seed
                // yerine ilk kullanımda oluşturulur — DocumentNumberGeneratorService'in seri bazlı
                // sayaçları (ör. "PURCHASE_INVOICE:AF.") aynı desenle ilk kullanımda yaratıyor.
                var sequenceKey = $"BARCODE_SCALE_{prefix}";
                var sequence = await dbContext.NumberSequences
                    .SingleOrDefaultAsync(x => x.Key == sequenceKey, cancellationToken);
                if (sequence is null)
                {
                    sequence = new NumberSequence { Key = sequenceKey, Prefix = prefix, NextNumber = 1, Padding = 5 };
                    dbContext.NumberSequences.Add(sequence);
                }

                var highestExisting = await FindHighestExistingSequenceAsync(prefix, 5, 0, cancellationToken);
                var candidateNumber = Math.Max(sequence.NextNumber, highestExisting + 1);
                if (candidateNumber > 99999)
                {
                    throw new InvalidOperationException($"{prefix} ön eki için kullanılabilir terazi barkodu kalmadı.");
                }

                var candidate = $"{prefix}{candidateNumber:D5}";
                sequence.NextNumber = candidateNumber + 1;
                sequence.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return candidate;
            });
        }, cancellationToken);
    }

    private Task<string> GenerateWithCheckDigitAsync(
        string sequenceKey,
        string prefix,
        int bodyLength,
        CancellationToken cancellationToken) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                var sequenceLength = bodyLength - prefix.Length;
                var maximum = (long)Math.Pow(10, sequenceLength) - 1;

                var sequence = await dbContext.NumberSequences
                    .SingleAsync(x => x.Key == sequenceKey, cancellationToken);

                // Self-heal: elle eklenmiş/katalog barkodlarının (Products.Barcode, ProductBarcodes)
                // gerisinde kalmışsa sayaç gerçek en büyük mevcut numaraya göre ileri alınır — asla
                // geriye almaz, asla silinenlerin boşluğunu doldurmaya çalışmaz.
                var highestExisting = await FindHighestExistingSequenceAsync(prefix, sequenceLength, 1, cancellationToken);
                var candidateNumber = Math.Max(sequence.NextNumber, highestExisting + 1);
                if (candidateNumber > maximum)
                {
                    throw new InvalidOperationException("Kullanılabilir otomatik barkod kalmadı.");
                }

                var body = $"{prefix}{candidateNumber.ToString($"D{sequenceLength}")}";
                var barcode = $"{body}{CalculateCheckDigit(body)}";

                sequence.NextNumber = candidateNumber + 1;
                sequence.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return barcode;
            });
        }, cancellationToken);

    // Sistemdeki gerçek en büyük sıra numarasının bir fazlasından başlar (silinen kayıtların
    // bıraktığı boşluk asla doldurulmaz). Hem Products.Barcode hem ProductBarcodes taranır.
    private async Task<long> FindHighestExistingSequenceAsync(
        string prefix,
        int sequenceLength,
        int suffixCharsToIgnore,
        CancellationToken cancellationToken)
    {
        var totalLength = prefix.Length + sequenceLength + suffixCharsToIgnore;

        var productCodes = await dbContext.Products
            .AsNoTracking()
            .Where(x => x.Barcode != null && x.Barcode.StartsWith(prefix) && x.Barcode.Length == totalLength)
            .Select(x => x.Barcode!)
            .ToListAsync(cancellationToken);

        var extraCodes = await dbContext.ProductBarcodes
            .AsNoTracking()
            .Where(x => x.Barcode.StartsWith(prefix) && x.Barcode.Length == totalLength)
            .Select(x => x.Barcode)
            .ToListAsync(cancellationToken);

        return productCodes.Concat(extraCodes)
            .Select(code => long.TryParse(code.Substring(prefix.Length, sequenceLength), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
    }

    private static int CalculateCheckDigit(string digits)
    {
        var sum = digits
            .Reverse()
            .Select((character, index) =>
                (character - '0') * (index % 2 == 0 ? 3 : 1))
            .Sum();
        return (10 - sum % 10) % 10;
    }
}
