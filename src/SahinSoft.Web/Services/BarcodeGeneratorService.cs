using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class BarcodeGeneratorService(ApplicationDbContext dbContext)
{
    public Task<string> GenerateEan13Async(CancellationToken cancellationToken = default) =>
        GenerateAsync(12, "1989", cancellationToken);

    public Task<string> GenerateEan8Async(CancellationToken cancellationToken = default) =>
        GenerateAsync(7, "1989", cancellationToken);

    public async Task<string> GenerateAsciiAsync(CancellationToken cancellationToken = default)
    {
        const string prefix = "AS";
        var highest = await FindHighestExistingSequenceAsync(prefix, 6, 0, cancellationToken);
        for (var sequence = highest + 1; sequence <= 999999; sequence++)
        {
            var candidate = $"{prefix}{sequence:D6}";
            if (!await ExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Kullanılabilir ASCII barkod kalmadı.");
    }

    public async Task<string> GenerateScaleBarcodeAsync(
        string prefix,
        CancellationToken cancellationToken = default)
    {
        if (prefix is not ("27" or "28" or "29"))
        {
            throw new ArgumentException("Terazi barkod ön eki 27, 28 veya 29 olmalıdır.", nameof(prefix));
        }

        var highest = await FindHighestExistingSequenceAsync(prefix, 5, 0, cancellationToken);
        for (var plu = highest + 1; plu <= 99999; plu++)
        {
            var candidate = $"{prefix}{plu:D5}";
            if (!await ExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"{prefix} ön eki için kullanılabilir terazi barkodu kalmadı.");
    }

    private async Task<string> GenerateAsync(
        int bodyLength,
        string prefix,
        CancellationToken cancellationToken)
    {
        var sequenceLength = bodyLength - prefix.Length;
        var maximum = (long)Math.Pow(10, sequenceLength) - 1;
        var highest = await FindHighestExistingSequenceAsync(prefix, sequenceLength, 1, cancellationToken);

        for (var sequence = highest + 1; sequence <= maximum; sequence++)
        {
            var body = $"{prefix}{sequence.ToString($"D{sequenceLength}")}";
            var candidate = $"{body}{CalculateCheckDigit(body)}";
            if (!await ExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Kullanılabilir otomatik barkod kalmadı.");
    }

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
            .Where(x => x.Barcode != null && x.Barcode.StartsWith(prefix) && x.Barcode.Length == totalLength)
            .Select(x => x.Barcode!)
            .ToListAsync(cancellationToken);

        var extraCodes = await dbContext.ProductBarcodes
            .Where(x => x.Barcode.StartsWith(prefix) && x.Barcode.Length == totalLength)
            .Select(x => x.Barcode)
            .ToListAsync(cancellationToken);

        return productCodes.Concat(extraCodes)
            .Select(code => long.TryParse(code.Substring(prefix.Length, sequenceLength), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
    }

    // Hem ürünün birincil barkodu (Products.Barcode) hem de ek barkodları (ProductBarcodes)
    // kontrol edilmeli — sadece ikincisine bakmak, katalog/seed verisiyle gelen birincil
    // barkodlarla çakışan "boş" aday üretilmesine yol açıyordu.
    private async Task<bool> ExistsAsync(string barcode, CancellationToken cancellationToken) =>
        await dbContext.Products.AnyAsync(x => x.Barcode == barcode, cancellationToken) ||
        await dbContext.ProductBarcodes.AnyAsync(x => x.Barcode == barcode, cancellationToken);

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
