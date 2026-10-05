using System.Text;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SahinSoft.Web.Services.Printing;

// LayoutJson'daki AYNI element listesini iki metotla yürütür - RenderHtmlPreview (tasarımcı
// canvas'ı) ve RenderEscPos (gerçek termal yazıcı baytı). İkisi de AYNI genişlik tablosunu
// (58mm=32 karakter, 80mm=48 karakter) ve hizalama/kalınlık bayraklarını okur - sürüklenme
// riski yok çünkü tek kod yolu her iki çıktıyı da üretiyor.
public sealed class PrintRenderingService
{
    private const byte Esc = 0x1B;
    private const byte Gs = 0x1D;

    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static List<PrintLayoutElement> ParseLayout(string layoutJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<PrintLayoutElement>>(layoutJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static int CharWidth(int paperWidthMm) => paperWidthMm >= 80 ? 48 : 32;

    // ---------------------------------------------------------------- HTML önizleme

    public string RenderHtmlPreview(List<PrintLayoutElement> layout, int paperWidthMm, PrintDataContext data)
    {
        var width = CharWidth(paperWidthMm);
        var sb = new StringBuilder();
        sb.Append("<div class=\"pr-paper\" style=\"font-family:'Courier New',monospace; white-space:pre; font-size:13px; line-height:1.35; width:")
          .Append(width).Append("ch; margin:0 auto; background:#fff; color:#111; padding:10px 6px;\">");

        foreach (var el in layout)
        {
            AppendHtmlElement(sb, el, width, data);
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    private void AppendHtmlElement(StringBuilder sb, PrintLayoutElement el, int width, PrintDataContext data)
    {
        void Line(string text, string align = "left", bool bold = false, bool underline = false)
        {
            var style = "display:block;";
            style += align switch { "center" => "text-align:center;", "right" => "text-align:right;", _ => "text-align:left;" };
            if (bold) style += "font-weight:700;";
            if (underline) style += "text-decoration:underline;";
            sb.Append("<span style=\"").Append(style).Append("\">").Append(System.Net.WebUtility.HtmlEncode(text)).Append("</span>");
        }

        switch (el.Type)
        {
            case "staticText":
                Line(el.StaticText ?? "", el.Align, el.Bold, el.Underline);
                break;
            case "text":
                var value = ResolveText(el, data);
                Line(el.Label is { Length: > 0 } lbl ? TwoCol(lbl, value, width) : value, el.Align, el.Bold, el.Underline);
                break;
            case "divider":
                Line(new string('-', width));
                break;
            case "spacer":
                sb.Append("<span style=\"display:block;height:").Append(el.HeightMm * 3).Append("px;\"></span>");
                break;
            case "cut":
                Line(new string('=', width), "center");
                break;
            case "lineItemsTable":
                foreach (var row in data.LineItems)
                {
                    Line(FormatItemRow(row, width));
                }
                break;
            case "totalsBlock":
            case "paymentBreakdown":
                foreach (var row in data.PaymentBreakdown)
                {
                    Line(FormatBreakdownRow(row, width));
                }
                break;
            case "discountBreakdown":
                foreach (var row in data.DiscountBreakdown)
                {
                    Line(FormatBreakdownRow(row, width));
                }
                break;
            case "cancellationBreakdown":
                foreach (var row in data.CancellationBreakdown)
                {
                    Line(FormatBreakdownRow(row, width));
                }
                break;
            case "qrcode":
                var qrValue = el.BindingKey is { Length: > 0 } qk ? data.Codes.GetValueOrDefault(qk, "") : "";
                if (!string.IsNullOrEmpty(qrValue))
                {
                    sb.Append("<div style=\"text-align:center;margin:6px 0;\">[QR: ").Append(System.Net.WebUtility.HtmlEncode(qrValue)).Append("]</div>");
                }
                break;
            case "barcode":
                var bcValue = el.BindingKey is { Length: > 0 } bk ? data.Codes.GetValueOrDefault(bk, "") : "";
                if (!string.IsNullOrEmpty(bcValue))
                {
                    sb.Append("<div style=\"text-align:center;font-family:monospace;letter-spacing:2px;margin:6px 0;\">|").Append(System.Net.WebUtility.HtmlEncode(bcValue)).Append("|</div>");
                }
                break;
            case "image":
                var imgPath = el.BindingKey is { Length: > 0 } ik ? data.ImagePaths.GetValueOrDefault(ik) : null;
                if (imgPath != null)
                {
                    sb.Append("<div style=\"text-align:center;margin:4px 0;\"><img src=\"").Append(imgPath).Append("\" style=\"max-height:60px;\" /></div>");
                }
                break;
        }
    }

    private static string ResolveText(PrintLayoutElement el, PrintDataContext data)
    {
        if (string.IsNullOrEmpty(el.BindingKey))
        {
            return "";
        }
        return data.GetText(el.BindingKey);
    }

    private static string TwoCol(string left, string right, int width)
    {
        var space = Math.Max(width - left.Length - right.Length, 1);
        return left + new string(' ', space) + right;
    }

    private static string FormatItemRow(PrintLineItem item, int width)
    {
        var qty = item.Quantity.ToString("0.##");
        var qtyCol = qty.PadRight(5);

        // Mutfak fişi gibi fiyat GÖSTERİLMEMESİ gereken çıktılarda LineTotal null gelir - tutar
        // kolonu tamamen atlanır (GERÇEK HATA olarak bulundu, 2026-09-28 canlı test yazdırmada:
        // önceden burada her zaman "0.00" basılıyordu).
        if (item.LineTotal is not decimal lineTotal)
        {
            var nameOnlyWidth = Math.Max(width - qtyCol.Length, 4);
            var nameOnly = item.ProductName.Length > nameOnlyWidth ? item.ProductName[..nameOnlyWidth] : item.ProductName;
            return qtyCol + nameOnly;
        }

        var amount = lineTotal.ToString("N2");
        var amountCol = amount.PadLeft(10);
        var nameWidth = Math.Max(width - qtyCol.Length - amountCol.Length, 4);
        var name = item.ProductName.Length > nameWidth ? item.ProductName[..nameWidth] : item.ProductName.PadRight(nameWidth);
        return qtyCol + name + amountCol;
    }

    private static string FormatBreakdownRow(PrintBreakdownRow row, int width)
    {
        var amount = row.Amount.ToString("N2");
        var countCol = row.Count.ToString().PadLeft(4);
        var amountCol = amount.PadLeft(12);
        var nameWidth = Math.Max(width - countCol.Length - amountCol.Length, 4);
        var name = row.Label.Length > nameWidth ? row.Label[..nameWidth] : row.Label.PadRight(nameWidth);
        return name + countCol + amountCol;
    }

    // ---------------------------------------------------------------- ESC/POS

    public async Task<byte[]> RenderEscPosAsync(List<PrintLayoutElement> layout, int paperWidthMm, PrintDataContext data, CancellationToken cancellationToken = default)
    {
        var width = CharWidth(paperWidthMm);
        var raster = new EscPosRasterWidth(paperWidthMm >= 80 ? 576 : 384);

        // "₺" termal yazıcı kod sayfasında yoktur ve "TL" (2 karakter) olarak basılır; genişlik hesabı (sağa hizalama/doldurma)
        // bu dönüşümden SONRA değil ÖNCE yapılmalı, yoksa tutar satırları 48 karakteri aşıp son harf alt satıra taşar.
        foreach (var key in data.Texts.Keys.ToList())
        {
            data.Texts[key] = data.Texts[key].Replace("₺", "TL");
        }

        using var ms = new MemoryStream();
        ms.Write([Esc, 0x40]); // init

        foreach (var el in layout)
        {
            await AppendEscPosElementAsync(ms, el, width, raster, data, cancellationToken);
        }

        ms.Write([0x0A, 0x0A, 0x0A]);
        return ms.ToArray();
    }

    private sealed record EscPosRasterWidth(int Dots);

    private async Task AppendEscPosElementAsync(MemoryStream ms, PrintLayoutElement el, int width, EscPosRasterWidth raster, PrintDataContext data, CancellationToken cancellationToken)
    {
        void SetAlign(string align) => ms.Write([Esc, 0x61, (byte)(align switch { "center" => 1, "right" => 2, _ => 0 })]);
        void SetBold(bool on) => ms.Write([Esc, 0x45, (byte)(on ? 1 : 0)]);
        void SetUnderline(bool on) => ms.Write([Esc, 0x2D, (byte)(on ? 1 : 0)]);
        void WriteLine(string text)
        {
            var bytes = EncodeTurkish(text + "\n");
            ms.Write(bytes);
        }
        void Line(string text, string align = "left", bool bold = false, bool underline = false)
        {
            SetAlign(align);
            SetBold(bold);
            SetUnderline(underline);
            WriteLine(text);
            SetBold(false);
            SetUnderline(false);
            SetAlign("left");
        }

        switch (el.Type)
        {
            case "staticText":
                Line(el.StaticText ?? "", el.Align, el.Bold, el.Underline);
                break;
            case "text":
                var value = ResolveText(el, data);
                Line(el.Label is { Length: > 0 } lbl ? TwoCol(lbl, value, width) : value, el.Align, el.Bold, el.Underline);
                break;
            case "divider":
                Line(new string('-', width));
                break;
            case "spacer":
                WriteLine("");
                break;
            case "cut":
                ms.Write([Gs, 0x56, 0x01]); // partial cut
                break;
            case "lineItemsTable":
                foreach (var row in data.LineItems)
                {
                    Line(FormatItemRow(row, width));
                }
                break;
            case "totalsBlock":
            case "paymentBreakdown":
                foreach (var row in data.PaymentBreakdown)
                {
                    Line(FormatBreakdownRow(row, width));
                }
                break;
            case "discountBreakdown":
                foreach (var row in data.DiscountBreakdown)
                {
                    Line(FormatBreakdownRow(row, width));
                }
                break;
            case "cancellationBreakdown":
                foreach (var row in data.CancellationBreakdown)
                {
                    Line(FormatBreakdownRow(row, width));
                }
                break;
            case "qrcode":
                var qrValue = el.BindingKey is { Length: > 0 } qk ? data.Codes.GetValueOrDefault(qk, "") : "";
                if (!string.IsNullOrEmpty(qrValue))
                {
                    SetAlign("center");
                    WriteQrCode(ms, qrValue);
                    SetAlign("left");
                }
                break;
            case "barcode":
                var bcValue = el.BindingKey is { Length: > 0 } bk ? data.Codes.GetValueOrDefault(bk, "") : "";
                if (!string.IsNullOrEmpty(bcValue))
                {
                    SetAlign("center");
                    WriteBarcode(ms, bcValue);
                    SetAlign("left");
                }
                break;
            case "image":
                var imgPath = el.BindingKey is { Length: > 0 } ik ? data.ImagePaths.GetValueOrDefault(ik) : null;
                if (imgPath != null)
                {
                    await WriteRasterImageAsync(ms, imgPath, raster.Dots, cancellationToken);
                }
                break;
        }
    }

    // Türkçe karakterler (İ/ı/Ğ/ğ/Ş/ş/Ö/ö/Ü/ü/Ç/ç) çoğu ESC/POS termal yazıcıda CP857/CP1254 code
    // page ile doğru basılır. .NET Core'da CodePages provider kayıtlı DEĞİLSE Encoding.GetEncoding
    // hata verir - o yüzden burada Latin harflere düşürülmüş (ASCII-safe) bir dönüşüm kullanılır,
    // gerçek donanımda code page seçimi (ESC t n) ile birlikte ince ayar yapılabilir; bu, "hiç
    // basmamak"tan güvenli bir varsayılandır.
    private static byte[] EncodeTurkish(string text)
    {
        var map = new Dictionary<char, char>
        {
            ['İ'] = 'I', ['ı'] = 'i', ['Ğ'] = 'G', ['ğ'] = 'g',
            ['Ş'] = 'S', ['ş'] = 's', ['Ö'] = 'O', ['ö'] = 'o',
            ['Ü'] = 'U', ['ü'] = 'u', ['Ç'] = 'C', ['ç'] = 'c'
        };
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            sb.Append(map.TryGetValue(ch, out var mapped) ? mapped : ch);
        }
        return Encoding.ASCII.GetBytes(sb.ToString().Replace("₺", "TL"));
    }

    private static void WriteQrCode(MemoryStream ms, string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        // Model 2 seç
        ms.Write([Gs, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00]);
        // Modül boyutu (6)
        ms.Write([Gs, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x06]);
        // Hata düzeltme seviyesi (M=49)
        ms.Write([Gs, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x31]);
        // Veri sakla
        var storeLen = bytes.Length + 3;
        ms.Write([Gs, 0x28, 0x6B, (byte)(storeLen & 0xFF), (byte)((storeLen >> 8) & 0xFF), 0x31, 0x50, 0x30]);
        ms.Write(bytes);
        // Yazdır
        ms.Write([Gs, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30]);
        ms.Write([0x0A]);
    }

    private static void WriteBarcode(MemoryStream ms, string data)
    {
        // CODE39 - sadece büyük harf/rakam/birkaç sembol destekler.
        var safe = new string(data.ToUpperInvariant().Where(c => char.IsLetterOrDigit(c) || c is '-' or '.' or ' ').ToArray());
        if (safe.Length == 0)
        {
            return;
        }
        ms.Write([Gs, 0x68, 0x50]); // barkod yüksekliği
        ms.Write([Gs, 0x77, 0x02]); // barkod genişliği
        var bytes = Encoding.ASCII.GetBytes(safe);
        ms.Write([Gs, 0x6B, 0x04, (byte)bytes.Length]);
        ms.Write(bytes);
        ms.Write([0x0A]);
    }

    private static async Task WriteRasterImageAsync(MemoryStream ms, string imagePath, int targetDots, CancellationToken cancellationToken)
    {
        var fullPath = imagePath.StartsWith('/') ? Path.Combine(AppContext.BaseDirectory, "wwwroot", imagePath.TrimStart('/')) : imagePath;
        if (!File.Exists(fullPath))
        {
            return;
        }

        using var image = await Image.LoadAsync<Rgba32>(fullPath, cancellationToken);
        var scale = (double)targetDots / image.Width;
        var newHeight = Math.Max(1, (int)(image.Height * scale));
        image.Mutate(x => x.Resize(targetDots, newHeight).Grayscale());

        var widthBytes = (targetDots + 7) / 8;
        var raster = new byte[widthBytes * newHeight];
        for (var y = 0; y < newHeight; y++)
        {
            for (var x = 0; x < targetDots; x++)
            {
                var px = image[x, y];
                var luminance = (px.R + px.G + px.B) / 3.0;
                if (luminance < 128)
                {
                    raster[y * widthBytes + x / 8] |= (byte)(0x80 >> (x % 8));
                }
            }
        }

        ms.Write([Gs, 0x76, 0x30, 0x00]);
        ms.Write([(byte)(widthBytes & 0xFF), (byte)((widthBytes >> 8) & 0xFF)]);
        ms.Write([(byte)(newHeight & 0xFF), (byte)((newHeight >> 8) & 0xFF)]);
        ms.Write(raster);
        ms.Write([0x0A]);
    }
}
