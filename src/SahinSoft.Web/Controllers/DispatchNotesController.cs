using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class DispatchNotesController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator,
    DispatchNotePostingService dispatchNotePostingService) : Controller
{
    public async Task<IActionResult> Index(InvoiceType? type, BusinessDocumentStatus? status, string? search)
    {
        var query = dbContext.DispatchNotes
            .AsNoTracking()
            .Include(x => x.Customer)
            .OrderByDescending(x => x.DispatchDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(x => x.DispatchType == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.DispatchNumber.Contains(search) || x.Customer.Name.Contains(search));
        }

        ViewBag.Type = type;
        ViewBag.Status = status;
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create(InvoiceType type)
    {
        var model = new DispatchNoteFormViewModel
        {
            DispatchType = type,
            Lines = [new DispatchNoteLineFormViewModel()]
        };
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Controller = "DispatchNotes",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() }
        };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DispatchNoteFormViewModel form)
    {
        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var dispatch = new DispatchNote
        {
            DispatchType = form.DispatchType,
            Status = BusinessDocumentStatus.Draft,
            DispatchNumber = await documentNumberGenerator.GenerateAsync(
                form.DispatchType == InvoiceType.Sales ? "SALES_DISPATCH" : "PURCHASE_DISPATCH"),
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
        };
        MapHeader(form, dispatch);
        await MapLinesAsync(form, dispatch);

        dbContext.DispatchNotes.Add(dispatch);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "İrsaliye taslağı oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = dispatch.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dispatch = await dbContext.DispatchNotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (dispatch is null)
        {
            return NotFound();
        }

        if (dispatch.Status != BusinessDocumentStatus.Draft)
        {
            return BadRequest("Yalnızca taslak irsaliyeler düzenlenebilir.");
        }

        var model = new DispatchNoteFormViewModel
        {
            Id = dispatch.Id,
            DispatchType = dispatch.DispatchType,
            CustomerId = dispatch.CustomerId,
            WarehouseId = dispatch.WarehouseId,
            DispatchDateUtc = dispatch.DispatchDateUtc,
            VehiclePlate = dispatch.VehiclePlate,
            CarrierName = dispatch.CarrierName,
            Notes = dispatch.Notes,
            Lines = dispatch.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new DispatchNoteLineFormViewModel { ProductId = x.ProductId, Quantity = x.Quantity })
                .ToList()
        };
        if (model.Lines.Count == 0)
        {
            model.Lines.Add(new DispatchNoteLineFormViewModel());
        }

        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id, dispatch.DispatchType);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var dispatch = await dbContext.DispatchNotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (dispatch is null)
        {
            return NotFound();
        }

        if (dispatch.Status != BusinessDocumentStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak irsaliyeler silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.DispatchNotes.Remove(dispatch);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "İrsaliye silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id, InvoiceType type)
    {
        var previousId = await dbContext.DispatchNotes.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.DispatchNotes.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "DispatchNotes",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() },
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = true
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DispatchNoteFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var dispatch = await dbContext.DispatchNotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (dispatch is null)
        {
            return NotFound();
        }

        if (dispatch.Status != BusinessDocumentStatus.Draft)
        {
            return BadRequest("Yalnızca taslak irsaliyeler düzenlenebilir.");
        }

        MapHeader(form, dispatch);
        dispatch.Lines.Clear();
        await MapLinesAsync(form, dispatch);
        dispatch.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "İrsaliye taslağı güncellendi.";
        return RedirectToAction(nameof(Details), new { id = dispatch.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var dispatch = await dbContext.DispatchNotes
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Warehouse)
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (dispatch is null)
        {
            return NotFound();
        }

        var settings = await dbContext.CompanySettings.AsNoTracking().SingleAsync(x => x.Id == 1);

        var model = new DispatchNoteDetailsViewModel
        {
            Id = dispatch.Id,
            DispatchType = dispatch.DispatchType,
            Status = dispatch.Status,
            DispatchNumber = dispatch.DispatchNumber,
            DispatchDateUtc = dispatch.DispatchDateUtc,
            CustomerName = dispatch.Customer.Name,
            CustomerPhone = dispatch.Customer.Phone,
            CustomerEmail = dispatch.Customer.Email,
            CustomerAddress = dispatch.Customer.Address,
            WarehouseName = dispatch.Warehouse.Name,
            Company = new PdfCompanyInfoViewModel
            {
                CompanyName = settings.CompanyName,
                Website = settings.Website,
                Email = settings.Email,
                Phone = settings.Phone,
                BankName = settings.BankName,
                Iban = settings.Iban
            },
            VehiclePlate = dispatch.VehiclePlate,
            CarrierName = dispatch.CarrierName,
            Notes = dispatch.Notes,
            ApprovedByUserId = dispatch.ApprovedByUserId,
            ApprovedAtUtc = dispatch.ApprovedAtUtc,
            Lines = dispatch.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new DispatchNoteDetailsLineViewModel { ProductName = x.Product.Name, Quantity = x.Quantity })
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        try
        {
            await dispatchNotePostingService.ApproveAsync(id, userId);
            TempData["Success"] = "İrsaliye onaylandı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Cancel(int id, string reason)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        try
        {
            await dispatchNotePostingService.CancelAsync(id, userId, reason);
            TempData["Success"] = "İrsaliye iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private static void ValidateLines(DispatchNoteFormViewModel form)
    {
        form.Lines = form.Lines.Where(x => x.ProductId is not null).ToList();
        if (form.Lines.Count == 0)
        {
            throw new InvalidOperationException("İrsaliyede en az bir satır bulunmalıdır.");
        }
    }

    private static void MapHeader(DispatchNoteFormViewModel source, DispatchNote target)
    {
        target.CustomerId = source.CustomerId!.Value;
        target.WarehouseId = source.WarehouseId!.Value;
        target.DispatchDateUtc = DateTime.SpecifyKind(source.DispatchDateUtc, DateTimeKind.Utc);
        target.VehiclePlate = source.VehiclePlate?.Trim();
        target.CarrierName = source.CarrierName?.Trim();
        target.Notes = source.Notes?.Trim();
    }

    private async Task MapLinesAsync(DispatchNoteFormViewModel source, DispatchNote target)
    {
        var productIds = source.Lines.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToList();
        var validProductIds = await dbContext.Products.Where(x => productIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();

        var lineNumber = 1;
        foreach (var line in source.Lines)
        {
            if (line.ProductId is null || !validProductIds.Contains(line.ProductId.Value))
            {
                continue;
            }

            target.Lines.Add(new DispatchNoteLine
            {
                LineNumber = lineNumber++,
                ProductId = line.ProductId.Value,
                Quantity = line.Quantity
            });
        }
    }

    private async Task PopulateSelectionsAsync(DispatchNoteFormViewModel model)
    {
        if (model.CustomerId is int customerId)
        {
            model.CustomerDisplay = await dbContext.Customers
                .Where(x => x.Id == customerId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        if (model.WarehouseId is int warehouseId)
        {
            model.WarehouseDisplay = await dbContext.Warehouses
                .Where(x => x.Id == warehouseId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        var productIds = model.Lines.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToList();
        var productDisplays = await dbContext.Products
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, Display = x.StockCode + " - " + x.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Display);

        foreach (var line in model.Lines)
        {
            if (line.ProductId is int productId && productDisplays.TryGetValue(productId, out var display))
            {
                line.ProductDisplay = display;
            }
        }
    }
}
