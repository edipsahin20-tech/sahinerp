using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// Yetki Mimarisi (Edip, 2026-09-04, madde 21) - kullanıcı tanımlı yetki profilleri, personel
// başına ÇOKLU profil atanabilir (herhangi biri izin veriyorsa işlem serbesttir). Administrator
// rolü profile hiç bakmadan her zaman tam yetkilidir. Hiç profili olmayan kullanıcı için
// VARSAYILAN TÜM işlemler serbesttir (spec: "sistem varsayılan olarak bütün yetkiler açık
// başlayabilir") - bu yüzden mevcut personelin çalışan akışı bu özellik eklendiği anda BOZULMAZ,
// kısıtlama sadece bilinçli olarak bir profil atandığında devreye girer.
public sealed class RestaurantPermissionService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
{
    public async Task<bool> HasPermissionAsync(string? userId, Func<RestaurantPermissionProfile, bool> permissionFlag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        if (await userManager.IsInRoleAsync(user, AppRoles.Administrator))
        {
            return true;
        }

        var assignedProfiles = await dbContext.RestaurantPersonnelPermissionProfiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.PermissionProfile)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        if (assignedProfiles.Count == 0)
        {
            return true;
        }

        return assignedProfiles.Any(permissionFlag);
    }

    public Task<bool> CanCancelOrderLineAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanCancelOrderLine, cancellationToken);

    public Task<bool> CanCancelReceiptAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanCancelReceipt, cancellationToken);

    public Task<bool> CanApplyDiscountAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanApplyDiscount, cancellationToken);

    public Task<bool> CanApplyComplimentaryAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanApplyComplimentary, cancellationToken);

    public Task<bool> CanEditKitchenSentLinesAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanEditKitchenSentLines, cancellationToken);

    public Task<bool> CanAddNoteAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanAddNote, cancellationToken);
}
