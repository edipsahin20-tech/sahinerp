using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager}")]
public sealed class RestaurantSectionsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var sections = await dbContext.RestaurantSections
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Tables)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .ToListAsync();
        return View(sections);
    }

    public async Task<IActionResult> Create()
    {
        var model = new RestaurantSectionFormViewModel();
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "RestaurantSections" };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantSectionFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var section = new RestaurantSection();
        Map(form, section);
        dbContext.RestaurantSections.Add(section);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Salon kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var section = await dbContext.RestaurantSections.SingleOrDefaultAsync(x => x.Id == id);
        if (section is null)
        {
            return NotFound();
        }

        var model = new RestaurantSectionFormViewModel
        {
            Id = section.Id,
            Name = section.Name,
            DisplayOrder = section.DisplayOrder,
            BranchId = section.BranchId,
            IsActive = section.IsActive
        };
        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RestaurantSectionFormViewModel form)
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

        var section = await dbContext.RestaurantSections.SingleOrDefaultAsync(x => x.Id == id);
        if (section is null)
        {
            return NotFound();
        }

        Map(form, section);
        section.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Salon güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.RestaurantSections.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.RestaurantSections.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "RestaurantSections",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = false
        };
    }

    private static void Map(RestaurantSectionFormViewModel source, RestaurantSection target)
    {
        target.Name = source.Name.Trim();
        target.DisplayOrder = source.DisplayOrder;
        target.BranchId = source.BranchId!.Value;
        target.IsActive = source.IsActive;
    }

    private async Task PopulateSelectionsAsync(RestaurantSectionFormViewModel model)
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
