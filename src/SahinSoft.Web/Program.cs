using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;
using SahinSoft.Web.Filters;
using SahinSoft.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
        sql.EnableRetryOnFailure()));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpContextAccessor();

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

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add<ConcurrencyExceptionFilter>());
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddScoped<StockTransferService>();
builder.Services.AddScoped<BarcodeGeneratorService>();
builder.Services.AddScoped<StockCodeGeneratorService>();
builder.Services.AddScoped<DocumentNumberGeneratorService>();
builder.Services.AddScoped<InvoicePostingService>();
builder.Services.AddScoped<StockSlipPostingService>();
builder.Services.AddScoped<InventoryCountPostingService>();
builder.Services.AddScoped<InventoryBalanceService>();
builder.Services.AddScoped<PaymentReceiptPostingService>();
builder.Services.AddScoped<NegotiableInstrumentPostingService>();
builder.Services.AddScoped<RestaurantPostingService>();
builder.Services.AddScoped<OverdueScheduleService>();
builder.Services.AddScoped<InvoiceCancellationOrchestrationService>();
builder.Services.AddScoped<DispatchNotePostingService>();

// Hibrit yerel/bulut senkron (Faz B): MerkezSync:Enabled kapalıyken bu servis
// hemen uyanıp tekrar uyur, hiçbir şeye dokunmaz - şube tamamen bağımsız çalışır.
builder.Services.Configure<MerkezSyncOptions>(builder.Configuration.GetSection(MerkezSyncOptions.SectionName));
builder.Services.AddHttpClient("MerkezSync");
builder.Services.AddHostedService<BranchSyncBackgroundService>();
builder.Services.AddHostedService<KitchenAutoReadyBackgroundService>();

var app = builder.Build();

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
    DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "tr-TR"),
    SupportedCultures = [new CultureInfo("en-US")],
    SupportedUICultures = [new CultureInfo("tr-TR")]
});

// app.UseHttpsRedirection();
app.UseStaticFiles();
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

app.Run();
