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
//
// Edip, 2026-10-02: "sadece burdan bu bölümü kaldır, ayarları artık muhasebe klasöründeki
// veritabanı ayarlarından yapacağız" - burada duran "Bağlantı Modu" (Bulut/Yerel + Merkez Sync)
// bölümü KALDIRILDI. Veritabanı bağlantısı artık TEK yerden yönetiliyor:
// SahinSoft.ConfigTool (SahinSoftVeritabaniAyarlari.exe) - ikisi de appsettings.Local.json'a
// yazıyordu, ama iki ayrı ekran olması kafa karıştırıyordu. Merkez Sync (offline terminal -
// merkez senkronu) bu kaldırmayla birlikte ŞU AN HİÇBİR YERDEN AYARLANAMIYOR - Edip'e açıkça
// soruldu, bilerek bu şekilde onayladı.
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager}")]
public sealed class RestaurantTerminalSettingsController(ApplicationDbContext dbContext, SahinSoft.Web.Services.RestaurantShellService shellService) : RestaurantControllerBase(dbContext, shellService)
{
    private readonly ApplicationDbContext dbContext = dbContext;

    public async Task<IActionResult> Index()
    {
        ActivePage = "settings";
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

        // Bu tarayıcının (terminalin) mevcut şube ve kasa seçimi - çerezlerden okunur.
        vm.TerminalBranchId = int.TryParse(Request.Cookies[TerminalBranchCookie], out var tb) ? tb : null;
        if (int.TryParse(Request.Cookies[TerminalCashCookie], out var cashId))
        {
            vm.TerminalRegisterId = vm.CashRegisters
                .FirstOrDefault(x => x.CashFinancialAccountId == cashId && x.BranchId == vm.TerminalBranchId)?.Id;
        }
        return View(vm);
    }

    private const string TerminalBranchCookie = "ss_terminal_branch";
    private const string TerminalCashCookie = "ss_terminal_cash";

    // Terminalin şubesini ve (varsa) kasasını bu tarayıcının çerezine yazar. Kasa, seçilen şubenin
    // aktif bir kasa tanımı olmalıdır; kasa seçilmezse kasa çerezi temizlenir.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetTerminal(int branchId, int? registerId)
    {
        var branchExists = await dbContext.Branches.AnyAsync(x => x.Id == branchId && x.IsActive);
        if (!branchExists)
        {
            TempData["Error"] = "Seçilen şube bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        RestaurantCashRegister? register = null;
        if (registerId is { } rid)
        {
            register = await dbContext.RestaurantCashRegisters.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == rid && x.IsActive && x.BranchId == branchId);
            if (register is null)
            {
                TempData["Error"] = "Seçilen kasa bu şubeye ait değil ya da pasif.";
                return RedirectToAction(nameof(Index));
            }
        }

        var cookieOptions = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = Request.IsHttps };
        Response.Cookies.Append(TerminalBranchCookie, branchId.ToString(), cookieOptions);
        if (register is not null)
        {
            Response.Cookies.Append(TerminalCashCookie, register.CashFinancialAccountId.ToString(), cookieOptions);
        }
        else
        {
            Response.Cookies.Delete(TerminalCashCookie);
        }

        TempData["Success"] = register is null
            ? "Terminal şubesi kaydedildi. Bu şube için kasa seçilmedi."
            : "Terminal şubesi ve kasası kaydedildi.";
        return RedirectToAction(nameof(Index));
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
