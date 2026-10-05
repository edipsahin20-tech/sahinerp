// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SahinSoft.Web.Data;
using SahinSoft.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SahinSoft.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private const string RememberEmailCookieName = "ss_remember_email";

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly DatabaseCatalog _catalog;
        private readonly IConfiguration _configuration;

        // Yeni oluşturulmuş bir firma veritabanı boştur; şema ve yönetici hesabı ilk kullanımda kurulur.
        private static readonly ConcurrentDictionary<string, bool> PreparedDatabases = new();

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, DatabaseCatalog catalog,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _logger = logger;
            _catalog = catalog;
            _configuration = configuration;
        }

        private async Task EnsureSelectedDatabaseReadyAsync(string profileName)
        {
            if (PreparedDatabases.ContainsKey(profileName))
            {
                return;
            }
            await DatabaseBootstrapper.PrepareAsync(HttpContext.RequestServices, _configuration);
            PreparedDatabases[profileName] = true;
        }

        // Edip, 2026-10-03: "firma hangi veritabanıyla çalışacağını kendi seçsin" - kayıtlı profiller
        // (ConfigTool'dan) burada listelenir. Liste boşsa seçici gösterilmez, eski tek-veritabanı davranışı.
        public IReadOnlyList<DatabaseProfile> DatabaseProfiles { get; set; } = [];

        // Masaüstü penceresinde firma seçimi yok - etkin (varsayılan) veritabanı kullanılır.
        public bool IsDesktop { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Beni hatırla")]
            public bool RememberMe { get; set; }

            // Boşsa (profil tanımlı değilse) varsayılan bağlantı kullanılır.
            public string DatabaseProfileName { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;

            IsDesktop = DatabaseRouter.IsDesktop(HttpContext);
            DatabaseProfiles = IsDesktop ? [] : _catalog.All();

            var rememberedEmail = Request.Cookies[RememberEmailCookieName];
            var lastDatabase = Request.Cookies[DatabaseRouter.SelectionKey];
            Input = new InputModel
            {
                Email = rememberedEmail,
                RememberMe = !string.IsNullOrEmpty(rememberedEmail),
                DatabaseProfileName = DatabaseProfiles.Any(x => x.Name == lastDatabase)
                    ? lastDatabase
                    : _catalog.Default()?.Name
            };
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            IsDesktop = DatabaseRouter.IsDesktop(HttpContext);
            DatabaseProfiles = IsDesktop ? [] : _catalog.All();

            if (!IsDesktop && DatabaseProfiles.Count > 0 && _catalog.Find(Input?.DatabaseProfileName) is null)
            {
                ModelState.AddModelError(string.Empty, "Lütfen geçerli bir veritabanı seçin.");
                return Page();
            }

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                Microsoft.AspNetCore.Identity.SignInResult result;
                try
                {
                    var effectiveDatabase = IsDesktop ? _catalog.Default()?.Name : Input.DatabaseProfileName;
                    if (!string.IsNullOrEmpty(effectiveDatabase))
                    {
                        await EnsureSelectedDatabaseReadyAsync(effectiveDatabase);
                    }
                    result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                }
                catch (SqlException ex)
                {
                    _logger.LogError(ex, "Login could not use the selected database {Database}.", Input.DatabaseProfileName);
                    ModelState.AddModelError(string.Empty, $"Seçilen veritabanı kullanılamadı ({ex.Message}). Lütfen başka bir veritabanı seçin veya yöneticinize başvurun.");
                    return Page();
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    if (Input.RememberMe)
                    {
                        Response.Cookies.Append(RememberEmailCookieName, Input.Email, new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(30),
                            HttpOnly = true,
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Lax
                        });
                    }
                    else
                    {
                        Response.Cookies.Delete(RememberEmailCookieName);
                    }

                    if (!IsDesktop && !string.IsNullOrEmpty(Input.DatabaseProfileName))
                    {
                        Response.Cookies.Append(DatabaseRouter.SelectionKey, Input.DatabaseProfileName, new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddYears(1),
                            HttpOnly = true,
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Lax
                        });
                    }

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
