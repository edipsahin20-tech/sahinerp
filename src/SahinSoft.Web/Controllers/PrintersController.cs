using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services.Printing;

namespace SahinSoft.Web.Controllers;

// Yazıcı Yönetimi (Edip'in "YAZDIRMA / RAPOR TASARIM ALTYAPISI" talimatı, madde 3-4) - şube
// bazlı Adisyon/Mutfak/X/Z yazıcı tanımları. RestaurantCashRegistersController ile AYNI desen
// (Evrak toolbar, lookup-picker Şube seçimi).
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager}")]
public sealed class PrintersController(ApplicationDbContext dbContext, PrintDispatchService dispatchService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var printers = await dbContext.Printers
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.PrintTemplate)
            .OrderBy(x => x.Branch.Name).ThenBy(x => x.Role).ThenBy(x => x.Name)
            .ToListAsync();
        return View(printers);
    }

    public async Task<IActionResult> Create()
    {
        var model = new PrinterFormViewModel();
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "Printers" };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrinterFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var printer = new Printer();
        Map(form, printer);
        dbContext.Printers.Add(printer);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Yazıcı tanımı kaydedildi.";
        return RedirectToAction(nameof(Edit), new { id = printer.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var printer = await dbContext.Printers.SingleOrDefaultAsync(x => x.Id == id);
        if (printer is null)
        {
            return NotFound();
        }

        var model = new PrinterFormViewModel
        {
            Id = printer.Id,
            Name = printer.Name,
            Role = printer.Role,
            BranchId = printer.BranchId,
            ConnectionType = printer.ConnectionType,
            ConnectionAddress = printer.ConnectionAddress,
            AgentBaseUrl = printer.AgentBaseUrl,
            PaperWidth = printer.PaperWidth,
            AutoCut = printer.AutoCut,
            CopyCount = printer.CopyCount,
            PrintTemplateId = printer.PrintTemplateId,
            KitchenStationId = printer.KitchenStationId,
            IsActive = printer.IsActive
        };
        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PrinterFormViewModel form)
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

        var printer = await dbContext.Printers.SingleOrDefaultAsync(x => x.Id == id);
        if (printer is null)
        {
            return NotFound();
        }

        Map(form, printer);
        printer.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Yazıcı tanımı güncellendi.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var printer = await dbContext.Printers.SingleOrDefaultAsync(x => x.Id == id);
        if (printer is null)
        {
            return NotFound();
        }
        var hasJobs = await dbContext.PrintJobs.AnyAsync(x => x.PrinterId == id);
        if (hasJobs)
        {
            // Geçmiş yazdırma kayıtları FK Restrict ile korunuyor - silmek yerine pasifleştir.
            printer.IsActive = false;
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Bu yazıcıya ait geçmiş yazdırma kayıtları olduğu için silinmedi, pasif hale getirildi.";
        }
        else
        {
            dbContext.Printers.Remove(printer);
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Yazıcı tanımı silindi.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestPrint(int id)
    {
        var (jobId, result) = await dispatchService.TestPrintAsync(id);
        return Json(new { jobId, success = result.Success, error = result.Error });
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.Printers.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.Printers.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "Printers",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true
        };
    }

    private static void Map(PrinterFormViewModel source, Printer target)
    {
        target.Name = source.Name.Trim();
        target.Role = source.Role;
        target.BranchId = source.BranchId!.Value;
        target.ConnectionType = source.ConnectionType;
        target.ConnectionAddress = source.ConnectionAddress.Trim();
        target.AgentBaseUrl = string.IsNullOrWhiteSpace(source.AgentBaseUrl) ? null : source.AgentBaseUrl.Trim();
        target.PaperWidth = source.PaperWidth;
        target.AutoCut = source.AutoCut;
        target.CopyCount = source.CopyCount;
        target.PrintTemplateId = source.PrintTemplateId;
        target.KitchenStationId = source.Role == PrinterRole.Mutfak ? source.KitchenStationId : null;
        target.IsActive = source.IsActive;
    }

    private async Task PopulateSelectionsAsync(PrinterFormViewModel model)
    {
        if (model.BranchId is int branchId)
        {
            model.BranchDisplay = await dbContext.Branches
                .Where(x => x.Id == branchId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        model.AvailableTemplates = await dbContext.PrintTemplates
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.TemplateType).ThenBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.TemplateType })
            .ToListAsync() is var templates
            ? templates.Select(x => (x.Id, $"{x.Name} ({x.TemplateType})")).ToList()
            : [];

        model.AvailableKitchenStations = await dbContext.KitchenStations
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync() is var stations
            ? stations.Select(x => (x.Id, x.Name)).ToList()
            : [];
    }
}
