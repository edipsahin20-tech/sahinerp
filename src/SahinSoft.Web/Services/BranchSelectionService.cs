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
        var companyDefault = await db.CompanySettings.AsNoTracking().Where(x => x.Id == 1).Select(x => x.DefaultBranchId).SingleOrDefaultAsync();
        if (companyDefault is int cd && await db.Branches.AnyAsync(x => x.Id == cd && x.IsActive)) { return cd; }
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

    // Evraktaki varsayılan depo: şube şirket varsayılan şubesiyse şirketin varsayılan deposu, değilse şubenin deposu.
    public async Task<int?> DefaultWarehouseIdAsync(int? branchId)
    {
        var company = await db.CompanySettings.AsNoTracking().Where(x => x.Id == 1).Select(x => new { x.DefaultBranchId, x.DefaultWarehouseId }).SingleOrDefaultAsync();
        if (company?.DefaultWarehouseId is int dw && (branchId is null || company.DefaultBranchId == branchId || company.DefaultBranchId is null)
            && await db.Warehouses.AnyAsync(x => x.Id == dw && x.IsActive)) { return dw; }
        return await WarehouseForBranchAsync(branchId);
    }

    public async Task<List<SahinSoft.Web.Models.WarehouseOption>> WarehouseOptionsAsync(int? selected = null)
    {
        return await db.Warehouses.AsNoTracking()
            .Where(x => x.IsActive || x.Id == selected)
            .OrderBy(x => x.Branch.Name).ThenByDescending(x => x.IsDefault).ThenBy(x => x.Name)
            .Select(x => new SahinSoft.Web.Models.WarehouseOption(x.Id, x.Code + " - " + x.Name + " (" + x.Branch.Name + ")", x.BranchId, x.IsDefault))
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> WarehouseSelectItemsAsync(int? selected = null)
    {
        var all = await WarehouseOptionsAsync(selected);
        return all.Select(x => new SelectListItem(x.Text, x.Id.ToString(), x.Id == selected)).ToList();
    }

    public async Task<bool> MikroAmountsAsync() =>
        await db.CompanySettings.AsNoTracking().Where(x => x.Id == 1).Select(x => x.MikroCompatibleAmounts).SingleOrDefaultAsync();

    public const string NoWarehouseMessage = "Seçilen şubeye bağlı aktif bir depo yok. Önce Stok > Depolar'dan bu şubeye bir depo tanımlayın.";

    // Gönderilen şube geçerliyse onu, değilse varsayılanı döner.
    public async Task<int?> ResolveAsync(int? requested, string? userId)
    {
        if (requested is int id && await db.Branches.AnyAsync(x => x.Id == id)) { return id; }
        return await DefaultBranchIdAsync(userId);
    }
}
