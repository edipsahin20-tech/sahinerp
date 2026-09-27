using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

// Terminal Ayarları (Edip, 2026-09-27: "giriş sayfasına ayarlar bölümü... kasa numarası kredi ve
// nakit kasası, şube ayarlarını yapabileceğim, yazar kasa ayarlarını yapabileceğim programa özel
// ayarlar bölümü - direkt muhasebe ayarlar bölümüne bağlanmasın ama aynı tabloya yazsın") - bu
// ekran RestaurantCashRegister/Branch/InventorySettings'e (ana ERP'nin GERÇEK tablolarına) yazar,
// yeni bir "restoran-only" ayrı ayar deposu İCAT EDİLMEDİ - sadece kiosk terminaline özel, sade bir
// arayüzle sunulur (BranchesController/RestaurantCashRegistersController/SettingsController'ın
// genel ERP ekranlarına yönlendirmez).
public sealed class RestaurantTerminalSettingsViewModel
{
    public List<Branch> Branches { get; set; } = [];
    public List<RestaurantCashRegister> CashRegisters { get; set; } = [];
    public List<FinancialAccount> FinancialAccounts { get; set; } = [];
    public FiscalDeviceType FiscalDeviceType { get; set; }
    public string? FiscalAgentUrl { get; set; }

    // Bağlantı Modu (Edip, 2026-09-27: "kendi lokalinde çalışacaksa kendi veritabanını, buluttan
    // çalışacaksa bulut ayarlarını yapabileceği bir bölüm" - hibrit yerel/bulut mimarisi, bkz.
    // [[project_sahinsoft_hybrid_local_cloud_vizyonu]]) - bu SADECE appsettings.Local.json'a
    // (bu makineye özel, appsettings.json ÜZERİNE binen katman) yazar; ana ERP'nin kullandığı
    // GERÇEK bulut veritabanı bağlantı dizesi HİÇ değiştirilmez/silinmez. "Yerel" seçilirse bu
    // terminal appsettings.Local.json'daki kendi ConnectionStrings:DefaultConnection'ını kullanır
    // ve MerkezSync (zaten inşa edilmiş BranchSyncBackgroundService/SyncController) ile merkezle
    // senkronize olur. Bağlantı dizesi/connection sadece PROGRAM BAŞLARKEN okunur - bu yüzden
    // kaydettikten sonra GERÇEKTEN etkili olması için programın yeniden başlatılması ZORUNLUDUR,
    // bu ekran çalışırken canlı olarak veritabanı değiştiremez.
    public bool IsLocalMode { get; set; }
    public string LocalServer { get; set; } = string.Empty;
    public string LocalDatabase { get; set; } = "SahinSoftDb";
    public bool LocalUseWindowsAuth { get; set; } = true;
    public string? LocalUserId { get; set; }
    public string? LocalPassword { get; set; }

    // Bulut SQL bağlantı alanları (Edip, 2026-09-27: "bulut dediğim buluttaki sql bağlantı
    // ayarlarını girebilmek için bide") - Yerel'in AYNI şekli, ama şu an FİİLEN etkili olan
    // bağlantıyı (appsettings.Local.json override'ı varsa o, yoksa appsettings.json'daki paylaşılan
    // bulut bağlantısı) gösterir/düzenlenebilir yapar - böylece bu terminal ihtiyaç halinde farklı
    // bir bulut SQL sunucusuna/kullanıcısına da yönlendirilebilir.
    public string CloudServer { get; set; } = string.Empty;
    public string CloudDatabase { get; set; } = "SahinSoftDb";
    public bool CloudUseWindowsAuth { get; set; }
    public string? CloudUserId { get; set; }
    public string? CloudPassword { get; set; }

    public bool MerkezSyncEnabled { get; set; }
    public string MerkezBaseUrl { get; set; } = string.Empty;
    public string MerkezBranchCode { get; set; } = string.Empty;
    public string MerkezApiKey { get; set; } = string.Empty;
    public int MerkezPollIntervalSeconds { get; set; } = 120;
    public bool HasLocalOverrideFile { get; set; }
}
