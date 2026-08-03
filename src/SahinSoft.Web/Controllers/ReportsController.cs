using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;
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

    public async Task<IActionResult> LastPurchasePrices(int? productId)
    {
        var query = dbContext.InvoiceLines
            .AsNoTracking()
            .Include(x => x.Invoice).ThenInclude(x => x.Customer)
            .Include(x => x.Product)
            .Where(x =>
                x.Invoice.InvoiceType == InvoiceType.Purchase &&
                x.Invoice.Status == InvoiceStatus.Approved &&
                x.ProductId != null)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(x => x.ProductId == productId.Value);
        }

        var purchaseLines = await query.ToListAsync();

        var lines = purchaseLines
            .GroupBy(x => new { x.ProductId, x.Invoice.CustomerId })
            .Select(g =>
            {
                var last = g.OrderByDescending(x => x.Invoice.InvoiceDateUtc).ThenByDescending(x => x.Invoice.Id).First();
                return new LastPurchasePriceLineViewModel
                {
                    ProductId = last.ProductId!.Value,
                    StockCode = last.Product!.StockCode,
                    ProductName = last.Product.Name,
                    SupplierName = last.Invoice.Customer.Name,
                    LastPurchaseDateUtc = last.Invoice.InvoiceDateUtc,
                    UnitPriceInclTax = Math.Round(last.UnitPrice * (1 + last.TaxRate / 100), 2, MidpointRounding.AwayFromZero),
                    Quantity = last.Quantity,
                    InvoiceNumber = last.Invoice.InvoiceNumber,
                    PurchaseCount = g.Select(x => x.InvoiceId).Distinct().Count()
                };
            })
            .OrderBy(x => x.ProductName)
            .ThenByDescending(x => x.LastPurchaseDateUtc)
            .ToList();

        var model = new LastPurchasePricesReportViewModel
        {
            ProductId = productId,
            Lines = lines,
            Products = await dbContext.Products
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem($"{x.StockCode} - {x.Name}", x.Id.ToString()))
                .ToListAsync()
        };

        return View(model);
    }

    // Kasa/Banka Hareketleri — Stok Hareketleri ile aynı desen. accountType ("Cash"/"Bank") verilirse
    // Kasa Hareket / Banka Hareket giriş noktalarından geldiği anlaşılır ve hesap listesi + başlık ona
    // göre daraltılır; financialAccountId ile tek bir hesaba daha da daraltılabilir.
    public async Task<IActionResult> FinancialTransactions(int? financialAccountId, string? accountType, DateTime? from, DateTime? to)
    {
        FinancialAccountType? parsedAccountType = accountType switch
        {
            "Cash" => FinancialAccountType.Cash,
            "Bank" => FinancialAccountType.Bank,
            _ => null
        };

        var query = dbContext.FinancialTransactions
            .AsNoTracking()
            .Include(x => x.FinancialAccount)
            .Include(x => x.Customer)
            .OrderByDescending(x => x.TransactionDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (financialAccountId.HasValue)
        {
            query = query.Where(x => x.FinancialAccountId == financialAccountId.Value);
        }
        else if (parsedAccountType.HasValue)
        {
            query = query.Where(x => x.FinancialAccount.AccountType == parsedAccountType.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.TransactionDateUtc >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.TransactionDateUtc < to.Value.AddDays(1));
        }

        var transactions = await query.Take(500).ToListAsync();

        var lines = transactions
            .Select(x => new FinancialTransactionReportLineViewModel
            {
                TransactionDateUtc = x.TransactionDateUtc,
                AccountName = x.FinancialAccount.Name,
                TransactionType = x.TransactionType.GetDisplayName(),
                IsIncoming = x.TransactionType is FinancialTransactionType.Collection or FinancialTransactionType.TransferIn or FinancialTransactionType.Opening,
                Amount = x.Amount,
                DocumentNumber = x.DocumentNumber,
                Description = x.Description,
                CustomerName = x.Customer != null ? x.Customer.Name : null
            })
            .ToList();

        var accountsQuery = dbContext.FinancialAccounts.AsNoTracking().Where(x => x.IsActive);
        if (parsedAccountType.HasValue)
        {
            accountsQuery = accountsQuery.Where(x => x.AccountType == parsedAccountType.Value);
        }

        var model = new FinancialTransactionReportViewModel
        {
            FinancialAccountId = financialAccountId,
            AccountType = accountType,
            From = from,
            To = to,
            PageTitle = parsedAccountType switch
            {
                FinancialAccountType.Cash => "Kasa Hareketleri",
                FinancialAccountType.Bank => "Banka Hareketleri",
                _ => "Kasa/Banka Hareketleri"
            },
            TotalIn = lines.Where(x => x.IsIncoming).Sum(x => x.Amount),
            TotalOut = lines.Where(x => !x.IsIncoming).Sum(x => x.Amount),
            NetChange = lines.Sum(x => x.IsIncoming ? x.Amount : -x.Amount),
            Lines = lines,
            FinancialAccounts = await accountsQuery
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem($"{x.Code} - {x.Name}", x.Id.ToString()))
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
