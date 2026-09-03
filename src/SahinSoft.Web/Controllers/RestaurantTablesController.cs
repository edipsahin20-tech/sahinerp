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

    // Toplu masa açma - Edip'in örneği (2026-09-03): "Salon-" + Aralık 1-20 -> Salon-1..Salon-20,
    // seçilen Lokasyon (Branch) altındaki bir Grup'a (RestaurantSection) bağlı olarak. Tekil masa
    // düzenleme hâlâ Edit'te (Form.cshtml), bu ekran sadece YENİ masa açmak için.
    public async Task<IActionResult> Create()
    {
        var model = new RestaurantTableBulkCreateViewModel();
        await PopulateBulkSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "RestaurantTables" };
        return View("BulkCreate", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantTableBulkCreateViewModel form)
    {
        if (form.RangeFrom > form.RangeTo)
        {
            ModelState.AddModelError(nameof(form.RangeTo), "Aralık bitişi başlangıçtan küçük olamaz.");
        }
        else if (form.RangeTo - form.RangeFrom + 1 > 200)
        {
            ModelState.AddModelError(nameof(form.RangeTo), "Tek seferde en fazla 200 masa açılabilir.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateBulkSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "RestaurantTables" };
            return View("BulkCreate", form);
        }

        var prefix = form.NamePrefix.Trim();
        var wantedNames = Enumerable.Range(form.RangeFrom, form.RangeTo - form.RangeFrom + 1)
            .Select(i => $"{prefix}{i}")
            .ToList();

        var existingNames = await dbContext.RestaurantTables
            .Where(x => x.RestaurantSectionId == form.RestaurantSectionId!.Value && wantedNames.Contains(x.Name))
            .Select(x => x.Name)
            .ToListAsync();

        var newNames = wantedNames.Where(x => !existingNames.Contains(x)).ToList();
        foreach (var name in newNames)
        {
            dbContext.RestaurantTables.Add(new RestaurantTable
            {
                Name = name,
                Capacity = form.Capacity,
                RestaurantSectionId = form.RestaurantSectionId!.Value,
                IsActive = true
            });
        }
        await dbContext.SaveChangesAsync();

        TempData["Success"] = existingNames.Count == 0
            ? $"{newNames.Count} masa açıldı."
            : $"{newNames.Count} masa açıldı, {existingNames.Count} tanesi zaten vardı ({string.Join(", ", existingNames)}) ve atlandı.";
        return RedirectToAction(nameof(Create));
    }

    // Lokasyon (Branch) değişince Grup (RestaurantSection) listesini o şubeye göre yeniden
    // doldurmak için - bkz. BulkCreate.cshtml'deki JS.
    [HttpGet]
    public async Task<IActionResult> SectionsForBranch(int branchId)
    {
        var sections = await dbContext.RestaurantSections
            .AsNoTracking()
            .Where(x => x.IsActive && x.BranchId == branchId)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new { id = x.Id, name = x.Name })
            .ToListAsync();
        return Json(sections);
    }

    // "Grup" alanının yanındaki "+" kısayolu - sayfadan ayrılmadan yeni bir RestaurantSection
    // açar (Edip'in mockup'ındaki inline ekleme, 2026-09-03).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSectionInline(string name, int branchId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Grup adı girilmelidir.");
        }

        var maxOrder = await dbContext.RestaurantSections
            .Where(x => x.BranchId == branchId)
            .Select(x => (int?)x.DisplayOrder)
            .MaxAsync() ?? 0;

        var section = new RestaurantSection
        {
            Name = name.Trim(),
            BranchId = branchId,
            DisplayOrder = maxOrder + 1,
            IsActive = true
        };
        dbContext.RestaurantSections.Add(section);
        await dbContext.SaveChangesAsync();

        return Json(new { id = section.Id, name = section.Name });
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

    private async Task PopulateBulkSelectionsAsync(RestaurantTableBulkCreateViewModel model)
    {
        model.Branches = await dbContext.Branches
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.IsHeadOffice).ThenBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();

        var branchId = model.BranchId ?? int.Parse(model.Branches.FirstOrDefault()?.Value ?? "0");
        model.BranchId ??= branchId;
        model.Sections = await dbContext.RestaurantSections
            .Where(x => x.IsActive && x.BranchId == branchId)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();
    }
}
