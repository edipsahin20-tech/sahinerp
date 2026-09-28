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

// Çıktı Tasarımcısı (Edip'in "YAZDIRMA / RAPOR TASARIM ALTYAPISI" talimatı, madde 1) - Adisyon/
// Mutfak/X/Z şablonlarını sürükle-bırak ile tasarlanabilir kılar. Tasarım değişince KOD
// DEĞİŞMEZ - LayoutJson veritabanında saklanır, PrintRenderingService AYNI JSON'ı hem burada
// (önizleme) hem gerçek yazdırmada (ESC/POS) yürütür.
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager}")]
public sealed class PrintTemplatesController(ApplicationDbContext dbContext, PrintRenderingService renderingService, IPrintDataProvider dataProvider, PrintDispatchService dispatchService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var templates = await dbContext.PrintTemplates
            .AsNoTracking()
            .OrderBy(x => x.TemplateType).ThenBy(x => x.Name)
            .Select(x => new PrintTemplateListItem(x.Id, x.Name, x.TemplateType, x.PaperWidthMm, x.IsDefault, x.IsActive, x.Version))
            .ToListAsync();

        return View(new PrintTemplateListViewModel { Items = templates });
    }

    public async Task<IActionResult> Designer(int? id, PrintTemplateType? type)
    {
        PrintTemplate? template = id.HasValue
            ? await dbContext.PrintTemplates.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id.Value)
            : null;

        if (template is null && id.HasValue)
        {
            return NotFound();
        }

        var effectiveType = template?.TemplateType ?? type ?? PrintTemplateType.Adisyon;

        var model = new PrintTemplateDesignerViewModel
        {
            Id = template?.Id ?? 0,
            Name = template?.Name ?? DefaultName(effectiveType),
            TemplateType = effectiveType,
            PaperWidthMm = template?.PaperWidthMm ?? 80,
            IsDefault = template?.IsDefault ?? false,
            Version = template?.Version ?? 1,
            LayoutJson = template?.LayoutJson ?? "[]",
            BranchId = template?.BranchId,
            Branches = await dbContext.Branches.AsNoTracking().OrderBy(x => x.Name).ToListAsync(),
            AvailableFields = PrintFieldRegistry.GetFields(effectiveType)
        };

        model.PreviewHtml = RenderPreview(model.LayoutJson, model.PaperWidthMm, effectiveType);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string name, PrintTemplateType templateType, int paperWidthMm, string layoutJson, int? branchId, bool setAsDefault)
    {
        var elements = PrintRenderingService.ParseLayout(layoutJson);
        var invalidKey = elements.FirstOrDefault(e => !PrintFieldRegistry.IsValidBindingKey(templateType, e.BindingKey));
        if (invalidKey != null)
        {
            return BadRequest(new { success = false, error = $"Geçersiz alan anahtarı: {invalidKey.BindingKey}" });
        }

        PrintTemplate template;
        if (id > 0)
        {
            template = await dbContext.PrintTemplates.SingleAsync(x => x.Id == id);
            template.Version++;
        }
        else
        {
            template = new PrintTemplate { TemplateType = templateType };
            dbContext.PrintTemplates.Add(template);
        }

        template.Name = string.IsNullOrWhiteSpace(name) ? DefaultName(templateType) : name.Trim();
        template.PaperWidthMm = paperWidthMm is 58 or 80 ? paperWidthMm : 80;
        template.LayoutJson = layoutJson;
        template.BranchId = branchId;
        template.IsActive = true;

        await dbContext.SaveChangesAsync();

        if (setAsDefault)
        {
            await SetDefaultInternalAsync(template.Id, template.TemplateType, template.BranchId);
        }

        return Json(new { success = true, id = template.Id, version = template.Version });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefault(int id)
    {
        var template = await dbContext.PrintTemplates.SingleOrDefaultAsync(x => x.Id == id);
        if (template is null)
        {
            return NotFound();
        }
        await SetDefaultInternalAsync(template.Id, template.TemplateType, template.BranchId);
        TempData["Success"] = "Varsayılan şablon olarak ayarlandı.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetDefaultInternalAsync(int templateId, PrintTemplateType type, int? branchId)
    {
        var siblings = await dbContext.PrintTemplates.Where(x => x.TemplateType == type && x.BranchId == branchId).ToListAsync();
        foreach (var s in siblings)
        {
            s.IsDefault = s.Id == templateId;
        }
        await dbContext.SaveChangesAsync();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var template = await dbContext.PrintTemplates.SingleOrDefaultAsync(x => x.Id == id);
        if (template is null)
        {
            return NotFound();
        }
        var inUse = await dbContext.Printers.AnyAsync(x => x.PrintTemplateId == id);
        if (inUse)
        {
            TempData["Success"] = "Bu şablon bir yazıcıya atanmış olduğu için silinmedi.";
            return RedirectToAction(nameof(Index));
        }
        dbContext.PrintTemplates.Remove(template);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Şablon silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult PreviewAjax(PrintTemplateType templateType, int paperWidthMm, string layoutJson)
    {
        return Json(new { html = RenderPreview(layoutJson, paperWidthMm, templateType) });
    }

    [HttpGet]
    public async Task<IActionResult> PrintersForType(PrintTemplateType templateType)
    {
        var role = templateType switch
        {
            PrintTemplateType.Adisyon => PrinterRole.Adisyon,
            PrintTemplateType.MutfakFisi => PrinterRole.Mutfak,
            PrintTemplateType.XRaporu => PrinterRole.XRaporu,
            PrintTemplateType.ZRaporu => PrinterRole.ZRaporu,
            _ => PrinterRole.Adisyon
        };
        var printers = await dbContext.Printers.AsNoTracking()
            .Where(x => x.Role == role && x.IsActive)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();
        return Json(printers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestPrintDraft(int printerId, PrintTemplateType templateType, int paperWidthMm, string layoutJson)
    {
        var (jobId, result) = await dispatchService.TestPrintDraftAsync(printerId, templateType, paperWidthMm, layoutJson);
        return Json(new { jobId, success = result.Success, error = result.Error });
    }

    private string RenderPreview(string layoutJson, int paperWidthMm, PrintTemplateType type)
    {
        var layout = PrintRenderingService.ParseLayout(layoutJson);
        var sample = dataProvider.BuildSample(type);
        return renderingService.RenderHtmlPreview(layout, paperWidthMm, sample);
    }

    private static string DefaultName(PrintTemplateType type) => type switch
    {
        PrintTemplateType.Adisyon => "Adisyon Fişi",
        PrintTemplateType.MutfakFisi => "Mutfak Fişi",
        PrintTemplateType.XRaporu => "X Raporu",
        PrintTemplateType.ZRaporu => "Z Raporu",
        _ => "Şablon"
    };
}
