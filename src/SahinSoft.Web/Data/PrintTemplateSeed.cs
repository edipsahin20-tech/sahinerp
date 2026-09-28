using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Data;

// Edip'in onayladığı SON 4 GÖRSEL tasarımı (Adisyon/Mutfak/X Raporu/Z Raporu, 2026-09-28)
// hedef alan varsayılan şablonlar - IdentitySeed ile AYNI "startup'ta yoksa oluştur" deseni.
// Sadece bu 4 tip için HİÇ aktif+varsayılan şablon yoksa çalışır - var olan bir şablonun
// ÜZERİNE YAZMAZ (kullanıcı düzenlemişse dokunulmaz).
public static class PrintTemplateSeed
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await EnsureDefaultAsync(dbContext, PrintTemplateType.Adisyon, "Adisyon Fişi", AdisyonLayout);
        await EnsureDefaultAsync(dbContext, PrintTemplateType.MutfakFisi, "Mutfak Fişi", MutfakLayout);
        await EnsureDefaultAsync(dbContext, PrintTemplateType.XRaporu, "X Raporu", XRaporuLayout);
        await EnsureDefaultAsync(dbContext, PrintTemplateType.ZRaporu, "Z Raporu", ZRaporuLayout);
    }

    private static async Task EnsureDefaultAsync(ApplicationDbContext dbContext, PrintTemplateType type, string name, string layoutJson)
    {
        var hasDefault = await dbContext.PrintTemplates.AnyAsync(x => x.TemplateType == type && x.IsDefault);
        if (hasDefault)
        {
            return;
        }

        dbContext.PrintTemplates.Add(new PrintTemplate
        {
            Name = name,
            TemplateType = type,
            LayoutJson = layoutJson,
            PaperWidthMm = 80,
            IsActive = true,
            IsDefault = true,
            Version = 1
        });
        await dbContext.SaveChangesAsync();
    }

    private const string AdisyonLayout = """
    [
      {"type":"staticText","staticText":"ŞAHİNSOFT","align":"center","bold":true},
      {"type":"text","bindingKey":"common.companyName","align":"center"},
      {"type":"text","bindingKey":"common.address","align":"center"},
      {"type":"text","bindingKey":"common.phone","align":"center"},
      {"type":"divider"},
      {"type":"staticText","staticText":"ADİSYON","align":"center","bold":true},
      {"type":"text","bindingKey":"common.dateTime","label":"Tarih"},
      {"type":"text","bindingKey":"adisyon.checkNumber","label":"Fiş No"},
      {"type":"text","bindingKey":"adisyon.tableName","label":"Masa"},
      {"type":"text","bindingKey":"adisyon.sectionName","label":"Bölüm"},
      {"type":"text","bindingKey":"adisyon.cashierName","label":"Kasiyer"},
      {"type":"text","bindingKey":"adisyon.guestCount","label":"Misafir"},
      {"type":"divider"},
      {"type":"lineItemsTable"},
      {"type":"divider"},
      {"type":"text","bindingKey":"adisyon.subtotal","label":"ARA TOPLAM"},
      {"type":"text","bindingKey":"adisyon.discount","label":"İNDİRİM"},
      {"type":"text","bindingKey":"adisyon.grandTotal","label":"GENEL TOPLAM","bold":true},
      {"type":"divider"},
      {"type":"staticText","staticText":"Teşekkür Ederiz","align":"center"},
      {"type":"staticText","staticText":"Yine Bekleriz...","align":"center"},
      {"type":"qrcode","bindingKey":"common.qrCode"},
      {"type":"spacer","heightMm":2},
      {"type":"staticText","staticText":"ŞAHİNSOFT","align":"center","bold":true},
      {"type":"staticText","staticText":"Restoran Çözümleri","align":"center"},
      {"type":"cut"}
    ]
    """;

    private const string MutfakLayout = """
    [
      {"type":"staticText","staticText":"ŞAHİNSOFT","align":"center","bold":true},
      {"type":"staticText","staticText":"Sipariş Fişi","align":"center"},
      {"type":"divider"},
      {"type":"text","bindingKey":"mutfak.tableName","label":"Masa"},
      {"type":"text","bindingKey":"mutfak.sectionName","label":"Bölüm"},
      {"type":"text","bindingKey":"mutfak.orderNumber","label":"Sıra No"},
      {"type":"text","bindingKey":"mutfak.dateTime","label":"A.Tarih"},
      {"type":"text","bindingKey":"mutfak.cashierName","label":"KASİYER"},
      {"type":"divider"},
      {"type":"lineItemsTable","bold":true},
      {"type":"cut"}
    ]
    """;

    private const string XRaporuLayout = """
    [
      {"type":"staticText","staticText":"ŞAHİNSOFT","align":"center","bold":true},
      {"type":"text","bindingKey":"common.companyName","align":"center"},
      {"type":"text","bindingKey":"common.address","align":"center"},
      {"type":"text","bindingKey":"common.phone","align":"center"},
      {"type":"divider"},
      {"type":"text","bindingKey":"report.title","align":"center","bold":true},
      {"type":"text","bindingKey":"report.reportDate","label":"Tarih"},
      {"type":"text","bindingKey":"report.reportTime","label":"Saat"},
      {"type":"divider"},
      {"type":"staticText","staticText":"SATIŞ ÖZETİ","bold":true},
      {"type":"paymentBreakdown"},
      {"type":"divider"},
      {"type":"text","bindingKey":"report.totalSalesRow","label":"TOPLAM SATIŞ","bold":true},
      {"type":"divider"},
      {"type":"staticText","staticText":"İNDİRİM / İKRAM / ÖDENMEZ","bold":true},
      {"type":"discountBreakdown"},
      {"type":"divider"},
      {"type":"staticText","staticText":"İPTALLER","bold":true},
      {"type":"cancellationBreakdown"},
      {"type":"divider"},
      {"type":"staticText","staticText":"Bu bir MALİ DEĞER taşımaz.","align":"center"},
      {"type":"staticText","staticText":"Bilgi amaçlıdır.","align":"center"},
      {"type":"text","bindingKey":"common.website","align":"center"},
      {"type":"cut"}
    ]
    """;

    private const string ZRaporuLayout = """
    [
      {"type":"staticText","staticText":"ŞAHİNSOFT","align":"center","bold":true},
      {"type":"text","bindingKey":"common.companyName","align":"center"},
      {"type":"text","bindingKey":"common.address","align":"center"},
      {"type":"text","bindingKey":"common.phone","align":"center"},
      {"type":"divider"},
      {"type":"text","bindingKey":"report.title","align":"center","bold":true},
      {"type":"text","bindingKey":"report.zNo","label":"Z No"},
      {"type":"text","bindingKey":"report.reportDate","label":"Tarih"},
      {"type":"text","bindingKey":"report.openedAt","label":"Açılış"},
      {"type":"text","bindingKey":"report.closedAt","label":"Kapanış"},
      {"type":"text","bindingKey":"report.cashierName","label":"Kasiyer"},
      {"type":"divider"},
      {"type":"staticText","staticText":"SATIŞ ÖZETİ","bold":true},
      {"type":"paymentBreakdown"},
      {"type":"divider"},
      {"type":"text","bindingKey":"report.totalSalesRow","label":"TOPLAM SATIŞ","bold":true},
      {"type":"divider"},
      {"type":"staticText","staticText":"İNDİRİM / İKRAM / ÖDENMEZ","bold":true},
      {"type":"discountBreakdown"},
      {"type":"divider"},
      {"type":"staticText","staticText":"İPTALLER","bold":true},
      {"type":"cancellationBreakdown"},
      {"type":"divider"},
      {"type":"staticText","staticText":"GENEL TOPLAMLAR","bold":true},
      {"type":"text","bindingKey":"report.grossTotal","label":"Brüt Ciro"},
      {"type":"text","bindingKey":"report.discountTotal","label":"Toplam İndirim"},
      {"type":"text","bindingKey":"report.complimentaryTotal","label":"Toplam İkram"},
      {"type":"text","bindingKey":"report.unpaidTotal","label":"Toplam Ödenmez"},
      {"type":"text","bindingKey":"report.receiptCancellationTotal","label":"Toplam Fiş İptal"},
      {"type":"divider"},
      {"type":"text","bindingKey":"report.netTotal","label":"NET CİRO","bold":true},
      {"type":"divider"},
      {"type":"staticText","staticText":"Z DÖNEMİ KAPANMIŞTIR","align":"center","bold":true},
      {"type":"staticText","staticText":"Teşekkür Ederiz","align":"center"},
      {"type":"text","bindingKey":"common.website","align":"center"},
      {"type":"cut"}
    ]
    """;
}
