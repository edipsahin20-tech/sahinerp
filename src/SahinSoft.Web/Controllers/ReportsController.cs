using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Common;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class ReportsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> StockMovements(int? productId, int? warehouseId, DateTime? from, DateTime? to)
    {
        var query = dbContext.StockMovements
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .OrderByDescending(x => x.MovementDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(x => x.ProductId == productId.Value);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == warehouseId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.MovementDateUtc >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.MovementDateUtc < to.Value.AddDays(1));
        }

        var movements = await query.Take(500).ToListAsync();

        var model = new StockMovementReportViewModel
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            From = from,
            To = to,
            TotalIn = movements.Where(x => x.Quantity > 0).Sum(x => x.Quantity),
            TotalOut = movements.Where(x => x.Quantity < 0).Sum(x => -x.Quantity),
            NetChange = movements.Sum(x => x.Quantity),
            Lines = movements
                .Select(x => new StockMovementReportLineViewModel
                {
                    MovementDateUtc = x.MovementDateUtc,
                    ProductName = x.Product.Name,
                    WarehouseName = x.Warehouse.Name,
                    MovementType = x.MovementType.GetDisplayName(),
                    Quantity = x.Quantity,
                    DocumentNumber = x.DocumentNumber,
                    Description = x.Description
                })
                .ToList(),
            Products = await dbContext.Products
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem($"{x.StockCode} - {x.Name}", x.Id.ToString()))
                .ToListAsync(),
            Warehouses = await dbContext.Warehouses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> StockReconciliation()
    {
        var movementTotals = await dbContext.StockMovements
            .AsNoTracking()
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Total);

        var products = await dbContext.Products
            .AsNoTracking()
            .Where(x => x.TrackStock)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.StockCode, x.Name, x.StockQuantity })
            .ToListAsync();

        var lines = products
            .Select(p =>
            {
                var movementTotal = movementTotals.GetValueOrDefault(p.Id);
                return new StockReconciliationLineViewModel
                {
                    ProductId = p.Id,
                    StockCode = p.StockCode,
                    ProductName = p.Name,
                    RecordedQuantity = p.StockQuantity,
                    MovementQuantity = movementTotal,
                    Difference = p.StockQuantity - movementTotal
                };
            })
            .Where(x => x.Difference != 0)
            .OrderByDescending(x => Math.Abs(x.Difference))
            .ToList();

        var model = new StockReconciliationReportViewModel
        {
            CheckedProductCount = products.Count,
            Lines = lines
        };

        return View(model);
    }
}
