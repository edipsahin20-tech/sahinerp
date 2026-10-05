using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// Evrak ekranlarındaki "Şube" alanı (Edip, 2026-10-05: cari ekstrede "Şube atanmamış" görünmesin).
// Varsayılan: kullanıcının şubesi, yoksa merkez şube, yoksa ilk aktif şube.
public sealed class BranchSelectionService(ApplicationDbContext db)
{
    public async Task<List<SelectListItem>> OptionsAsync(int? selected = null)
    {
        var branches = await db.Branches.AsNoTracking()
            .Where(x => x.IsActive || x.Id == selected)
            .OrderByDescending(x => x.IsHeadOffice).ThenBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Code })
            .ToListAsync();
        return branches.Select(x => new SelectListItem($"{x.Code} - {x.Name}", x.Id.ToString(), x.Id == selected)).ToList();
    }

    public async Task<int?> DefaultBranchIdAsync(string? userId)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            var userBranch = await db.Users.AsNoTracking().Where(x => x.Id == userId).Select(x => x.BranchId).SingleOrDefaultAsync();
            if (userBranch is not null && await db.Branches.AnyAsync(x => x.Id == userBranch && x.IsActive)) { return userBranch; }
        }
        return await db.Branches.AsNoTracking().Where(x => x.IsActive)
            .OrderByDescending(x => x.IsHeadOffice).ThenBy(x => x.Id)
            .Select(x => (int?)x.Id).FirstOrDefaultAsync();
    }

    // Evrakta depo seçimi yok: stok hareketi şubeye bağlı depoya yazılır (varsayılan depo, yoksa ilk aktif depo).
    public async Task<int?> WarehouseForBranchAsync(int? branchId)
    {
        if (branchId is not int id) { return null; }
        return await db.Warehouses.AsNoTracking()
            .Where(x => x.BranchId == id && x.IsActive)
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id)
            .Select(x => (int?)x.Id).FirstOrDefaultAsync();
    }

    public const string NoWarehouseMessage = "Seçilen şubeye bağlı aktif bir depo yok. Önce Stok > Depolar'dan bu şubeye bir depo tanımlayın.";

    // Gönderilen şube geçerliyse onu, değilse varsayılanı döner.
    public async Task<int?> ResolveAsync(int? requested, string? userId)
    {
        if (requested is int id && await db.Branches.AnyAsync(x => x.Id == id)) { return id; }
        return await DefaultBranchIdAsync(userId);
    }
}
