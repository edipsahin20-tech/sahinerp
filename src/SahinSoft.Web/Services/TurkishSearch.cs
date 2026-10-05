using System.Text;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// Büyük/küçük harf ve Türkçe karakter farkı gözetmeyen arama (kofte → KÖFTE, izgara → IZGARA, sutlac → SÜTLAÇ).
// restaurant-pos.js'teki turkishNormalize ile AYNI eşleme: İ/I/ı/i hepsi "i".
public static class TurkishSearch
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            sb.Append(ch switch
            {
                'İ' or 'I' or 'ı' or 'i' => 'i',
                'Ş' or 'ş' => 's',
                'Ğ' or 'ğ' => 'g',
                'Ü' or 'ü' => 'u',
                'Ö' or 'ö' => 'o',
                'Ç' or 'ç' => 'c',
                _ => char.ToLowerInvariant(ch)
            });
        }
        return sb.ToString();
    }

    // Aktif ürünlerden, adı veya stok kodu normalize edilmiş aranan metni içerenlerin Id'leri.
    public static async Task<List<int>> MatchingProductIdsAsync(ApplicationDbContext db, string term, CancellationToken cancellationToken = default)
    {
        var needle = Normalize(term);
        if (needle.Length == 0) return [];
        var rows = await db.Products.AsNoTracking().Where(x => x.IsActive).Select(x => new { x.Id, x.Name, x.StockCode }).ToListAsync(cancellationToken);
        return rows.Where(x => Normalize(x.Name).Contains(needle) || Normalize(x.StockCode).Contains(needle)).Select(x => x.Id).ToList();
    }
}
