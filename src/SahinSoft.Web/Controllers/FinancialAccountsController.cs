using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class FinancialAccountsController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.FinancialAccounts.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    }

    public IActionResult Create()
    {
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "FinancialAccounts" };
        return View("Form", new FinancialAccountFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FinancialAccountFormViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.Code))
        {
            var sequenceKey = form.AccountType == FinancialAccountType.Bank
                ? "FINANCIAL_ACCOUNT_BANK"
                : "FINANCIAL_ACCOUNT_CASH";
            form.Code = await documentNumberGenerator.GenerateAsync(sequenceKey);
        }
        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var account = new FinancialAccount();
        Map(form, account);
        dbContext.FinancialAccounts.Add(account);

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Hesap kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var account = await dbContext.FinancialAccounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account is null)
        {
            return NotFound();
        }

        await SetToolbarAsync(id);
        return View("Form", new FinancialAccountFormViewModel
        {
            Id = account.Id,
            Code = account.Code,
            Name = account.Name,
            AccountType = account.AccountType,
            CurrencyCode = account.CurrencyCode,
            BankName = account.BankName,
            BranchName = account.BranchName,
            Iban = account.Iban,
            IsActive = account.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await dbContext.FinancialAccounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account is null)
        {
            return NotFound();
        }

        if (await dbContext.FinancialTransactions.AnyAsync(x => x.FinancialAccountId == id))
        {
            TempData["Error"] = "Bu hesaba ait hareketler var, silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        dbContext.FinancialAccounts.Remove(account);
        try
        {
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Hesap silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Bu hesaba bağlı kayıtlar var, silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.FinancialAccounts.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.FinancialAccounts.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var hasMovements = await dbContext.FinancialTransactions.AnyAsync(x => x.FinancialAccountId == id);

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "FinancialAccounts",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = !hasMovements,
            DeleteBlockedReason = hasMovements ? "Bu hesaba ait hareketler var, silinemez." : null
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FinancialAccountFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var account = await dbContext.FinancialAccounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account is null)
        {
            return NotFound();
        }

        Map(form, account);
        account.UpdatedAtUtc = DateTime.UtcNow;

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Hesap güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task ValidateUniqueCodeAsync(FinancialAccountFormViewModel model)
    {
        if (await dbContext.FinancialAccounts.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu hesap kodu zaten kullanılıyor.");
        }
    }

    private static void Map(FinancialAccountFormViewModel source, FinancialAccount target)
    {
        target.Code = source.Code.Trim();
        target.Name = source.Name.Trim();
        target.AccountType = source.AccountType;
        target.CurrencyCode = source.CurrencyCode.Trim().ToUpperInvariant();
        target.BankName = source.BankName?.Trim();
        target.BranchName = source.BranchName?.Trim();
        target.Iban = source.Iban?.Trim();
        target.IsActive = source.IsActive;
    }

    private async Task<bool> TrySaveAsync()
    {
        try
        {
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(FinancialAccountFormViewModel.Code), "Bu hesap kodu başka bir kayıtta kullanılıyor.");
            return false;
        }
    }
}
