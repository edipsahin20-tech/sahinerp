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
public sealed class StockTransfersController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator,
    StockTransferService stockTransferService) : Controller
{
    public async Task<IActionResult> Index(StockTransferStatus? status, string? search)
    {
        var query = dbContext.StockTransfers
            .AsNoTracking()
            .Include(x => x.FromWarehouse)
            .Include(x => x.ToWarehouse)
            .OrderByDescending(x => x.TransferDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.TransferNumber.Contains(search));
        }

        ViewBag.Status = status;
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        var model = new StockTransferFormViewModel
        {
            Lines = [new StockTransferLineFormViewModel()]
        };
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "StockTransfers" };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockTransferFormViewModel form)
    {
        ValidateLines(form);
        if (form.FromWarehouseId.HasValue && form.FromWarehouseId == form.ToWarehouseId)
        {
            ModelState.AddModelError(nameof(form.ToWarehouseId), "Çıkış ve giriş deposu aynı olamaz.");
        }
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "StockTransfers" };
            return View("Form", form);
        }

        var transfer = new StockTransfer
        {
            Status = StockTransferStatus.Draft,
            TransferNumber = await documentNumberGenerator.GenerateAsync("STOCK_TRANSFER"),
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
        };
        MapHeader(form, transfer);
        await MapLinesAsync(form, transfer);

        dbContext.StockTransfers.Add(transfer);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Transfer taslağı oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = transfer.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var transfer = await dbContext.StockTransfers
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (transfer is null)
        {
            return NotFound();
        }

        if (transfer.Status != StockTransferStatus.Draft)
        {
            return BadRequest("Yalnızca taslak transferler düzenlenebilir.");
        }

        var model = new StockTransferFormViewModel
        {
            Id = transfer.Id,
            TransferNumber = transfer.TransferNumber,
            FromWarehouseId = transfer.FromWarehouseId,
            ToWarehouseId = transfer.ToWarehouseId,
            TransferDateUtc = transfer.TransferDateUtc,
            Description = transfer.Description,
            Lines = transfer.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new StockTransferLineFormViewModel { ProductId = x.ProductId, Quantity = x.Quantity })
                .ToList()
        };
        if (model.Lines.Count == 0)
        {
            model.Lines.Add(new StockTransferLineFormViewModel());
        }

        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var transfer = await dbContext.StockTransfers
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (transfer is null)
        {
            return NotFound();
        }

        if (transfer.Status != StockTransferStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak transferler silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.StockTransfers.Remove(transfer);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Transfer silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.StockTransfers.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.StockTransfers.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "StockTransfers",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = true
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StockTransferFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        ValidateLines(form);
        if (form.FromWarehouseId.HasValue && form.FromWarehouseId == form.ToWarehouseId)
        {
            ModelState.AddModelError(nameof(form.ToWarehouseId), "Çıkış ve giriş deposu aynı olamaz.");
        }
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            await SetToolbarAsync(id);
            return View("Form", form);
        }

        var transfer = await dbContext.StockTransfers
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (transfer is null)
        {
            return NotFound();
        }

        if (transfer.Status != StockTransferStatus.Draft)
        {
            return BadRequest("Yalnızca taslak transferler düzenlenebilir.");
        }

        MapHeader(form, transfer);
        transfer.Lines.Clear();
        await MapLinesAsync(form, transfer);
        transfer.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Transfer taslağı güncellendi.";
        return RedirectToAction(nameof(Details), new { id = transfer.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var transfer = await dbContext.StockTransfers
            .AsNoTracking()
            .Include(x => x.FromWarehouse)
            .Include(x => x.ToWarehouse)
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (transfer is null)
        {
            return NotFound();
        }

        var model = new StockTransferDetailsViewModel
        {
            Id = transfer.Id,
            Status = transfer.Status,
            TransferNumber = transfer.TransferNumber,
            TransferDateUtc = transfer.TransferDateUtc,
            FromWarehouseName = transfer.FromWarehouse.Name,
            ToWarehouseName = transfer.ToWarehouse.Name,
            Description = transfer.Description,
            ApprovedByUserId = transfer.ApprovedByUserId,
            ApprovedAtUtc = transfer.ApprovedAtUtc,
            Lines = transfer.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new StockTransferDetailsLineViewModel { ProductName = x.Product.Name, Quantity = x.Quantity })
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
            await stockTransferService.ApproveAsync(id, userId);
            TempData["Success"] = "Transfer onaylandı.";
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
            await stockTransferService.CancelAsync(id, userId, reason);
            TempData["Success"] = "Transfer iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private static void ValidateLines(StockTransferFormViewModel form)
    {
        form.Lines = form.Lines.Where(x => x.ProductId is not null).ToList();
        if (form.Lines.Count == 0)
        {
            throw new InvalidOperationException("Transferde en az bir satır bulunmalıdır.");
        }
    }

    private static void MapHeader(StockTransferFormViewModel source, StockTransfer target)
    {
        target.FromWarehouseId = source.FromWarehouseId!.Value;
        target.ToWarehouseId = source.ToWarehouseId!.Value;
        target.TransferDateUtc = DateTime.SpecifyKind(source.TransferDateUtc, DateTimeKind.Utc);
        target.Description = source.Description?.Trim();
    }

    private async Task MapLinesAsync(StockTransferFormViewModel source, StockTransfer target)
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

            target.Lines.Add(new StockTransferLine
            {
                LineNumber = lineNumber++,
                ProductId = line.ProductId.Value,
                Quantity = line.Quantity
            });
        }
    }

    private async Task PopulateSelectionsAsync(StockTransferFormViewModel model)
    {
        if (model.FromWarehouseId is int fromWarehouseId)
        {
            model.FromWarehouseDisplay = await dbContext.Warehouses
                .Where(x => x.Id == fromWarehouseId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        if (model.ToWarehouseId is int toWarehouseId)
        {
            model.ToWarehouseDisplay = await dbContext.Warehouses
                .Where(x => x.Id == toWarehouseId)
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
