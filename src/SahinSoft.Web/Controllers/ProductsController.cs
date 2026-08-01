using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Common;
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
    public async Task<IActionResult> GenerateBarcodeApi(string type)
    {
        string barcode;
        switch (type)
        {
            case "EAN8":
                barcode = await barcodeGenerator.GenerateEan8Async();
                break;
            case "ASCII":
                barcode = await barcodeGenerator.GenerateAsciiAsync();
                break;
            case "TERAZI":
                var settings = await dbContext.InventorySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
                barcode = await barcodeGenerator.GenerateScaleBarcodeAsync(settings.DefaultScalePrefix);
                break;
            default:
                barcode = await barcodeGenerator.GenerateEan13Async();
                break;
        }

        return Json(new { barcode });
    }

    public async Task<IActionResult> Index(string? search)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var barcodeMatchId = await dbContext.Products
                .Where(x => x.Barcode == search)
                .Select(x => (int?)x.Id)
                .SingleOrDefaultAsync();
            if (barcodeMatchId is null)
            {
                barcodeMatchId = await dbContext.ProductBarcodes
                    .Where(x => x.Barcode == search)
                    .Select(x => (int?)x.ProductId)
                    .SingleOrDefaultAsync();
            }
            if (barcodeMatchId is int productId)
            {
                return RedirectToAction(nameof(Edit), new { id = productId });
            }
        }

        var query = dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.TaxRate)
            .OrderBy(x => x.StockCode)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                EF.Functions.Collate(x.StockCode, "Turkish_CI_AI").Contains(search) ||
                EF.Functions.Collate(x.Name, "Turkish_CI_AI").Contains(search) ||
                (x.Brand != null && EF.Functions.Collate(x.Brand, "Turkish_CI_AI").Contains(search)) ||
                (x.Barcode != null && x.Barcode.Contains(search)));
        }

        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    // Cari Ekstresi (CustomersController.Statement) ile aynı desen: ürüne özel tüm stok
    // hareketlerini tarih sırasıyla koşan bakiye ile listeler. Cari kolonu, hareket bir Satış/Alış
    // faturasından geldiyse (InvoiceLineId dolu ise) faturanın carisinden türetilir; Stok Fişi,
    // Transfer, Sayım, İrsaliye gibi fatura dışı hareketlerde boş kalır.
    public async Task<IActionResult> Statement(int id, DateTime? from, DateTime? to)
    {
        var product = await dbContext.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        var movementsQuery = dbContext.StockMovements
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.InvoiceLine).ThenInclude(x => x!.Invoice).ThenInclude(x => x.Customer)
            .Where(x => x.ProductId == id);

        var openingBalance = from.HasValue
            ? await movementsQuery.Where(x => x.MovementDateUtc < from.Value).SumAsync(x => (decimal?)x.Quantity) ?? 0
            : 0;

        var rangeQuery = movementsQuery;
        if (from.HasValue)
        {
            rangeQuery = rangeQuery.Where(x => x.MovementDateUtc >= from.Value);
        }
        if (to.HasValue)
        {
            rangeQuery = rangeQuery.Where(x => x.MovementDateUtc < to.Value.AddDays(1));
        }

        var movements = await rangeQuery
            .OrderBy(x => x.MovementDateUtc)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var runningBalance = openingBalance;
        var lines = new List<ProductStatementLineViewModel>();
        foreach (var movement in movements)
        {
            runningBalance += movement.Quantity;
            lines.Add(new ProductStatementLineViewModel
            {
                MovementDateUtc = movement.MovementDateUtc,
                WarehouseName = movement.Warehouse.Name,
                DocumentNumber = movement.DocumentNumber,
                CustomerName = movement.InvoiceLine?.Invoice.Customer.Name,
                MovementType = movement.MovementType.GetDisplayName(),
                Description = movement.Description,
                QuantityIn = movement.Quantity > 0 ? movement.Quantity : 0,
                QuantityOut = movement.Quantity < 0 ? -movement.Quantity : 0,
                RunningBalance = runningBalance
            });
        }

        // Alış/Satış Fiyat Hareketi: onaylı fatura satırlarından türetilen fiyat geçmişi (KDV dahil,
        // Stok Kartı'ndaki fiyat kuralıyla tutarlı) — bu ürün kime/kimden hangi tarihte hangi
        // fiyattan alınıp satılmış, tüm geçmişiyle.
        var purchasePriceHistory = await BuildPriceHistoryAsync(id, InvoiceType.Purchase);
        var salePriceHistory = await BuildPriceHistoryAsync(id, InvoiceType.Sales);

        // Depolar Arası Stok Miktarı: bu ürünün her depodaki güncel bakiyesi (tarih filtresinden
        // bağımsız, her zaman tüm zamanların toplamı — bir anlık fotoğraf).
        var warehouseQuantities = (await dbContext.StockMovements
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Where(x => x.ProductId == id)
                .ToListAsync())
            .GroupBy(x => x.Warehouse.Name)
            .Select(g => new ProductWarehouseQuantityViewModel { WarehouseName = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .Where(x => x.Quantity != 0)
            .OrderByDescending(x => x.Quantity)
            .ToList();

        var model = new ProductStatementViewModel
        {
            ProductId = product.Id,
            ProductName = product.Name,
            StockCode = product.StockCode,
            From = from,
            To = to,
            OpeningBalance = openingBalance,
            ClosingBalance = runningBalance,
            Lines = lines,
            PurchasePriceHistory = purchasePriceHistory,
            SalePriceHistory = salePriceHistory,
            WarehouseQuantities = warehouseQuantities
        };

        return View(model);
    }

    private async Task<List<ProductPriceHistoryLineViewModel>> BuildPriceHistoryAsync(int productId, InvoiceType invoiceType)
    {
        var lines = await dbContext.InvoiceLines
            .AsNoTracking()
            .Include(x => x.Invoice).ThenInclude(x => x.Customer)
            .Where(x => x.ProductId == productId
                && x.Invoice.InvoiceType == invoiceType
                && x.Invoice.Status == InvoiceStatus.Approved)
            .OrderByDescending(x => x.Invoice.InvoiceDateUtc)
            .ThenByDescending(x => x.InvoiceId)
            .ToListAsync();

        return lines
            .Select(x => new ProductPriceHistoryLineViewModel
            {
                InvoiceDateUtc = x.Invoice.InvoiceDateUtc,
                InvoiceNumber = x.Invoice.InvoiceNumber,
                CustomerName = x.Invoice.Customer.Name,
                Quantity = x.Quantity,
                UnitPriceInclTax = Math.Round(x.UnitPrice * (1 + x.TaxRate / 100), 2, MidpointRounding.AwayFromZero)
            })
            .ToList();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkSetUnitToAdet()
    {
        var adet = await dbContext.UnitsOfMeasure.SingleOrDefaultAsync(x => x.Code == "ADET");
        if (adet is null)
        {
            TempData["Error"] = "\"Adet\" birimi tanımlı değil.";
            return RedirectToAction(nameof(Index));
        }

        var updatedCount = await dbContext.Products.ExecuteUpdateAsync(s => s
            .SetProperty(p => p.Unit, adet.Name)
            .SetProperty(p => p.UnitOfMeasureId, adet.Id));

        TempData["Success"] = $"{updatedCount} ürünün birimi \"Adet\" olarak güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Create(int? carryOverFromId)
    {
        var model = new ProductFormViewModel();

        if (carryOverFromId is int sourceId)
        {
            var source = await dbContext.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == sourceId);
            if (source is not null)
            {
                model.CategoryId = source.CategoryId;
                model.TaxRateId = source.TaxRateId;
                model.UnitOfMeasureId = source.UnitOfMeasureId;
                model.Brand = source.Brand;
                model.Model = source.Model;
                model.ProductType = source.ProductType;
                model.AlternateName = source.AlternateName;
                model.ShelfLifeDays = source.ShelfLifeDays;
                model.CountryOfOrigin = source.CountryOfOrigin;
                model.TrackSerialNumbers = source.TrackSerialNumbers;
                model.MinimumStockQuantity = source.MinimumStockQuantity;
                model.TrackStock = source.TrackStock;
                model.IsActive = source.IsActive;
                model.Description = source.Description;
                model.ShowAsShortcut = source.ShowAsShortcut;
                model.ShowInMobile = source.ShowInMobile;
                model.ShowInOnlineOrder = source.ShowInOnlineOrder;
                model.KitchenPrinterName = source.KitchenPrinterName;
                // Stok kodu, barkod, fiyatlar, miktar ve puan bilinçli olarak boş bırakılır.
            }
        }

        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "Products" };
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

        // EnableRetryOnFailure() (Program.cs) sets a retrying execution strategy; elle açılan
        // transaction'lar bununla uyumlu değil, tüm bloğun CreateExecutionStrategy() üzerinden
        // "tekrar denenebilir birim" olarak sarılması gerekiyor (aksi halde InvalidOperationException).
        var strategy = dbContext.Database.CreateExecutionStrategy();
        var (saved, product) = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            var newProduct = new Product();
            await MapAsync(form, newProduct, dbContext);
            SyncPrimaryBarcode(newProduct, form.Barcode);
            dbContext.Products.Add(newProduct);
            if (!await TrySaveAsync(nameof(form.Barcode)))
            {
                return (false, newProduct);
            }

            if (newProduct.TrackStock && newProduct.StockQuantity != 0)
            {
                var defaultWarehouseId = await dbContext.Warehouses
                    .Where(x => x.IsDefault && x.IsActive)
                    .Select(x => x.Id)
                    .SingleAsync();
                dbContext.StockMovements.Add(new StockMovement
                {
                    MovementDateUtc = DateTime.UtcNow,
                    MovementType = StockMovementType.Opening,
                    Quantity = newProduct.StockQuantity,
                    UnitCost = newProduct.PurchasePrice,
                    DocumentNumber = $"ACILIS-{newProduct.StockCode}",
                    Description = "Stok kartı açılış miktarı",
                    ProductId = newProduct.Id,
                    WarehouseId = defaultWarehouseId
                });
                await dbContext.SaveChangesAsync();
            }
            await transaction.CommitAsync();
            return (true, newProduct);
        });

        if (!saved)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        TempData["Success"] = "Stok kartı kaydedildi.";

        if (Request.Form["saveMode"] == "saveAndNew")
        {
            return RedirectToAction(nameof(Create), new { carryOverFromId = product.Id });
        }
        return RedirectToAction(nameof(Create));
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
            UnitOfMeasureId = product.UnitOfMeasureId,
            ProductType = product.ProductType,
            AlternateName = product.AlternateName,
            ShelfLifeDays = product.ShelfLifeDays,
            CountryOfOrigin = product.CountryOfOrigin,
            TrackSerialNumbers = product.TrackSerialNumbers,
            PurchasePrice = product.PurchasePrice,
            SalePrice = product.SalePrice,
            StockQuantity = product.StockQuantity,
            MinimumStockQuantity = product.MinimumStockQuantity,
            TrackStock = product.TrackStock,
            IsActive = product.IsActive,
            ImagePath = product.ImagePath,
            Description = product.Description,
            LoyaltyPoints = product.LoyaltyPoints,
            ShowAsShortcut = product.ShowAsShortcut,
            ShowInMobile = product.ShowInMobile,
            ShowInOnlineOrder = product.ShowInOnlineOrder,
            KitchenPrinterName = product.KitchenPrinterName
        };

        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await dbContext.Products
            .Include(x => x.Barcodes)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        if (await dbContext.StockMovements.AnyAsync(x => x.ProductId == id))
        {
            TempData["Error"] = "Bu ürüne ait stok hareketleri var, silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        dbContext.Products.Remove(product);
        try
        {
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Stok kartı silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Bu ürüne bağlı kayıtlar var, silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.Products.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.Products.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var hasMovements = await dbContext.StockMovements.AnyAsync(x => x.ProductId == id);

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "Products",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = !hasMovements,
            DeleteBlockedReason = hasMovements ? "Bu ürüne ait stok hareketleri var, silinemez." : null
        };
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
        await MapAsync(form, product, dbContext);
        product.StockQuantity = currentStockQuantity;
        SyncPrimaryBarcode(product, form.Barcode);
        product.UpdatedAtUtc = DateTime.UtcNow;
        if (!await TrySaveAsync(nameof(form.Barcode)))
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }
        TempData["Success"] = "Stok kartı güncellendi.";
        return RedirectToAction(nameof(Create));
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
        var stockCodeEntered = !string.IsNullOrWhiteSpace(form.StockCode);
        var barcodeEntered = !string.IsNullOrWhiteSpace(form.Barcode);

        if (stockCodeEntered && barcodeEntered)
        {
            // İkisi de girilmiş: dokunma.
            return;
        }

        if (stockCodeEntered && !barcodeEntered)
        {
            // Sadece stok kodu girilmiş: barkod eksikliğini bildir.
            ModelState.AddModelError(nameof(form.Barcode), "Barkod zorunludur.");
            return;
        }

        if (!stockCodeEntered && barcodeEntered)
        {
            // Sadece barkod girilmiş: stok kodu eksikliğini bildir.
            ModelState.AddModelError(nameof(form.StockCode), "Stok kodu zorunludur.");
            return;
        }

        // İkisi de boş: her ikisini de otomatik/varsayılan ayarla doldur.
        form.StockCode = await stockCodeGenerator.GenerateAsync();
        ModelState.Remove(nameof(form.StockCode));

        var settings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1);
        form.Barcode = settings.DefaultBarcodeType == "EAN8"
            ? await barcodeGenerator.GenerateEan8Async()
            : await barcodeGenerator.GenerateEan13Async();
        ModelState.Remove(nameof(form.Barcode));
    }

    private async Task PopulateSelectionsAsync(ProductFormViewModel model)
    {
        ViewBag.IsRestaurantModuleEnabled = await dbContext.InventorySettings
            .AsNoTracking()
            .Where(x => x.Id == 1)
            .Select(x => x.IsRestaurantModuleEnabled)
            .SingleOrDefaultAsync();

        if (model.CategoryId > 0)
        {
            model.CategoryDisplay = await dbContext.ProductCategories
                .Where(x => x.Id == model.CategoryId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        // KDV artık serbest metin aramalı bir seçici değil, sadece orana göre kapalı bir liste
        // (kullanıcı isteği: "sadece oran seçilsin, başka değer girilmesin").
        model.TaxRateOptions = await dbContext.TaxRates
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Rate)
            .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem($"%{x.Rate.ToString("N0")}", x.Id.ToString()))
            .ToListAsync();

        if (model.UnitOfMeasureId is int unitOfMeasureId)
        {
            // Birim seçicisinde Kod ve Ad genelde aynı kelime olduğu için ("Adet" - "Adet")
            // Kategori/KDV gibi "Kod - Ad" değil, sadece Ad gösterilir.
            model.UnitOfMeasureDisplay = await dbContext.UnitsOfMeasure
                .Where(x => x.Id == unitOfMeasureId)
                .Select(x => x.Name)
                .SingleOrDefaultAsync();
        }
    }

    private static async Task MapAsync(ProductFormViewModel source, Product target, ApplicationDbContext dbContext)
    {
        target.StockCode = source.StockCode.Trim();
        target.Name = source.Name.Trim();
        target.CategoryId = source.CategoryId;
        target.TaxRateId = source.TaxRateId;
        target.Brand = source.Brand?.Trim();
        target.Model = source.Model?.Trim();
        target.Barcode = string.IsNullOrWhiteSpace(source.Barcode) ? null : source.Barcode.Trim();
        target.UnitOfMeasureId = source.UnitOfMeasureId;
        if (source.UnitOfMeasureId is int unitOfMeasureId)
        {
            target.Unit = await dbContext.UnitsOfMeasure
                .Where(x => x.Id == unitOfMeasureId)
                .Select(x => x.Name)
                .SingleAsync();
        }
        target.ProductType = source.ProductType.Trim();
        target.AlternateName = source.AlternateName?.Trim();
        target.ShelfLifeDays = source.ShelfLifeDays;
        target.CountryOfOrigin = source.CountryOfOrigin?.Trim();
        target.TrackSerialNumbers = source.TrackSerialNumbers;
        target.PurchasePrice = source.PurchasePrice;
        target.SalePrice = source.SalePrice;
        target.StockQuantity = source.StockQuantity;
        target.MinimumStockQuantity = source.MinimumStockQuantity;
        target.TrackStock = source.TrackStock;
        target.IsActive = source.IsActive;
        target.ImagePath = source.ImagePath?.Trim();
        target.Description = source.Description?.Trim();
        target.LoyaltyPoints = source.LoyaltyPoints;
        target.ShowAsShortcut = source.ShowAsShortcut;
        target.ShowInMobile = source.ShowInMobile;
        target.ShowInOnlineOrder = source.ShowInOnlineOrder;
        target.KitchenPrinterName = string.IsNullOrWhiteSpace(source.KitchenPrinterName) ? null : source.KitchenPrinterName.Trim();
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
