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
public sealed class RestaurantTerminalSettingsController(ApplicationDbContext dbContext) : Controller
{
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
        return View(vm);
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
