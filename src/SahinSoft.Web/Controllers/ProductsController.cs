using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
        try
        {
            return await CreateCoreAsync(form);
        }
        catch (ConcurrencyRetryExhaustedException ex)
        {
            // ApplyIdentifierPolicyAsync (stok kodu/barkod üretimi) veya aşağıdaki tekrar deneme
            // döngüsündeki yeniden üretim çağrıları, kendi dahili deneme haklarını (12) tüketirse
            // buraya düşer — ham 500 yerine araç çubuğu korunarak dostane bir "tekrar deneyin"
            // mesajı gösterilir (bkz. DocumentNumberGeneratorService.ConcurrencyRetryExhaustedException
            // sözleşmesi, diğer evrak controller'larında da aynı şekilde yakalanıyor).
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }
    }

    private async Task<IActionResult> CreateCoreAsync(ProductFormViewModel form)
    {
        var (stockCodeAutoGenerated, barcodeAutoGenerated) = await ApplyIdentifierPolicyAsync(form);
        await ValidateUniqueFieldsAsync(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        // Otomatik üretilen stok kodu/barkod, eşzamanlı başka bir istekle tam olarak aynı ana denk
        // gelip DB seviyesindeki unique index'e çarpabilir (bkz. StockCodeGeneratorService/
        // BarcodeGeneratorService — üretim sırasında henüz kimse SaveChanges yapmadığı için aynı
        // "boş" aday iki isteğe de verilebilir). Bu durumda kullanıcı hiçbir şey fark etmeden taze
        // bir kod/barkod üretilip otomatik tekrar denenir — sadece ELLE girilmiş bir değer çakışırsa
        // veya deneme hakkı tükenirse kullanıcıya gösterilir.
        // "En büyükten sonraki boş adayı bul" yaklaşımı, hiç kimse henüz commit etmemişken aynı anda
        // tarayan N eşzamanlı istek için en kötü senaryoda O(N) tur gerektirebilir (her turda sadece
        // bir kazanan commit eder, kaybedenler bir sonraki turda tekrar dener) — bu yüzden deneme
        // sayısı beklenen eşzamanlılık düzeyinin (10 eşzamanlı istekle test edildi) belirgin şekilde
        // üzerinde tutulur.
        const int maxAttempts = 15;
        Product? newProduct = null;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            var (saved, candidate, conflictField) = await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                var product = new Product();
                await MapAsync(form, product, dbContext);
                SyncPrimaryBarcode(product, form.Barcode);
                dbContext.Products.Add(product);
                var (ok, conflict) = await TrySaveAsync();
                if (!ok)
                {
                    return (false, product, conflict);
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
                return (true, product, (string?)null);
            });

            if (saved)
            {
                newProduct = candidate;
                break;
            }

            var canRetryStockCode = conflictField == nameof(Product.StockCode) && stockCodeAutoGenerated;
            var canRetryBarcode = conflictField == nameof(Product.Barcode) && barcodeAutoGenerated;
            if ((canRetryStockCode || canRetryBarcode) && attempt < maxAttempts)
            {
                foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
                {
                    entry.State = EntityState.Detached;
                }

                // Rastgele bir gecikme olmadan, aynı anda çakışan birden fazla istek yeniden
                // üretilen adayda da birbirleriyle "kilitli adımda" (lockstep) çakışmaya devam
                // edebiliyordu — barkod üreteci, henüz hiç kimse commit etmemişken hep aynı
                // "bir sonraki boş" adaya yakınsıyor (canlı ortamda 10 eşzamanlı istekle
                // doğrulandı: aynı barkod art arda 5 kez çakıştı). Küçük rastgele bir bekleme,
                // eşzamanlı isteklerin farklı zamanlarda yeniden denemesini sağlayıp bu
                // "sürü" (thundering herd) etkisini kırıyor.
                await Task.Delay(Random.Shared.Next(15, 60) * attempt);

                if (canRetryStockCode)
                {
                    form.StockCode = await stockCodeGenerator.GenerateAsync();
                }
                if (canRetryBarcode)
                {
                    form.Barcode = await GenerateDefaultBarcodeAsync();
                }
                continue;
            }

            ModelState.AddModelError(
                conflictField == nameof(Product.StockCode) ? nameof(form.StockCode) : nameof(form.Barcode),
                "Stok kodu veya barkod başka bir kartta kullanılıyor. Aynı değer tekrar kaydedilemez.");
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        if (newProduct is null)
        {
            ModelState.AddModelError(string.Empty, "Kaydetme sırasında yoğun eşzamanlı istek nedeniyle sorun oluştu, lütfen tekrar deneyin.");
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        TempData["Success"] = "Stok kartı kaydedildi.";

        if (Request.Form["saveMode"] == "saveAndNew")
        {
            return RedirectToAction(nameof(Create), new { carryOverFromId = newProduct.Id });
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
        var (saved, conflictField) = await TrySaveAsync();
        if (!saved)
        {
            ModelState.AddModelError(
                conflictField == nameof(Product.StockCode) ? nameof(form.StockCode) : nameof(form.Barcode),
                "Stok kodu veya barkod başka bir kartta kullanılıyor. Aynı değer tekrar kaydedilemez.");
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

    // Dönüş değeri, Create'in eşzamanlılık çakışmasında hangi alanı sessizce yeniden üretip tekrar
    // deneyebileceğini (StockCodeAutoGenerated/BarcodeAutoGenerated) bilmesi için kullanılır — elle
    // girilmiş bir değer asla kullanıcıya sormadan değiştirilmez.
    private async Task<(bool StockCodeAutoGenerated, bool BarcodeAutoGenerated)> ApplyIdentifierPolicyAsync(ProductFormViewModel form)
    {
        var stockCodeEntered = !string.IsNullOrWhiteSpace(form.StockCode);
        var barcodeEntered = !string.IsNullOrWhiteSpace(form.Barcode);

        if (stockCodeEntered && barcodeEntered)
        {
            // İkisi de girilmiş: dokunma.
            return (false, false);
        }

        if (stockCodeEntered && !barcodeEntered)
        {
            // Sadece stok kodu girilmiş: barkod eksikliğini bildir.
            ModelState.AddModelError(nameof(form.Barcode), "Barkod zorunludur.");
            return (false, false);
        }

        if (!stockCodeEntered && barcodeEntered)
        {
            // Sadece barkod girilmiş: stok kodu eksikliğini bildir.
            ModelState.AddModelError(nameof(form.StockCode), "Stok kodu zorunludur.");
            return (false, false);
        }

        // İkisi de boş: her ikisini de otomatik/varsayılan ayarla doldur.
        form.StockCode = await stockCodeGenerator.GenerateAsync();
        ModelState.Remove(nameof(form.StockCode));
        form.Barcode = await GenerateDefaultBarcodeAsync();
        ModelState.Remove(nameof(form.Barcode));
        return (true, true);
    }

    private async Task<string> GenerateDefaultBarcodeAsync()
    {
        var settings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1);
        return settings.DefaultBarcodeType == "EAN8"
            ? await barcodeGenerator.GenerateEan8Async()
            : await barcodeGenerator.GenerateEan13Async();
    }

    private async Task PopulateSelectionsAsync(ProductFormViewModel model)
    {
        ViewBag.IsRestaurantModuleEnabled = await dbContext.InventorySettings
            .AsNoTracking()
            .Where(x => x.Id == 1)
            .Select(x => x.IsRestaurantModuleEnabled)
            .SingleOrDefaultAsync();

        // KDV, Kategori ve Birim artık serbest metin aramalı seçiciler değil, sadece tanımlı
        // kayıtlar arasından seçilebilen kapalı listeler (kullanıcı isteği: "sadece seçim yapılsın,
        // başka değer girilmesin", aynı mantık KDV/Kategori/Birim için).
        model.TaxRateOptions = await dbContext.TaxRates
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Rate)
            .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem($"%{x.Rate.ToString("N0")}", x.Id.ToString()))
            .ToListAsync();

        model.CategoryOptions = await dbContext.ProductCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();

        model.UnitOfMeasureOptions = await dbContext.UnitsOfMeasure
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();
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

        var barcodeType = InferBarcodeType(normalizedBarcode);
        if (primaryBarcode is null)
        {
            product.Barcodes.Add(new ProductBarcode
            {
                Barcode = normalizedBarcode,
                BarcodeType = barcodeType,
                UnitMultiplier = 1,
                IsPrimary = true
            });
        }
        else
        {
            primaryBarcode.Barcode = normalizedBarcode;
            primaryBarcode.BarcodeType = barcodeType;
        }
    }

    // Uzunluğa göre yalnızca EAN8/EAN13 ayrımı yapmak, 7 haneli Terazi barkodlarını (ör.
    // "2700001") ve "AS" ile başlayan 8 haneli ASCII barkodları (ör. "AS000001") YANLIŞ
    // sınıflandırıyordu — ikisi de EAN13'e "düşüp" DB'deki CK_ProductBarcodes_Length/_Numeric
    // kısıtlarına çarpıyor, ham 500 hatasına yol açıyordu (BarkodGeneratorService'in ÜRETTİĞİ
    // geçerli bir barkodu bile kaydederken). Artık önek/uzunluğa göre doğru tür seçiliyor; hiçbirine
    // uymayan (ör. elle girilmiş harf içeren veya alışılmadık uzunluktaki) değerler DB kısıtlarının
    // dışında kalan "OTHER" olarak işaretlenir — ham SQL hatası yerine olduğu gibi kaydedilir.
    private static string InferBarcodeType(string barcode)
    {
        if (barcode.Length == 13 && barcode.All(char.IsDigit))
        {
            return "EAN13";
        }
        if (barcode.Length == 8 && barcode.All(char.IsDigit))
        {
            return "EAN8";
        }
        if (barcode.Length == 7 && barcode.All(char.IsDigit) && (barcode.StartsWith("27") || barcode.StartsWith("28") || barcode.StartsWith("29")))
        {
            return "SCALE";
        }
        return "OTHER";
    }

    // Hangi alanın çakıştığını (StockCode/Barcode/null) SQL hata mesajından ayırt eder — çağıran bu
    // bilgiyle otomatik üretilmiş bir değeri sessizce yenileyip tekrar deneyebilir, elle girilmiş bir
    // değeri asla kullanıcıya sormadan değiştirmez.
    private async Task<(bool Saved, string? ConflictField)> TrySaveAsync()
    {
        try
        {
            await dbContext.SaveChangesAsync();
            return (true, null);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } sqlEx)
        {
            // Barkod, hem Products.Barcode hem ProductBarcodes.Barcode üzerinde ayrı unique index'e
            // sahip (SyncPrimaryBarcode ikisine de aynı değeri yazar) — ikisi de "Barcode" içerir.
            var conflictField = sqlEx.Message.Contains("StockCode", StringComparison.OrdinalIgnoreCase)
                ? nameof(Product.StockCode)
                : sqlEx.Message.Contains("Barcode", StringComparison.OrdinalIgnoreCase)
                    ? nameof(Product.Barcode)
                    : null;
            return (false, conflictField);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException)
        {
            // Unique index dışındaki bir DB kısıtı (ör. CK_ProductBarcodes_Length/_Numeric — elle
            // girilmiş, InferBarcodeType'ın "OTHER" dışında sınıflandırdığı ama yine de kısıta
            // uymayan bir değer) — ham SQL/500 yerine barkod alanında dostane bir hata gösterilir.
            return (false, nameof(Product.Barcode));
        }
    }
}
