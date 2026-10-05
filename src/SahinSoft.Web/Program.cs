using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Filters;
using SahinSoft.Web.Identity;
using SahinSoft.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// appsettings.Local.json ARTIK uygulamanın kendi kurulum klasöründe DEĞİL, %ProgramData%\SahinSoft
// altında (Edip, 2026-09-27, gerçek hata: "appsettings.Local.json dosyasına yazılamadı - Access to
// the path 'C:\SitesSahinSoft\appsettings.Local.json' is denied") - IIS altında çalışan bir site
// klasörü genellikle SADECE OKUNABİLİR (app pool kimliğinin yazma izni yoktur, hatta her
// publish'te üzerine yazılır); ProgramData ise TAM BUNUN İÇİN VAR OLAN, standart, her zaman
// yazılabilir bir Windows makine-geneli ayar konumu. RestaurantTerminalSettingsController'daki
// LocalOverridePath AYNI formülü kullanıyor - tek kaynak burada, path'i DEĞİŞTİRMEK isterseniz
// SADECE burayı ve o controller'ı güncelleyin.
var localOverridePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SahinSoft", "appsettings.Local.json");
builder.Configuration.AddJsonFile(localOverridePath, optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<SahinSoft.Web.Services.DatabaseCatalog>();
builder.Services.AddSingleton<SahinSoft.Web.Services.DatabaseRouter>();
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
    options.UseSqlServer(sp.GetRequiredService<SahinSoft.Web.Services.DatabaseRouter>().ConnectionString, sql =>
        sql.EnableRetryOnFailure()));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Varsayılan UserValidator<ApplicationUser> kaydını kaldırıp PIN-only personeli e-posta
// zorunluluğundan muaf tutan PinOnlyUserValidator ile değiştiriyoruz (yukarıdaki not).
builder.Services.RemoveAll<IUserValidator<ApplicationUser>>();
builder.Services.AddScoped<IUserValidator<ApplicationUser>, PinOnlyUserValidator>();

// Restoran modülü açıkken giriş yapılmamış istekler normal e-posta/şifre ekranı (/Identity/Account/Login)
// yerine PIN ekranına (RestaurantAuthController) yönlendirilir - restoran personelinin e-posta/karmaşık
// şifre öğrenmesine gerek kalmaz. Modül kapalıysa (ön muhasebe-only kurulum) davranış hiç değişmez.
builder.Services.ConfigureApplicationCookie(options =>
{
    var defaultRedirect = options.Events.OnRedirectToLogin;
    options.Events.OnRedirectToLogin = async context =>
    {
        // Edip (2026-09-02): "2 ayrı program olsun bi muhasebe bide restorant olsun" - kök adres
        // (/) ve muhasebe sayfaları ARTIK restoran'a hiç kaçırılmaz, her zaman normal e-posta/
        // şifre girişine gider (Program 1). SADECE bir restoran sayfası (/Restaurant...) doğrudan
        // istenip oturum yoksa Kasiyer Girişi'ne yönlendirilir (Program 2 - masaüstü kabuk bu
        // appsettings.json'daki "RestaurantShell:Url" ayarıyla doğrudan /RestaurantDashboard'a
        // bakıyor, bkz. SahinSoft.DesktopShell/ShellConfig.cs).
        // Önceki sürüm restaurantEnabled true olduğunda KÖK ADRESİ DE kaçırıyordu - bu, canlıda
        // muhasebe tarafının "kaybolduğu" izlenimi yaratan gerçek bir kullanılabilirlik hatasıydı.
        var requestPath = context.Request.Path.Value ?? "/";
        var isRestaurantPath = requestPath.StartsWith("/Restaurant", StringComparison.OrdinalIgnoreCase);

        if (isRestaurantPath)
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var restaurantEnabled = await dbContext.InventorySettings
                .AsNoTracking()
                .Where(x => x.Id == 1)
                .Select(x => x.IsRestaurantModuleEnabled)
                .SingleOrDefaultAsync();

            if (restaurantEnabled)
            {
                var returnUrl = requestPath + context.Request.QueryString;
                context.Response.Redirect($"/RestaurantAuth/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                return;
            }
        }

        await defaultRedirect(context);
    };
});

// Oturum kaydı (LoginSession): her oturum açmaya benzersiz bir "ss_sid" anahtarı eklenir ve sunucuda
// saklanır. İstekte anahtar iptal edilmişse oturum reddedilir; çıkış YALNIZCA o anahtarı iptal eder, aynı
// kullanıcının diğer terminal oturumları açık kalır. PostConfigure, Identity'nin kendi olay kayıtlarından
// sonra çalışır; önceki işleyiciler zincirlenerek korunur.
const string LoginSessionClaim = "ss_sid";
builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
{
    // Varsayılan Identity süresi 14 gün (kayan). Yalnızca açıkça yapılandırılırsa değişir (test/sertleştirme).
    var lifetimeMinutes = builder.Configuration.GetValue<int?>("Auth:CookieLifetimeMinutes");
    if (lifetimeMinutes is { } minutes and > 0)
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(minutes);
    }

    var previousSigningIn = options.Events.OnSigningIn;
    options.Events.OnSigningIn = async context =>
    {
        if (previousSigningIn is not null) await previousSigningIn(context);

        var identity = context.Principal?.Identities.FirstOrDefault();
        var userId = identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (identity is null || string.IsNullOrEmpty(userId) || identity.HasClaim(c => c.Type == LoginSessionClaim))
        {
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

        // Güvenlik damgası yenilemesi (Identity, ~30 dakikada bir) aynı kullanıcı için oturumu yeniden imzalar;
        // mevcut anahtar aynı kullanıcıdaysa korunur, böylece eski çerez kopyası açık kalmaz.
        var currentUser = context.HttpContext.User;
        var currentKey = currentUser?.FindFirst(LoginSessionClaim)?.Value;
        var currentUserId = currentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(currentKey) && currentUserId == userId)
        {
            identity.AddClaim(new Claim(LoginSessionClaim, currentKey));
            return;
        }

        var sessionKey = Guid.NewGuid().ToString("N");
        identity.AddClaim(new Claim(LoginSessionClaim, sessionKey));
        db.LoginSessions.Add(new LoginSession { SessionKey = sessionKey, UserId = userId, CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
    };

    var previousSigningOut = options.Events.OnSigningOut;
    options.Events.OnSigningOut = async context =>
    {
        if (previousSigningOut is not null) await previousSigningOut(context);

        var sessionKey = context.HttpContext.User?.FindFirst(LoginSessionClaim)?.Value;
        if (string.IsNullOrEmpty(sessionKey)) return;

        var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        await db.LoginSessions
            .Where(x => x.SessionKey == sessionKey && x.RevokedAtUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAtUtc, DateTime.UtcNow));
    };

    var previousValidate = options.Events.OnValidatePrincipal;
    options.Events.OnValidatePrincipal = async context =>
    {
        if (previousValidate is not null) await previousValidate(context);

        var sessionKey = context.Principal?.FindFirst(LoginSessionClaim)?.Value;
        if (string.IsNullOrEmpty(sessionKey)) return; // Anahtarı olmayan eski oturumlar (yalnızca bu sürümden önce açılanlar) olduğu gibi kabul edilir.

        var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        var active = await db.LoginSessions.AsNoTracking()
            .AnyAsync(x => x.SessionKey == sessionKey && x.RevokedAtUtc == null);
        if (!active)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
    };
});

builder.Services.AddScoped<SahinSoft.Web.Services.RestaurantShellService>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ConcurrencyExceptionFilter>();
    // Türkçe ondalık virgül ("125,50") 100 kat büyümesin: tüm decimal form alanları bu binder'dan geçer.
    options.ModelBinderProviders.Insert(0, new SahinSoft.Web.Services.TurkishDecimalModelBinderProvider());
});
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

// Data Protection anahtarları KALICI bir konuma yazılır - varsayılan otomatik konum tahmini,
// masaüstü kabuğun (SahinSoft.exe) her güncellemede farklı bir klasöre kopyalanabilmesi veya
// kısıtlı kullanıcı profili gibi durumlarda kararsız olabiliyor; kararsız/bulunamayan anahtar
// halkası her istek arasında farklı bir anahtar kullanılmasına, dolayısıyla antiforgery
// doğrulamasının HER SEFERİNDE (Edip, 2026-09-03: "şifre girdiğimde her zaman bu geliyor")
// çıplak, içeriksiz bir 400 ile başarısız olmasına yol açabiliyordu. %ProgramData% Windows'ta
// güncellemeden etkilenmeyen, sabit ve yazılabilir bir konum - SADECE Windows'ta uygulanır,
// yereldeki (macOS) geliştirme/test ortamında CommonApplicationData yazılabilir olmayabiliyor
// (bkz. /usr/share), o yüzden orada .NET'in kendi varsayılanı kullanılmaya devam eder.
if (OperatingSystem.IsWindows())
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "SahinSoft", "DataProtection-Keys")))
        .SetApplicationName("SahinSoftWeb");
}
builder.Services.AddScoped<StockTransferService>();
builder.Services.AddScoped<BarcodeGeneratorService>();
builder.Services.AddScoped<StockCodeGeneratorService>();
builder.Services.AddScoped<DocumentNumberGeneratorService>();
builder.Services.AddScoped<InvoicePostingService>();
builder.Services.AddScoped<SahinSoft.Web.Services.DocumentHardDeleteService>();
builder.Services.AddScoped<SahinSoft.Web.Services.BranchSelectionService>();
builder.Services.AddScoped<StockSlipPostingService>();
builder.Services.AddScoped<InventoryCountPostingService>();
builder.Services.AddScoped<InventoryBalanceService>();
builder.Services.AddScoped<PaymentReceiptPostingService>();
builder.Services.AddScoped<NegotiableInstrumentPostingService>();
builder.Services.AddScoped<RestaurantPostingService>();
builder.Services.AddScoped<RestaurantPermissionService>();
builder.Services.AddScoped<OverdueScheduleService>();
builder.Services.AddScoped<InvoiceCancellationOrchestrationService>();
builder.Services.AddScoped<DispatchNotePostingService>();
builder.Services.AddScoped<SahinSoft.Web.Services.Printing.IPrintDataProvider, SahinSoft.Web.Services.Printing.RestaurantPrintDataProvider>();
builder.Services.AddSingleton<SahinSoft.Web.Services.Printing.PrintRenderingService>();
builder.Services.AddScoped<SahinSoft.Web.Services.Printing.PrintDispatchService>();
builder.Services.AddHttpClient("PrintAgent");

// Hibrit yerel/bulut senkron (Faz B): MerkezSync:Enabled kapalıyken bu servis
// hemen uyanıp tekrar uyur, hiçbir şeye dokunmaz - şube tamamen bağımsız çalışır.
builder.Services.Configure<MerkezSyncOptions>(builder.Configuration.GetSection(MerkezSyncOptions.SectionName));
builder.Services.AddHttpClient("MerkezSync");
builder.Services.AddHostedService<BranchSyncBackgroundService>();
builder.Services.AddHostedService<KitchenAutoReadyBackgroundService>();
builder.Services.AddHostedService<RestaurantAutoZBackgroundService>();
builder.Services.AddHostedService<SahinSoft.Web.Services.Printing.PrintDispatchBackgroundService>();

var app = builder.Build();

// Giriş ekranı arka planı için (Edip, 2026-09-05: "bana bir yol yap ben görseli oraya attığımda
// otomatik görsel değişsin") - "uploads/" klasörü .gitignore'da hariç tutulan ve dotnet publish
// çıktısının HİÇ parçası olmayan tek klasör (bkz. ProductsController "uploads/products" - AYNI
// desen), bu yüzden Kurulum.ps1'in üzerine yazdığı her güncellemede korunur. Klasör burada
// önceden oluşturulur ki kurulumdan hemen sonra bile klasör gerçekten var olsun (fotoğraf
// yükleme akışının aksine, ilk kullanımı "kod içinden yazma" değil "dışarıdan dosya bırakma").
Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath, "uploads", "branding"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture: "tr-TR", uiCulture: "tr-TR"),
    SupportedCultures = [new CultureInfo("tr-TR")],
    SupportedUICultures = [new CultureInfo("tr-TR")]
});

// app.UseHttpsRedirection();
app.UseStaticFiles();

// Yeni oluşturulan firma veritabanını (ConfigTool "Yeni Veritabanı Oluştur") hazır hale getirir.
// Sadece bu makineden (loopback) kabul edilir; istek gövdesindeki profil adı, kayıtlı bir profil olmalı.
app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method)
        && context.Request.Path.Equals("/Setup/Prepare", StringComparison.OrdinalIgnoreCase))
    {
        var remote = context.Connection.RemoteIpAddress;
        if (remote is null || !System.Net.IPAddress.IsLoopback(remote))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var request = await System.Text.Json.JsonSerializer.DeserializeAsync<SahinSoft.Web.Services.PrepareDatabaseRequest>(context.Request.Body,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var catalog = context.RequestServices.GetRequiredService<SahinSoft.Web.Services.DatabaseCatalog>();
        if (request?.Profile is null || catalog.Find(request.Profile) is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("Profil bulunamadı.");
            return;
        }

        context.Items[SahinSoft.Web.Services.DatabaseRouter.SelectionKey] = request.Profile;
        try
        {
            await SahinSoft.Web.Data.DatabaseBootstrapper.PrepareAsync(context.RequestServices, app.Configuration);
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync("hazır");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Database preparation failed for profile {Profile}.", request.Profile);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
        return;
    }
    await next();
});

// Firmalar arası ana veri aktarımı (kategori/stok kartı/cari kartı). Sadece loopback; hedef firma
// istek kapsamında yönlendirilir, kaynak firma ayrı bir bağlantıyla okunur.
app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method)
        && context.Request.Path.Equals("/Setup/Transfer", StringComparison.OrdinalIgnoreCase))
    {
        var remote = context.Connection.RemoteIpAddress;
        if (remote is null || !System.Net.IPAddress.IsLoopback(remote))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var request = await System.Text.Json.JsonSerializer.DeserializeAsync<SahinSoft.Web.Services.MasterDataTransferRequest>(context.Request.Body,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var catalog = context.RequestServices.GetRequiredService<SahinSoft.Web.Services.DatabaseCatalog>();
        if (request?.Source is null || request.Target is null || catalog.Find(request.Source) is null || catalog.Find(request.Target) is null
            || request.Source.Equals(request.Target, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Kaynak ve hedef geçerli ve farklı firmalar olmalı.");
            return;
        }

        context.Items[SahinSoft.Web.Services.DatabaseRouter.SelectionKey] = request.Target;
        try
        {
            var target = context.RequestServices.GetRequiredService<SahinSoft.Web.Data.ApplicationDbContext>();
            var accessor = context.RequestServices.GetRequiredService<IHttpContextAccessor>();
            var report = await SahinSoft.Web.Services.MasterDataTransfer.RunAsync(request, catalog, target, accessor);
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync(string.Join("\n", report));
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Master data transfer failed from {Source} to {Target}.", request.Source, request.Target);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
        return;
    }
    await next();
});

// Giriş formunda seçilen veritabanı, kimlik doğrulama DbContext'i oluşturulmadan ÖNCE bilinmeli -
// bu yüzden POST gövdesindeki seçim burada okunup isteğin Items'ına konuyor (bkz. DatabaseRouter).
app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method)
        && context.Request.Path.StartsWithSegments("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase)
        && context.Request.HasFormContentType)
    {
        var form = await context.Request.ReadFormAsync();
        if (form.TryGetValue("Input.DatabaseProfileName", out var selected) && !string.IsNullOrWhiteSpace(selected))
        {
            context.Items[SahinSoft.Web.Services.DatabaseRouter.SelectionKey] = selected.ToString();
        }
    }
    await next();
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

try
{
    using var migrationScope = app.Services.CreateScope();
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    app.Logger.LogInformation("Database migrations applied on startup.");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database migration error on startup.");
}

try
{
    await IdentitySeed.InitializeAsync(app.Services, app.Configuration);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database seed error on startup.");
}

try
{
    await SahinSoft.Web.Data.NumberSequenceRepair.RunAsync(app.Services, app.Logger);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Numara sayacı onarımı başarısız.");
}

try
{
    await SahinSoft.Web.Data.PrintTemplateSeed.InitializeAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Print template seed error on startup.");
}

app.Run();
