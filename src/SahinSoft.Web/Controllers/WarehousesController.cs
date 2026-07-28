using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class WarehousesController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.Warehouses.AsNoTracking().Include(x => x.Branch).OrderBy(x => x.Name).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        var model = new WarehouseFormViewModel();
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "Warehouses" };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WarehouseFormViewModel form)
    {
        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var warehouse = new Warehouse();
        Map(form, warehouse);

        if (warehouse.IsDefault)
        {
            await ClearOtherDefaultsAsync();
        }

        dbContext.Warehouses.Add(warehouse);

        if (!await TrySaveAsync())
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        TempData["Success"] = "Depo kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var warehouse = await dbContext.Warehouses.SingleOrDefaultAsync(x => x.Id == id);
        if (warehouse is null)
        {
            return NotFound();
        }

        var model = new WarehouseFormViewModel
        {
            Id = warehouse.Id,
            Code = warehouse.Code,
            Name = warehouse.Name,
            BranchId = warehouse.BranchId,
            IsDefault = warehouse.IsDefault,
            IsActive = warehouse.IsActive
        };
        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var warehouse = await dbContext.Warehouses.SingleOrDefaultAsync(x => x.Id == id);
        if (warehouse is null)
        {
            return NotFound();
        }

        if (await dbContext.StockMovements.AnyAsync(x => x.WarehouseId == id))
        {
            TempData["Error"] = "Bu depoya ait stok hareketleri var, silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        dbContext.Warehouses.Remove(warehouse);
        try
        {
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Depo silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Bu depoya bağlı kayıtlar var, silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.Warehouses.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.Warehouses.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var hasMovements = await dbContext.StockMovements.AnyAsync(x => x.WarehouseId == id);

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "Warehouses",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = !hasMovements,
            DeleteBlockedReason = hasMovements ? "Bu depoya ait stok hareketleri var, silinemez." : null
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WarehouseFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var warehouse = await dbContext.Warehouses.SingleOrDefaultAsync(x => x.Id == id);
        if (warehouse is null)
        {
            return NotFound();
        }

        Map(form, warehouse);
        warehouse.UpdatedAtUtc = DateTime.UtcNow;

        if (warehouse.IsDefault)
        {
            await ClearOtherDefaultsAsync(warehouse.Id);
        }

        if (!await TrySaveAsync())
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        TempData["Success"] = "Depo güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task ClearOtherDefaultsAsync(int? exceptId = null)
    {
        var others = await dbContext.Warehouses
            .Where(x => x.IsDefault && x.Id != (exceptId ?? -1))
            .ToListAsync();
        foreach (var other in others)
        {
            other.IsDefault = false;
        }
    }

    private async Task ValidateUniqueCodeAsync(WarehouseFormViewModel model)
    {
        if (await dbContext.Warehouses.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu depo kodu zaten kullanılıyor.");
        }
    }

    private static void Map(WarehouseFormViewModel source, Warehouse target)
    {
        target.Code = source.Code.Trim();
        target.Name = source.Name.Trim();
        target.BranchId = source.BranchId!.Value;
        target.IsDefault = source.IsDefault;
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
            ModelState.AddModelError(nameof(WarehouseFormViewModel.Code), "Bu depo kodu başka bir kayıtta kullanılıyor.");
            return false;
        }
    }

    private async Task PopulateSelectionsAsync(WarehouseFormViewModel model)
    {
        if (model.BranchId is int branchId)
        {
            model.BranchDisplay = await dbContext.Branches
                .Where(x => x.Id == branchId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }
    }
}
