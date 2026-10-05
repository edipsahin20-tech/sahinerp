using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class MasterDataTransferRequest
{
    public string? Source { get; set; }
    public string? Target { get; set; }
    public List<string> Categories { get; set; } = [];
    public List<string> Products { get; set; } = [];
    public List<string> Customers { get; set; } = [];
}

// Firmalar arası ana veri aktarımı: kategori, stok kartı, cari kartı. Hareketler, bakiyeler ve stok
// miktarları ASLA taşınmaz. Eşleme iş anahtarıyla yapılır (kod); hedefte aynı kod varsa kayıt atlanır.
// Stok kartı, hedefte bulunmayan KDV oranı/kategori/birim yüzünden atlanırsa sebebi raporlanır.
public static class MasterDataTransfer
{
    private const string TransferUserId = "veri-aktarimi";

    public static async Task<List<string>> RunAsync(MasterDataTransferRequest request, DatabaseCatalog catalog,
        ApplicationDbContext target, IHttpContextAccessor accessor)
    {
        var source = catalog.Find(request.Source) ?? throw new InvalidOperationException("Kaynak firma bulunamadı.");
        var sourceOptions = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(source.BuildConnectionString()).Options;
        await using var src = new ApplicationDbContext(sourceOptions, accessor);

        var report = new List<string>();

        // Vergi oranları ve birimler: hedefte kod/oran ile eşleşen kayıt aranır.
        var srcTaxRates = await src.TaxRates.AsNoTracking().ToDictionaryAsync(x => x.Id);
        var tgtTaxRates = await target.TaxRates.ToListAsync();
        var srcUnits = await src.UnitsOfMeasure.AsNoTracking().ToDictionaryAsync(x => x.Id);
        var tgtUnits = await target.UnitsOfMeasure.ToListAsync();

        int? MapTaxRate(int srcId)
        {
            if (!srcTaxRates.TryGetValue(srcId, out var s)) return null;
            return tgtTaxRates.FirstOrDefault(t => t.Code == s.Code)?.Id
                ?? tgtTaxRates.FirstOrDefault(t => t.Rate == s.Rate && t.IsExempt == s.IsExempt)?.Id;
        }

        int? MapUnit(int? srcId)
        {
            if (srcId is null || !srcUnits.TryGetValue(srcId.Value, out var s)) return null;
            return tgtUnits.FirstOrDefault(u => u.Code == s.Code)?.Id;
        }

        // 1) Kategoriler (üst kategori önce gelsin diye sıralı, döngüyle)
        var srcCategories = await src.ProductCategories.AsNoTracking().ToDictionaryAsync(x => x.Id);
        var tgtCategoryCodes = await target.ProductCategories.ToDictionaryAsync(x => x.Code);
        var pending = srcCategories.Values.Where(x => request.Categories.Contains(x.Code)).ToList();
        var categoriesAdded = 0;
        var progress = true;
        while (pending.Count > 0 && progress)
        {
            progress = false;
            foreach (var c in pending.ToList())
            {
                var parentCode = c.ParentCategoryId is int pid && srcCategories.TryGetValue(pid, out var p) ? p.Code : null;
                if (parentCode is not null && request.Categories.Contains(parentCode) && !tgtCategoryCodes.ContainsKey(parentCode))
                {
                    continue; // üst kategori henüz eklenmedi
                }

                pending.Remove(c);
                progress = true;
                if (tgtCategoryCodes.ContainsKey(c.Code))
                {
                    report.Add($"Kategori '{c.Code}' zaten hedefte var, atlandı.");
                    continue;
                }

                var newCategory = new ProductCategory
                {
                    RecordId = Guid.NewGuid(),
                    Code = c.Code,
                    Name = c.Name,
                    AlternateName = c.AlternateName,
                    Unit = c.Unit,
                    WebsitePath = c.WebsitePath,
                    IsActive = c.IsActive,
                    Color = c.Color,
                    DisplayOrder = c.DisplayOrder,
                    DiscountPercentSale = c.DiscountPercentSale,
                    DiscountPercentPurchase = c.DiscountPercentPurchase,
                    LoyaltyPoints = c.LoyaltyPoints,
                    LoyaltyPointsPercent = c.LoyaltyPointsPercent,
                    ShowInReceiptImage = c.ShowInReceiptImage,
                    ShowAsShortcut = c.ShowAsShortcut,
                    ShowInMobile = c.ShowInMobile,
                    ShowInOnlineOrder = c.ShowInOnlineOrder,
                    VisibleInBranches = c.VisibleInBranches,
                    DiscountNotApplicable = c.DiscountNotApplicable,
                    PromotionNotApplicable = c.PromotionNotApplicable,
                    TaxRateId = c.TaxRateId is int tr ? MapTaxRate(tr) : null,
                    ParentCategoryId = parentCode is not null && tgtCategoryCodes.TryGetValue(parentCode, out var tp) ? tp.Id : null
                };
                target.ProductCategories.Add(newCategory);
                await target.SaveChangesAsync();
                tgtCategoryCodes[c.Code] = newCategory;
                categoriesAdded++;
            }
        }
        if (pending.Count > 0)
        {
            report.Add($"{pending.Count} kategori üst kategori döngüsü yüzünden eklenemedi.");
        }
        report.Add($"Kategori: {categoriesAdded} eklendi.");

        // 2) Stok kartları
        var srcProducts = await src.Products.AsNoTracking()
            .Where(x => request.Products.Contains(x.StockCode)).ToListAsync();
        var tgtProductCodes = await target.Products.Select(x => x.StockCode).ToHashSetAsync();
        var tgtBarcodes = await target.Products.Where(x => x.Barcode != null).Select(x => x.Barcode!).ToHashSetAsync();
        var productsAdded = 0;
        var productsSkipped = 0;
        foreach (var p in srcProducts)
        {
            if (tgtProductCodes.Contains(p.StockCode))
            {
                report.Add($"Stok '{p.StockCode}' zaten hedefte var, atlandı.");
                productsSkipped++;
                continue;
            }

            var srcCategory = srcCategories.GetValueOrDefault(p.CategoryId);
            var categoryTarget = srcCategory is not null && tgtCategoryCodes.TryGetValue(srcCategory.Code, out var ct) ? ct : null;
            var taxId = MapTaxRate(p.TaxRateId);
            if (categoryTarget is null || taxId is null)
            {
                report.Add($"Stok '{p.StockCode}' atlandı: hedefte kategori veya KDV oranı bulunamadı.");
                productsSkipped++;
                continue;
            }

            var barcode = p.Barcode;
            if (barcode is not null && tgtBarcodes.Contains(barcode))
            {
                barcode = null;
            }

            target.Products.Add(new Product
            {
                RecordId = Guid.NewGuid(),
                StockCode = p.StockCode,
                Name = p.Name,
                AlternateName = p.AlternateName,
                Brand = p.Brand,
                Model = p.Model,
                Barcode = barcode,
                Unit = p.Unit,
                UnitOfMeasureId = MapUnit(p.UnitOfMeasureId),
                ProductType = p.ProductType,
                ShelfLifeDays = p.ShelfLifeDays,
                CountryOfOrigin = p.CountryOfOrigin,
                Description = p.Description,
                PurchasePrice = p.PurchasePrice,
                SalePrice = p.SalePrice,
                StockQuantity = 0,
                MinimumStockQuantity = p.MinimumStockQuantity,
                TrackStock = p.TrackStock,
                IsActive = p.IsActive,
                LoyaltyPoints = p.LoyaltyPoints,
                ShowAsShortcut = p.ShowAsShortcut,
                ShowInMobile = p.ShowInMobile,
                ShowInOnlineOrder = p.ShowInOnlineOrder,
                VisibleInBranches = p.VisibleInBranches,
                DiscountNotApplicable = p.DiscountNotApplicable,
                PromotionNotApplicable = p.PromotionNotApplicable,
                KitchenPrinterName = p.KitchenPrinterName,
                TrackSerialNumbers = p.TrackSerialNumbers,
                TrackLots = p.TrackLots,
                CategoryId = categoryTarget.Id,
                TaxRateId = taxId.Value
            });
            tgtProductCodes.Add(p.StockCode);
            if (barcode is not null)
            {
                tgtBarcodes.Add(barcode);
            }
            productsAdded++;
        }
        await target.SaveChangesAsync();
        report.Add($"Stok kartı: {productsAdded} eklendi, {productsSkipped} atlandı.");

        // 3) Cari kartları
        var srcCustomers = await src.Customers.AsNoTracking()
            .Where(x => request.Customers.Contains(x.Code)).ToListAsync();
        var tgtCustomerCodes = await target.Customers.Select(x => x.Code).ToHashSetAsync();
        var customersAdded = 0;
        foreach (var c in srcCustomers)
        {
            if (tgtCustomerCodes.Contains(c.Code))
            {
                report.Add($"Cari '{c.Code}' zaten hedefte var, atlandı.");
                continue;
            }

            target.Customers.Add(new Customer
            {
                RecordId = Guid.NewGuid(),
                Code = c.Code,
                Name = c.Name,
                AccountType = c.AccountType,
                TaxOffice = c.TaxOffice,
                TaxNumber = c.TaxNumber,
                IdentityNumber = c.IdentityNumber,
                CustomerGroup = c.CustomerGroup,
                RiskLimit = c.RiskLimit,
                DefaultPaymentTermDays = c.DefaultPaymentTermDays,
                AuthorizedPerson = c.AuthorizedPerson,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                City = c.City,
                District = c.District,
                Notes = c.Notes,
                IsCustomer = c.IsCustomer,
                IsSupplier = c.IsSupplier,
                IsActive = c.IsActive,
                IsCollectionCari = c.IsCollectionCari,
                CreatedByUserId = TransferUserId
            });
            tgtCustomerCodes.Add(c.Code);
            customersAdded++;
        }
        await target.SaveChangesAsync();
        report.Add($"Cari kartı: {customersAdded} eklendi.");

        return report;
    }
}
