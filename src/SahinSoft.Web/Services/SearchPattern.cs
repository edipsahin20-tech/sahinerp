namespace SahinSoft.Web.Services;

// Arama kutularında joker karakter: "150*" = 150 ile BAŞLAYAN, "*150" = 150 ile BİTEN, "*150*" veya "150" = İÇEREN.
// SQL LIKE desenine çevirir (özel karakterler kaçırılır). EF.Functions.Like(alan, SearchPattern.ToLike(q)) ile kullanılır.
public static class SearchPattern
{
    public static string ToLike(string? q)
    {
        var term = (q ?? string.Empty).Trim();
        var hasStar = term.Contains('*');
        var sb = new System.Text.StringBuilder();
        foreach (var ch in term)
        {
            switch (ch)
            {
                case '*': sb.Append('%'); break;
                case '%': sb.Append("[%]"); break;
                case '_': sb.Append("[_]"); break;
                case '[': sb.Append("[[]"); break;
                default: sb.Append(ch); break;
            }
        }
        return hasStar ? sb.ToString() : "%" + sb + "%";
    }
}
