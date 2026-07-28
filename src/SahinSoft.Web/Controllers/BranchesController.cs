using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class BranchesController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.Branches.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    }

    public IActionResult Create()
    {
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "Branches" };
        return View("Form", new BranchFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BranchFormViewModel form)
    {
        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var branch = new Branch();
        Map(form, branch);
        dbContext.Branches.Add(branch);

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Şube kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var branch = await dbContext.Branches.SingleOrDefaultAsync(x => x.Id == id);
        if (branch is null)
        {
            return NotFound();
        }

        await SetToolbarAsync(id);
        return View("Form", new BranchFormViewModel
        {
            Id = branch.Id,
            Code = branch.Code,
            Name = branch.Name,
            Address = branch.Address,
            Phone = branch.Phone,
            IsHeadOffice = branch.IsHeadOffice,
            IsActive = branch.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var branch = await dbContext.Branches.SingleOrDefaultAsync(x => x.Id == id);
        if (branch is null)
        {
            return NotFound();
        }

        if (await dbContext.Warehouses.AnyAsync(x => x.BranchId == id))
        {
            TempData["Error"] = "Bu şubeye bağlı depolar var, silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        dbContext.Branches.Remove(branch);
        try
        {
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Şube silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Bu şubeye bağlı kayıtlar var, silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.Branches.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.Branches.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var hasWarehouses = await dbContext.Warehouses.AnyAsync(x => x.BranchId == id);

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "Branches",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = !hasWarehouses,
            DeleteBlockedReason = hasWarehouses ? "Bu şubeye bağlı depolar var, silinemez." : null
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BranchFormViewModel form)
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

        var branch = await dbContext.Branches.SingleOrDefaultAsync(x => x.Id == id);
        if (branch is null)
        {
            return NotFound();
        }

        Map(form, branch);
        branch.UpdatedAtUtc = DateTime.UtcNow;

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Şube güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task ValidateUniqueCodeAsync(BranchFormViewModel model)
    {
        if (await dbContext.Branches.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu şube kodu zaten kullanılıyor.");
        }
    }

    private static void Map(BranchFormViewModel source, Branch target)
    {
        target.Code = source.Code.Trim();
        target.Name = source.Name.Trim();
        target.Address = source.Address?.Trim();
        target.Phone = source.Phone?.Trim();
        target.IsHeadOffice = source.IsHeadOffice;
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
            ModelState.AddModelError(nameof(BranchFormViewModel.Code), "Bu şube kodu başka bir kayıtta kullanılıyor.");
            return false;
        }
    }
}
