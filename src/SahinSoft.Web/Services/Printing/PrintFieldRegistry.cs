using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Services.Printing;

public sealed record PrintFieldDefinition(string Key, string Label, string Category, string ElementType);

// Tasarımcı paleti SADECE bu listeden alan sunar; sunucu tarafında her bindingKey kaydetme
// sırasında bu listeye karşı doğrulanır (serbest SQL/expression YOK).
public static class PrintFieldRegistry
{
    private static readonly List<PrintFieldDefinition> CommonHeaderFields =
    [
        new("common.logo", "Logo", "Temel Alanlar", "image"),
        new("common.companyName", "Firma Adı", "Temel Alanlar", "text"),
        new("common.branchName", "Şube Adı", "Temel Alanlar", "text"),
        new("common.address", "Adres", "Temel Alanlar", "text"),
        new("common.phone", "Telefon", "Temel Alanlar", "text"),
        new("common.taxOffice", "Vergi Dairesi/No", "Temel Alanlar", "text"),
        new("common.website", "Web Sitesi", "Temel Alanlar", "text"),
        new("common.dateTime", "Tarih - Saat", "Temel Alanlar", "text"),
        new("common.divider", "Çizgi (Ayraç)", "Temel Alanlar", "divider"),
        new("common.spacer", "Boşluk", "Temel Alanlar", "spacer"),
        new("common.qrCode", "QR Kod", "Temel Alanlar", "qrcode"),
        new("common.barcode", "Barkod", "Temel Alanlar", "barcode"),
        new("common.staticText", "Özel Metin", "Temel Alanlar", "staticText"),
        new("common.cut", "Kesme Komutu", "Sistem Alanları", "cut")
    ];

    private static readonly List<PrintFieldDefinition> AdisyonFields =
    [
        .. CommonHeaderFields,
        new("adisyon.checkNumber", "Adisyon No", "Temel Alanlar", "text"),
        new("adisyon.tableName", "Masa No", "Temel Alanlar", "text"),
        new("adisyon.sectionName", "Bölüm", "Temel Alanlar", "text"),
        new("adisyon.guestCount", "Kişi Sayısı", "Temel Alanlar", "text"),
        new("adisyon.cashierName", "Kasiyer/Garson", "Temel Alanlar", "text"),
        new("adisyon.saleType", "Satış Türü", "Temel Alanlar", "text"),
        new("adisyon.note", "Açıklama/Not", "Temel Alanlar", "text"),
        new("adisyon.lineItems", "Ürün Tablosu", "Ürün Bilgileri", "lineItemsTable"),
        new("adisyon.subtotal", "Ara Toplam", "Ürün Bilgileri", "text"),
        new("adisyon.discount", "İndirim", "Ürün Bilgileri", "text"),
        new("adisyon.tax", "KDV", "Ürün Bilgileri", "text"),
        new("adisyon.grandTotal", "Genel Toplam", "Ürün Bilgileri", "text"),
        new("adisyon.paymentBreakdown", "Ödeme Türleri", "Ürün Bilgileri", "paymentBreakdown"),
        new("adisyon.changeAmount", "Para Üstü", "Ürün Bilgileri", "text")
    ];

    private static readonly List<PrintFieldDefinition> MutfakFields =
    [
        new("common.divider", "Çizgi (Ayraç)", "Temel Alanlar", "divider"),
        new("common.spacer", "Boşluk", "Temel Alanlar", "spacer"),
        new("common.staticText", "Özel Metin", "Temel Alanlar", "staticText"),
        new("common.cut", "Kesme Komutu", "Sistem Alanları", "cut"),
        new("mutfak.tableName", "Masa", "Temel Alanlar", "text"),
        new("mutfak.sectionName", "Bölüm", "Temel Alanlar", "text"),
        new("mutfak.orderNumber", "Sıra No", "Temel Alanlar", "text"),
        new("mutfak.dateTime", "Tarih - Saat", "Temel Alanlar", "text"),
        new("mutfak.cashierName", "Kasiyer", "Temel Alanlar", "text"),
        new("mutfak.lineItems", "Ürün/Miktar Tablosu", "Ürün Bilgileri", "lineItemsTable")
    ];

    private static readonly List<PrintFieldDefinition> ReportCommonFields =
    [
        .. CommonHeaderFields,
        new("report.title", "Rapor Başlığı", "Rapor Alanları", "text"),
        new("report.reportDate", "Tarih", "Rapor Alanları", "text"),
        new("report.reportTime", "Saat", "Rapor Alanları", "text"),
        new("report.cashierName", "Kasiyer", "Rapor Alanları", "text"),
        new("report.zNo", "Z No", "Rapor Alanları", "text"),
        new("report.openedAt", "Açılış", "Rapor Alanları", "text"),
        new("report.closedAt", "Kapanış", "Rapor Alanları", "text"),
        new("report.receiptCount", "Fiş Sayısı", "Rapor Alanları", "text"),
        new("report.paymentSummary", "Satış Özeti (Ödeme Türü/Adet/Tutar)", "Rapor Alanları", "paymentBreakdown"),
        new("report.totalSalesRow", "Toplam Satış", "Rapor Alanları", "text"),
        new("report.discountSummary", "İndirim/İkram/Ödenmez Dökümü", "Rapor Alanları", "discountBreakdown"),
        new("report.cancellationSummary", "İptaller Dökümü", "Rapor Alanları", "cancellationBreakdown"),
        new("report.discountTotal", "İndirim Toplamı", "Rapor Alanları", "text"),
        new("report.complimentaryTotal", "İkram Toplamı", "Rapor Alanları", "text"),
        new("report.unpaidTotal", "Ödenmez Toplamı", "Rapor Alanları", "text"),
        new("report.lineCancellationTotal", "Satır İptal Toplamı", "Rapor Alanları", "text"),
        new("report.receiptCancellationTotal", "Fiş İptal Toplamı", "Rapor Alanları", "text")
    ];

    private static readonly List<PrintFieldDefinition> ZOnlyFields =
    [
        new("report.grossTotal", "Brüt Ciro", "Rapor Alanları", "text"),
        new("report.netTotal", "Net Ciro", "Rapor Alanları", "text")
    ];

    public static IReadOnlyList<PrintFieldDefinition> GetFields(PrintTemplateType type) => type switch
    {
        PrintTemplateType.Adisyon => AdisyonFields,
        PrintTemplateType.MutfakFisi => MutfakFields,
        PrintTemplateType.XRaporu => ReportCommonFields,
        PrintTemplateType.ZRaporu => [.. ReportCommonFields, .. ZOnlyFields],
        _ => []
    };

    public static bool IsValidBindingKey(PrintTemplateType type, string? bindingKey)
    {
        if (string.IsNullOrWhiteSpace(bindingKey))
        {
            return true; // staticText/divider/spacer/cut gibi bindingKey gerektirmeyen tipler
        }
        return GetFields(type).Any(f => f.Key == bindingKey);
    }
}
