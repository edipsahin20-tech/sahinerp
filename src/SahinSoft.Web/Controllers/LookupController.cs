using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
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
        // Boşlukla ayrılmış her kelime (sırası fark etmeksizin) Stok Kodu/Stok Adı/Barkod'un
        // herhangi birinde geçmeli; kelime içinde "*" LIKE joker karakterine çevrilir
        // (örn. "hii*9" -> "%hii%9%").
        foreach (var token in SplitSearchTokens(q))
        {
            var pattern = BuildLikePattern(token);
            query = query.Where(x =>
                EF.Functions.Like(EF.Functions.Collate(x.StockCode, TurkishInsensitive), pattern) ||
                EF.Functions.Like(EF.Functions.Collate(x.Name, TurkishInsensitive), pattern) ||
                (x.Barcode != null && EF.Functions.Like(EF.Functions.Collate(x.Barcode, TurkishInsensitive), pattern)));
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
                stock = x.StockQuantity,
                // Son Alış Fiyatı: en son onaylı alış faturası satırındaki KDV dahil birim fiyat.
                lastPurchasePrice = dbContext.InvoiceLines
                    .Where(l => l.ProductId == x.Id && l.Invoice.InvoiceType == InvoiceType.Purchase && l.Invoice.Status == InvoiceStatus.Approved)
                    .OrderByDescending(l => l.Invoice.InvoiceDateUtc).ThenByDescending(l => l.InvoiceId)
                    .Select(l => (decimal?)Math.Round(l.UnitPrice * (1 + l.TaxRate / 100), 2, MidpointRounding.AwayFromZero))
                    .FirstOrDefault(),
                // Son Satış Fiyatı: fatura geçmişinden değil, stok kartındaki güncel Satış Fiyatı (zaten KDV dahil).
                lastSalePrice = (decimal?)x.SalePrice
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

    // "..." (Evrak Belge Sıra yanı) butonuyla açılan, tüm faturaları tarih tarih listeleyip
    // tıklandığında ilgili faturayı açan gözat penceresi için — Invoices/Form.cshtml.
    public async Task<IActionResult> InvoicesBrowse(string? q, SahinSoft.Domain.Enums.InvoiceType? type, DateTime? from, DateTime? to)
    {
        var query = dbContext.Invoices.AsNoTracking().Include(x => x.Customer).AsQueryable();
        if (type.HasValue)
        {
            query = query.Where(x => x.InvoiceType == type.Value);
        }
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.InvoiceNumber, TurkishInsensitive).Contains(q) ||
                EF.Functions.Collate(x.Customer.Name, TurkishInsensitive).Contains(q));
        }
        if (from.HasValue)
        {
            query = query.Where(x => x.InvoiceDateUtc >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(x => x.InvoiceDateUtc < to.Value.AddDays(1));
        }

        var items = await query
            .OrderByDescending(x => x.InvoiceDateUtc)
            .ThenByDescending(x => x.Id)
            .Take(50)
            .Select(x => new
            {
                id = x.Id,
                code = x.InvoiceNumber,
                name = x.Customer.Name,
                customerName = x.Customer.Name,
                invoiceDate = x.InvoiceDateUtc.ToString("dd.MM.yyyy"),
                statusText = x.Status == SahinSoft.Domain.Enums.InvoiceStatus.Draft ? "Taslak"
                    : x.Status == SahinSoft.Domain.Enums.InvoiceStatus.Approved ? "Onaylı" : "İptal",
                grandTotal = x.GrandTotal
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

    private static string[] SplitSearchTokens(string? q) =>
        string.IsNullOrWhiteSpace(q)
            ? []
            : q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    // Kullanıcının yazdığı "*" karakteri LIKE joker karakterine (%) çevrilir; mevcut LIKE özel
    // karakterleri ([, %, _) önce kaçırılır ki arama metninde geçerlerse literal aransınlar.
    // Sonuç her zaman baştan/sondan esnek eşleşir (örn. "hii*9" -> "%hii%9%").
    private static string BuildLikePattern(string token)
    {
        var escaped = token.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        return "%" + escaped.Replace("*", "%") + "%";
    }
}
