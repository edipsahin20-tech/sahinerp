using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class TaxRatesController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.TaxRates.AsNoTracking().OrderBy(x => x.Rate).ToListAsync());
    }

    public IActionResult Create()
    {
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "TaxRates" };
        return View("Form", new TaxRateFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaxRateFormViewModel form)
    {
        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var taxRate = new TaxRate();
        Map(form, taxRate);
        dbContext.TaxRates.Add(taxRate);

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "KDV oranı kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var taxRate = await dbContext.TaxRates.SingleOrDefaultAsync(x => x.Id == id);
        if (taxRate is null)
        {
            return NotFound();
        }

        await SetToolbarAsync(id);
        return View("Form", new TaxRateFormViewModel
        {
            Id = taxRate.Id,
            Code = taxRate.Code,
            Name = taxRate.Name,
            Rate = taxRate.Rate,
            IsExempt = taxRate.IsExempt,
            IsActive = taxRate.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var taxRate = await dbContext.TaxRates.SingleOrDefaultAsync(x => x.Id == id);
        if (taxRate is null)
        {
            return NotFound();
        }

        if (await dbContext.Products.AnyAsync(x => x.TaxRateId == id))
        {
            TempData["Error"] = "Bu KDV oranını kullanan ürünler var, silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        dbContext.TaxRates.Remove(taxRate);
        try
        {
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "KDV oranı silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Bu KDV oranına bağlı kayıtlar var, silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.TaxRates.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.TaxRates.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var hasProducts = await dbContext.Products.AnyAsync(x => x.TaxRateId == id);

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "TaxRates",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = !hasProducts,
            DeleteBlockedReason = hasProducts ? "Bu KDV oranını kullanan ürünler var, silinemez." : null
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaxRateFormViewModel form)
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

        var taxRate = await dbContext.TaxRates.SingleOrDefaultAsync(x => x.Id == id);
        if (taxRate is null)
        {
            return NotFound();
        }

        Map(form, taxRate);
        taxRate.UpdatedAtUtc = DateTime.UtcNow;

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "KDV oranı güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task ValidateUniqueCodeAsync(TaxRateFormViewModel model)
    {
        if (await dbContext.TaxRates.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu KDV kodu zaten kullanılıyor.");
        }
    }

    private static void Map(TaxRateFormViewModel source, TaxRate target)
    {
        target.Code = source.Code.Trim();
        target.Name = source.Name.Trim();
        target.Rate = source.Rate;
        target.IsExempt = source.IsExempt;
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
            ModelState.AddModelError(nameof(TaxRateFormViewModel.Code), "Bu KDV kodu başka bir kayıtta kullanılıyor.");
            return false;
        }
    }
}
