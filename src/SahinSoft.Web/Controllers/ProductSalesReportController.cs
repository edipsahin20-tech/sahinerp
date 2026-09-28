using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Ürün Satış Raporu (Edip, 2026-09-28: "sonra ürün satış raporunu ekle filitre aynı mantıkta
// olsun odeme tipini kaldır oraya ketegori,marka,urun gibi filitreler ekle") - ANA ERP'de yaşar,
// RetailSaleLine kayıtlarını (iptal edilmiş fişler hariç) ürün bazında toplar.
[Authorize]
public sealed class ProductSalesReportController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(int? branchId, DateTime? dateFrom, DateTime? dateTo, int? categoryId, string? brand, string? productQuery)
    {
        var query = dbContext.RetailSaleLines
            .AsNoTracking()
            .Where(l => l.RetailSale.Status != RetailSaleStatus.Cancelled);

        if (branchId.HasValue)
        {
            query = query.Where(l => l.RetailSale.RestaurantCheck.RestaurantTableSession.BranchId == branchId.Value);
        }
        if (dateFrom.HasValue)
        {
            query = query.Where(l => l.RetailSale.IssuedAtUtc >= dateFrom.Value.Date.ToUniversalTime());
        }
        if (dateTo.HasValue)
        {
            var toExclusive = dateTo.Value.Date.AddDays(1).ToUniversalTime();
            query = query.Where(l => l.RetailSale.IssuedAtUtc < toExclusive);
        }
        if (categoryId.HasValue)
        {
            query = query.Where(l => l.Product.CategoryId == categoryId.Value);
        }
        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(l => l.Product.Brand == brand);
        }
        if (!string.IsNullOrWhiteSpace(productQuery))
        {
            query = query.Where(l => l.Product.Name.Contains(productQuery));
        }

        var items = await query
            .GroupBy(l => new { l.ProductId, l.Product.Name, l.Product.Brand, CategoryName = l.Product.Category.Name })
            .Select(g => new ProductSalesReportItemViewModel
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                Brand = g.Key.Brand,
                CategoryName = g.Key.CategoryName,
                QuantitySold = g.Sum(x => x.Quantity),
                DiscountAmount = g.Sum(x => x.DiscountAmountSnapshot),
                TotalAmount = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToListAsync();

        var model = new ProductSalesReportViewModel
        {
            Branches = await dbContext.Branches.AsNoTracking().OrderBy(x => x.Name).ToListAsync(),
            Categories = await dbContext.ProductCategories.AsNoTracking().OrderBy(x => x.Name).ToListAsync(),
            Brands = await dbContext.Products.AsNoTracking()
                .Where(x => x.Brand != null && x.Brand != "")
                .Select(x => x.Brand!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(),
            BranchId = branchId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            CategoryId = categoryId,
            Brand = brand,
            ProductQuery = productQuery,
            Items = items,
            TotalQuantity = items.Sum(x => x.QuantitySold),
            TotalAmount = items.Sum(x => x.TotalAmount),
            TotalDiscount = items.Sum(x => x.DiscountAmount)
        };

        return View(model);
    }
}
