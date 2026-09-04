using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

// Masa–adisyon–mutfak akışı (Faz 2). Ödeme/kasa/perakende fiş/muhasebe posting burada YOK — bkz.
// CLEAN_ROOM_DEVELOPMENT.md. RestaurantManager/Waiter/Kitchen rolleri yalnızca menüde gizlenmekle
// kalmaz, her aksiyon burada [Authorize(Roles=...)] ile de zorunlu kılınır.
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager},{AppRoles.Waiter}")]
public sealed class RestaurantController(ApplicationDbContext dbContext, RestaurantPostingService postingService, RestaurantPermissionService permissionService) : RestaurantControllerBase(dbContext)
{
    public async Task<IActionResult> Index()
    {
        ActivePage = "tables";
        var sections = await dbContext.RestaurantSections
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Tables.Where(t => t.IsActive))
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .ToListAsync();

        var openSessions = await dbContext.RestaurantTableSessions
            .AsNoTracking()
            .Where(x => x.Status == RestaurantTableSessionStatus.Open)
            .Include(x => x.Checks)
            .ToListAsync();

        var activeReservations = await dbContext.RestaurantTableReservations
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync();

        var model = new RestaurantFloorViewModel
        {
            Sections = sections.Select(section => new RestaurantFloorSectionViewModel
            {
                Name = section.Name,
                Tables = section.Tables.OrderBy(t => t.Name).Select(table =>
                {
                    var session = openSessions.SingleOrDefault(s => s.RestaurantTableId == table.Id);
                    var check = session?.Checks.SingleOrDefault(c => c.Status == RestaurantCheckStatus.Open);
                    var reservation = session is null ? activeReservations.SingleOrDefault(r => r.RestaurantTableId == table.Id) : null;
                    return new RestaurantFloorTableViewModel
                    {
                        TableId = table.Id,
                        Name = table.Name,
                        Capacity = table.Capacity,
                        IsOccupied = session is not null,
                        SessionId = session?.Id,
                        CheckId = check?.Id,
                        GuestCount = session?.GuestCount,
                        OpenedAtUtc = session?.OpenedAtUtc,
                        RunningTotal = check is null ? 0 : ComputeCheckRunningTotal(check.Id),
                        BillRequested = check?.BillRequestedAtUtc is not null,
                        IsReserved = reservation is not null,
                        ReservationId = reservation?.Id,
                        ReservedForUtc = reservation?.ReservedForUtc,
                        ReservationGuestCount = reservation?.GuestCount,
                        ReservationNote = reservation?.Note
                    };
                }).ToList()
            }).ToList()
        };

        return View(model);
    }

    // Product.SalePrice (ve ProductPortion.PriceOverride) sistemde KDV DAHİL tutar olarak
    // tutulur — bkz. LookupController.Products'taki "stok kartındaki KDV dahil tutar" kuralı,
    // fatura/teklif satırına yazılırken buradan KDV çıkarılıyor. Restoran tarafında da AYNI tek
    // kaynak politikası izlenir: UnitPriceSnapshot zaten KDV dahildir, burada KDV bir daha
    // eklenmez. TaxRateSnapshot yalnızca adisyon KAPANIŞINDA (Faz 3, RetailSale/Fatura üretirken)
    // KDV'yi tutardan geriye doğru ayrıştırmak için saklanır.
    private static List<string> SplitPresetLines(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private decimal ComputeCheckRunningTotal(int checkId) =>
        dbContext.RestaurantOrderLines
            .Where(x => x.RestaurantOrder.RestaurantCheckId == checkId && x.Status != RestaurantOrderLineStatus.Cancelled)
            .Select(x => x.Quantity * x.UnitPriceSnapshot - x.DiscountAmountSnapshot)
            .Sum();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenTable(int tableId, int guestCount, Guid submissionKey)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            var (_, check) = await postingService.OpenTableSessionAsync(tableId, guestCount, userId, userId, submissionKey);
            return RedirectToAction(nameof(Check), new { id = check.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // MASTER tasarımdaki REZERVE rozeti - boş bir masayı ileri bir saat için ayırır.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(int tableId, DateTime reservedForLocal, int guestCount, string? note)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        try
        {
            await postingService.CreateReservationAsync(tableId, reservedForLocal.ToUniversalTime(), guestCount, note, userId);
            TempData["Success"] = "Masa rezerve edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelReservation(int reservationId)
    {
        try
        {
            await postingService.CancelReservationAsync(reservationId);
            TempData["Success"] = "Rezervasyon iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // MASTER tasarımdaki HESAP İSTENDİ rozeti - garson adisyon ekranından işaretler.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestBill(int checkId)
    {
        try
        {
            await postingService.RequestBillAsync(checkId);
            TempData["Success"] = "Hesap istendi olarak işaretlendi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Check), new { id = checkId });
    }

    // "Masayı Boşalt" - her şey iptal edildiğinde/hiç sipariş girilmediğinde ödemesiz çıkış -
    // bkz. VoidEmptyCheckAsync yorumu (Edip, 2026-09-03).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VoidEmptyCheck(int checkId)
    {
        try
        {
            await postingService.VoidEmptyCheckAsync(checkId, CurrentUserId);
            TempData["Success"] = "Masa boşaltıldı.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Check), new { id = checkId });
        }
    }

    // "İndirim" (alttaki hızlı işlem / Tahsilat'taki İndirim) - satır bazlı (sabit çubuktaki)
    // İndirim'den FARKLI, adisyonun TOPLAMINA bir tutar indirimi uygular (Edip, 2026-09-03:
    // "altta bastığında indirim tuşuna tutar indirimi toplam tutara, bide tahsilatta indirim
    // o da tutar indirimi sayılsın"). JSON döner - restaurant-pos.js sayfayı kendi yeniler.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyDiscount(int checkId, decimal amount)
    {
        try
        {
            await postingService.ApplyTicketDiscountAsync(checkId, amount);
            return Json(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // Fiş İkram (madde 11) - adisyondaki TÜM aktif satırları ikram eder. JSON döner -
    // restaurant-pos.js sayfayı kendi yeniler (ApplyDiscount ile AYNI desen).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyReceiptComplimentary(int checkId, string? reasonFor, string? reasonWhy, string? note)
    {
        try
        {
            await postingService.ApplyReceiptComplimentaryAsync(checkId, CurrentUserId, reasonFor, reasonWhy, note);
            return Json(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // Cari Ekle (madde 13, onaylı Self Satış tasarımı) - adisyona bir müşteri bağlar/kaldırır.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AttachCustomer(int checkId, int? customerId)
    {
        try
        {
            await postingService.AttachCustomerAsync(checkId, customerId);
            return Json(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCheckNote(int checkId, string? note)
    {
        try
        {
            await postingService.UpdateCheckNoteAsync(checkId, note);
            TempData["Success"] = "Fiş notu kaydedildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Check), new { id = checkId });
    }

    // Fiş Listesi (POS ekranındaki sağ ikon şeridi) - bugün kesilmiş fişlerin kısa listesi,
    // sayfa yenilemeden modalda gösterilsin diye JSON (Edip, 2026-09-03: "fiş listesi günlük fiş
    // listesini gösterir isterse ekrana alıp düzeltebilir"). Raporlar'daki Günlük sekmesiyle AYNI
    // kapsam (tüm restoran, tek kullanıcıya özel değil) - sadece burada tam sayfa yerine modal.
    [HttpGet]
    public async Task<IActionResult> TodayReceipts()
    {
        var todayStartUtc = DateTime.Now.Date.ToUniversalTime();
        var todayEndUtc = todayStartUtc.AddDays(1);
        var receipts = await dbContext.RetailSales
            .AsNoTracking()
            .Where(x => x.IssuedAtUtc >= todayStartUtc && x.IssuedAtUtc < todayEndUtc && x.Status != RetailSaleStatus.Cancelled)
            .OrderByDescending(x => x.IssuedAtUtc)
            .Select(x => new
            {
                id = x.Id,
                documentNumber = x.DocumentNumber,
                timeLabel = x.IssuedAtUtc.ToLocalTime().ToString("HH:mm"),
                grandTotal = x.GrandTotal,
                tableName = x.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name
            })
            .ToListAsync();
        return Json(receipts);
    }

    // Ürün/barkod arama - POS ekranındaki kategori kısayolu grid'i sadece ShowAsShortcut/
    // ShowInMobile işaretli ürünleri gösterir (bkz. Check action), ama kasiyer arama kutusuna
    // yazdığında TÜM aktif stoktan aramalı - kısayol olarak tanımlanmamış ama satılabilir bir
    // ürün de bulunabilsin diye (Edip, 2026-09-03: "stok ve ürün arama sol tarafta kategoride
    // değil stok listesinde arasın veritabanında stok" / "stoklarda arama yapsın"). Aynı JSON
    // şekli (RestaurantCatalogProductViewModel ile aynı alanlar) - istemci addToCart'ı hiç
    // değiştirmeden kullanabilsin diye.
    // Ürün Listesi modalı (madde 15-16) - "mode" parametresi ile ad/barkod arama ayrılabilir.
    // "barcode" iken sadece barkod alanlarında (Contains - okutma sırasında kısmi eşleşme için)
    // arar, kategori/sepet alanına HİÇ dokunmaz - o alan tamamen istemci tarafında ayrı yönetilir.
    [HttpGet]
    public async Task<IActionResult> SearchProducts(string term, string? mode = null)
    {
        term = (term ?? string.Empty).Trim();
        if (term.Length == 0) return Json(Array.Empty<object>());

        var products = await dbContext.Products
            .AsNoTracking()
            .Where(x => x.IsActive && (mode == "barcode"
                ? (x.Barcode != null && x.Barcode.Contains(term)) || x.Barcodes.Any(b => b.IsActive && b.Barcode.Contains(term))
                : (x.Name.Contains(term) || x.Barcode == term || x.Barcodes.Any(b => b.IsActive && b.Barcode == term))))
            .Include(x => x.TaxRate)
            .Include(x => x.Portions.Where(p => p.IsActive))
            .Include(x => x.Barcodes.Where(b => b.IsActive))
            .OrderBy(x => x.Name)
            .Take(30)
            .Select(p => new
            {
                productId = p.Id,
                name = p.Name,
                stockCode = p.StockCode,
                salePrice = p.SalePrice,
                taxRate = p.TaxRate.Rate,
                hasKitchenStation = p.DefaultKitchenStationId != null,
                imagePath = p.ImagePath,
                unit = p.Unit,
                barcodes = (p.Barcode != null ? new[] { p.Barcode } : Array.Empty<string>())
                    .Concat(p.Barcodes.Select(b => b.Barcode))
                    .Distinct()
                    .ToList(),
                portions = p.Portions.OrderBy(x => x.DisplayOrder).Select(portion => new
                {
                    portionId = portion.Id,
                    name = portion.Name,
                    priceOverride = portion.PriceOverride,
                    isDefault = portion.IsDefault
                }).ToList()
            })
            .ToListAsync();

        return Json(products, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
    }

    // "Ekrana Al" - fişin satırlarını (productId/quantity) döner, istemci zaten yüklü kataloğundan
    // eşleştirip sepete ekler (bkz. restaurant-pos.js) - ayrı bir ürün DTO'su gerekmez.
    [HttpGet]
    public async Task<IActionResult> ReceiptLines(int retailSaleId)
    {
        var lines = await dbContext.RetailSaleLines
            .AsNoTracking()
            .Where(x => x.RetailSaleId == retailSaleId)
            .Select(x => new { productId = x.ProductId, quantity = x.Quantity })
            .ToListAsync();
        return Json(lines);
    }

    // "Düzelt" (Fiş Listesi > Ekrana Al, kapanmış bir fiş üzerinde) - Edip, 2026-09-03: "muhasebe
    // mantığında öyle çalışsın ama restorant mantığında adisyonu veya fişi ekrana alıp ödeme
    // tipini değiştirebilir düzenleyebilir ürün ekleyip silebilir". Muhasebe bütünlüğü (kapanmış
    // fiş asla yerinde değişmez, sadece ters kayıtla iptal edilir - bkz. CancelRetailSaleAsync)
    // KORUNUR: restoran ekranındaki "düzelt" aslında ÖNCE mevcut RestaurantReportsController.
    // CancelReceipt ile AYNI iptali tetikler (bu yüzden Administrator gerekir - sıradan bir
    // kasiyer düzenleme diye kapanmış fişleri iptal edemez), SONRA satırları sepete klonlar ki
    // kasiyer ürün ekleyip/silip/ödeme tipini değiştirip yeniden ringleyebilsin. JSON döner (bu
    // sayfa modal içinde kullanır, Raporlar'daki tam sayfa yönlendirmeli sürümden ayrı).
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> CancelReceiptForEdit(int retailSaleId)
    {
        try
        {
            await postingService.CancelRetailSaleAsync(retailSaleId, CurrentUserId, "Restoran ekranından düzeltme (ekrana alındı)");
            return Json(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> Check(int id)
    {
        ActivePage = "tables";
        var check = await dbContext.RestaurantChecks
            .AsNoTracking()
            .Include(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable).ThenInclude(x => x.RestaurantSection)
            .Include(x => x.Orders).ThenInclude(x => x.Lines).ThenInclude(x => x.KitchenTicketLines)
            .Include(x => x.Orders).ThenInclude(x => x.Lines).ThenInclude(x => x.Product)
            .Include(x => x.AttachedCustomer)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (check is null)
        {
            return NotFound();
        }

        // Sipariş satırı listesinde "kim, ne zaman gönderdi" bilgisi için (Edip, 2026-09-03:
        // kendi eski POS'undaki gibi her siparişin altında garson adı görünsün).
        var orderedByIds = check.Orders.Select(x => x.OrderedByUserId).Distinct().ToList();
        var orderedByNames = await dbContext.Users
            .AsNoTracking()
            .Where(x => orderedByIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName);

        if (check.Status != RestaurantCheckStatus.Open)
        {
            TempData["Error"] = "Bu adisyon artık açık değil.";
            return RedirectToAction(nameof(Index));
        }

        // Kısayolda/Mobilde görünsün mü ayarları hem ürün hem kategori düzeyinde var - ikisi de
        // AÇIK olmalı ki ürün POS'ta görünsün (Edip, 2026-09-03: "kategorideki kısayolda gözüksün
        // tiki pasif ise restoranta veya mobil tiki işaretli değilse gözükmesin, stok kartında
        // aynı tanımlamalar çalışsın").
        var categories = await dbContext.Products
            .AsNoTracking()
            .Where(x => x.IsActive && x.ShowAsShortcut && x.ShowInMobile
                     && x.Category.ShowAsShortcut && x.Category.ShowInMobile)
            .Include(x => x.Category)
            .Include(x => x.TaxRate)
            .Include(x => x.Portions.Where(p => p.IsActive))
            .Include(x => x.Barcodes.Where(b => b.IsActive))
            .OrderBy(x => x.Category.Name).ThenBy(x => x.Name)
            .ToListAsync();

        var financialAccounts = await dbContext.FinancialAccounts
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new RestaurantFinancialAccountViewModel
            {
                FinancialAccountId = x.Id,
                Name = x.Code + " - " + x.Name
            })
            .ToListAsync();

        var fiscalSettings = await dbContext.InventorySettings
            .AsNoTracking()
            .Where(x => x.Id == 1)
            .Select(x => new { x.FiscalDeviceType, x.FiscalAgentUrl, x.IsKitchenTrackingEnabled, x.RequireCancellationReason, x.CancellationReasonPresets, x.QuickNotePresets, x.RequireSecondApprovalForCancelOrderLine, x.RequireSecondApprovalForEditKitchenSentLines, x.RequireSecondApprovalForComplimentary, x.RequireReceiptPromptAfterQuickPay, x.ShowUnpaidPaymentType,
                x.EnableTicketNoteButton, x.EnableTableTransferButton, x.EnableSendToKitchenButton, x.EnablePriceCheckButton, x.EnableKeyboardButton, x.EnableHoldReceiptButton, x.EnableHeldReceiptsButton, x.EnableProductListButton, x.EnableComplimentaryReceiptButton, x.EnableReceiptListButton, x.EnableClearOrderButton })
            .SingleOrDefaultAsync();

        var isSelfSaleCheck = check.RestaurantTableSession.RestaurantTable.RestaurantSection.Name == RestaurantPostingService.SelfSaleSectionName;
        var availableTables = isSelfSaleCheck
            ? await dbContext.RestaurantTables
                .AsNoTracking()
                .Where(x => x.IsActive && x.RestaurantSection.IsActive)
                .Include(x => x.RestaurantSection)
                .OrderBy(x => x.RestaurantSection.DisplayOrder).ThenBy(x => x.Name)
                .Select(x => new RestaurantTransferTableOptionViewModel(
                    x.Id,
                    x.RestaurantSection.Name,
                    x.Name,
                    x.Sessions.Any(s => s.Status == RestaurantTableSessionStatus.Open)))
                .ToListAsync()
            : [];

        var model = new RestaurantCheckViewModel
        {
            CheckId = check.Id,
            CheckNumber = check.CheckNumber,
            TableId = check.RestaurantTableSession.RestaurantTableId,
            TableName = check.RestaurantTableSession.RestaurantTable.Name,
            SectionName = check.RestaurantTableSession.RestaurantTable.RestaurantSection.Name,
            GuestCount = check.RestaurantTableSession.GuestCount,
            OpenedAtUtc = check.RestaurantTableSession.OpenedAtUtc,
            IsSelfSaleCheck = isSelfSaleCheck,
            AvailableTables = availableTables,
            BillRequested = check.BillRequestedAtUtc is not null,
            TicketNote = check.Note,
            IsFiscalEnabled = fiscalSettings is { FiscalDeviceType: not FiscalDeviceType.None } && !string.IsNullOrWhiteSpace(fiscalSettings.FiscalAgentUrl),
            FiscalAgentUrl = fiscalSettings?.FiscalAgentUrl,
            IsKitchenTrackingEnabled = fiscalSettings?.IsKitchenTrackingEnabled ?? false,
            RequireCancellationReason = fiscalSettings?.RequireCancellationReason ?? false,
            RequireSecondApprovalForCancelOrderLine = fiscalSettings?.RequireSecondApprovalForCancelOrderLine ?? false,
            RequireSecondApprovalForEditKitchenSentLines = fiscalSettings?.RequireSecondApprovalForEditKitchenSentLines ?? false,
            RequireSecondApprovalForComplimentary = fiscalSettings?.RequireSecondApprovalForComplimentary ?? false,
            RequireReceiptPromptAfterQuickPay = fiscalSettings?.RequireReceiptPromptAfterQuickPay ?? false,
            ShowUnpaidPaymentType = fiscalSettings?.ShowUnpaidPaymentType ?? false,
            AttachedCustomerId = check.AttachedCustomerId,
            AttachedCustomerDisplay = check.AttachedCustomer is null ? null : $"{check.AttachedCustomer.Code} - {check.AttachedCustomer.Name}",
            CollectionCaris = await dbContext.Customers
                .AsNoTracking()
                .Where(x => x.IsActive && x.IsCollectionCari)
                .OrderBy(x => x.Name)
                .Select(x => new RestaurantCollectionCariViewModel { CustomerId = x.Id, Name = x.Name })
                .ToListAsync(),
            // Sağ İşlem Menüsü (madde 22) - sistem geneli (Ayarlar) VE kullanıcı yetkisi (profil)
            // İKİSİ DE açık olmalı. Fiş Notu/Fiş İkram zaten CanAddNote/CanApplyComplimentary'yi
            // kullanıyor (ayrı bir görünürlük bayrağı YOK, aynı işlemle birebir örtüşüyor).
            ShowTicketNoteButton = (fiscalSettings?.EnableTicketNoteButton ?? true) && await permissionService.CanAddNoteAsync(CurrentUserId),
            ShowTableTransferButton = (fiscalSettings?.EnableTableTransferButton ?? true) && await permissionService.CanSeeTableTransferAsync(CurrentUserId),
            ShowSendToKitchenButton = (fiscalSettings?.EnableSendToKitchenButton ?? true) && await permissionService.CanSeeSendToKitchenAsync(CurrentUserId),
            ShowPriceCheckButton = (fiscalSettings?.EnablePriceCheckButton ?? true) && await permissionService.CanSeePriceCheckAsync(CurrentUserId),
            ShowKeyboardButton = (fiscalSettings?.EnableKeyboardButton ?? true) && await permissionService.CanSeeKeyboardAsync(CurrentUserId),
            ShowHoldReceiptButton = (fiscalSettings?.EnableHoldReceiptButton ?? true) && await permissionService.CanSeeHoldReceiptAsync(CurrentUserId),
            ShowHeldReceiptsButton = (fiscalSettings?.EnableHeldReceiptsButton ?? true) && await permissionService.CanSeeHeldReceiptsAsync(CurrentUserId),
            ShowProductListButton = (fiscalSettings?.EnableProductListButton ?? true) && await permissionService.CanSeeProductListAsync(CurrentUserId),
            ShowComplimentaryReceiptButton = (fiscalSettings?.EnableComplimentaryReceiptButton ?? true) && await permissionService.CanApplyComplimentaryAsync(CurrentUserId),
            ShowReceiptListButton = (fiscalSettings?.EnableReceiptListButton ?? true) && await permissionService.CanSeeReceiptListAsync(CurrentUserId),
            ShowClearOrderButton = (fiscalSettings?.EnableClearOrderButton ?? true) && await permissionService.CanClearOrderAsync(CurrentUserId),
            CancellationReasonPresets = SplitPresetLines(fiscalSettings?.CancellationReasonPresets),
            QuickNotePresets = SplitPresetLines(fiscalSettings?.QuickNotePresets),
            SentOrders = check.Orders.OrderBy(x => x.OrderedAtUtc).Select(order => new RestaurantSentOrderViewModel
            {
                OrderId = order.Id,
                OrderedAtUtc = order.OrderedAtUtc,
                OrderedByName = orderedByNames.GetValueOrDefault(order.OrderedByUserId, ""),
                Lines = order.Lines.OrderBy(x => x.Id).Select(line => new RestaurantSentOrderLineViewModel
                {
                    LineId = line.Id,
                    ProductName = line.ProductNameSnapshot,
                    PortionName = line.PortionNameSnapshot,
                    Quantity = line.Quantity,
                    Unit = line.Product.Unit,
                    UnitPrice = line.UnitPriceSnapshot,
                    DiscountAmount = line.DiscountAmountSnapshot,
                    TaxRate = line.TaxRateSnapshot,
                    IsComplimentary = line.IsComplimentary,
                    KitchenNote = line.KitchenNote,
                    Status = line.Status.ToString(),
                    SentToKitchen = line.KitchenTicketLines.Count > 0,
                    CanCancel = line.Status != RestaurantOrderLineStatus.Cancelled
                }).ToList()
            }).ToList(),
            Catalog = categories
                .GroupBy(x => x.CategoryId)
                .Select(g => new RestaurantCatalogCategoryViewModel
                {
                    CategoryName = g.First().Category.Name,
                    Color = g.First().Category.Color,
                    Products = g.Select(p => new RestaurantCatalogProductViewModel
                    {
                        ProductId = p.Id,
                        Name = p.Name,
                        SalePrice = p.SalePrice,
                        TaxRate = p.TaxRate.Rate,
                        HasKitchenStation = p.DefaultKitchenStationId is not null,
                        Unit = p.Unit,
                        ImagePath = p.ImagePath,
                        Barcodes = (p.Barcode != null ? new[] { p.Barcode } : Array.Empty<string>())
                            .Concat(p.Barcodes.Select(b => b.Barcode))
                            .Distinct()
                            .ToList(),
                        Portions = p.Portions.OrderBy(x => x.DisplayOrder).Select(portion => new RestaurantCatalogPortionViewModel
                        {
                            PortionId = portion.Id,
                            Name = portion.Name,
                            PriceOverride = portion.PriceOverride,
                            IsDefault = portion.IsDefault
                        }).ToList()
                    }).ToList()
                })
                .ToList(),
            FinancialAccounts = financialAccounts,
            PayableTotal = ComputeCheckRunningTotal(check.Id),
            PendingPayments = await dbContext.RestaurantCheckPendingPayments
                .AsNoTracking()
                .Where(x => x.RestaurantCheckId == check.Id)
                .OrderBy(x => x.RecordedAtUtc)
                .Select(x => new RestaurantPendingPaymentViewModel
                {
                    PendingPaymentId = x.Id,
                    Method = (int)x.PaymentMethod,
                    FinancialAccountId = x.FinancialAccountId,
                    Amount = x.Amount
                })
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClosePayment([FromBody] RestaurantClosePaymentRequest request)
    {
        if (request.Payments.Count == 0)
        {
            return BadRequest(new { error = "En az bir ödeme satırı girilmelidir." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var payments = request.Payments
            .Select(p => new RestaurantPaymentInput((RestaurantPaymentMethod)p.Method, p.FinancialAccountId, p.Amount))
            .ToList();

        try
        {
            var fiscalInfo = request.FiscalReceiptNumber is null
                ? null
                : new FiscalReceiptInfo(request.FiscalReceiptNumber, request.FiscalZNo, request.FiscalDeviceSerialNumber);

            var retailSale = await postingService.CloseCheckAsync(
                request.CheckId,
                payments,
                request.CustomerId,
                userId,
                request.SubmissionKey,
                fiscalInfo);

            return Ok(new { RetailSaleId = retailSale.Id, retailSale.DocumentNumber, retailSale.GrandTotal });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Kısmi ödeme (madde 4-8) - "Ödemeyi Al" modalında bir yöntem tuşuna basılınca çağrılır,
    // sunucuya KALICI kaydeder (bkz. RestaurantCheckPendingPayment/AddPendingPaymentAsync).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPendingPayment([FromBody] RestaurantPendingPaymentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        try
        {
            var pending = await postingService.AddPendingPaymentAsync(
                request.CheckId,
                (RestaurantPaymentMethod)request.Method,
                request.Amount,
                request.FinancialAccountId,
                userId);

            return Ok(new { pendingPaymentId = pending.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePendingPayment(int pendingPaymentId)
    {
        try
        {
            await postingService.RemovePendingPaymentAsync(pendingPaymentId);
            return Ok(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // "Ödeme İptal" (madde 7) - Adisyona Dön'den farklı olarak kısmi ödemeleri sıfırlar, kullanıcı
    // ödeme ekranında kalır (bkz. Check.cshtml #closePaymentModal, restaurant-close-payment.js).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelPendingPayments(int checkId)
    {
        await postingService.CancelPendingPaymentsAsync(checkId);
        return Ok(new { success = true });
    }

    // Self Satış Hızlı Ödeme (madde 3) - "Fiş Yazdır" için yalın, yazdırmaya hazır fiş görünümü.
    // RestaurantReportsController'daki "Fişi Gör" modalıyla AYNI veri şeklini kullanır ama o
    // controller Waiter rolüne kapalı (Administrator/RestaurantManager/Cashier) - burası bu
    // controller'ın kendi rol kapsamında (Administrator/RestaurantManager/Waiter) kalması için
    // ayrı, küçük bir aksiyon.
    [HttpGet]
    public async Task<IActionResult> Receipt(int id)
    {
        var sale = await dbContext.RetailSales
            .AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.Payments)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (sale is null)
        {
            return NotFound();
        }

        var model = new RestaurantReceiptDetailViewModel
        {
            DocumentNumber = sale.DocumentNumber,
            IssuedAtUtc = sale.IssuedAtUtc,
            SourceLabel = "Self Satış",
            SourceType = "self",
            IsCancelled = sale.Status == RetailSaleStatus.Cancelled,
            Lines = sale.Lines.Select(l => new RestaurantReceiptDetailLine(l.ProductNameSnapshot, l.Quantity, l.LineTotal)).ToList(),
            SubtotalAmount = sale.SubtotalAmount,
            DiscountAmount = sale.DiscountAmount,
            TaxAmount = sale.TaxAmount,
            GrandTotal = sale.GrandTotal,
            Payments = sale.RestaurantCheck.Payments.Where(p => !p.IsReversal).Select(p => new RestaurantReceiptDetailPayment(
                p.PaymentMethod switch { RestaurantPaymentMethod.Cash => "Nakit", RestaurantPaymentMethod.CreditCard => "Kredi Kartı", _ => "Yemek Kartı" },
                p.Amount)).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendToKitchen([FromBody] RestaurantSendToKitchenRequest request)
    {
        if (request.Lines.Count == 0)
        {
            return BadRequest(new { success = false, error = "Gönderilecek en az bir ürün seçmelisiniz." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            var lines = request.Lines.Select(x => new RestaurantOrderLineInput(
                x.ProductId,
                x.ProductPortionId,
                x.Quantity,
                x.DiscountAmount,
                x.IsComplimentary,
                x.KitchenNote,
                x.Modifiers?.Select(m => new RestaurantOrderLineModifierInput(m.NameSnapshot, m.PriceSnapshot, m.Quantity)).ToList())).ToList();

            var result = await postingService.SendOrderToKitchenAsync(request.CheckId, lines, userId, request.SubmissionKey);

            return Json(new
            {
                success = true,
                orderId = result.Order.Id,
                unroutedProductNames = result.UnroutedProductNames
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrderLine(int lineId, int checkId, string reason, string? approverPin = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            await postingService.CancelOrderLineAsync(lineId, userId, reason, approverPin);
            TempData["Success"] = "Sipariş satırı iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Check), new { id = checkId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustOrderLineQuantity(int lineId, int checkId, decimal quantity, string? approverPin = null)
    {
        try
        {
            await postingService.AdjustOrderLineQuantityAsync(lineId, quantity, CurrentUserId, approverPin);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Check), new { id = checkId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLineComplimentary(int lineId, int checkId, string? approverPin = null)
    {
        try
        {
            await postingService.ToggleLineComplimentaryAsync(lineId, CurrentUserId, approverPin);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Check), new { id = checkId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveTable(int sessionId, int toTableId)
    {
        try
        {
            await postingService.MoveTableSessionAsync(sessionId, toTableId, CurrentUserId, reason: null);
            TempData["Success"] = "Masa taşındı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MergeTables(int fromSessionId, int intoSessionId)
    {
        try
        {
            await postingService.MergeTableSessionsAsync(fromSessionId, intoSessionId, CurrentUserId);
            TempData["Success"] = "Masalar birleştirildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
