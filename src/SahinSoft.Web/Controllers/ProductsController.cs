using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class ProductsController(
    ApplicationDbContext dbContext,
    BarcodeGeneratorService barcodeGenerator,
    StockCodeGeneratorService stockCodeGenerator) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.TaxRate)
            .OrderBy(x => x.StockCode)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.StockCode.Contains(search) ||
                x.Name.Contains(search) ||
                (x.Brand != null && x.Brand.Contains(search)));
        }

        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel();
        await PopulateSelectionsAsync(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel form)
    {
        await ApplyIdentifierPolicyAsync(form);
        await ValidateUniqueFieldsAsync(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var product = new Product();
        Map(form, product);
        SyncPrimaryBarcode(product, form.Barcode);
        dbContext.Products.Add(product);
        if (!await TrySaveAsync(nameof(form.Barcode)))
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        if (product.TrackStock && product.StockQuantity != 0)
        {
            var defaultWarehouseId = await dbContext.Warehouses
                .Where(x => x.IsDefault && x.IsActive)
                .Select(x => x.Id)
                .SingleAsync();
            dbContext.StockMovements.Add(new StockMovement
            {
                MovementDateUtc = DateTime.UtcNow,
                MovementType = StockMovementType.Opening,
                Quantity = product.StockQuantity,
                UnitCost = product.PurchasePrice,
                DocumentNumber = $"ACILIS-{product.StockCode}",
                Description = "Stok kartı açılış miktarı",
                ProductId = product.Id,
                WarehouseId = defaultWarehouseId
            });
            await dbContext.SaveChangesAsync();
        }
        await transaction.CommitAsync();
        TempData["Success"] = "Stok kartı kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await dbContext.Products
            .Include(x => x.Barcodes)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        var model = new ProductFormViewModel
        {
            Id = product.Id,
            StockCode = product.StockCode,
            Name = product.Name,
            CategoryId = product.CategoryId,
            TaxRateId = product.TaxRateId,
            Brand = product.Brand,
            Model = product.Model,
            Barcode = product.Barcode,
            Unit = product.Unit,
            ProductType = product.ProductType,
            PurchasePrice = product.PurchasePrice,
            SalePrice = product.SalePrice,
            StockQuantity = product.StockQuantity,
            MinimumStockQuantity = product.MinimumStockQuantity,
            TrackStock = product.TrackStock,
            IsActive = product.IsActive,
            ImagePath = product.ImagePath,
            Description = product.Description
        };

        await PopulateSelectionsAsync(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        await ApplyIdentifierPolicyAsync(form);
        await ValidateUniqueFieldsAsync(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var product = await dbContext.Products
            .Include(x => x.Barcodes)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        var currentStockQuantity = product.StockQuantity;
        Map(form, product);
        product.StockQuantity = currentStockQuantity;
        SyncPrimaryBarcode(product, form.Barcode);
        product.UpdatedAtUtc = DateTime.UtcNow;
        if (!await TrySaveAsync(nameof(form.Barcode)))
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }
        TempData["Success"] = "Stok kartı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateUniqueFieldsAsync(ProductFormViewModel model)
    {
        if (await dbContext.Products.AnyAsync(x =>
                x.StockCode == model.StockCode && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.StockCode), "Bu stok kodu zaten kullanılıyor.");
        }

        if (!string.IsNullOrWhiteSpace(model.Barcode) &&
            (await dbContext.Products.AnyAsync(x =>
                 x.Barcode == model.Barcode && x.Id != model.Id) ||
             await dbContext.ProductBarcodes.AnyAsync(x =>
                 x.Barcode == model.Barcode && x.ProductId != model.Id)))
        {
            ModelState.AddModelError(nameof(model.Barcode), "Bu barkod zaten kullanılıyor.");
        }
    }

    private async Task ApplyIdentifierPolicyAsync(ProductFormViewModel form)
    {
        var stockCodeWasEntered = !string.IsNullOrWhiteSpace(form.StockCode);

        if (!stockCodeWasEntered)
        {
            form.StockCode = await stockCodeGenerator.GenerateAsync();
            ModelState.Remove(nameof(form.StockCode));
        }

        var settings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1);

        if (!string.IsNullOrWhiteSpace(form.Barcode))
        {
            return;
        }

        if (!stockCodeWasEntered && settings.AutoGenerateBarcode)
        {
            form.Barcode = settings.DefaultBarcodeType == "EAN8"
                ? await barcodeGenerator.GenerateEan8Async()
                : await barcodeGenerator.GenerateEan13Async();
            ModelState.Remove(nameof(form.Barcode));
            return;
        }

        if (stockCodeWasEntered || settings.RequireBarcode)
        {
            ModelState.AddModelError(nameof(form.Barcode), "Barkod zorunludur.");
        }
    }

    private async Task PopulateSelectionsAsync(ProductFormViewModel model)
    {
        model.Categories = await dbContext.ProductCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();

        model.TaxRates = await dbContext.TaxRates
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Rate)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();
    }

    private static void Map(ProductFormViewModel source, Product target)
    {
        target.StockCode = source.StockCode.Trim();
        target.Name = source.Name.Trim();
        target.CategoryId = source.CategoryId;
        target.TaxRateId = source.TaxRateId;
        target.Brand = source.Brand?.Trim();
        target.Model = source.Model?.Trim();
        target.Barcode = string.IsNullOrWhiteSpace(source.Barcode) ? null : source.Barcode.Trim();
        target.Unit = source.Unit.Trim();
        target.ProductType = source.ProductType.Trim();
        target.PurchasePrice = source.PurchasePrice;
        target.SalePrice = source.SalePrice;
        target.StockQuantity = source.StockQuantity;
        target.MinimumStockQuantity = source.MinimumStockQuantity;
        target.TrackStock = source.TrackStock;
        target.IsActive = source.IsActive;
        target.ImagePath = source.ImagePath?.Trim();
        target.Description = source.Description?.Trim();
    }

    private static void SyncPrimaryBarcode(Product product, string? barcode)
    {
        var normalizedBarcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
        var primaryBarcode = product.Barcodes.SingleOrDefault(x => x.IsPrimary);

        if (normalizedBarcode is null)
        {
            if (primaryBarcode is not null)
            {
                product.Barcodes.Remove(primaryBarcode);
            }
            return;
        }

        if (primaryBarcode is null)
        {
            product.Barcodes.Add(new ProductBarcode
            {
                Barcode = normalizedBarcode,
                BarcodeType = normalizedBarcode.Length == 8 ? "EAN8" : "EAN13",
                UnitMultiplier = 1,
                IsPrimary = true
            });
        }
        else
        {
            primaryBarcode.Barcode = normalizedBarcode;
            primaryBarcode.BarcodeType = normalizedBarcode.Length == 8 ? "EAN8" : "EAN13";
        }
    }

    private async Task<bool> TrySaveAsync(string barcodeField)
    {
        try
        {
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                barcodeField,
                "Stok kodu veya barkod başka bir kartta kullanılıyor. Aynı değer tekrar kaydedilemez.");
            return false;
        }
    }
}
