using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class UnitsOfMeasureController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.UnitsOfMeasure.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    }

    public IActionResult Create()
    {
        return View("Form", new UnitOfMeasureFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UnitOfMeasureFormViewModel form)
    {
        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var unit = new UnitOfMeasure();
        Map(form, unit);
        dbContext.UnitsOfMeasure.Add(unit);

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Birim tanımı kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var unit = await dbContext.UnitsOfMeasure.SingleOrDefaultAsync(x => x.Id == id);
        if (unit is null)
        {
            return NotFound();
        }

        return View("Form", new UnitOfMeasureFormViewModel
        {
            Id = unit.Id,
            Code = unit.Code,
            Name = unit.Name,
            DecimalPlaces = unit.DecimalPlaces,
            IsActive = unit.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UnitOfMeasureFormViewModel form)
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

        var unit = await dbContext.UnitsOfMeasure.SingleOrDefaultAsync(x => x.Id == id);
        if (unit is null)
        {
            return NotFound();
        }

        Map(form, unit);
        unit.UpdatedAtUtc = DateTime.UtcNow;

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Birim tanımı güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task ValidateUniqueCodeAsync(UnitOfMeasureFormViewModel model)
    {
        if (await dbContext.UnitsOfMeasure.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu birim kodu zaten kullanılıyor.");
        }
    }

    private static void Map(UnitOfMeasureFormViewModel source, UnitOfMeasure target)
    {
        target.Code = source.Code.Trim();
        target.Name = source.Name.Trim();
        target.DecimalPlaces = source.DecimalPlaces;
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
            ModelState.AddModelError(nameof(UnitOfMeasureFormViewModel.Code), "Bu birim kodu başka bir kayıtta kullanılıyor.");
            return false;
        }
    }
}
