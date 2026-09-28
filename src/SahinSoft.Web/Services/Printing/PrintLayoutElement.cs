namespace SahinSoft.Web.Services.Printing;

// Çıktı Tasarımcısı LayoutJson'ının TEK öğe şekli - hem HTML önizleme hem ESC/POS aynı bu
// listeden yürütülür (PrintRenderingService). Serbest HTML/CSS veya kod DEĞİL, sabit bir
// element sözlüğü - tasarımcı UI'ı ve sunucu tarafı doğrulaması bu tipe karşı çalışır.
public sealed class PrintLayoutElement
{
    // text | staticText | divider | lineItemsTable | totalsBlock | paymentBreakdown |
    // discountBreakdown | cancellationBreakdown | qrcode | barcode | image | spacer | cut
    public string Type { get; set; } = "text";

    // text/qrcode/barcode/image için PrintFieldRegistry'deki bir anahtar (örn. "adisyon.checkNumber").
    public string? BindingKey { get; set; }

    // staticText için serbest metin (örn. "Teşekkür Ederiz"), text için opsiyonel sol etiket
    // (örn. "TARİH" - "TARİH : 04.09.2026" satırı oluşturur).
    public string? Label { get; set; }
    public string? StaticText { get; set; }

    public string Align { get; set; } = "left"; // left | center | right
    public bool Bold { get; set; }
    public bool Underline { get; set; }
    public bool DoubleWidth { get; set; }
    public int HeightMm { get; set; } = 4; // spacer için
}
