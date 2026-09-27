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
public sealed class RestaurantTerminalSettingsController(ApplicationDbContext dbContext, IWebHostEnvironment env) : Controller
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

        if (!System.IO.File.Exists(LocalOverridePath))
        {
            return;
        }

        try
        {
            var root = JsonNode.Parse(System.IO.File.ReadAllText(LocalOverridePath))?.AsObject();
            if (root is null) { return; }

            vm.HasLocalOverrideFile = true;

            var connString = root["ConnectionStrings"]?["DefaultConnection"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(connString))
            {
                vm.IsLocalMode = true;
                ParseConnectionString(connString, vm);
            }

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
        catch
        {
            // Bozuk/elle düzenlenmiş bir appsettings.Local.json - sessizce varsayılanlarla devam,
            // kaydetme SIFIRDAN yazacağı için kendini kendi kendine onarır.
        }
    }

    // Yalnızca BU uygulamanın kendi ürettiği (SaveConnectionMode) basit "Anahtar=Değer;..."
    // biçimini geri okur - genel amaçlı bir ADO.NET connection string ayrıştırıcısı DEĞİLDİR.
    private static void ParseConnectionString(string connectionString, RestaurantTerminalSettingsViewModel vm)
    {
        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = part.IndexOf('=');
            if (eq <= 0) { continue; }
            var key = part[..eq].Trim();
            var value = part[(eq + 1)..].Trim();
            switch (key.ToLowerInvariant())
            {
                case "server": vm.LocalServer = value; break;
                case "database": vm.LocalDatabase = value; break;
                case "user id": vm.LocalUserId = value; vm.LocalUseWindowsAuth = false; break;
                case "password": vm.LocalPassword = value; break;
                case "trusted_connection" when value.Equals("true", StringComparison.OrdinalIgnoreCase): vm.LocalUseWindowsAuth = true; break;
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
    // "tavuk-yumurta" sorunu). appsettings.json'daki paylaşılan bulut bağlantısı HİÇ değiştirilmez.
    // "Bulut" seçilirse ConnectionStrings override'ı YAZILMAZ (silinir) - böylece uygulama
    // appsettings.json'daki varsayılan bulut bağlantısına geri düşer.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveConnectionMode(
        bool isLocalMode,
        string? localServer,
        string? localDatabase,
        bool localUseWindowsAuth,
        string? localUserId,
        string? localPassword,
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
            var database = string.IsNullOrWhiteSpace(localDatabase) ? "SahinSoftDb" : localDatabase.Trim();
            var connectionString = localUseWindowsAuth
                ? $"Server={localServer!.Trim()};Database={database};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
                : $"Server={localServer!.Trim()};Database={database};User Id={localUserId};Password={localPassword};TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";
            root["ConnectionStrings"] = new JsonObject { ["DefaultConnection"] = connectionString };
        }

        root["MerkezSync"] = new JsonObject
        {
            ["Enabled"] = isLocalMode && merkezSyncEnabled,
            ["BaseUrl"] = merkezBaseUrl ?? string.Empty,
            ["BranchCode"] = merkezBranchCode ?? string.Empty,
            ["ApiKey"] = merkezApiKey ?? string.Empty,
            ["PollIntervalSeconds"] = merkezPollIntervalSeconds > 0 ? merkezPollIntervalSeconds : 120
        };

        System.IO.File.WriteAllText(LocalOverridePath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        TempData["Success"] = "Bağlantı ayarları kaydedildi. DEĞİŞİKLİKLERİN ETKİLİ OLMASI İÇİN PROGRAMI YENİDEN BAŞLATMANIZ GEREKİYOR.";
        return RedirectToAction(nameof(Index));
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
