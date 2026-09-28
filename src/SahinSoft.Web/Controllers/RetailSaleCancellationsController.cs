using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Fiş İptal Listesi (Edip, 2026-09-28: "bide fiş iptal listesi diye bir rapor yap unude ekle") -
// ANA ERP'de yaşar (RetailSalesController ile AYNI genel erişim deseni), iptal edilmiş
// RetailSale kayıtlarını listeler. Detay tıklaması mevcut RetailSales/Detail ekranını yeniden
// kullanır (o ekran zaten "İPTAL EDİLDİ" durumunu gösteriyor - ayrı bir detay view İCAT EDİLMEDİ).
[Authorize]
public sealed class RetailSaleCancellationsController(ApplicationDbContext dbContext) : Controller
{
    private const string SelfSaleSectionName = "Self Satış";
    private const string PackageSectionName = "Paket";

    private static string ResolveChannel(RestaurantSection section) => section.Name switch
    {
        SelfSaleSectionName => "Self Satış",
        PackageSectionName => "Paket",
        _ => "Masa"
    };

    public async Task<IActionResult> Index(int? branchId, DateTime? dateFrom, DateTime? dateTo, string? channel, int page = 1)
    {
        const int pageSize = 50;
        var query = dbContext.RetailSales
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable).ThenInclude(x => x.RestaurantSection)
            .Where(x => x.Status == RetailSaleStatus.Cancelled)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(x => x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.BranchId == branchId.Value);
        }
        if (dateFrom.HasValue)
        {
            query = query.Where(x => x.IssuedAtUtc >= dateFrom.Value.Date.ToUniversalTime());
        }
        if (dateTo.HasValue)
        {
            var toExclusive = dateTo.Value.Date.AddDays(1).ToUniversalTime();
            query = query.Where(x => x.IssuedAtUtc < toExclusive);
        }
        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = channel switch
            {
                "SelfSatis" => query.Where(x => x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name == SelfSaleSectionName),
                "Paket" => query.Where(x => x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name == PackageSectionName),
                "Masa" => query.Where(x => x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name != SelfSaleSectionName
                    && x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name != PackageSectionName),
                _ => query
            };
        }

        query = query.OrderByDescending(x => x.CancelledAtUtc);
        var totalCount = await query.CountAsync();
        var sales = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var branches = await dbContext.Branches.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        var branchNamesById = branches.ToDictionary(x => x.Id, x => x.Name);

        var cancelledByUserIds = sales.Where(x => x.CancelledByUserId != null).Select(x => x.CancelledByUserId!).Distinct().ToList();
        var userNamesById = await dbContext.Users.AsNoTracking()
            .Where(x => cancelledByUserIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName);

        var model = new CancelledSaleListViewModel
        {
            Branches = branches,
            BranchId = branchId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Channel = channel,
            Page = page,
            TotalCount = totalCount,
            PageSize = pageSize,
            Items = sales.Select(x =>
            {
                var section = x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection;
                return new CancelledSaleListItemViewModel
                {
                    RetailSaleId = x.Id,
                    DocumentNumber = x.DocumentNumber,
                    IssuedAtUtc = x.IssuedAtUtc,
                    BranchName = branchNamesById.GetValueOrDefault(section.BranchId, ""),
                    Channel = ResolveChannel(section),
                    SourceLabel = x.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name,
                    GrandTotal = x.GrandTotal,
                    CustomerName = x.Customer?.Name,
                    CancelledAtUtc = x.CancelledAtUtc,
                    CancelledByUserName = x.CancelledByUserId != null ? userNamesById.GetValueOrDefault(x.CancelledByUserId, x.CancelledByUserId) : null,
                    CancellationReason = x.CancellationReason
                };
            }).ToList()
        };

        return View(model);
    }
}
