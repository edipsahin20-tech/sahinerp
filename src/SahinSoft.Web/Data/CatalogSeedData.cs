using SahinSoft.Domain.Entities;

namespace SahinSoft.Web.Data;

public static class CatalogSeedData
{
    private static readonly DateTime SeedDate = new(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc);

    public static IReadOnlyList<TaxRate> TaxRates =>
    [
        new() { Id = 1, Code = "KDV10", Name = "KDV %10", Rate = 10, CreatedAtUtc = SeedDate },
        new() { Id = 2, Code = "KDV20", Name = "KDV %20", Rate = 20, CreatedAtUtc = SeedDate },
        new() { Id = 3, Code = "KDV0", Name = "KDV %0", Rate = 0, IsExempt = true, CreatedAtUtc = SeedDate },
        new() { Id = 4, Code = "KDV1", Name = "KDV %1", Rate = 1, CreatedAtUtc = SeedDate }
    ];

    public static IReadOnlyList<ProductCategory> Categories =>
    [
        Category(1, "YAZARKASA", "Yazar Kasa POS", "yazarkasa-pos.html"),
        Category(2, "TERAZI", "Teraziler", "teraziler.html"),
        Category(3, "BARKOD", "Barkod Okuyucular", "barkod-okuyucular.html"),
        Category(4, "YAZICI", "Yazıcılar", "yazicilar.html"),
        Category(5, "ELTERM", "El Terminalleri", "el-terminali.html"),
        Category(6, "POSPC", "Dokunmatik POS", "dokunmatik-pos.html"),
        Category(7, "YAZILIM", "Yazılım ve Entegrasyon", "kurumsal-yazilim.html"),
        Category(8, "POSEKIP", "POS Çevre Birimleri", "index.html")
    ];

    public static IReadOnlyList<Product> Products =>
    [
        Product(1, "YK-0001", "Ingenico IDE280", "Ingenico", "IDE280", 1, 1, "yazarkasa-pos.html"),
        Product(2, "YK-0002", "Ingenico Move 5000F", "Ingenico", "Move 5000F", 1, 1, "yazarkasa-pos.html"),
        Product(3, "YK-0003", "PAYGO SP630PRO ECR", "PayGo", "SP630PRO ECR", 1, 1, "yazarkasa-pos.html"),
        Product(4, "YK-0004", "Profilo S900", "Profilo", "S900", 1, 1, "yazarkasa-pos.html"),
        Product(5, "YK-0005", "inPOS m530 Mobil POS", "inPOS", "m530", 1, 1, "yazarkasa-pos.html"),

        Product(6, "TR-0001", "CAS CL3000 Market Terazisi", "CAS", "CL3000", 2, 2, "teraziler.html"),
        Product(7, "TR-0002", "CAS CL8000 Dokunmatik Terazi", "CAS", "CL8000", 2, 2, "teraziler.html"),
        Product(8, "TR-0003", "CAS CN-1 Sistem Terazisi", "CAS", "CN-1", 2, 2, "teraziler.html"),
        Product(9, "TR-0004", "Digi SM100P Boyunlu Terazi", "Digi", "SM100P", 2, 2, "teraziler.html"),
        Product(10, "TR-0005", "Digi SM-120T Dokunmatik Terazi", "Digi", "SM-120T", 2, 2, "teraziler.html"),
        Product(11, "TR-0006", "CAS ER-JR Masaüstü Terazi", "CAS", "ER-JR", 2, 2, "teraziler.html"),
        Product(12, "TR-0007", "CAS FW-500 Su Geçirmez Terazi", "CAS", "FW-500", 2, 2, "teraziler.html"),
        Product(13, "TR-0008", "CAS PDI Ankastre Kasa Terazisi", "CAS", "PDI", 2, 2, "teraziler.html"),

        Product(14, "BO-0001", "Hillpos HRS-28", "Hillpos", "HRS-28", 3, 2, "barkod-okuyucular.html"),
        Product(15, "BO-0002", "Hillpos HSC-82", "Hillpos", "HSC-82", 3, 2, "barkod-okuyucular.html"),
        Product(16, "BO-0003", "Hillpos HSD-92", "Hillpos", "HSD-92", 3, 2, "barkod-okuyucular.html"),
        Product(17, "BO-0004", "Newland TP-13", "Newland", "TP-13", 3, 2, "barkod-okuyucular.html"),
        Product(18, "BO-0005", "Newland TP-14", "Newland", "TP-14", 3, 2, "barkod-okuyucular.html"),
        Product(19, "BO-0006", "Hillpos HS-6700", "Hillpos", "HS-6700", 3, 2, "barkod-okuyucular.html"),
        Product(20, "BO-0007", "Hillpos VS-6800", "Hillpos", "VS-6800", 3, 2, "barkod-okuyucular.html"),

        Product(21, "YZ-0001", "Argox OS-214 Plus Barkod Yazıcı", "Argox", "OS-214 Plus", 4, 2, "yazicilar.html"),
        Product(22, "YZ-0002", "Hillpos HDT-400 Barkod Yazıcı", "Hillpos", "HDT-400", 4, 2, "yazicilar.html"),
        Product(23, "YZ-0003", "Hillpos HTT-440 Barkod Yazıcı", "Hillpos", "HTT-440", 4, 2, "yazicilar.html"),
        Product(24, "YZ-0004", "TSC TTP-244CE Barkod Yazıcı", "TSC", "TTP-244CE", 4, 2, "yazicilar.html"),
        Product(25, "YZ-0005", "Xprinter XP-470B Barkod Yazıcı", "Xprinter", "XP-470B", 4, 2, "yazicilar.html"),
        Product(26, "YZ-0006", "Hillpos H380 Fiş Yazıcı", "Hillpos", "H380", 4, 2, "yazicilar.html"),
        Product(27, "YZ-0007", "Hillpos Q800 Fiş Yazıcı", "Hillpos", "Q800", 4, 2, "yazicilar.html"),
        Product(28, "YZ-0008", "Bixolon SPP-R310 Mobil Fiş Yazıcı", "Bixolon", "SPP-R310", 4, 2, "yazicilar.html"),

        Product(29, "ET-0001", "Chainway C61", "Chainway", "C61", 5, 2, "el-terminali.html"),
        Product(30, "ET-0002", "Chainway C66", "Chainway", "C66", 5, 2, "el-terminali.html"),
        Product(31, "ET-0003", "Hillpos C7X Tablet", "Hillpos", "C7X", 5, 2, "el-terminali.html"),
        Product(32, "ET-0004", "Hillpos CM550X", "Hillpos", "CM550X", 5, 2, "el-terminali.html"),
        Product(33, "ET-0005", "Hillpos HT42", "Hillpos", "HT42", 5, 2, "el-terminali.html"),
        Product(34, "ET-0006", "Hillpos HT42K", "Hillpos", "HT42K", 5, 2, "el-terminali.html"),
        Product(35, "ET-0007", "Hillpos HT44", "Hillpos", "HT44", 5, 2, "el-terminali.html"),

        Product(36, "PC-0001", "Hillpos Touch Pro 15", "Hillpos", "Touch Pro 15", 6, 2, "dokunmatik-pos.html"),
        Product(37, "PC-0002", "Hillpos All-in-One Dual POS", "Hillpos", "All-in-One Dual POS", 6, 2, "dokunmatik-pos.html"),
        Product(38, "PC-0003", "Hillpos Slim Touch 15", "Hillpos", "Slim Touch 15", 6, 2, "dokunmatik-pos.html"),
        Product(39, "PC-0004", "Hillpos Kiosk POS 21.5", "Hillpos", "Kiosk POS 21.5", 6, 2, "dokunmatik-pos.html"),

        Software(40, "YW-0001", "Özel ERP & CRM Yazılımları"),
        Software(41, "YW-0002", "Stok ve Depo Yönetimi Yazılımı"),
        Software(42, "YW-0003", "Fabrika ve Üretim Takibi Yazılımı"),
        Software(43, "YW-0004", "API ve Donanım Entegrasyonları"),
        Software(44, "YW-0005", "GİB & E-Fatura Çözümleri"),

        Product(45, "PE-0001", "Para Çekmecesi", "Genel", "Metal Kasa", 8, 2, "index.html"),
        Product(46, "PE-0002", "Fiyat Gör Cihazı", "Genel", "Fiyat Sorgulama Terminali", 8, 2, "index.html"),
        Product(47, "PE-0003", "Mobil Yazıcı", "Genel", "Mobil Fiş Yazıcı", 8, 2, "index.html")
    ];

    public static IReadOnlyList<ProductBarcode> Barcodes =>
        Products.Select(product => new ProductBarcode
        {
            Id = product.Id,
            ProductId = product.Id,
            Barcode = product.Barcode!,
            BarcodeType = "EAN13",
            UnitMultiplier = 1,
            IsPrimary = true,
            CreatedAtUtc = SeedDate
        }).ToList();

    private static ProductCategory Category(int id, string code, string name, string websitePath) =>
        new()
        {
            Id = id,
            Code = code,
            Name = name,
            WebsitePath = websitePath,
            CreatedAtUtc = SeedDate
        };

    private static Product Product(
        int id,
        string stockCode,
        string name,
        string brand,
        string model,
        int categoryId,
        int taxRateId,
        string websitePath) =>
        new()
        {
            Id = id,
            StockCode = stockCode,
            Name = name,
            Brand = brand,
            Model = model,
            CategoryId = categoryId,
            TaxRateId = taxRateId,
            WebsitePath = websitePath,
            Barcode = GenerateEan13(id),
            CreatedAtUtc = SeedDate
        };

    private static Product Software(int id, string stockCode, string name) =>
        new()
        {
            Id = id,
            StockCode = stockCode,
            Name = name,
            CategoryId = 7,
            TaxRateId = 3,
            ProductType = "Yazılım",
            TrackStock = false,
            WebsitePath = "kurumsal-yazilim.html",
            Barcode = GenerateEan13(id),
            CreatedAtUtc = SeedDate
        };

    private static string GenerateEan13(int id)
    {
        var firstTwelveDigits = $"20{id:D10}";
        var sum = firstTwelveDigits
            .Select((character, index) =>
                (character - '0') * (index % 2 == 0 ? 1 : 3))
            .Sum();
        var checkDigit = (10 - sum % 10) % 10;
        return $"{firstTwelveDigits}{checkDigit}";
    }
}
