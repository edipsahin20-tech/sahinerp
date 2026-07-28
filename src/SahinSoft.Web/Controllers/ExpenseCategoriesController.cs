using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class ExpenseCategoriesController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.ExpenseCategories.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    }

    public IActionResult Create()
    {
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "ExpenseCategories" };
        return View("Form", new ExpenseCategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseCategoryFormViewModel form)
    {
        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var category = new ExpenseCategory();
        Map(form, category);
        dbContext.ExpenseCategories.Add(category);

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Masraf kategorisi kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await dbContext.ExpenseCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        await SetToolbarAsync(id);
        return View("Form", new ExpenseCategoryFormViewModel
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            IsActive = category.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await dbContext.ExpenseCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        if (await dbContext.Expenses.AnyAsync(x => x.ExpenseCategoryId == id))
        {
            TempData["Error"] = "Bu kategoriye bağlı masraflar var, silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        dbContext.ExpenseCategories.Remove(category);
        try
        {
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Masraf kategorisi silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Bu kategoriye bağlı kayıtlar var, silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.ExpenseCategories.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.ExpenseCategories.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var hasExpenses = await dbContext.Expenses.AnyAsync(x => x.ExpenseCategoryId == id);

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "ExpenseCategories",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = !hasExpenses,
            DeleteBlockedReason = hasExpenses ? "Bu kategoriye bağlı masraflar var, silinemez." : null
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExpenseCategoryFormViewModel form)
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

        var category = await dbContext.ExpenseCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        Map(form, category);
        category.UpdatedAtUtc = DateTime.UtcNow;

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Masraf kategorisi güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task ValidateUniqueCodeAsync(ExpenseCategoryFormViewModel model)
    {
        if (await dbContext.ExpenseCategories.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu kod zaten kullanılıyor.");
        }
    }

    private static void Map(ExpenseCategoryFormViewModel source, ExpenseCategory target)
    {
        target.Code = source.Code.Trim();
        target.Name = source.Name.Trim();
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
            ModelState.AddModelError(nameof(ExpenseCategoryFormViewModel.Code), "Bu kod başka bir kayıtta kullanılıyor.");
            return false;
        }
    }
}
