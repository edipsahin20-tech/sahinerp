using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class CategoriesController(ApplicationDbContext dbContext) : Controller
{
    // Liste ve form artık tek ekranda (Dinosoft tarzı: solda ağaç, sağda form) - ayrı bir liste
    // sayfası yok, "Kategori Tanımla" menüsü doğrudan Create'e (boş form + ağaç) düşer.
    public IActionResult Index() => RedirectToAction(nameof(Create));

    public async Task<IActionResult> Create()
    {
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "Categories" };
        var model = new CategoryFormViewModel();
        await PopulateOptionsAsync(model);
        return View("Form", model);
    }

    // Sol ağaçtaki kategori listesini besler - sadece 2 seviye (ana/alt), her ana kategori kendi
    // alt kategorileriyle birlikte gelir. Form.cshtml bunu ViewBag.Tree üzerinden okur.
    private async Task<List<ProductCategory>> BuildTreeAsync()
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .Include(x => x.SubCategories.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name))
            .Where(x => x.ParentCategoryId == null)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .ToListAsync();
    }

    // Stok Tanıtım Kartı'ndaki Kategori dropdown'ının yanındaki "+" için: sayfadan hiç ayrılmadan
    // yeni kategori eklenir. Code alanı isimden türetilir (mevcut ekranda kullanıcı elle giriyor,
    // burada akışı kesmemek için otomatik üretilir) — çakışırsa sayısal sonek eklenir.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuickApi([FromForm] string name)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { error = "Kategori adı zorunludur." });
        }

        var baseCode = new string(name.ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
        if (baseCode.Length > 12)
        {
            baseCode = baseCode[..12];
        }
        if (baseCode.Length == 0)
        {
            baseCode = "KAT";
        }

        var code = baseCode;
        var suffix = 1;
        while (await dbContext.ProductCategories.AnyAsync(x => x.Code == code))
        {
            suffix++;
            code = $"{baseCode}{suffix}";
        }

        var category = new ProductCategory { Code = code, Name = name };
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        return Json(new { id = category.Id, name = category.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel form)
    {
        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(form);
            return View("Form", form);
        }

        if (form.ParentCategoryId is int parentId && await dbContext.ProductCategories.AnyAsync(x => x.Id == parentId && x.ParentCategoryId != null))
        {
            ModelState.AddModelError(nameof(form.ParentCategoryId), "Bir alt kategori, başka bir kategorinin ana kategorisi olamaz.");
            await PopulateOptionsAsync(form);
            return View("Form", form);
        }

        var category = new ProductCategory();
        Map(form, category);
        dbContext.ProductCategories.Add(category);

        if (!await TrySaveAsync())
        {
            await PopulateOptionsAsync(form);
            return View("Form", form);
        }

        TempData["Success"] = "Kategori kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await dbContext.ProductCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        var model = new CategoryFormViewModel
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            AlternateName = category.AlternateName,
            Unit = category.Unit,
            WebsitePath = category.WebsitePath,
            Color = category.Color,
            DisplayOrder = category.DisplayOrder,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = category.IsActive,
            ShowAsShortcut = category.ShowAsShortcut,
            ShowInMobile = category.ShowInMobile,
            ShowInOnlineOrder = category.ShowInOnlineOrder,
            VisibleInBranches = category.VisibleInBranches,
            DiscountNotApplicable = category.DiscountNotApplicable,
            PromotionNotApplicable = category.PromotionNotApplicable,
            TaxRateId = category.TaxRateId,
            DefaultKitchenStationId = category.DefaultKitchenStationId,
            DiscountPercentSale = category.DiscountPercentSale,
            DiscountPercentPurchase = category.DiscountPercentPurchase,
            LoyaltyPoints = category.LoyaltyPoints,
            LoyaltyPointsPercent = category.LoyaltyPointsPercent,
            ShowInReceiptImage = category.ShowInReceiptImage
        };
        await PopulateOptionsAsync(model, id);

        await SetToolbarAsync(id);
        return View("Form", model);
    }

    // "Stoklara Kaydet": kategorideki TÜM ayarları (KDV, mutfak istasyonu, kısayol/mobil/online
    // sipariş görünürlüğü, şube görünürlüğü, indirim/promosyon kısıtlaması) bu kategorideki TÜM
    // ürünlere toplu olarak yazar. Normal "Kaydet" sadece kategorinin kendisini kaydeder - bu
    // ayrı buton bilinçli bir ek adım, kategori kaydedilirken otomatik tetiklenmez.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyToProducts(int id)
    {
        var category = await dbContext.ProductCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        var products = await dbContext.Products.Where(x => x.CategoryId == id).ToListAsync();
        foreach (var product in products)
        {
            if (category.TaxRateId is int taxRateId)
            {
                product.TaxRateId = taxRateId;
            }
            if (category.DefaultKitchenStationId is int stationId)
            {
                product.DefaultKitchenStationId = stationId;
            }
            product.ShowAsShortcut = category.ShowAsShortcut;
            product.ShowInMobile = category.ShowInMobile;
            product.ShowInOnlineOrder = category.ShowInOnlineOrder;
            product.VisibleInBranches = category.VisibleInBranches;
            product.DiscountNotApplicable = category.DiscountNotApplicable;
            product.PromotionNotApplicable = category.PromotionNotApplicable;
            product.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();

        TempData["Success"] = $"{products.Count} üründe kategori ayarları (KDV, yazıcı, görünürlük, indirim/promosyon kısıtlaması) eşleştirildi.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await dbContext.ProductCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        if (await dbContext.Products.AnyAsync(x => x.CategoryId == id))
        {
            TempData["Error"] = "Bu kategoriye bağlı ürünler var, silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        dbContext.ProductCategories.Remove(category);
        try
        {
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Kategori silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Bu kategoriye bağlı kayıtlar var, silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MergeInto(int id, int targetCategoryId)
    {
        if (id == targetCategoryId)
        {
            TempData["Error"] = "Kaynak ve hedef kategori aynı olamaz.";
            return RedirectToAction(nameof(Index));
        }

        var source = await dbContext.ProductCategories.SingleOrDefaultAsync(x => x.Id == id);
        var target = await dbContext.ProductCategories.SingleOrDefaultAsync(x => x.Id == targetCategoryId);
        if (source is null || target is null)
        {
            return NotFound();
        }

        var affectedProducts = await dbContext.Products.Where(x => x.CategoryId == id).ToListAsync();
        foreach (var product in affectedProducts)
        {
            product.CategoryId = targetCategoryId;
            product.UpdatedAtUtc = DateTime.UtcNow;
        }

        dbContext.ProductCategories.Remove(source);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = $"\"{source.Name}\" kategorisi \"{target.Name}\" ile birleştirildi ({affectedProducts.Count} ürün taşındı).";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.ProductCategories.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.ProductCategories.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var hasProducts = await dbContext.Products.AnyAsync(x => x.CategoryId == id);

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "Categories",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = !hasProducts,
            DeleteBlockedReason = hasProducts ? "Bu kategoriye bağlı ürünler var, silinemez." : null
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        await ValidateUniqueCodeAsync(form);
        if (form.ParentCategoryId == id)
        {
            ModelState.AddModelError(nameof(form.ParentCategoryId), "Bir kategori kendi ana kategorisi olamaz.");
        }
        else if (form.ParentCategoryId is int parentId && await dbContext.ProductCategories.AnyAsync(x => x.Id == parentId && x.ParentCategoryId != null))
        {
            ModelState.AddModelError(nameof(form.ParentCategoryId), "Bir alt kategori, başka bir kategorinin ana kategorisi olamaz.");
        }
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(form, id);
            return View("Form", form);
        }

        var category = await dbContext.ProductCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        Map(form, category);
        category.UpdatedAtUtc = DateTime.UtcNow;

        if (!await TrySaveAsync())
        {
            await PopulateOptionsAsync(form, id);
            return View("Form", form);
        }

        TempData["Success"] = "Kategori güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private async Task ValidateUniqueCodeAsync(CategoryFormViewModel model)
    {
        if (await dbContext.ProductCategories.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu kategori kodu zaten kullanılıyor.");
        }
    }

    private static void Map(CategoryFormViewModel source, ProductCategory target)
    {
        target.Code = source.Code.Trim();
        target.Name = source.Name.Trim();
        target.AlternateName = string.IsNullOrWhiteSpace(source.AlternateName) ? null : source.AlternateName.Trim();
        target.Unit = string.IsNullOrWhiteSpace(source.Unit) ? null : source.Unit.Trim();
        target.WebsitePath = string.IsNullOrWhiteSpace(source.WebsitePath) ? null : source.WebsitePath.Trim();
        target.Color = source.Color.Trim();
        target.DisplayOrder = source.DisplayOrder;
        target.ParentCategoryId = source.ParentCategoryId;
        target.IsActive = source.IsActive;
        target.ShowAsShortcut = source.ShowAsShortcut;
        target.ShowInMobile = source.ShowInMobile;
        target.ShowInOnlineOrder = source.ShowInOnlineOrder;
        target.VisibleInBranches = source.VisibleInBranches;
        target.DiscountNotApplicable = source.DiscountNotApplicable;
        target.PromotionNotApplicable = source.PromotionNotApplicable;
        target.TaxRateId = source.TaxRateId;
        target.DefaultKitchenStationId = source.DefaultKitchenStationId;
        target.DiscountPercentSale = source.DiscountPercentSale;
        target.DiscountPercentPurchase = source.DiscountPercentPurchase;
        target.LoyaltyPoints = source.LoyaltyPoints;
        target.LoyaltyPointsPercent = source.LoyaltyPointsPercent;
        target.ShowInReceiptImage = source.ShowInReceiptImage;
    }

    private async Task PopulateOptionsAsync(CategoryFormViewModel model, int? excludeId = null)
    {
        ViewBag.Tree = await BuildTreeAsync();

        model.TaxRateOptions = await dbContext.TaxRates
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Rate)
            .Select(x => new ValueTuple<int, string>(x.Id, x.Name + " (%" + x.Rate + ")"))
            .ToListAsync();

        model.KitchenStationOptions = await dbContext.KitchenStations
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new ValueTuple<int, string>(x.Id, x.Name + (x.PrinterName != null ? " (" + x.PrinterName + ")" : "")))
            .ToListAsync();

        // Sadece 2 seviye destekleniyor (Ana Kategori / Alt Kategori) - ana kategori olarak
        // yalnızca kendisi de bir alt kategori OLMAYAN kategoriler seçilebilir, bu da döngü
        // ("alt kategori kendi üst kategorisini ana kategori seçer" gibi) oluşmasını mimari
        // olarak imkansız kılıyor.
        var query = dbContext.ProductCategories.AsNoTracking().Where(x => x.ParentCategoryId == null);
        if (excludeId is int id)
        {
            query = query.Where(x => x.Id != id && x.ParentCategoryId != id);
        }
        model.ParentCategoryOptions = await query
            .OrderBy(x => x.Name)
            .Select(x => new ValueTuple<int, string>(x.Id, x.Name))
            .ToListAsync();
    }

    private async Task<bool> TrySaveAsync()
    {
        try
        {
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(CategoryFormViewModel.Code), "Bu kategori kodu başka bir kayıtta kullanılıyor.");
            return false;
        }
    }
}
