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
        for (var sequence = 1; sequence <= 999999; sequence++)
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

        for (var plu = 1; plu <= 99999; plu++)
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

        for (long sequence = 1; sequence <= maximum; sequence++)
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

    private Task<bool> ExistsAsync(string barcode, CancellationToken cancellationToken) =>
        dbContext.ProductBarcodes.AnyAsync(x => x.Barcode == barcode, cancellationToken);

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
