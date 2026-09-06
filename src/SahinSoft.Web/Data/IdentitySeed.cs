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
        await EnsureSystemCashierAccountAsync(scope.ServiceProvider, userManager, configuration);
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

    // Restoran modülü için PIN'le giriş yapan, tam yetkili SABİT bir sistem/servis kasiyeri
    // (2026-09-06, Edip: "66 admin yetkili... sabit olucak her müşteride"). appsettings.json'a
    // BAĞIMLI DEĞİLDİR - PersonnelCode ve PIN kasıtlı olarak kod sabiti: bu bir gizli/kişisel
    // secret DEĞİL, tanım gereği HER kurulumda aynı olması istenen, dokümante edilmiş bir servis
    // erişim kodudur (bkz. [[feedback_sahinsoft_secrets_in_package]] olayı - o, gerçek bir DB
    // şifresiydi, bununla karıştırılmamalı). Her uygulama başlangıcında çalışır ve İKİ senaryoyu
    // da tek yerde çözer: (a) hesap hiç yoksa oluşturur, (b) hesap zaten varsa (upgrade edilen
    // kurulum) PIN hash'inin GERÇEKTEN "66" ile doğrulandığını kontrol eder, doğrulanmıyorsa
    // (örn. eski/yanlış bir hash kalmışsa) düzeltir - idempotent, hash zaten doğruysa hiçbir
    // yazma yapmadan çıkar. PersonnelController.Edit bu hesabın PIN'ini/aktiflik durumunu
    // ApplicationUser.IsProtectedSystemAccount bayrağıyla ayrıca korur.
    private const string SystemCashierPersonnelCode = "KASIYER01";
    private const string SystemCashierPin = "66";

    private static async Task EnsureSystemCashierAccountAsync(IServiceProvider scopedServices, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        var passwordHasher = scopedServices.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        var existing = await userManager.Users.SingleOrDefaultAsync(x => x.PersonnelCode == SystemCashierPersonnelCode);

        if (existing is not null)
        {
            var verifyResult = existing.RestaurantPinHash is null
                ? PasswordVerificationResult.Failed
                : passwordHasher.VerifyHashedPassword(existing, existing.RestaurantPinHash, SystemCashierPin);
            var needsUpdate = false;

            if (verifyResult == PasswordVerificationResult.Failed)
            {
                existing.RestaurantPinHash = passwordHasher.HashPassword(existing, SystemCashierPin);
                needsUpdate = true;
            }

            if (!existing.IsProtectedSystemAccount)
            {
                existing.IsProtectedSystemAccount = true;
                needsUpdate = true;
            }

            if (!existing.IsActive)
            {
                existing.IsActive = true;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await userManager.UpdateAsync(existing);
            }

            var existingRoles = await userManager.GetRolesAsync(existing);
            foreach (var role in new[] { AppRoles.Administrator, AppRoles.RestaurantManager, AppRoles.Cashier, AppRoles.Waiter })
            {
                if (!existingRoles.Contains(role))
                {
                    EnsureSucceeded(
                        await userManager.AddToRoleAsync(existing, role),
                        $"Sistem kasiyerine '{role}' rolü tamamlanamadı.");
                }
            }

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
        var syntheticEmail = $"{SystemCashierPersonnelCode.ToLowerInvariant()}@kasiyer.local";

        var user = new ApplicationUser
        {
            UserName = SystemCashierPersonnelCode,
            Email = syntheticEmail,
            EmailConfirmed = false,
            FullName = configuration["BootstrapCashier:FullName"] ?? "Kasiyer",
            IsActive = true,
            PersonnelCode = SystemCashierPersonnelCode,
            BranchId = headOfficeBranchId,
            IsProtectedSystemAccount = true
        };

        var randomPart = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(18))
            .Replace("+", "A").Replace("/", "b").Replace("=", "9");
        var systemPassword = $"Aa1!{randomPart}";

        EnsureSucceeded(
            await userManager.CreateAsync(user, systemPassword),
            "Sistem kasiyeri oluşturulamadı.");

        user.RestaurantPinHash = passwordHasher.HashPassword(user, SystemCashierPin);
        await userManager.UpdateAsync(user);

        // "Tam yetkili" - Administrator zaten her restoran ekranının yetki listesinde var (bkz.
        // AppRoles.cs yorumu), diğerleri PIN giriş ekranındaki "Kasiyer" etiketiyle ve olası dar
        // [Authorize] kontrolleriyle tutarlı olsun diye eklendi.
        foreach (var role in new[] { AppRoles.Administrator, AppRoles.RestaurantManager, AppRoles.Cashier, AppRoles.Waiter })
        {
            EnsureSucceeded(
                await userManager.AddToRoleAsync(user, role),
                $"Sistem kasiyerine '{role}' rolü atanamadı.");
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
