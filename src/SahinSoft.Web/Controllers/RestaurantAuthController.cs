using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Restoran POS ekranı için hafif giriş: e-posta/karmaşık şifre yerine isim seçip kısa bir PIN
// giriyor (bkz. ApplicationUser.RestaurantPinHash, Personnel formundaki "PIN" alanı). Restoran
// modülü açıkken varsayılan giriş ekranı bu olur (Program.cs, cookie OnRedirectToLogin) - normal
// e-posta girişine "Yönetici Girişi" linkiyle geçilebilir.
[AllowAnonymous]
public sealed class RestaurantAuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IPasswordHasher<ApplicationUser> passwordHasher) : Controller
{
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        var staff = await userManager.Users
            .Where(x => x.IsActive && x.RestaurantPinHash != null)
            .OrderBy(x => x.FullName)
            .Select(x => new RestaurantStaffPickerItem { Id = x.Id, FullName = x.FullName })
            .ToListAsync();

        return View(new RestaurantPinLoginViewModel { Staff = staff, ReturnUrl = returnUrl ?? DefaultReturnUrl });
    }

    // Restoran modülü kendi shell'inde açılır - giriş sonrası ön muhasebenin Home/Index'ine değil,
    // doğrudan restoran Dashboard'una düşer (returnUrl belirtilmemişse).
    private string DefaultReturnUrl => Url.Action(nameof(RestaurantDashboardController.Index), "RestaurantDashboard") ?? "/";

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string userId, string pin, string? returnUrl = null)
    {
        returnUrl ??= DefaultReturnUrl;
        var user = await userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive || string.IsNullOrEmpty(user.RestaurantPinHash) || string.IsNullOrWhiteSpace(pin))
        {
            TempData["Error"] = "PIN hatalı.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var verifyResult = passwordHasher.VerifyHashedPassword(user, user.RestaurantPinHash, pin.Trim());
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            TempData["Error"] = "PIN hatalı.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        // PasswordSignInAsync değil - PIN zaten yukarıda doğrulandı, burada doğrudan cookie
        // oluşturuluyor (Identity'nin normal e-posta/şifre kontrolünü tekrar tetiklemeye gerek yok).
        await signInManager.SignInAsync(user, isPersistent: true);
        return LocalRedirect(returnUrl);
    }
}
