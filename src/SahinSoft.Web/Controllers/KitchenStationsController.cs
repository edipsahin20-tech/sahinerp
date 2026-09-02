using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Mutfak istasyonları = restoran tarafında "yazıcı" kavramı (bkz. KitchenStation.PrinterName).
// Faz 1'den beri şemada vardı ama hiç yönetim ekranı yoktu - Edip'in "kategoriye göre birden
// fazla yazıcı" isteği (Izgara->Yazıcı 2, Soğuk İçecek->Yazıcı 3 gibi) için bu ekran gerekiyor.
[Authorize(Roles = AppRoles.Administrator)]
public sealed class KitchenStationsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.KitchenStations
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .ToListAsync());
    }

    public IActionResult Create()
    {
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "KitchenStations" };
        return View("Form", new KitchenStationFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KitchenStationFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        // Şimdilik tek şube (MERKEZ, Id=1) - bkz. §11 hibrit mimari notu, çok şubeli senkron
        // ayrı bir faz. Branch seçimi bu ekranda bilerek yok, gereksiz karmaşıklık.
        var defaultBranchId = await dbContext.Branches
            .Where(x => x.IsHeadOffice)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        var station = new KitchenStation { BranchId = defaultBranchId };
        Map(form, station);
        dbContext.KitchenStations.Add(station);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Mutfak istasyonu (yazıcı) kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var station = await dbContext.KitchenStations.SingleOrDefaultAsync(x => x.Id == id);
        if (station is null)
        {
            return NotFound();
        }

        await SetToolbarAsync(id);
        return View("Form", new KitchenStationFormViewModel
        {
            Id = station.Id,
            Name = station.Name,
            PrinterName = station.PrinterName,
            DisplayOrder = station.DisplayOrder,
            IsActive = station.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, KitchenStationFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var station = await dbContext.KitchenStations.SingleOrDefaultAsync(x => x.Id == id);
        if (station is null)
        {
            return NotFound();
        }

        Map(form, station);
        station.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Mutfak istasyonu (yazıcı) güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var station = await dbContext.KitchenStations.SingleOrDefaultAsync(x => x.Id == id);
        if (station is null)
        {
            return NotFound();
        }

        var inUse = await dbContext.Products.AnyAsync(x => x.DefaultKitchenStationId == id)
            || await dbContext.ProductCategories.AnyAsync(x => x.DefaultKitchenStationId == id);
        if (inUse)
        {
            TempData["Error"] = "Bu istasyonu kullanan ürün/kategori var, silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        dbContext.KitchenStations.Remove(station);
        try
        {
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Mutfak istasyonu silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Bu istasyona bağlı kayıtlar var, silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.KitchenStations.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.KitchenStations.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var inUse = await dbContext.Products.AnyAsync(x => x.DefaultKitchenStationId == id)
            || await dbContext.ProductCategories.AnyAsync(x => x.DefaultKitchenStationId == id);

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "KitchenStations",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = !inUse,
            DeleteBlockedReason = inUse ? "Bu istasyonu kullanan ürün/kategori var, silinemez." : null
        };
    }

    private static void Map(KitchenStationFormViewModel source, KitchenStation target)
    {
        target.Name = source.Name.Trim();
        target.PrinterName = string.IsNullOrWhiteSpace(source.PrinterName) ? null : source.PrinterName.Trim();
        target.DisplayOrder = source.DisplayOrder;
        target.IsActive = source.IsActive;
    }
}
