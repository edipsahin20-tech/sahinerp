namespace SahinSoft.Web.Services.Printing;

// LineTotal null ise (mutfak fişi gibi fiyat GÖSTERİLMEMESİ gereken çıktılarda) tutar kolonu
// hiç basılmaz - "0.00" gibi yanıltıcı bir değer göstermek yerine.
public sealed record PrintLineItem(decimal Quantity, string ProductName, decimal UnitPrice, decimal? LineTotal);

public sealed record PrintBreakdownRow(string Label, int Count, decimal Amount);

// PrintRenderingService'in LayoutJson'daki her elementi doldururken okuduğu, önceden çözülmüş
// veri torbası - RenderHtmlPreview ve RenderEscPos AYNI bu nesneyi kullanır (sürüklenme riski
// olmaması için tek kod yolu).
public sealed class PrintDataContext
{
    public Dictionary<string, string> Texts { get; } = [];
    public Dictionary<string, string> ImagePaths { get; } = [];
    public Dictionary<string, string> Codes { get; } = [];
    public List<PrintLineItem> LineItems { get; set; } = [];
    public List<PrintBreakdownRow> PaymentBreakdown { get; set; } = [];
    public List<PrintBreakdownRow> DiscountBreakdown { get; set; } = [];
    public List<PrintBreakdownRow> CancellationBreakdown { get; set; } = [];

    public string GetText(string key) => Texts.GetValueOrDefault(key, "");
}
