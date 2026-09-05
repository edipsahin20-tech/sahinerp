using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Kasa Tanımları (Edip, 2026-09-05, madde 27) - "Her kasa için şube bazlı tanım yapılabilmelidir
// ... Hangi şubedeki hangi kasanın hangi hesabı kullandığı KESİN OLARAK İZLENMELİDİR." Yapı
// RestaurantSectionsController ile AYNI desen (Evrak toolbar, lookup-picker Şube seçimi).
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager}")]
public sealed class RestaurantCashRegistersController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var registers = await dbContext.RestaurantCashRegisters
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.CashFinancialAccount)
            .Include(x => x.CreditCardFinancialAccount)
            .Include(x => x.MealCardFinancialAccount)
            .OrderBy(x => x.Branch.Name).ThenBy(x => x.Name)
            .ToListAsync();
        return View(registers);
    }

    public async Task<IActionResult> Create()
    {
        var model = new RestaurantCashRegisterFormViewModel();
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "RestaurantCashRegisters" };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantCashRegisterFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var register = new RestaurantCashRegister();
        Map(form, register);
        dbContext.RestaurantCashRegisters.Add(register);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Kasa tanımı kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var register = await dbContext.RestaurantCashRegisters.SingleOrDefaultAsync(x => x.Id == id);
        if (register is null)
        {
            return NotFound();
        }

        var model = new RestaurantCashRegisterFormViewModel
        {
            Id = register.Id,
            Name = register.Name,
            BranchId = register.BranchId,
            CashFinancialAccountId = register.CashFinancialAccountId,
            CreditCardFinancialAccountId = register.CreditCardFinancialAccountId,
            MealCardFinancialAccountId = register.MealCardFinancialAccountId,
            Note = register.Note,
            IsActive = register.IsActive
        };
        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RestaurantCashRegisterFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var register = await dbContext.RestaurantCashRegisters.SingleOrDefaultAsync(x => x.Id == id);
        if (register is null)
        {
            return NotFound();
        }

        Map(form, register);
        register.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Kasa tanımı güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.RestaurantCashRegisters.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.RestaurantCashRegisters.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "RestaurantCashRegisters",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = false
        };
    }

    private static void Map(RestaurantCashRegisterFormViewModel source, RestaurantCashRegister target)
    {
        target.Name = source.Name.Trim();
        target.BranchId = source.BranchId!.Value;
        target.CashFinancialAccountId = source.CashFinancialAccountId!.Value;
        target.CreditCardFinancialAccountId = source.CreditCardFinancialAccountId!.Value;
        target.MealCardFinancialAccountId = source.MealCardFinancialAccountId;
        target.Note = string.IsNullOrWhiteSpace(source.Note) ? null : source.Note.Trim();
        target.IsActive = source.IsActive;
    }

    private async Task PopulateSelectionsAsync(RestaurantCashRegisterFormViewModel model)
    {
        if (model.BranchId is int branchId)
        {
            model.BranchDisplay = await dbContext.Branches
                .Where(x => x.Id == branchId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }
        if (model.CashFinancialAccountId is int cashId)
        {
            model.CashFinancialAccountDisplay = await dbContext.FinancialAccounts
                .Where(x => x.Id == cashId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }
        if (model.CreditCardFinancialAccountId is int cardId)
        {
            model.CreditCardFinancialAccountDisplay = await dbContext.FinancialAccounts
                .Where(x => x.Id == cardId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }
        if (model.MealCardFinancialAccountId is int mealId)
        {
            model.MealCardFinancialAccountDisplay = await dbContext.FinancialAccounts
                .Where(x => x.Id == mealId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }
    }
}
