using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;

namespace SahinSoft.Web.Data;

public static class IdentitySeed
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                EnsureSucceeded(roleResult, $"'{role}' rolü oluşturulamadı.");
            }
        }

        await SeedBootstrapAdminAsync(userManager, configuration);
        await SeedBootstrapCashierAsync(scope.ServiceProvider, userManager, configuration);
    }

    private static async Task SeedBootstrapAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = configuration["BootstrapAdmin:FullName"] ?? "ŞahinSoft Yöneticisi"
            };

            EnsureSucceeded(
                await userManager.CreateAsync(user, password),
                "Başlangıç yöneticisi oluşturulamadı.");
        }

        if (!await userManager.IsInRoleAsync(user, AppRoles.Administrator))
        {
            EnsureSucceeded(
                await userManager.AddToRoleAsync(user, AppRoles.Administrator),
                "Başlangıç yöneticisine rol atanamadı.");
        }
    }

    // Restoran modülü için PIN'le giriş yapan, tam yetkili bir varsayılan kasiyer - BootstrapAdmin
    // ile AYNI desen (appsettings.json'daki "BootstrapCashier" bölümü boşsa hiçbir şey yapılmaz,
    // her kurulumda kendi appsettings.json'ında ayrıca tanımlanması gerekir, koda GÖMÜLÜ bir PIN
    // değeri YOKTUR - bkz. [[feedback_sahinsoft_secrets_in_package]]). Personel Kodu tabanlı
    // kullanıcı adı + rastgele güçlü sistem şifresi + ayrı PIN hash'i, PersonnelController.Create
    // ile AYNI kalıp (RestaurantPinHash normal AspNetUsers.PasswordHash'ten tamamen ayrı).
    private static async Task SeedBootstrapCashierAsync(IServiceProvider scopedServices, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        var pin = configuration["BootstrapCashier:Pin"];
        if (string.IsNullOrWhiteSpace(pin))
        {
            return;
        }

        var personnelCode = configuration["BootstrapCashier:PersonnelCode"] ?? "KASIYER01";
        var existing = await userManager.Users.SingleOrDefaultAsync(x => x.PersonnelCode == personnelCode);
        if (existing is not null)
        {
            return;
        }

        var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();
        var headOfficeBranchId = await dbContext.Branches
            .Where(x => x.IsHeadOffice)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();

        // Program.cs'te RequireUniqueEmail=true olduğu için Email null/boş bırakılamaz (Identity
        // "Email '' is invalid" diye reddediyor) - PIN'le giriş yapacağı için gerçek bir e-postaya
        // ihtiyacı yok, personelCode'dan türeyen sentetik ama geçerli formatlı bir adres veriliyor.
        var syntheticEmail = $"{personnelCode.ToLowerInvariant()}@kasiyer.local";

        var user = new ApplicationUser
        {
            UserName = personnelCode,
            Email = syntheticEmail,
            EmailConfirmed = false,
            FullName = configuration["BootstrapCashier:FullName"] ?? "Kasiyer",
            IsActive = true,
            PersonnelCode = personnelCode,
            BranchId = headOfficeBranchId
        };

        var randomPart = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(18))
            .Replace("+", "A").Replace("/", "b").Replace("=", "9");
        var systemPassword = $"Aa1!{randomPart}";

        EnsureSucceeded(
            await userManager.CreateAsync(user, systemPassword),
            "Başlangıç kasiyeri oluşturulamadı.");

        var passwordHasher = scopedServices.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        user.RestaurantPinHash = passwordHasher.HashPassword(user, pin.Trim());
        await userManager.UpdateAsync(user);

        // "Tam yetkili" - Administrator zaten her restoran ekranının yetki listesinde var (bkz.
        // AppRoles.cs yorumu), diğerleri PIN giriş ekranındaki "Kasiyer" etiketiyle ve olası dar
        // [Authorize] kontrolleriyle tutarlı olsun diye eklendi.
        foreach (var role in new[] { AppRoles.Administrator, AppRoles.RestaurantManager, AppRoles.Cashier, AppRoles.Waiter })
        {
            EnsureSucceeded(
                await userManager.AddToRoleAsync(user, role),
                $"Başlangıç kasiyerine '{role}' rolü atanamadı.");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"{message} {string.Join(" ", result.Errors.Select(error => error.Description))}");
        }
    }
}
