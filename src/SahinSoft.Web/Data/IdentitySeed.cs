using Microsoft.AspNetCore.Identity;
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

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"{message} {string.Join(" ", result.Errors.Select(error => error.Description))}");
        }
    }
}
