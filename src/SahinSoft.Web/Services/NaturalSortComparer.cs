using System.Text.RegularExpressions;

namespace SahinSoft.Web.Services;

// Edip, 2026-09-29: "işin bitince açılan masalar sıralı gözüksün rastgele değil, 1-20 kadar masa
// açtım ilk masa 1'den başlayarak sıralansın, bu HER ZAMAN böyle olsun" - masa adları ("VIP-1",
// "VIP-2", ..., "VIP-10", ..., "VIP-20") düz string sıralamasında (OrderBy(t => t.Name)) sözlük
// sırasına göre dizilir: VIP-1, VIP-10, VIP-11, ..., VIP-19, VIP-2, VIP-20, VIP-3, ... - istenen
// 1,2,3...20 DEĞİL. Bu comparer adı sayısal/sayısal-olmayan parçalara bölüp sayısal parçaları
// SAYI olarak karşılaştırır (doğal/"natural" sıralama) - yeni bir DisplayOrder alanı/migration
// icat edilmedi, çünkü masa adları zaten kendi doğal sırasını taşıyor.
public sealed partial class NaturalSortComparer : IComparer<string>
{
    public static readonly NaturalSortComparer Instance = new();

    [GeneratedRegex(@"\d+|\D+")]
    private static partial Regex ChunkPattern { get; }

    public int Compare(string? x, string? y)
    {
        if (x is null || y is null)
        {
            return string.CompareOrdinal(x, y);
        }

        var xChunks = ChunkPattern.Matches(x);
        var yChunks = ChunkPattern.Matches(y);
        var count = Math.Min(xChunks.Count, yChunks.Count);

        for (var i = 0; i < count; i++)
        {
            var xChunk = xChunks[i].Value;
            var yChunk = yChunks[i].Value;

            int result;
            if (char.IsDigit(xChunk[0]) && char.IsDigit(yChunk[0]))
            {
                // Uzun sayı dizileri (Int64 taşması) için önce basamak sayısına, sonra ordinal
                // metne bakılır - masa adlarında pratikte hiç olmayacak ama güvenli varsayılan.
                result = xChunk.TrimStart('0').Length != yChunk.TrimStart('0').Length
                    ? xChunk.TrimStart('0').Length.CompareTo(yChunk.TrimStart('0').Length)
                    : string.CompareOrdinal(xChunk.TrimStart('0'), yChunk.TrimStart('0'));
            }
            else
            {
                result = string.Compare(xChunk, yChunk, StringComparison.OrdinalIgnoreCase);
            }

            if (result != 0)
            {
                return result;
            }
        }

        return xChunks.Count.CompareTo(yChunks.Count);
    }
}
