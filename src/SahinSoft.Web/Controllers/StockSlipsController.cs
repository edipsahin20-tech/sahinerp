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
public sealed class StockSlipsController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator,
    StockSlipPostingService stockSlipPostingService) : Controller
{
    public async Task<IActionResult> Index(StockSlipType? type, StockSlipStatus? status, string? search)
    {
        var query = dbContext.StockSlips
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .OrderByDescending(x => x.SlipDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(x => x.SlipType == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.SlipNumber.Contains(search));
        }

        ViewBag.Type = type;
        ViewBag.Status = status;
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create(StockSlipType type)
    {
        var model = new StockSlipFormViewModel
        {
            SlipType = type,
            Lines = [new StockSlipLineFormViewModel()]
        };
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Controller = "StockSlips",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() }
        };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockSlipFormViewModel form)
    {
        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var slip = new StockSlip
        {
            SlipType = form.SlipType,
            Status = StockSlipStatus.Draft,
            SlipNumber = await documentNumberGenerator.GenerateAsync(
                form.SlipType == StockSlipType.Receipt ? "STOCK_RECEIPT" : "STOCK_ISSUE"),
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
        };
        MapHeader(form, slip);
        await MapLinesAsync(form, slip);

        dbContext.StockSlips.Add(slip);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Stok fişi taslağı oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = slip.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var slip = await dbContext.StockSlips
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (slip is null)
        {
            return NotFound();
        }

        if (slip.Status != StockSlipStatus.Draft)
        {
            return BadRequest("Yalnızca taslak fişler düzenlenebilir.");
        }

        var model = new StockSlipFormViewModel
        {
            Id = slip.Id,
            SlipType = slip.SlipType,
            WarehouseId = slip.WarehouseId,
            SlipDateUtc = slip.SlipDateUtc,
            Description = slip.Description,
            Lines = slip.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new StockSlipLineFormViewModel
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitCost = x.UnitCost,
                    Description = x.Description
                })
                .ToList()
        };
        if (model.Lines.Count == 0)
        {
            model.Lines.Add(new StockSlipLineFormViewModel());
        }

        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id, slip.SlipType);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var slip = await dbContext.StockSlips
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (slip is null)
        {
            return NotFound();
        }

        if (slip.Status != StockSlipStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak fişler silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.StockSlips.Remove(slip);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Stok fişi silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id, StockSlipType type)
    {
        var previousId = await dbContext.StockSlips.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.StockSlips.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "StockSlips",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() },
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = true
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StockSlipFormViewModel form)
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

        var slip = await dbContext.StockSlips
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (slip is null)
        {
            return NotFound();
        }

        if (slip.Status != StockSlipStatus.Draft)
        {
            return BadRequest("Yalnızca taslak fişler düzenlenebilir.");
        }

        MapHeader(form, slip);
        slip.Lines.Clear();
        await MapLinesAsync(form, slip);
        slip.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Stok fişi taslağı güncellendi.";
        return RedirectToAction(nameof(Details), new { id = slip.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var slip = await dbContext.StockSlips
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (slip is null)
        {
            return NotFound();
        }

        var model = new StockSlipDetailsViewModel
        {
            Id = slip.Id,
            SlipType = slip.SlipType,
            Status = slip.Status,
            SlipNumber = slip.SlipNumber,
            SlipDateUtc = slip.SlipDateUtc,
            WarehouseName = slip.Warehouse.Name,
            Description = slip.Description,
            ApprovedByUserId = slip.ApprovedByUserId,
            ApprovedAtUtc = slip.ApprovedAtUtc,
            Lines = slip.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new StockSlipDetailsLineViewModel
                {
                    ProductName = x.Product.Name,
                    Quantity = x.Quantity,
                    UnitCost = x.UnitCost,
                    Description = x.Description
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
            await stockSlipPostingService.ApproveAsync(id, userId);
            TempData["Success"] = "Stok fişi onaylandı.";
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
            await stockSlipPostingService.CancelAsync(id, userId, reason);
            TempData["Success"] = "Stok fişi iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private static void ValidateLines(StockSlipFormViewModel form)
    {
        form.Lines = form.Lines.Where(x => x.ProductId is not null).ToList();
        if (form.Lines.Count == 0)
        {
            throw new InvalidOperationException("Fişte en az bir satır bulunmalıdır.");
        }
    }

    private static void MapHeader(StockSlipFormViewModel source, StockSlip target)
    {
        target.WarehouseId = source.WarehouseId!.Value;
        target.SlipDateUtc = DateTime.SpecifyKind(source.SlipDateUtc, DateTimeKind.Utc);
        target.Description = source.Description?.Trim();
    }

    private async Task MapLinesAsync(StockSlipFormViewModel source, StockSlip target)
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

            target.Lines.Add(new StockSlipLine
            {
                LineNumber = lineNumber++,
                ProductId = line.ProductId.Value,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                Description = line.Description?.Trim()
            });
        }
    }

    private async Task PopulateSelectionsAsync(StockSlipFormViewModel model)
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
