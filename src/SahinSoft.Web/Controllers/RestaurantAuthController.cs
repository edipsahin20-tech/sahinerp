using Microsoft.AspNetCore.Antiforgery;
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
    IPasswordHasher<ApplicationUser> passwordHasher,
    IAntiforgery antiforgery) : Controller
{
    // Personel PIN giriş ekranı (2026-09-05 teknik doküman madde 12, onaylı görsel
    // 01_Giris_Ekrani_ONAYLI.png) - eskiden burada aktif/PIN'i olan tüm personelin isim listesi
    // yüklenip gösteriliyordu (RestaurantStaffPickerItem). Artık personel seçimi TAMAMEN
    // KALDIRILDI - sağda SADECE PIN pad var, doğru PIN doğru kullanıcıyı kendisi bulur (bkz.
    // POST Login, RestaurantPermissionService.VerifyApproverPinAsync'teki AYNI desen).
    public IActionResult Login(string? returnUrl = null)
        => View(new RestaurantPinLoginViewModel { ReturnUrl = returnUrl ?? DefaultReturnUrl });

    // Restoran modülü kendi shell'inde açılır - giriş sonrası ön muhasebenin Home/Index'ine değil,
    // doğrudan restoran Dashboard'una düşer (returnUrl belirtilmemişse).
    private string DefaultReturnUrl => Url.Action(nameof(RestaurantDashboardController.Index), "RestaurantDashboard") ?? "/";

    // [ValidateAntiForgeryToken] KASITLI kullanılmıyor - o filtre doğrulama başarısız olunca
    // hiçbir içerik taşımayan çıplak bir 400 döner (framework'ün kendi varsayılan davranışı,
    // UseExceptionHandler'a hiç uğramaz), tarayıcı da bunu "Bu sayfa şu anda çalışmıyor" diye
    // boş bir hata sayfası olarak gösterir (Edip, 2026-09-03: "şifre girdiğimde bu geliyor" -
    // yerelde antiforgery token'ı bozup AYNI çıplak 400'ü üretip doğruladım). Doğrulama burada
    // elle yapılıp başarısız olursa "PIN hatalı" ile AYNI, kullanıcının zaten bildiği akışa
    // (Login ekranına dön) düşülüyor - kasiyer için tek fark görünmüyor, sadece tekrar dener.
    //
    // userId ARTIK YOK (madde 12: "kullanıcı adı ifşa edilmeden hata gösterilir") - PIN, aktif
    // ve PIN'i tanımlı TÜM personelin hash'ine karşı denenir (VerifyApproverPinAsync'teki AYNI
    // desen), eşleşen İLK kullanıcı giriş yapan olur. Personel sayısı küçük olduğundan (bir
    // restoranın kadrosu) bu döngü performans sorunu yaratmaz.
    [HttpPost]
    public async Task<IActionResult> Login(string pin, string? returnUrl = null)
    {
        returnUrl ??= DefaultReturnUrl;

        try
        {
            await antiforgery.ValidateRequestAsync(HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            TempData["Error"] = "Oturum süresi doldu, lütfen tekrar deneyin.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        if (string.IsNullOrWhiteSpace(pin))
        {
            TempData["Error"] = "PIN hatalı.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var candidates = await userManager.Users
            .Where(x => x.IsActive && x.RestaurantPinHash != null)
            .ToListAsync();

        ApplicationUser? matched = null;
        foreach (var candidate in candidates)
        {
            var result = passwordHasher.VerifyHashedPassword(candidate, candidate.RestaurantPinHash!, pin.Trim());
            if (result != PasswordVerificationResult.Failed)
            {
                matched = candidate;
                break;
            }
        }

        if (matched is null)
        {
            TempData["Error"] = "PIN hatalı.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        // PasswordSignInAsync değil - PIN zaten yukarıda doğrulandı, burada doğrudan cookie
        // oluşturuluyor (Identity'nin normal e-posta/şifre kontrolünü tekrar tetiklemeye gerek yok).
        //
        // isPersistent: false (2026-09-27, Edip: "program nasıl kapattıysam kapatsın, şifre giriş
        // ekranı gelsin") - ÖNCEDEN true idi, bu yüzden WebView2'nin kalıcı çerez deposu programı
        // kapatıp AÇTIKTAN SONRA BİLE eski oturumu canlı tutuyordu (PIN ekranı hiç görünmüyordu).
        // false ile çerez bir "oturum çerezi" olur - kabuk (SahinSoft.DesktopShell) kapanınca
        // WebView2'nin oturumu sona erer, bir sonraki açılışta PIN ekranı HER ZAMAN gelir.
        await signInManager.SignInAsync(matched, isPersistent: false);
        return LocalRedirect(returnUrl);
    }

    // Ayarlar bölümüne giriş kapısı (2026-09-27, Edip: "ayarlar bölümüne tıkladığımda bir giriş
    // şifresi sorsun, 66 yetkili şifre ile giriş yapabilirsin") - kasiyer POS ekranında mevcut bir
    // oturum açıksa bile Ayarlar'a girmeden ÖNCE bilinçli olarak signOut yapılır, böylece
    // [Authorize(Roles=Administrator,RestaurantManager)] HER ZAMAN yeniden PIN sorar - düşük
    // yetkili bir kasiyerin oturumu açıkken sessizce içeri girilmesi engellenir.
    public async Task<IActionResult> SettingsGate()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login), new
        {
            returnUrl = Url.Action("Index", "RestaurantTerminalSettings")
        });
    }
}
