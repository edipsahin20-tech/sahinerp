using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class LookupController(
    ApplicationDbContext dbContext,
    RoleManager<IdentityRole> roleManager) : Controller
{
    private const int MaxResults = 25;

    // Türkçe karakter/büyük-küçük harf duyarsız arama (İ/I/ı/i, ç/ş/ğ/ü/ö vb. fark etmeksizin eşleşir).
    private const string TurkishInsensitive = "Turkish_CI_AI";

    public async Task<IActionResult> Products(string? q)
    {
        var query = dbContext.Products.AsNoTracking().Include(x => x.TaxRate).Include(x => x.Category).Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.StockCode, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Name, TurkishInsensitive).Contains(q) ||
                (x.Barcode != null && x.Barcode == q));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Take(MaxResults)
            .Select(x => new
            {
                id = x.Id,
                code = x.StockCode,
                name = x.Name,
                category = x.Category.Name,
                // Arama penceresinde gösterilen fiyat stok kartındaki KDV dahil tutardır (Alış/Satış
                // Fiyatı). unitPrice/purchaseUnitPrice ise fatura/teklif satırının Birim Fiyat alanına
                // yazılacak KDV hariç tutardır — stok kartının KDV oranına göre hesaplanır.
                salePrice = x.SalePrice,
                purchasePrice = x.PurchasePrice,
                unitPrice = Math.Round(x.SalePrice / (1 + x.TaxRate.Rate / 100), 3, MidpointRounding.AwayFromZero),
                purchaseUnitPrice = Math.Round(x.PurchasePrice / (1 + x.TaxRate.Rate / 100), 3, MidpointRounding.AwayFromZero),
                taxRate = x.TaxRate.Rate,
                unit = x.Unit,
                stock = x.StockQuantity
            })
            .ToListAsync();

        return Json(new { items });
    }

    public async Task<IActionResult> Customers(string? q, string? type)
    {
        var query = dbContext.Customers.AsNoTracking().Where(x => x.IsActive);
        if (type == "Customer")
        {
            query = query.Where(x => x.IsCustomer);
        }
        else if (type == "Supplier")
        {
            query = query.Where(x => x.IsSupplier);
        }
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.Code, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Name, TurkishInsensitive).Contains(q));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Take(MaxResults)
            .Select(x => new
            {
                id = x.Id,
                code = x.Code,
                name = x.Name,
                debit = dbContext.CurrentAccountTransactions.Where(t => t.CustomerId == x.Id).Sum(t => (decimal?)t.Debit) ?? 0,
                credit = dbContext.CurrentAccountTransactions.Where(t => t.CustomerId == x.Id).Sum(t => (decimal?)t.Credit) ?? 0
            })
            .ToListAsync();

        return Json(new { items });
    }

    public async Task<IActionResult> Categories(string? q)
    {
        var query = dbContext.ProductCategories.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.Code, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Name, TurkishInsensitive).Contains(q));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Take(MaxResults)
            .Select(x => new { id = x.Id, code = x.Code, name = x.Name })
            .ToListAsync();

        return Json(new { items });
    }

    public async Task<IActionResult> ExpenseCategories(string? q)
    {
        var query = dbContext.ExpenseCategories.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.Code, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Name, TurkishInsensitive).Contains(q));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Take(MaxResults)
            .Select(x => new { id = x.Id, code = x.Code, name = x.Name })
            .ToListAsync();

        return Json(new { items });
    }

    public async Task<IActionResult> TaxRates(string? q)
    {
        var query = dbContext.TaxRates.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.Code, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Name, TurkishInsensitive).Contains(q));
        }

        var items = await query
            .OrderBy(x => x.Rate)
            .Take(MaxResults)
            .Select(x => new { id = x.Id, code = x.Code, name = x.Name, rate = x.Rate })
            .ToListAsync();

        return Json(new { items });
    }

    public async Task<IActionResult> Branches(string? q)
    {
        var query = dbContext.Branches.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.Code, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Name, TurkishInsensitive).Contains(q));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Take(MaxResults)
            .Select(x => new { id = x.Id, code = x.Code, name = x.Name })
            .ToListAsync();

        return Json(new { items });
    }

    public async Task<IActionResult> Warehouses(string? q)
    {
        var query = dbContext.Warehouses.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.Code, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Name, TurkishInsensitive).Contains(q));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Take(MaxResults)
            .Select(x => new { id = x.Id, code = x.Code, name = x.Name })
            .ToListAsync();

        return Json(new { items });
    }

    public async Task<IActionResult> FinancialAccounts(string? q)
    {
        var query = dbContext.FinancialAccounts.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.Code, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Name, TurkishInsensitive).Contains(q));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Take(MaxResults)
            .Select(x => new { id = x.Id, code = x.Code, name = x.Name })
            .ToListAsync();

        return Json(new { items });
    }

    public async Task<IActionResult> PriceLists(string? q)
    {
        var query = dbContext.PriceLists.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.Code, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Name, TurkishInsensitive).Contains(q));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Take(MaxResults)
            .Select(x => new { id = x.Id, code = x.Code, name = x.Name })
            .ToListAsync();

        return Json(new { items });
    }

    public async Task<IActionResult> UnitsOfMeasure(string? q)
    {
        var query = dbContext.UnitsOfMeasure.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.Code, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Name, TurkishInsensitive).Contains(q));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Take(MaxResults)
            .Select(x => new { id = x.Id, code = x.Code, name = x.Name })
            .ToListAsync();

        return Json(new { items });
    }

    public IActionResult Roles(string? q)
    {
        var query = roleManager.Roles.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x => EF.Functions.Collate(x.Name!, TurkishInsensitive).Contains(q));
        }

        var items = query
            .OrderBy(x => x.Name)
            .Take(MaxResults)
            .Select(x => new { id = x.Id, code = x.Name, name = x.Name })
            .ToList();

        return Json(new { items });
    }
}
