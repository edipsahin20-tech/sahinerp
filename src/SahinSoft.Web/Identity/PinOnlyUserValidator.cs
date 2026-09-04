using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Identity;

// PIN ile giriş yapan restoran personelinin e-postası yoktur (Email = null). Varsayılan
// UserValidator<TUser>, RequireUniqueEmail=true olduğunda null/boş e-postayı bile
// "Email '' is invalid." hatasıyla reddediyor - PIN-only personel oluşturmayı tamamen
// engelleyen gerçek bir üretim hatasıydı (2026-09-04). Bu doğrulayıcı e-posta alanı
// GİRİLMİŞSE format + benzersizlik kontrolü yapar; boşsa e-posta doğrulamasını atlar.
// Program.cs'te varsayılan UserValidator<ApplicationUser> kaydı kaldırılıp yerine bu konur.
public sealed class PinOnlyUserValidator : IUserValidator<ApplicationUser>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        var errors = new List<IdentityError>();

        var userName = await manager.GetUserNameAsync(user);
        if (string.IsNullOrWhiteSpace(userName))
        {
            errors.Add(new IdentityError { Code = "InvalidUserName", Description = $"Kullanıcı adı '{userName}' geçersiz." });
        }
        else
        {
            var userId = await manager.GetUserIdAsync(user);
            var owner = await manager.FindByNameAsync(userName);
            if (owner is not null && !string.Equals(await manager.GetUserIdAsync(owner), userId, StringComparison.Ordinal))
            {
                errors.Add(new IdentityError { Code = "DuplicateUserName", Description = $"'{userName}' kullanıcı adı zaten kullanımda." });
            }
        }

        var email = await manager.GetEmailAsync(user);
        if (!string.IsNullOrWhiteSpace(email))
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                errors.Add(new IdentityError { Code = "InvalidEmail", Description = $"E-posta '{email}' geçersiz." });
            }
            else if (manager.Options.User.RequireUniqueEmail)
            {
                var userId = await manager.GetUserIdAsync(user);
                var owner = await manager.FindByEmailAsync(email);
                if (owner is not null && !string.Equals(await manager.GetUserIdAsync(owner), userId, StringComparison.Ordinal))
                {
                    errors.Add(new IdentityError { Code = "DuplicateEmail", Description = $"'{email}' e-postası zaten kullanımda." });
                }
            }
        }

        return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed([.. errors]);
    }
}
