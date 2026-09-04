using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Yetki Mimarisi (Edip, 2026-09-04, madde 21) - kullanıcı tanımlı yetki profilleri burada
// oluşturulur/düzenlenir, personele atama PersonnelController'daki "Yetki Listesi" alanından
// yapılır (bkz. o form). Yalnızca Administrator/RestaurantManager erişebilir - bu ekran yetki
// dağıtımının kendisi, en hassas kısım.
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager}")]
public sealed class RestaurantPermissionProfilesController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var profiles = await dbContext.RestaurantPermissionProfiles
            .AsNoTracking()
            .Include(x => x.Assignments)
            .OrderBy(x => x.Name)
            .ToListAsync();
        return View(profiles);
    }

    public IActionResult Create() => View("Form", new RestaurantPermissionProfileFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantPermissionProfileFormViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.Name))
        {
            ModelState.AddModelError(nameof(form.Name), "Profil adı zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var profile = new RestaurantPermissionProfile();
        Map(form, profile);
        dbContext.RestaurantPermissionProfiles.Add(profile);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Yetki profili kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var profile = await dbContext.RestaurantPermissionProfiles.SingleOrDefaultAsync(x => x.Id == id);
        if (profile is null)
        {
            return NotFound();
        }

        var model = new RestaurantPermissionProfileFormViewModel
        {
            Id = profile.Id,
            Name = profile.Name,
            IsActive = profile.IsActive,
            CanCancelOrderLine = profile.CanCancelOrderLine,
            CanCancelReceipt = profile.CanCancelReceipt,
            CanApplyDiscount = profile.CanApplyDiscount,
            CanApplyComplimentary = profile.CanApplyComplimentary,
            CanEditKitchenSentLines = profile.CanEditKitchenSentLines,
            CanAddNote = profile.CanAddNote,
            CanSeeTableTransfer = profile.CanSeeTableTransfer,
            CanSeeSendToKitchen = profile.CanSeeSendToKitchen,
            CanSeePriceCheck = profile.CanSeePriceCheck,
            CanSeeKeyboard = profile.CanSeeKeyboard,
            CanSeeHoldReceipt = profile.CanSeeHoldReceipt,
            CanSeeHeldReceipts = profile.CanSeeHeldReceipts,
            CanSeeProductList = profile.CanSeeProductList,
            CanSeeReceiptList = profile.CanSeeReceiptList,
            CanClearOrder = profile.CanClearOrder
        };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RestaurantPermissionProfileFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        if (string.IsNullOrWhiteSpace(form.Name))
        {
            ModelState.AddModelError(nameof(form.Name), "Profil adı zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var profile = await dbContext.RestaurantPermissionProfiles.SingleOrDefaultAsync(x => x.Id == id);
        if (profile is null)
        {
            return NotFound();
        }

        Map(form, profile);
        profile.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Yetki profili güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var profile = await dbContext.RestaurantPermissionProfiles.SingleOrDefaultAsync(x => x.Id == id);
        if (profile is not null)
        {
            profile.IsActive = false;
            profile.UpdatedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Yetki profili pasifleştirildi.";
        }
        return RedirectToAction(nameof(Index));
    }

    private static void Map(RestaurantPermissionProfileFormViewModel source, RestaurantPermissionProfile target)
    {
        target.Name = source.Name.Trim();
        target.IsActive = source.IsActive;
        target.CanCancelOrderLine = source.CanCancelOrderLine;
        target.CanCancelReceipt = source.CanCancelReceipt;
        target.CanApplyDiscount = source.CanApplyDiscount;
        target.CanApplyComplimentary = source.CanApplyComplimentary;
        target.CanEditKitchenSentLines = source.CanEditKitchenSentLines;
        target.CanAddNote = source.CanAddNote;
        target.CanSeeTableTransfer = source.CanSeeTableTransfer;
        target.CanSeeSendToKitchen = source.CanSeeSendToKitchen;
        target.CanSeePriceCheck = source.CanSeePriceCheck;
        target.CanSeeKeyboard = source.CanSeeKeyboard;
        target.CanSeeHoldReceipt = source.CanSeeHoldReceipt;
        target.CanSeeHeldReceipts = source.CanSeeHeldReceipts;
        target.CanSeeProductList = source.CanSeeProductList;
        target.CanSeeReceiptList = source.CanSeeReceiptList;
        target.CanClearOrder = source.CanClearOrder;
    }
}
