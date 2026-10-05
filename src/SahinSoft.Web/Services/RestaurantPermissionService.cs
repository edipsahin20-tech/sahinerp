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
public sealed class RestaurantPermissionService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IPasswordHasher<ApplicationUser> passwordHasher)
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

    // Sağ İşlem Menüsü görünürlüğü (madde 22) - profil bazlı katman. Sistem geneli katmanı
    // (InventorySettings.Enable*) ayrı, ikisi de kontrol edilmeli - bkz. Check.cshtml/
    // RestaurantController.
    public Task<bool> CanSeeTableTransferAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanSeeTableTransfer, cancellationToken);
    public Task<bool> CanSeeSendToKitchenAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanSeeSendToKitchen, cancellationToken);
    public Task<bool> CanSeePriceCheckAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanSeePriceCheck, cancellationToken);
    public Task<bool> CanSeeKeyboardAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanSeeKeyboard, cancellationToken);
    public Task<bool> CanSeeHoldReceiptAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanSeeHoldReceipt, cancellationToken);
    public Task<bool> CanSeeHeldReceiptsAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanSeeHeldReceipts, cancellationToken);
    public Task<bool> CanSeeProductListAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanSeeProductList, cancellationToken);
    public Task<bool> CanSeeReceiptListAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanSeeReceiptList, cancellationToken);
    public Task<bool> CanClearOrderAsync(string? userId, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(userId, p => p.CanClearOrder, cancellationToken);

    // Fiş İkram (madde 11) - Administrator gerekçe girmeden ikram uygulayabilir, diğer yetkili
    // kullanıcılar (CanApplyComplimentary açık ama Administrator değil) Kime/Neden doldurmak
    // zorundadır.
    public async Task<bool> IsAdministratorAsync(string? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        var user = await userManager.FindByIdAsync(userId);
        return user is not null && await userManager.IsInRoleAsync(user, AppRoles.Administrator);
    }

    // "Şifre sorulsun mu?" (madde 21) - InventorySettings'teki ilgili bayrak açıksa, bu işlem
    // yetkili kullanıcı tarafından yapılsa BİLE ikinci bir yetkilinin PIN onayı şart.
    public async Task<bool> RequiresSecondApprovalAsync(Func<Domain.Entities.InventorySettings, bool> settingFlag, CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.InventorySettings.AsNoTracking().Where(x => x.Id == 1).SingleOrDefaultAsync(cancellationToken);
        return settings is not null && settingFlag(settings);
    }

    public Task<bool> RequiresSecondApprovalForCancelOrderLineAsync(CancellationToken cancellationToken = default) =>
        RequiresSecondApprovalAsync(s => s.RequireSecondApprovalForCancelOrderLine, cancellationToken);

    public Task<bool> RequiresSecondApprovalForCancelReceiptAsync(CancellationToken cancellationToken = default) =>
        RequiresSecondApprovalAsync(s => s.RequireSecondApprovalForCancelReceipt, cancellationToken);

    public Task<bool> RequiresSecondApprovalForDiscountAsync(CancellationToken cancellationToken = default) =>
        RequiresSecondApprovalAsync(s => s.RequireSecondApprovalForDiscount, cancellationToken);

    public Task<bool> RequiresSecondApprovalForComplimentaryAsync(CancellationToken cancellationToken = default) =>
        RequiresSecondApprovalAsync(s => s.RequireSecondApprovalForComplimentary, cancellationToken);

    public Task<bool> RequiresSecondApprovalForEditKitchenSentLinesAsync(CancellationToken cancellationToken = default) =>
        RequiresSecondApprovalAsync(s => s.RequireSecondApprovalForEditKitchenSentLines, cancellationToken);

    public Task<bool> RequiresSecondApprovalForAddNoteAsync(CancellationToken cancellationToken = default) =>
        RequiresSecondApprovalAsync(s => s.RequireSecondApprovalForAddNote, cancellationToken);

    // Girilen PIN'i AKTİF, PIN'i olan tüm personelin hash'ine karşı dener (onaylayan, mevcut
    // oturumdaki kişi olmak zorunda değil - hangi isim olduğu PIN eşleşmesinden anlaşılır, bkz.
    // RestaurantAuthController'daki aynı VerifyHashedPassword deseni). Eşleşen kullanıcı bulunsa
    // bile, o kullanıcı bu SPESİFİK işlem için yetkili değilse onay reddedilir. Başarılıysa
    // onaylayan kullanıcının Id'sini döner.
    public async Task<string?> VerifyApproverPinAsync(string? pin, Func<RestaurantPermissionProfile, bool> requiredFlag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pin))
        {
            return null;
        }

        var candidates = await dbContext.Users
            .Where(x => x.IsActive && x.RestaurantPinHash != null)
            .ToListAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            var result = passwordHasher.VerifyHashedPassword(candidate, candidate.RestaurantPinHash!, pin.Trim());
            if (result != PasswordVerificationResult.Failed)
            {
                var authorized = await HasPermissionAsync(candidate.Id, requiredFlag, cancellationToken);
                return authorized ? candidate.Id : null;
            }
        }

        return null;
    }

    public async Task LogApprovalAsync(string action, string performedByUserId, string approverUserId, int? checkId = null, int? orderLineId = null, string? details = null, CancellationToken cancellationToken = default)
    {
        dbContext.RestaurantPermissionAuditLogs.Add(new RestaurantPermissionAuditLog
        {
            Action = action,
            PerformedByUserId = performedByUserId,
            ApproverUserId = approverUserId,
            RestaurantCheckId = checkId,
            RestaurantOrderLineId = orderLineId,
            Details = details,
            CreatedAtUtc = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
