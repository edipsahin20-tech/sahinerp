using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Terminal Ayarları (Edip, 2026-09-27) - PIN giriş ekranındaki "Ayarlar" butonundan açılır.
// [Authorize] ZATEN girişte kimlik doğrulama isteyip (Identity e-posta/şifre) yönlendirir - ayrı
// bir PIN/parola mekanizması İCAT EDİLMEDİ, mevcut admin girişi kullanılır. Bu ekran ana ERP'nin
// GERÇEK tablolarına (Branch/RestaurantCashRegister/InventorySettings) yazar - kendi başına ayrı
// bir "restoran-only" ayar deposu değil, sadece kiosk terminaline özel SADE bir arayüzdür
// (BranchesController/RestaurantCashRegistersController/SettingsController'ın genel çok-sekmeli
// ERP ekranlarına yönlendirmez).
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager}")]
public sealed class RestaurantTerminalSettingsController(ApplicationDbContext dbContext, IWebHostEnvironment env, IConfiguration configuration) : Controller
{
    // Bu makineye özel katman (Program.cs: "AddJsonFile(appsettings.Local.json, optional:true,
    // reloadOnChange:true)") - GİT'e HİÇ girmez (.gitignore), her terminal kendi kopyasını tutar.
    // Bağlantı dizesi/senkron ayarları BİLEREK buraya yazılır, ana appsettings.json'a DEĞİL - o
    // dosya paylaşımlı/varsayılan (bulut) yapılandırmayı taşır ve asla bu ekrandan değiştirilmez.
    private string LocalOverridePath => Path.Combine(env.ContentRootPath, "appsettings.Local.json");

    public async Task<IActionResult> Index()
    {
        var settings = await dbContext.InventorySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        var vm = new RestaurantTerminalSettingsViewModel
        {
            Branches = await dbContext.Branches.AsNoTracking().OrderBy(x => x.Name).ToListAsync(),
            CashRegisters = await dbContext.RestaurantCashRegisters
                .AsNoTracking()
                .Include(x => x.Branch)
                .Include(x => x.CashFinancialAccount)
                .Include(x => x.CreditCardFinancialAccount)
                .Include(x => x.MealCardFinancialAccount)
                .OrderBy(x => x.Branch.Name).ThenBy(x => x.Name)
                .ToListAsync(),
            FinancialAccounts = await dbContext.FinancialAccounts.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
            FiscalDeviceType = settings.FiscalDeviceType,
            FiscalAgentUrl = settings.FiscalAgentUrl
        };
        PopulateConnectionMode(vm);
        return View(vm);
    }

    private void PopulateConnectionMode(RestaurantTerminalSettingsViewModel vm)
    {
        vm.MerkezPollIntervalSeconds = 120;
        vm.LocalDatabase = "SahinSoftDb";
        vm.LocalUseWindowsAuth = true;
        vm.CloudDatabase = "SahinSoftDb";

        var overrideExists = System.IO.File.Exists(LocalOverridePath);
        JsonObject? root = null;
        string? overrideConnString = null;
        string? savedMode = null;

        if (overrideExists)
        {
            try
            {
                root = JsonNode.Parse(System.IO.File.ReadAllText(LocalOverridePath))?.AsObject();
                if (root is not null)
                {
                    vm.HasLocalOverrideFile = true;
                    overrideConnString = root["ConnectionStrings"]?["DefaultConnection"]?.GetValue<string>();
                    savedMode = root["TerminalConnectionMode"]?.GetValue<string>();

                    var merkez = root["MerkezSync"];
                    if (merkez is not null)
                    {
                        vm.MerkezSyncEnabled = merkez["Enabled"]?.GetValue<bool>() ?? false;
                        vm.MerkezBaseUrl = merkez["BaseUrl"]?.GetValue<string>() ?? string.Empty;
                        vm.MerkezBranchCode = merkez["BranchCode"]?.GetValue<string>() ?? string.Empty;
                        vm.MerkezApiKey = merkez["ApiKey"]?.GetValue<string>() ?? string.Empty;
                        vm.MerkezPollIntervalSeconds = merkez["PollIntervalSeconds"]?.GetValue<int>() ?? 120;
                    }
                }
            }
            catch
            {
                // Bozuk/elle düzenlenmiş bir appsettings.Local.json - sessizce varsayılanlarla
                // devam, kaydetme SIFIRDAN yazacağı için kendini kendi kendine onarır.
            }
        }

        // "TerminalConnectionMode" işareti VARSA ona güvenilir (Bulut sekmesi de artık kendi özel
        // sunucusunu yazabildiği için "ConnectionStrings var mı" tek başına hangi sekmenin aktif
        // olduğunu AYIRT EDEMEZ). İşaret YOKSA (eski/elle oluşturulmuş bir dosya) eski davranışa -
        // ConnectionStrings varlığına bakmaya - geri düşülür.
        var isLocal = savedMode is not null
            ? savedMode == "Local"
            : !string.IsNullOrWhiteSpace(overrideConnString);

        vm.IsLocalMode = isLocal;
        if (isLocal && !string.IsNullOrWhiteSpace(overrideConnString))
        {
            ParseConnectionString(overrideConnString, vm, isLocal: true);
        }
        else if (!isLocal && !string.IsNullOrWhiteSpace(overrideConnString))
        {
            // Bulut sekmesinden özel bir sunucu kaydedilmiş - o değerleri Cloud* alanlarına yükle.
            ParseConnectionString(overrideConnString, vm, isLocal: false);
        }
        else if (!isLocal)
        {
            // Hiç override yok - uygulama şu an appsettings.json'daki paylaşımlı bulut bağlantısını
            // KULLANIYOR (Edip, 2026-09-27: "bulut dediğim buluttaki sql bağlantı ayarlarını
            // girebilmek için bide") - Bulut sekmesi boş görünmesin diye GERÇEKTEN aktif olan bu
            // bağlantıyı IConfiguration üzerinden okuyup Cloud* alanlarına dolduruyoruz.
            var effectiveConnString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(effectiveConnString))
            {
                ParseConnectionString(effectiveConnString, vm, isLocal: false);
            }
        }
    }

    // Hem BU uygulamanın kendi ürettiği (SaveConnectionMode) basit "Anahtar=Değer;..." biçimini,
    // hem appsettings.json'daki paylaşımlı bulut bağlantı dizesini AYNI şekilde okur (ikisi de aynı
    // ADO.NET anahtar isimlerini kullanıyor) - genel amaçlı bir ayrıştırıcı DEĞİL, sadece bu iki
    // kaynağın ürettiği basit biçimi çözer.
    private static void ParseConnectionString(string connectionString, RestaurantTerminalSettingsViewModel vm, bool isLocal)
    {
        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = part.IndexOf('=');
            if (eq <= 0) { continue; }
            var key = part[..eq].Trim();
            var value = part[(eq + 1)..].Trim();
            var isTrue = value.Equals("true", StringComparison.OrdinalIgnoreCase);
            switch (key.ToLowerInvariant())
            {
                case "server":
                    if (isLocal) { vm.LocalServer = value; } else { vm.CloudServer = value; }
                    break;
                case "database":
                    if (isLocal) { vm.LocalDatabase = value; } else { vm.CloudDatabase = value; }
                    break;
                case "user id":
                    if (isLocal) { vm.LocalUserId = value; vm.LocalUseWindowsAuth = false; }
                    else { vm.CloudUserId = value; vm.CloudUseWindowsAuth = false; }
                    break;
                case "password":
                    if (isLocal) { vm.LocalPassword = value; } else { vm.CloudPassword = value; }
                    break;
                case "trusted_connection" when isTrue:
                    if (isLocal) { vm.LocalUseWindowsAuth = true; } else { vm.CloudUseWindowsAuth = true; }
                    break;
            }
        }
    }

    // Şube Ayarları - Code/IsHeadOffice/ApiKey gibi geri kalan tüm alanlar bu ekranın kapsamı
    // dışında (BranchesController'ın kendi tam formu hâlâ ana ERP'de duruyor) - burada SADECE
    // Edip'in istediği Ad/Adres/Telefon.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveBranch(int id, string name, string? address, string? phone)
    {
        var branch = await dbContext.Branches.SingleOrDefaultAsync(x => x.Id == id);
        if (branch is null)
        {
            TempData["Error"] = "Şube bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Şube adı zorunludur.";
            return RedirectToAction(nameof(Index));
        }

        branch.Name = name.Trim();
        branch.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        branch.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Şube ayarları güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // Kasa Ayarları - RestaurantCashRegistersController.Create/Edit ile AYNI alanlar/tablo, tek
    // fark bu ekranın kendi (Kasa Numarası/Nakit/Kredi Kartı odaklı) sade formundan gelmesi.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCashRegister(int id, int branchId, string name, int cashFinancialAccountId, int creditCardFinancialAccountId, int? mealCardFinancialAccountId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Kasa numarası/adı zorunludur.";
            return RedirectToAction(nameof(Index));
        }

        var register = id > 0
            ? await dbContext.RestaurantCashRegisters.SingleOrDefaultAsync(x => x.Id == id)
            : null;
        if (id > 0 && register is null)
        {
            TempData["Error"] = "Kasa tanımı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        register ??= new RestaurantCashRegister();
        register.BranchId = branchId;
        register.Name = name.Trim();
        register.CashFinancialAccountId = cashFinancialAccountId;
        register.CreditCardFinancialAccountId = creditCardFinancialAccountId;
        register.MealCardFinancialAccountId = mealCardFinancialAccountId;
        register.IsActive = true;

        if (id == 0)
        {
            dbContext.RestaurantCashRegisters.Add(register);
        }
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Kasa ayarları kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateCashRegister(int id)
    {
        var register = await dbContext.RestaurantCashRegisters.SingleOrDefaultAsync(x => x.Id == id);
        if (register is not null)
        {
            register.IsActive = false;
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Kasa pasif yapıldı.";
        }
        return RedirectToAction(nameof(Index));
    }

    // Gerçek silme (Edip, 2026-09-27: "var olan kasayı ... silebilirim") - diğer finansal
    // varlıkların aksine ([[feedback_sahinsoft_conventions]]: ters kayıt, hiç hard-delete yok)
    // RestaurantCashRegister SAF bir eşleme/yapılandırma tablosudur - RestaurantPayment kendi
    // FinancialAccountId'sini doğrudan taşır, bu satıya bir RestaurantCashRegisterId İLE hiç
    // bağlanmaz, yani silinmesi hiçbir işlem kaydını yetim bırakmaz.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCashRegister(int id)
    {
        var register = await dbContext.RestaurantCashRegisters.SingleOrDefaultAsync(x => x.Id == id);
        if (register is not null)
        {
            dbContext.RestaurantCashRegisters.Remove(register);
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Kasa silindi.";
        }
        return RedirectToAction(nameof(Index));
    }

    // Bağlantı Modu (Edip, 2026-09-27) - appsettings.Local.json'a yazar, VERİTABANINA DEĞİL (bu
    // makinenin HANGİ veritabanına bağlanacağı zaten veritabanının kendisinden okunamaz - klasik
    // "tavuk-yumurta" sorunu). appsettings.json'daki paylaşılan bulut bağlantısı DOSYA olarak HİÇ
    // değiştirilmez - "Bulut" alanları BOŞ bırakılırsa (Edip özel bir sunucu/kullanıcı girmediyse)
    // ConnectionStrings override'ı hiç yazılmaz, uygulama appsettings.json'daki varsayılana geri
    // düşer. Ama Edip artık (2026-09-27: "bulut dediğim buluttaki sql bağlantı ayarlarını
    // girebilmek için bide") Bulut alanlarını DOLDURURSA, bu terminal o özel bulut sunucusuna/
    // kullanıcısına yönlendirilir - appsettings.Local.json üzerinden, appsettings.json'a
    // DOKUNULMADAN.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveConnectionMode(
        bool isLocalMode,
        string? localServer,
        string? localDatabase,
        bool localUseWindowsAuth,
        string? localUserId,
        string? localPassword,
        string? cloudServer,
        string? cloudDatabase,
        bool cloudUseWindowsAuth,
        string? cloudUserId,
        string? cloudPassword,
        bool merkezSyncEnabled,
        string? merkezBaseUrl,
        string? merkezBranchCode,
        string? merkezApiKey,
        int merkezPollIntervalSeconds)
    {
        if (isLocalMode && string.IsNullOrWhiteSpace(localServer))
        {
            TempData["Error"] = "Yerel mod için Sunucu adresi zorunludur.";
            return RedirectToAction(nameof(Index));
        }

        var root = new JsonObject();
        if (isLocalMode)
        {
            root["ConnectionStrings"] = new JsonObject
            {
                ["DefaultConnection"] = BuildConnectionString(localServer!, localDatabase, localUseWindowsAuth, localUserId, localPassword)
            };
        }
        else if (!string.IsNullOrWhiteSpace(cloudServer))
        {
            root["ConnectionStrings"] = new JsonObject
            {
                ["DefaultConnection"] = BuildConnectionString(cloudServer, cloudDatabase, cloudUseWindowsAuth, cloudUserId, cloudPassword)
            };
        }

        // "TerminalConnectionMode" işareti (2026-09-27 hata düzeltmesi) - Bulut sekmesi de artık
        // (kullanıcı özel bir sunucu girdiyse) bir ConnectionStrings override'ı yazabildiği için,
        // sayfa bir sonraki açılışta hangi sekmenin aktif gösterileceğine SADECE "ConnectionStrings
        // var mı yok mu" bakarak karar veremez (ikisi de yazabiliyor artık) - bu yüzden hangi
        // sekmenin GERÇEKTEN seçildiği ayrıca, açıkça kaydediliyor.
        root["TerminalConnectionMode"] = isLocalMode ? "Local" : "Cloud";

        root["MerkezSync"] = new JsonObject
        {
            ["Enabled"] = isLocalMode && merkezSyncEnabled,
            ["BaseUrl"] = merkezBaseUrl ?? string.Empty,
            ["BranchCode"] = merkezBranchCode ?? string.Empty,
            ["ApiKey"] = merkezApiKey ?? string.Empty,
            ["PollIntervalSeconds"] = merkezPollIntervalSeconds > 0 ? merkezPollIntervalSeconds : 120
        };

        // Dosya yazımı (2026-09-27, Edip: "kaydet dediğimde hata verdi") - gerçek Windows kurulum
        // klasöründe yazma izni olmayabilir (ör. korumalı bir klasöre kurulmuşsa); ÖNCEDEN bu
        // satırın etrafında hiç try/catch YOKTU, bir IOException/UnauthorizedAccessException
        // doğrudan işlenmemiş istisna olarak genel "Bir hata oluştu" sayfasına düşüyordu. Artık
        // yakalanıp kullanıcıya Türkçe, anlaşılır bir mesajla gösteriliyor.
        try
        {
            System.IO.File.WriteAllText(LocalOverridePath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            TempData["Error"] = "Ayar dosyasına yazılamadı - programın kurulu olduğu klasörde yazma izni olmayabilir. (" + ex.Message + ")";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Bağlantı ayarları kaydedildi. DEĞİŞİKLİKLERİN ETKİLİ OLMASI İÇİN PROGRAMI YENİDEN BAŞLATMANIZ GEREKİYOR.";
        return RedirectToAction(nameof(Index));
    }

    private static string BuildConnectionString(string server, string? database, bool useWindowsAuth, string? userId, string? password)
    {
        var db = string.IsNullOrWhiteSpace(database) ? "SahinSoftDb" : database.Trim();
        return useWindowsAuth
            ? $"Server={server.Trim()};Database={db};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
            : $"Server={server.Trim()};Database={db};User Id={userId};Password={password};TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";
    }

    // Yazar Kasa Ayarları - InventorySettings'in GERÇEK FiscalDeviceType/FiscalAgentUrl
    // alanlarına yazar (SettingsController'daki genel Ayarlar ekranıyla AYNI iki alan) - ikinci
    // bir kopya alan/ayrı bir tablo İCAT EDİLMEDİ.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFiscal(FiscalDeviceType fiscalDeviceType, string? fiscalAgentUrl)
    {
        var settings = await dbContext.InventorySettings.SingleAsync(x => x.Id == 1);
        settings.FiscalDeviceType = fiscalDeviceType;
        settings.FiscalAgentUrl = fiscalDeviceType == FiscalDeviceType.None
            ? null
            : (string.IsNullOrWhiteSpace(fiscalAgentUrl) ? null : fiscalAgentUrl.Trim());
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Yazar kasa ayarları güncellendi.";
        return RedirectToAction(nameof(Index));
    }
}
