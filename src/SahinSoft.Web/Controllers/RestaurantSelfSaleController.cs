using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

// "Self Satış" (HIZLI PERAKENDE) - masasız hızlı satış, bir markette kasadan ürün alıp ödeyip
// çıkmak gibi (Edip'in onayı, 2026-08-09). Kendi ürün grid/adisyon/ödeme ekranını YENİDEN
// YAZMAK yerine (aynı iş mantığı zaten Restaurant/Check'te var ve test edildi) her satış kendi
// tek-kullanımlık gizli sanal masa/oturum/adisyonuna bağlanır (bkz. RestaurantPostingService.
// CreateSelfSaleCheckAsync) - KALICI/paylaşılan tek bir masa YOKTUR, bu yüzden aynı anda birden
// çok kasiyer/kiosk çakışmadan çalışabilir. "Benim açık satışım" kavramı KULLANICI bazlı
// sorgulanır: kullanıcı ekrandan ayrılıp geri dönerse (yanlışlıkla Dashboard'a tıklama gibi)
// yarım kalmış sepeti kaybetmesin diye, o kullanıcının açık bir Self Satış adisyonu varsa ona
// devam edilir; yoksa yeni bir tane açılır.
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager},{AppRoles.Waiter},{AppRoles.Cashier}")]
public sealed class RestaurantSelfSaleController(ApplicationDbContext dbContext, RestaurantPostingService postingService) : RestaurantControllerBase(dbContext)
{
    public async Task<IActionResult> Index()
    {
        ActivePage = "self";

        var userId = CurrentUserId;

        var openCheckId = await dbContext.RestaurantChecks
            .Where(x => x.Status == RestaurantCheckStatus.Open
                && x.HeldAtUtc == null
                && x.RestaurantTableSession.OpenedByUserId == userId
                && x.RestaurantTableSession.RestaurantTable.RestaurantSection.Name == RestaurantPostingService.SelfSaleSectionName)
            .OrderByDescending(x => x.OpenedAtUtc)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();

        int checkId;
        if (openCheckId is not null)
        {
            checkId = openCheckId.Value;
        }
        else
        {
            var userBranchId = await dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.BranchId)
                .SingleOrDefaultAsync();
            var branchId = userBranchId ?? await dbContext.Branches.Where(x => x.IsHeadOffice).Select(x => x.Id).FirstAsync();

            var check = await postingService.CreateSelfSaleCheckAsync(branchId, userId);
            checkId = check.Id;
        }

        return RedirectToAction("Check", "Restaurant", new { id = checkId });
    }

    // "Fişi Beklet" - Index()'in aksine BİLEREK yeni bir Self Satış adisyonu açar, mevcut check
    // gerçekten DB'de Held olarak işaretlenir (bkz. RestaurantCheck.HeldAtUtc). Kasiyer bir
    // sonraki müşteriye boş bir sepetle başlar; bekleyen fiş "Bekleyen Fişler" listesinden geri
    // çağrılır.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hold(int checkId)
    {
        var userId = CurrentUserId;

        // Fişi Beklet (madde 8) - artık GERÇEKTEN veri tabanında bekleyen olarak işaretlenir
        // (bkz. RestaurantCheck.HeldAtUtc yorumu) - istemci bu forma submit etmeden ÖNCE
        // bekleyen sepetteki (henüz gönderilmemiş) satırları mutfağa gönderir (bkz.
        // restaurant-pos.js side-hold-btn), böylece hiçbir şey sadece bu tarayıcının
        // localStorage'ında kalmaz - başka bir terminalden de görünür/geri çağrılabilir.
        await postingService.HoldCheckAsync(checkId, userId);

        var userBranchId = await dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => x.BranchId)
            .SingleOrDefaultAsync();
        var branchId = userBranchId ?? await dbContext.Branches.Where(x => x.IsHeadOffice).Select(x => x.Id).FirstAsync();

        var check = await postingService.CreateSelfSaleCheckAsync(branchId, userId);
        return RedirectToAction("Check", "Restaurant", new { id = check.Id });
    }

    // Bekleyen Fişler (madde 8) - GERÇEK bir DB sorgusu, şube bazlı, hangi terminalden/kasiyerden
    // bakılırsa bakılsın aynı listeyi gösterir (localStorage YOK). En eski en üstte.
    [HttpGet]
    public async Task<IActionResult> HeldReceipts()
    {
        var userId = CurrentUserId;
        var userBranchId = await dbContext.Users.Where(x => x.Id == userId).Select(x => x.BranchId).SingleOrDefaultAsync();

        var held = await dbContext.RestaurantChecks
            .AsNoTracking()
            .Where(x => x.Status == RestaurantCheckStatus.Open && x.HeldAtUtc != null
                     && (userBranchId == null || x.RestaurantTableSession.RestaurantTable.RestaurantSection.BranchId == userBranchId))
            .Include(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable).ThenInclude(x => x.RestaurantSection)
            .OrderBy(x => x.HeldAtUtc)
            .Select(x => new
            {
                checkId = x.Id,
                checkNumber = x.CheckNumber,
                sourceLabel = x.RestaurantTableSession.RestaurantTable.RestaurantSection.Name == RestaurantPostingService.SelfSaleSectionName
                    ? "Self Satış"
                    : x.RestaurantTableSession.RestaurantTable.RestaurantSection.Name + " — " + x.RestaurantTableSession.RestaurantTable.Name,
                heldAtUtc = x.HeldAtUtc,
                itemCount = x.Orders.SelectMany(o => o.Lines).Count(l => l.Status != RestaurantOrderLineStatus.Cancelled),
                total = x.Orders.SelectMany(o => o.Lines).Where(l => l.Status != RestaurantOrderLineStatus.Cancelled)
                    .Sum(l => (decimal?)(l.Quantity * l.UnitPriceSnapshot - l.DiscountAmountSnapshot)) ?? 0
            })
            .ToListAsync();

        return Json(held);
    }

    // Bekleyen fişi geri çağır - HeldAtUtc'yi temizler (artık gerçekten "açık" sayılır) ve
    // normal Check ekranına döner. localStorage/?restoreHeld=1 kalıntısı YOK.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Recall(int checkId)
    {
        await postingService.RecallHeldCheckAsync(checkId);
        return RedirectToAction("Check", "Restaurant", new { id = checkId });
    }

    // Ödeme alınmadan önce açık Self Satış sepetini gerçek bir masaya taşır - bkz.
    // RestaurantPostingService.TransferSelfSaleToTableAsync yorumu (ürün/mutfak fişi ikinci
    // kez oluşmaz, sadece mevcut RestaurantOrder'lar hedef adisyona yeniden bağlanır).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TransferToTable(int checkId, int targetTableId)
    {
        try
        {
            var targetCheck = await postingService.TransferSelfSaleToTableAsync(checkId, targetTableId, CurrentUserId);
            TempData["Success"] = "Sepet masaya aktarıldı.";
            return RedirectToAction("Check", "Restaurant", new { id = targetCheck.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Check", "Restaurant", new { id = checkId });
        }
    }
}
