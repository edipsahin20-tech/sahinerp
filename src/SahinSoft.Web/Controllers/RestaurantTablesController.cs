using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager}")]
public sealed class RestaurantTablesController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var tables = await dbContext.RestaurantTables
            .AsNoTracking()
            .Include(x => x.RestaurantSection)
            .OrderBy(x => x.RestaurantSection.DisplayOrder).ThenBy(x => x.Name)
            .ToListAsync();
        return View(tables);
    }

    public async Task<IActionResult> Create()
    {
        var model = new RestaurantTableFormViewModel();
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "RestaurantTables" };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantTableFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var table = new RestaurantTable();
        Map(form, table);
        dbContext.RestaurantTables.Add(table);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Masa kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var table = await dbContext.RestaurantTables.SingleOrDefaultAsync(x => x.Id == id);
        if (table is null)
        {
            return NotFound();
        }

        var model = new RestaurantTableFormViewModel
        {
            Id = table.Id,
            Name = table.Name,
            Capacity = table.Capacity,
            RestaurantSectionId = table.RestaurantSectionId,
            IsActive = table.IsActive
        };
        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RestaurantTableFormViewModel form)
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

        var table = await dbContext.RestaurantTables.SingleOrDefaultAsync(x => x.Id == id);
        if (table is null)
        {
            return NotFound();
        }

        Map(form, table);
        table.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Masa güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.RestaurantTables.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.RestaurantTables.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "RestaurantTables",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = false
        };
    }

    private static void Map(RestaurantTableFormViewModel source, RestaurantTable target)
    {
        target.Name = source.Name.Trim();
        target.Capacity = source.Capacity;
        target.RestaurantSectionId = source.RestaurantSectionId!.Value;
        target.IsActive = source.IsActive;
    }

    private async Task PopulateSelectionsAsync(RestaurantTableFormViewModel model)
    {
        model.Sections = await dbContext.RestaurantSections
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();
    }
}
