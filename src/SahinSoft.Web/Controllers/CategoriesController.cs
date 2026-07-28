using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class CategoriesController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = dbContext.ProductCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        }

        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public IActionResult Create()
    {
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "Categories" };
        return View("Form", new CategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel form)
    {
        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var category = new ProductCategory();
        Map(form, category);
        dbContext.ProductCategories.Add(category);

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Kategori kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await dbContext.ProductCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        var model = new CategoryFormViewModel
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            WebsitePath = category.WebsitePath,
            IsActive = category.IsActive
        };

        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await dbContext.ProductCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        if (await dbContext.Products.AnyAsync(x => x.CategoryId == id))
        {
            TempData["Error"] = "Bu kategoriye bağlı ürünler var, silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        dbContext.ProductCategories.Remove(category);
        try
        {
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Kategori silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Bu kategoriye bağlı kayıtlar var, silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.ProductCategories.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.ProductCategories.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var hasProducts = await dbContext.Products.AnyAsync(x => x.CategoryId == id);

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "Categories",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = !hasProducts,
            DeleteBlockedReason = hasProducts ? "Bu kategoriye bağlı ürünler var, silinemez." : null
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel form)
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

        var category = await dbContext.ProductCategories.SingleOrDefaultAsync(x => x.Id == id);
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

        TempData["Success"] = "Kategori güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task ValidateUniqueCodeAsync(CategoryFormViewModel model)
    {
        if (await dbContext.ProductCategories.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu kategori kodu zaten kullanılıyor.");
        }
    }

    private static void Map(CategoryFormViewModel source, ProductCategory target)
    {
        target.Code = source.Code.Trim();
        target.Name = source.Name.Trim();
        target.WebsitePath = string.IsNullOrWhiteSpace(source.WebsitePath) ? null : source.WebsitePath.Trim();
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
            ModelState.AddModelError(nameof(CategoryFormViewModel.Code), "Bu kategori kodu başka bir kayıtta kullanılıyor.");
            return false;
        }
    }
}
