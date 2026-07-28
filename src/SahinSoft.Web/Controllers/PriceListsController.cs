using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class PriceListsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.PriceLists.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    }

    public IActionResult Create()
    {
        return View("Form", new PriceListFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PriceListFormViewModel form)
    {
        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var priceList = new PriceList();
        Map(form, priceList);
        dbContext.PriceLists.Add(priceList);

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Fiyat tanımı kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var priceList = await dbContext.PriceLists.SingleOrDefaultAsync(x => x.Id == id);
        if (priceList is null)
        {
            return NotFound();
        }

        return View("Form", new PriceListFormViewModel
        {
            Id = priceList.Id,
            Code = priceList.Code,
            Name = priceList.Name,
            IsActive = priceList.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PriceListFormViewModel form)
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

        var priceList = await dbContext.PriceLists.SingleOrDefaultAsync(x => x.Id == id);
        if (priceList is null)
        {
            return NotFound();
        }

        Map(form, priceList);
        priceList.UpdatedAtUtc = DateTime.UtcNow;

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Fiyat tanımı güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task ValidateUniqueCodeAsync(PriceListFormViewModel model)
    {
        if (await dbContext.PriceLists.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu fiyat kodu zaten kullanılıyor.");
        }
    }

    private static void Map(PriceListFormViewModel source, PriceList target)
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
            ModelState.AddModelError(nameof(PriceListFormViewModel.Code), "Bu fiyat kodu başka bir kayıtta kullanılıyor.");
            return false;
        }
    }
}
