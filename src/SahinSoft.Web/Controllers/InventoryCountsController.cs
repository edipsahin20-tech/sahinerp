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
public sealed class InventoryCountsController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator,
    InventoryCountPostingService inventoryCountPostingService) : Controller
{
    public async Task<IActionResult> Index(InventoryCountStatus? status, string? search)
    {
        var query = dbContext.InventoryCounts
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .OrderByDescending(x => x.CountDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.CountNumber.Contains(search));
        }

        ViewBag.Status = status;
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        var model = new InventoryCountFormViewModel
        {
            Lines = [new InventoryCountLineFormViewModel()]
        };
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "InventoryCounts" };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InventoryCountFormViewModel form)
    {
        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "InventoryCounts" };
            return View("Form", form);
        }

        var count = new InventoryCount
        {
            Status = InventoryCountStatus.Draft,
            CountNumber = await documentNumberGenerator.GenerateAsync("STOCK_COUNT"),
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
        };
        MapHeader(form, count);
        await MapLinesAsync(form, count);

        dbContext.InventoryCounts.Add(count);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Sayım taslağı oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = count.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var count = await dbContext.InventoryCounts
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (count is null)
        {
            return NotFound();
        }

        if (count.Status != InventoryCountStatus.Draft)
        {
            return BadRequest("Yalnızca taslak sayımlar düzenlenebilir.");
        }

        var model = new InventoryCountFormViewModel
        {
            Id = count.Id,
            CountNumber = count.CountNumber,
            WarehouseId = count.WarehouseId,
            CountDateUtc = count.CountDateUtc,
            Lines = count.Lines
                .Select(x => new InventoryCountLineFormViewModel
                {
                    ProductId = x.ProductId,
                    CountedQuantity = x.CountedQuantity
                })
                .ToList()
        };
        if (model.Lines.Count == 0)
        {
            model.Lines.Add(new InventoryCountLineFormViewModel());
        }

        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var count = await dbContext.InventoryCounts
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (count is null)
        {
            return NotFound();
        }

        if (count.Status != InventoryCountStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak sayımlar silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.InventoryCounts.Remove(count);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Sayım silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.InventoryCounts.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.InventoryCounts.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "InventoryCounts",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = true
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InventoryCountFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            await SetToolbarAsync(id);
            return View("Form", form);
        }

        var count = await dbContext.InventoryCounts
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (count is null)
        {
            return NotFound();
        }

        if (count.Status != InventoryCountStatus.Draft)
        {
            return BadRequest("Yalnızca taslak sayımlar düzenlenebilir.");
        }

        MapHeader(form, count);
        count.Lines.Clear();
        await MapLinesAsync(form, count);
        count.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Sayım taslağı güncellendi.";
        return RedirectToAction(nameof(Details), new { id = count.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var count = await dbContext.InventoryCounts
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (count is null)
        {
            return NotFound();
        }

        var model = new InventoryCountDetailsViewModel
        {
            Id = count.Id,
            Status = count.Status,
            CountNumber = count.CountNumber,
            CountDateUtc = count.CountDateUtc,
            WarehouseName = count.Warehouse.Name,
            ApprovedByUserId = count.ApprovedByUserId,
            ApprovedAtUtc = count.ApprovedAtUtc,
            Lines = count.Lines
                .Select(x => new InventoryCountDetailsLineViewModel
                {
                    ProductName = x.Product.Name,
                    SystemQuantity = x.SystemQuantity,
                    CountedQuantity = x.CountedQuantity
                })
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
            await inventoryCountPostingService.ApproveAsync(id, userId);
            TempData["Success"] = "Sayım onaylandı, stok farkları işlendi.";
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
            await inventoryCountPostingService.CancelAsync(id, userId, reason);
            TempData["Success"] = "Sayım iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private static void ValidateLines(InventoryCountFormViewModel form)
    {
        form.Lines = form.Lines.Where(x => x.ProductId is not null).ToList();
        if (form.Lines.Count == 0)
        {
            throw new InvalidOperationException("Sayımda en az bir satır bulunmalıdır.");
        }
    }

    private static void MapHeader(InventoryCountFormViewModel source, InventoryCount target)
    {
        target.WarehouseId = source.WarehouseId!.Value;
        target.CountDateUtc = DateTime.SpecifyKind(source.CountDateUtc, DateTimeKind.Utc);
    }

    private async Task MapLinesAsync(InventoryCountFormViewModel source, InventoryCount target)
    {
        var productIds = source.Lines.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToList();
        var validProductIds = await dbContext.Products.Where(x => productIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();

        foreach (var line in source.Lines)
        {
            if (line.ProductId is null || !validProductIds.Contains(line.ProductId.Value))
            {
                continue;
            }

            target.Lines.Add(new InventoryCountLine
            {
                ProductId = line.ProductId.Value,
                CountedQuantity = line.CountedQuantity
            });
        }
    }

    private async Task PopulateSelectionsAsync(InventoryCountFormViewModel model)
    {
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
