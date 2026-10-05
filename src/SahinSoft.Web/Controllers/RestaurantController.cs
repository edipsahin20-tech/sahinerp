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
public sealed class RestaurantController(ApplicationDbContext dbContext, RestaurantPostingService postingService, RestaurantPermissionService permissionService, RestaurantShellService shellService) : RestaurantControllerBase(dbContext, shellService)
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

        // Edip, 2026-09-28: "masa 2'den 20 TL tahsilat aldım, ana ekran 400 göstermesi lazım hala
        // 420 gösteriyor" - kısmi tahsilatlar (RestaurantCheckPendingPayments, adisyon TAM
        // kapanana kadar bekleyen) Masa Satış ızgarasındaki RunningTotal'a hiç yansıtılmıyordu;
        // Check.cshtml (Adisyon ekranı) zaten PayableTotal - PaidTotal ile doğru gösteriyordu, ama
        // ana ızgara ComputeCheckRunningTotal'ı DOĞRUDAN (ödemeler düşülmeden) kullanıyordu.
        var pendingPaymentTotalsByCheckId = await dbContext.RestaurantCheckPendingPayments
            .AsNoTracking()
            .GroupBy(x => x.RestaurantCheckId)
            .Select(g => new { CheckId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.CheckId, x => x.Total);

        var askGuestCount = await dbContext.InventorySettings
            .AsNoTracking()
            .Where(x => x.Id == 1)
            .Select(x => x.AskGuestCountOnTableOpen)
            .SingleOrDefaultAsync();

        var model = new RestaurantFloorViewModel
        {
            AskGuestCountOnTableOpen = askGuestCount,
            Sections = sections.Select(section => new RestaurantFloorSectionViewModel
            {
                Name = section.Name,
                Tables = section.Tables.OrderBy(t => t.Name, NaturalSortComparer.Instance).Select(table =>
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
                        RunningTotal = check is null
                            ? 0
                            : Math.Max(ComputeCheckRunningTotal(check.Id) - pendingPaymentTotalsByCheckId.GetValueOrDefault(check.Id), 0),
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

    // Adisyon/tutar indirimi (TicketDiscountAmount) satır toplamından AYRICA düşülür - satırlara
    // dağıtılmaz (2026-09-05 teknik doküman madde 4). bkz. RestaurantPostingService.ApplyTicketDiscountAsync.
    private decimal ComputeCheckRunningTotal(int checkId)
    {
        var netLinesTotal = dbContext.RestaurantOrderLines
            .Where(x => x.RestaurantOrder.RestaurantCheckId == checkId && x.Status != RestaurantOrderLineStatus.Cancelled)
            .Select(x => x.Quantity * x.UnitPriceSnapshot - x.DiscountAmountSnapshot)
            .Sum();
        var ticketDiscount = dbContext.RestaurantChecks
            .Where(x => x.Id == checkId)
            .Select(x => x.TicketDiscountAmount)
            .SingleOrDefault();
        return Math.Max(netLinesTotal - ticketDiscount, 0);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenTable(int tableId, int guestCount, Guid submissionKey)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // Adisyonun şubesi masanın bölümünden gelir; terminal başka şubedeyse o masadan adisyon açılmaz.
        var openError = await TerminalBranchErrorForTableAsync(tableId);
        if (openError is not null)
        {
            TempData["Error"] = openError;
            return RedirectToAction(nameof(Index));
        }

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
        var reserveError = await TerminalBranchErrorForTableAsync(tableId);
        if (reserveError is not null)
        {
            TempData["Error"] = reserveError;
            return RedirectToAction(nameof(Index));
        }

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
    public async Task<IActionResult> ApplyDiscount(int checkId, decimal amount, string? approverPin = null)
    {
        try
        {
            await postingService.ApplyTicketDiscountAsync(checkId, amount, CurrentUserId, approverPin);
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

    private static string ReceiptPaymentLabel(List<RestaurantPaymentMethod> methods) => methods.Distinct().Count() switch
    {
        0 => "Ödemesiz",
        1 => SinglePaymentMethodLabel(methods[0]),
        _ => "Karma Ödeme"
    };

    private static string SinglePaymentMethodLabel(RestaurantPaymentMethod method) => method switch
    {
        RestaurantPaymentMethod.Cash => "Nakit",
        RestaurantPaymentMethod.CreditCard => "Kredi Kartı",
        RestaurantPaymentMethod.MealCard => "Yemek Kartı",
        RestaurantPaymentMethod.Unpaid => "Ödenmez",
        RestaurantPaymentMethod.OpenAccount => "Açık Hesap",
        _ => method.ToString()
    };

    private static string ReceiptPaymentKey(List<RestaurantPaymentMethod> methods) => methods.Distinct().Count() switch
    {
        0 => "none",
        1 => methods[0].ToString(),
        _ => "mixed"
    };

    // Fiş Listesi (2026-09-05 teknik doküman madde 9, onaylı görsel 04_Fis_Listesi_ONAYLI.png) -
    // Excel-vari filtrelenebilir, tarih ARALIĞI seçilebilir profesyonel liste. Raporlar'ın kendi
    // "Günlük Fişler" sekmesiyle (RestaurantReportsController) AYNI iş mantığına (kaynak türü,
    // ödeme türü, GERÇEK filtre seçenekleri) dayanır ama BİLİNÇLİ olarak o sayfanın Kasiyer/KDV/
    // Kategori kırılımlarını TEKRARLAMAZ - bu sadece hızlı bir fiş arama/görüntüleme modalı,
    // derinlemesine analiz Raporlar'da kalır.
    [HttpGet]
    public async Task<IActionResult> FilteredReceipts(DateTime? from = null, DateTime? to = null, string? sourceType = null, string? payment = null, string? status = null, string? q = null)
    {
        var fromUtc = (from?.Date ?? DateTime.Now.Date).ToUniversalTime();
        var toUtc = (to?.Date ?? DateTime.Now.Date).AddDays(1).ToUniversalTime();

        var query = dbContext.RetailSales
            .AsNoTracking()
            .Where(x => x.IssuedAtUtc >= fromUtc && x.IssuedAtUtc < toUtc)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x!.RestaurantTable)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.Payments)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.AttachedCustomer);

        var packageNumbers = await dbContext.PackageOrders.AsNoTracking()
            .Where(x => x.RestaurantCheck.LinkedRetailSaleId != null)
            .ToDictionaryAsync(x => x.RestaurantCheckId, x => x.PackageNumber);

        var all = await query.ToListAsync();

        var openerIds = all.Select(x => x.RestaurantCheck.RestaurantTableSession.OpenedByUserId).Distinct().ToList();
        var openerNames = await dbContext.Users.AsNoTracking()
            .Where(x => openerIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName);

        var rows = all.Select(x =>
        {
            var channel = x.RestaurantCheck.RestaurantTableSession.Channel;
            var thisSourceType = channel == RestaurantSaleChannel.SelfSatis ? "self" : channel == RestaurantSaleChannel.Paket ? "package" : "table";
            var methods = x.RestaurantCheck.Payments.Select(p => p.PaymentMethod).ToList();
            var masaLabel = thisSourceType == "package"
                ? (packageNumbers.GetValueOrDefault(x.RestaurantCheckId) ?? "Paket")
                : thisSourceType == "self" ? "Self Satış" : x.RestaurantCheck.RestaurantTableSession.RestaurantTable?.Name ?? "";
            return new
            {
                id = x.Id,
                dateLabel = x.IssuedAtUtc.ToLocalTime().ToString("dd.MM.yyyy"),
                timeLabel = x.IssuedAtUtc.ToLocalTime().ToString("HH:mm"),
                documentNumber = x.DocumentNumber,
                masaLabel,
                sourceType = thisSourceType,
                // x.Customer HER ZAMAN dolu ("Perakende Satışlar Carisi" walk-in cariye düşer,
                // bkz. GetDefaultRetailCustomerIdAsync) - GERÇEK müşteri sadece kasiyer "Cari
                // Ekle" ile açıkça bağladıysa (RestaurantCheck.AttachedCustomerId) anlamlıdır.
                customerName = x.RestaurantCheck.AttachedCustomer != null ? x.RestaurantCheck.AttachedCustomer.Name : "-",
                cashierName = openerNames.GetValueOrDefault(x.RestaurantCheck.RestaurantTableSession.OpenedByUserId, "-"),
                grandTotal = x.GrandTotal,
                // GERÇEK HATA (2026-09-06, kabul testinde bulundu, madde 18) - gerçek bir Tahsilat
                // Carisi (Trendyol vb.) ile yapılan ödeme, Açık Hesap ile AYNI mekanizmayı
                // kullandığından (bkz. restaurant-close-payment.js "collectionCariId") burada da
                // düz "Açık Hesap" gösteriliyordu - fiş üzerinde GERÇEK tahsilat carisinin adı
                // (ör. "Trendyol") kayboluyordu. Tek yöntemli ödemede bağlı cari işaretli bir
                // Tahsilat Carisi ise, onun adı payment etiketi olarak kullanılır.
                paymentLabel = methods.Distinct().Count() == 1
                    && methods[0] == RestaurantPaymentMethod.OpenAccount
                    && x.RestaurantCheck.AttachedCustomer is { IsCollectionCari: true } cari
                        ? cari.Name
                        : ReceiptPaymentLabel(methods),
                paymentKey = ReceiptPaymentKey(methods),
                isCancelled = x.Status == RetailSaleStatus.Cancelled
            };
        }).ToList();

        var sourceFiltered = string.IsNullOrWhiteSpace(sourceType) || sourceType == "all"
            ? rows
            : rows.Where(x => x.sourceType == sourceType).ToList();

        var availablePaymentFilters = sourceFiltered
            .Select(x => x.paymentKey)
            .Distinct()
            .OrderBy(x => x)
            .Select(k => new { key = k, label = k == "mixed" ? "Karma Ödeme" : k == "none" ? "Ödemesiz" : Enum.TryParse<RestaurantPaymentMethod>(k, true, out var m) ? ReceiptPaymentLabel([m]) : k })
            .ToList();

        var finalRows = sourceFiltered.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(payment)) finalRows = finalRows.Where(x => x.paymentKey == payment);
        if (status == "completed") finalRows = finalRows.Where(x => !x.isCancelled);
        else if (status == "cancelled") finalRows = finalRows.Where(x => x.isCancelled);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            finalRows = finalRows.Where(x => x.documentNumber.Contains(term, StringComparison.OrdinalIgnoreCase)
                || x.masaLabel.Contains(term, StringComparison.OrdinalIgnoreCase)
                || x.customerName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var finalList = finalRows.OrderByDescending(x => x.id).ToList();
        var totals = new
        {
            count = finalList.Count,
            grandTotal = finalList.Where(x => !x.isCancelled).Sum(x => x.grandTotal),
            cancelledCount = finalList.Count(x => x.isCancelled)
        };

        return Json(new { rows = finalList, availablePaymentFilters, totals });
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

    // Ürün Listesi modalı (Edip, 2026-09-05, onaylı görsel) - SearchProducts'tan FARKLI: arama
    // terimi ZORUNLU DEĞİL (varsayılan olarak TÜM aktif ürünleri sayfalı listeler), Kategori/KDV
    // Oranı/Stok Durumu filtreleri + gerçek stok miktarı/kategori adı döner. SearchProducts'a
    // dokunulmadı - o zaten başka akışlarda (Fiyat Gör, sepete anında ekleme) kullanılıyor.
    [HttpGet]
    public async Task<IActionResult> ProductCatalogList(string? term, int? categoryId, int? taxRateId, bool inStockOnly = false, int page = 1, int pageSize = 12)
    {
        term = (term ?? string.Empty).Trim();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Products.AsNoTracking().Where(x => x.IsActive);
        if (term.Length > 0)
        {
            query = query.Where(x => x.Name.Contains(term) || x.StockCode.Contains(term) || x.Barcode == term || x.Barcodes.Any(b => b.IsActive && b.Barcode == term));
        }
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);
        if (taxRateId.HasValue) query = query.Where(x => x.TaxRateId == taxRateId.Value);
        if (inStockOnly) query = query.Where(x => x.StockQuantity > 0);

        var totalCount = await query.CountAsync();
        var rows = await query
            .Include(x => x.Category)
            .Include(x => x.TaxRate)
            .Include(x => x.Barcodes.Where(b => b.IsActive))
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                productId = p.Id,
                stockCode = p.StockCode,
                barcode = p.Barcode ?? p.Barcodes.Select(b => b.Barcode).FirstOrDefault() ?? "",
                name = p.Name,
                categoryName = p.Category.Name,
                taxRatePercent = p.TaxRate.Rate,
                stockQuantity = p.StockQuantity,
                salePrice = p.SalePrice,
                unit = p.Unit,
                hasKitchenStation = p.DefaultKitchenStationId != null,
                imagePath = p.ImagePath,
                taxRate = p.TaxRate.Rate
            })
            .ToListAsync();

        var categories = await dbContext.ProductCategories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(c => new { id = c.Id, name = c.Name }).ToListAsync();
        var taxRates = await dbContext.TaxRates.AsNoTracking().OrderBy(x => x.Rate)
            .Select(t => new { id = t.Id, rate = t.Rate }).ToListAsync();

        return Json(new
        {
            rows,
            totalCount,
            page,
            pageSize,
            categories,
            taxRates
        }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
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
    // Terminal ayarı (şube + kasa) - tarayıcı çerezinde tutulur, sunucu dosyasında DEĞİL. Her
    // terminal/tarayıcı kendi seçimini korur; başka terminalin seçimini etkilemez.
    private const string TerminalBranchCookie = "ss_terminal_branch";
    private const string TerminalCashCookie = "ss_terminal_cash";

    private int? TerminalBranchId => int.TryParse(Request.Cookies[TerminalBranchCookie], out var id) ? id : null;

    // Terminal şubesi seçiliyse masa/oturum o şubeye ait olmalıdır. Seçili değilse kontrol yapılmaz (eski davranış).
    private const string TerminalBranchMismatchMessage = "Bu masa/oturum terminalin şubesine ait değil. Masa işlemleri kendi şubesinin terminalinden yapılmalıdır.";

    private async Task<string?> TerminalBranchErrorForTableAsync(int tableId)
    {
        if (TerminalBranchId is not { } terminalBranch) return null;
        var tableBranchId = await dbContext.RestaurantTables.AsNoTracking()
            .Where(x => x.Id == tableId)
            .Select(x => (int?)x.RestaurantSection.BranchId)
            .SingleOrDefaultAsync();
        return tableBranchId == terminalBranch ? null : TerminalBranchMismatchMessage;
    }

    private async Task<string?> TerminalBranchErrorForSessionAsync(int sessionId)
    {
        if (TerminalBranchId is not { } terminalBranch) return null;
        var sessionBranchId = await dbContext.RestaurantTableSessions.AsNoTracking()
            .Where(x => x.Id == sessionId)
            .Select(x => (int?)x.BranchId)
            .SingleOrDefaultAsync();
        return sessionBranchId == terminalBranch ? null : TerminalBranchMismatchMessage;
    }

    // Terminal şube + kasa seçim ekranı (bu tarayıcı için). Kayıt SetTerminal ile yapılır.
    public async Task<IActionResult> TerminalSettings()
    {
        ActivePage = "terminal";
        ViewData["Branches"] = await dbContext.Branches.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name }).ToListAsync();
        ViewData["Accounts"] = await dbContext.FinancialAccounts.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Name).Select(x => new { x.Id, x.Name, x.BranchId, x.IsShared }).ToListAsync();
        ViewData["CurrentBranchId"] = TerminalBranchId;
        ViewData["CurrentCashId"] = int.TryParse(Request.Cookies[TerminalCashCookie], out var cash) ? cash : (int?)null;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetTerminal(int branchId, int? cashAccountId)
    {
        var branchExists = await dbContext.Branches.AnyAsync(x => x.Id == branchId && x.IsActive);
        if (!branchExists)
        {
            TempData["Error"] = "Seçilen şube bulunamadı.";
            return RedirectToAction(nameof(TerminalSettings));
        }

        if (cashAccountId is { } cashId)
        {
            var cashOk = await dbContext.FinancialAccounts.AnyAsync(x => x.Id == cashId && x.IsActive && (x.BranchId == branchId || x.IsShared));
            if (!cashOk)
            {
                TempData["Error"] = "Seçilen kasa bu şubeye ait değil.";
                return RedirectToAction(nameof(TerminalSettings));
            }
        }

        var cookieOptions = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = Request.IsHttps };
        Response.Cookies.Append(TerminalBranchCookie, branchId.ToString(), cookieOptions);
        if (cashAccountId is { } c)
        {
            Response.Cookies.Append(TerminalCashCookie, c.ToString(), cookieOptions);
        }
        else
        {
            Response.Cookies.Delete(TerminalCashCookie);
        }

        TempData["Success"] = "Terminal şube ayarı bu tarayıcıda kaydedildi.";
        return RedirectToAction(nameof(TerminalSettings));
    }

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
            .Include(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable).ThenInclude(x => x!.RestaurantSection)
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
            // Adisyon artık açık değilse (ödeme alındı, iptal oldu, ya da son satır iptaliyle
            // kendiliğinden kapandı - madde 20) masa/self/paket fark etmeksizin HER ZAMAN boş
            // Self Satış ekranına dönülür, SKARY bir hata YOK (Edip, 2026-09-05: "işlem bittiğinde
            // veya ödeme alındığında veya iptal olduğunda direkt self satış ekranında kalsın hata
            // vermesin" - ClosePayment akışının Edip 2026-09-03 kararıyla AYNI ilke, bu GET yolunda
            // eksikti).
            return RedirectToAction(nameof(RestaurantSelfSaleController.Index), "RestaurantSelfSale");
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

        // Terminal şubesi, bu TARAYICININ çerezinde tutulur (ss_terminal_branch). appsettings
        // sunucuda ortak olduğu için terminal ayarı oraya yazılmaz - her terminal kendi seçimini
        // taşır. Seçili değilse eski davranış (tüm hesaplar).
        var terminalBranchId = TerminalBranchId;
        var financialAccounts = await dbContext.FinancialAccounts
            .AsNoTracking()
            .Where(x => x.IsActive && (terminalBranchId == null || x.BranchId == terminalBranchId || x.IsShared))
            .OrderBy(x => x.Name)
            .Select(x => new RestaurantFinancialAccountViewModel
            {
                FinancialAccountId = x.Id,
                Name = x.Code + " - " + x.Name
            })
            .ToListAsync();

        // Kasa Tanımları (madde 27, 2026-09-05) - kasiyerin şubesine bağlı AKTİF kasadan ödeme
        // yöntemi başına gerçek hesap. RestaurantControllerBase.BuildShellAsync'teki AYNI "kullanıcı
        // branch'i yoksa merkez şube" deseni - tek şubeli kurulumlarda (bugünkü durum) zaten tek
        // kasa/tek şube var, davranış değişmez; çok şubeli kurulumda artık GERÇEKTEN şubeye göre
        // ayrışır (öncesinde her şubede aynı ilk hesaba yazıyordu).
        var currentUserBranchId = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == CurrentUserId)
            .Select(x => x.BranchId)
            .SingleOrDefaultAsync();
        // Kasa atamaları TERMİNAL şubesine göre çözülür (terminal seçiliyse). Terminal şubesi yoksa
        // kullanıcının şubesi kullanılır (eski davranış). Böylece Atabulvarı terminali Merkez kasasını
        // görmez.
        var registerBranchId = TerminalBranchId ?? currentUserBranchId;
        var resolvedCashRegister = await dbContext.RestaurantCashRegisters
            .AsNoTracking()
            .Where(x => x.IsActive && (registerBranchId == x.BranchId || (TerminalBranchId == null && x.Branch.IsHeadOffice)))
            .OrderByDescending(x => registerBranchId == x.BranchId)
            .Select(x => new { x.CashFinancialAccountId, x.CreditCardFinancialAccountId, x.MealCardFinancialAccountId })
            .FirstOrDefaultAsync();

        var fiscalSettings = await dbContext.InventorySettings
            .AsNoTracking()
            .Where(x => x.Id == 1)
            .Select(x => new { x.FiscalDeviceType, x.FiscalAgentUrl, x.IsKitchenTrackingEnabled, x.RequireCancellationReason, x.CancellationReasonPresets, x.QuickNotePresets, x.RequireSecondApprovalForCancelOrderLine, x.RequireSecondApprovalForEditKitchenSentLines, x.RequireSecondApprovalForComplimentary, x.RequireSecondApprovalForDiscount, x.RequireReceiptPromptAfterQuickPay, x.ShowUnpaidPaymentType,
                x.EnableTicketNoteButton, x.EnableTableTransferButton, x.EnableSendToKitchenButton, x.EnablePriceCheckButton, x.EnableKeyboardButton, x.EnableHoldReceiptButton, x.EnableHeldReceiptsButton, x.EnableProductListButton, x.EnableComplimentaryReceiptButton, x.EnableReceiptListButton, x.EnableClearOrderButton })
            .SingleOrDefaultAsync();

        var isSelfSaleCheck = check.RestaurantTableSession.Channel == RestaurantSaleChannel.SelfSatis;

        // Masasız (Self Satış/Paket) adisyonlarda gösterilecek gerçek bir masa/bölüm yok - Paket
        // için kendi paket numarasını, Self Satış için sabit etiketi gösteriyoruz (Edip, 2026-09-29:
        // "Self/Paket'te masa alanı yoksa sahte masa adı basmamalı, satış tipi/fiş no gösterilmeli").
        string? packageNumberForDisplay = null;
        if (check.RestaurantTableSession.Channel == RestaurantSaleChannel.Paket)
        {
            packageNumberForDisplay = await dbContext.PackageOrders
                .AsNoTracking()
                .Where(x => x.RestaurantCheckId == check.Id)
                .Select(x => x.PackageNumber)
                .SingleOrDefaultAsync();
        }
        // Edip, 2026-09-28: "ürün bazlı masa transfer... sağ tarafa bir buton ekle, masa
        // bölümleri açılsın" - eskiden bu liste SADECE Self Satış'ın "Masaya Aktar" özelliği için
        // dolduruluyordu; artık Masa/Paket'teki yeni ürün-bazlı transfer de AYNI listeyi kullanıyor,
        // bu yüzden self satış olup olmadığına bakılmadan HER ZAMAN dolduruluyor (kendi masası hariç).
        // Doğal sıralama (Edip, 2026-09-29: "masalar 1'den başlayarak sıralansın, HER ZAMAN böyle
        // olsun") - .Name'e göre düz SQL ORDER BY sözlük sırası üretir (VIP-1, VIP-10, VIP-2, ...),
        // bu yüzden önce materialize edilip NaturalSortComparer ile bellek içinde sıralanıyor -
        // IsOccupied de bu yüzden ayrı bir toplu sorguyla (openTableIds) hesaplanıyor, her masanın
        // TÜM Sessions geçmişini belleğe çekmemek için.
        var openTableIds = (await dbContext.RestaurantTableSessions
            .AsNoTracking()
            .Where(x => x.Status == RestaurantTableSessionStatus.Open && x.RestaurantTableId != null)
            .Select(x => x.RestaurantTableId!.Value)
            .ToListAsync())
            .ToHashSet();

        var availableTables = (await dbContext.RestaurantTables
            .AsNoTracking()
            .Where(x => x.IsActive && x.RestaurantSection.IsActive && x.Id != check.RestaurantTableSession.RestaurantTableId)
            .Include(x => x.RestaurantSection)
            .Select(x => new { x.Id, SectionName = x.RestaurantSection.Name, x.Name, x.RestaurantSection.DisplayOrder })
            .ToListAsync())
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name, NaturalSortComparer.Instance)
            .Select(x => new RestaurantTransferTableOptionViewModel(x.Id, x.SectionName, x.Name, openTableIds.Contains(x.Id)))
            .ToList();

        var model = new RestaurantCheckViewModel
        {
            CheckId = check.Id,
            CheckNumber = check.CheckNumber,
            TableId = check.RestaurantTableSession.RestaurantTableId,
            TableName = check.RestaurantTableSession.Channel switch
            {
                RestaurantSaleChannel.Paket => packageNumberForDisplay ?? RestaurantPostingService.PackageSectionName,
                RestaurantSaleChannel.SelfSatis => RestaurantPostingService.SelfSaleSectionName,
                _ => check.RestaurantTableSession.RestaurantTable?.Name ?? ""
            },
            SectionName = check.RestaurantTableSession.Channel switch
            {
                RestaurantSaleChannel.Paket => RestaurantPostingService.PackageSectionName,
                RestaurantSaleChannel.SelfSatis => RestaurantPostingService.SelfSaleSectionName,
                _ => check.RestaurantTableSession.RestaurantTable?.RestaurantSection.Name ?? ""
            },
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
            RequireSecondApprovalForDiscount = fiscalSettings?.RequireSecondApprovalForDiscount ?? false,
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
            // Mutfağa Gönder SADECE gerçek Masa ve Paket siparişlerinde gösterilir/çalışır -
            // Self Satış hızlı tezgah üstü satıştır (madde 12, "bir markette kasadan ürün alıp
            // ödeyip çıkmak gibi"), mutfağa fiziksel sipariş gönderimi gerekmez; ürünler yine de
            // Ödemeyi Al/Fişi Beklet gibi akışlardan önce sunucuya flushCartToKitchen ile
            // otomatik yazılır, sadece bu MANUEL buton gizlenir (Edip, 2026-09-05). Self Satış
            // "Masaya Aktar" ile gerçek bir masaya taşınırsa check artık Self Satış SAYILMAZ
            // (SectionName değişir) - buton kendiliğinden geri görünür, ekstra kod gerekmez.
            ShowSendToKitchenButton = !isSelfSaleCheck && (fiscalSettings?.EnableSendToKitchenButton ?? true) && await permissionService.CanSeeSendToKitchenAsync(CurrentUserId),
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
                    CanCancel = line.Status != RestaurantOrderLineStatus.Cancelled,
                    IsSettled = line.SettledByPendingPaymentId != null
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
            CashRegisterCashAccountId = resolvedCashRegister?.CashFinancialAccountId,
            CashRegisterCreditCardAccountId = resolvedCashRegister?.CreditCardFinancialAccountId,
            CashRegisterMealCardAccountId = resolvedCashRegister?.MealCardFinancialAccountId,
            PayableTotal = ComputeCheckRunningTotal(check.Id),
            // Edip, 2026-09-06: "satır indirimi yaptığında alt tarafa indirim bölümü açılsın ve
            // ara toplam indirim toplam ona göre çalışsın matematik hesabı" - Ara Toplam artık
            // TAM BRÜT (satır indirimi dahi düşülmeden) - satır indirimleri LineDiscountsTotal
            // olarak AYRI toplanıp aşağıda TicketDiscountAmount ile birleştirilip "İndirim"
            // satırında gösteriliyor. PayableTotal (ComputeCheckRunningTotal, netLinesTotal -
            // ticketDiscount) HİÇ değişmedi - Ara Toplam - İndirim matematiksel olarak hâlâ AYNI
            // PayableTotal'a eşit (netLinesTotal = grossLinesTotal - lineDiscounts).
            AraToplam = await dbContext.RestaurantOrderLines
                .Where(x => x.RestaurantOrder.RestaurantCheckId == check.Id && x.Status != RestaurantOrderLineStatus.Cancelled)
                .Select(x => x.Quantity * x.UnitPriceSnapshot)
                .SumAsync(),
            LineDiscountsTotal = await dbContext.RestaurantOrderLines
                .Where(x => x.RestaurantOrder.RestaurantCheckId == check.Id && x.Status != RestaurantOrderLineStatus.Cancelled)
                .Select(x => x.DiscountAmountSnapshot)
                .SumAsync(),
            TicketDiscountAmount = check.TicketDiscountAmount,
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
        model.PaidTotal = model.PendingPayments.Sum(x => x.Amount);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClosePayment([FromBody] RestaurantClosePaymentRequest request)
    {
        // Hesap uygunluğu ÖDEME KAYDEDİLİRKEN de denetlenir: terminal bir şubeye bağlıysa, seçilen
        // her hesap o şubenin hesabı veya ortak hesap olmalıdır. Bu, eski/başka şube hesabıyla
        // bir ödemenin karışmasını engeller (ekran filtresi tek başına yeterli değildir).
        // Terminal şubesi seçili değilse tahsilat YAPILMAZ (kontrol dışı ödeme kalmasın).
        if (TerminalBranchId is not { } terminalBranch)
        {
            return BadRequest(new { message = "Terminal şubesi seçilmemiş. Tahsilat için önce Terminal Ayarları'ndan şube ve kasa seçin." });
        }

        {
            var accountIds = request.Payments.Select(p => p.FinancialAccountId).Distinct().ToList();
            var invalidAccounts = await dbContext.FinancialAccounts
                .AsNoTracking()
                .Where(x => accountIds.Contains(x.Id) && !(x.IsActive && (x.BranchId == terminalBranch || x.IsShared)))
                .Select(x => x.Name)
                .ToListAsync();
            if (invalidAccounts.Count > 0)
            {
                return BadRequest(new { message = $"Bu hesap bu şubede kullanılamaz: {string.Join(", ", invalidAccounts)}" });
            }
        }

        // GERÇEK HATA (2026-09-06, kabul testinde bulundu, Fiş İkram) - "Payments.Count == 0"
        // koşulsuz reddi kaldırıldı: tam İkram edilmiş bir adisyonda (grandTotal=0) toplanacak
        // hiçbir tutar yoktur, sıfır ödeme satırıyla kapatmak GEÇERLİDİR - CloseCheckAsync'teki
        // "paymentsTotal != grandTotal" kontrolü zaten normal (grandTotal>0) adisyonları korur.
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
                fiscalInfo,
                TerminalBranchId);

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
                userId,
                request.OrderLineIds);

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
    //
    // GERÇEK HATA (2026-09-29, "sahte masa temizliği" regresyon testinde bulundu) - SourceLabel/
    // SourceType SABİT "Self Satış" idi; bu aksiyon sadece Self Satış hızlı ödemesinden
    // çağrıldığı için o zamana kadar fark edilmemişti, ama HeldReceipts/Fiş Listesi üzerinden
    // GERÇEK bir Masa Satış adisyonunun fişi de bu aksiyondan açılabiliyor - o durumda gerçek masa
    // adı yerine yanlışlıkla "Self Satış" basılıyordu. RestaurantReportsController.SourceTypeOf ile
    // AYNI Channel tabanlı mantık burada da uygulanıyor.
    [HttpGet]
    public async Task<IActionResult> Receipt(int id)
    {
        var sale = await dbContext.RetailSales
            .AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.Payments)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x!.RestaurantTable)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (sale is null)
        {
            return NotFound();
        }

        var channel = sale.RestaurantCheck.RestaurantTableSession.Channel;
        var sourceType = channel switch
        {
            RestaurantSaleChannel.SelfSatis => "self",
            RestaurantSaleChannel.Paket => "package",
            _ => "table"
        };
        var tableName = sale.RestaurantCheck.RestaurantTableSession.RestaurantTable?.Name ?? "";
        string sourceLabel;
        if (sourceType == "package")
        {
            var pkgOrder = await dbContext.PackageOrders.AsNoTracking().SingleOrDefaultAsync(x => x.RestaurantCheckId == sale.RestaurantCheckId);
            sourceLabel = pkgOrder is null ? tableName : $"{pkgOrder.PackageNumber} · {pkgOrder.CustomerName}";
        }
        else if (sourceType == "self")
        {
            sourceLabel = "Self Satış";
        }
        else
        {
            sourceLabel = tableName;
        }

        var model = new RestaurantReceiptDetailViewModel
        {
            DocumentNumber = sale.DocumentNumber,
            IssuedAtUtc = sale.IssuedAtUtc,
            SourceLabel = sourceLabel,
            SourceType = sourceType,
            IsCancelled = sale.Status == RetailSaleStatus.Cancelled,
            Lines = sale.Lines.Select(l => new RestaurantReceiptDetailLine(l.ProductNameSnapshot, l.Quantity, l.UnitPriceSnapshot, l.LineTotal)).ToList(),
            SubtotalAmount = sale.SubtotalAmount,
            DiscountAmount = sale.DiscountAmount,
            TaxAmount = sale.TaxAmount,
            GrandTotal = sale.GrandTotal,
            // GERÇEK HATA (2026-09-06, Z düzeltme kabul testinde bulundu) - bu satır içi switch
            // yalnızca Nakit/Kredi Kartı'nı tanıyordu, geri kalan HER ödeme türünü (Açık Hesap,
            // Ödenmez, Yemek Kartı) "Yemek Kartı" olarak etiketliyordu - Fiş Gör penceresinde bir
            // Açık Hesap tahsilatı yanlışlıkla Yemek Kartı gösteriliyordu.
            Payments = sale.RestaurantCheck.Payments.Where(p => !p.IsReversal).Select(p => new RestaurantReceiptDetailPayment(
                SinglePaymentMethodLabel(p.PaymentMethod),
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

            // Hızlı ödeme/Ödemeyi Al akışının sayfa yenilemeden (2026-09-26, Edip: "ödeme
            // alındıktan sonra akışı hızlansın") devam edebilmesi için - istemci artık bu tutarı
            // kullanarak RestaurantQuickPay/openPaymentModal'ı DOĞRUDAN çağırıyor, aradaki
            // "?quickpay=.../?openPayment=1 ile sayfayı yenile" adımı kaldırıldı.
            var payableTotal = ComputeCheckRunningTotal(request.CheckId);

            return Json(new
            {
                success = true,
                orderId = result.Order.Id,
                unroutedProductNames = result.UnroutedProductNames,
                payableTotal
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    // Ürün bazlı masa transferi (Edip, 2026-09-28: "ürünleri tıkladığımda sarı olsun, seçtiğim
    // ürünleri o masaya transfer etsin, ekran kalan ürünlerle devam etsin") - MoveTable/MergeTables
    // BÜTÜN adisyonu taşırken, bu SADECE seçilen satırları taşır (bkz. TransferOrderLinesAsync).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TransferOrderLines([FromBody] RestaurantTransferOrderLinesRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        try
        {
            await postingService.TransferOrderLinesAsync(request.CheckId, request.OrderLineIds, request.TargetTableId, userId);
            return Json(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    // "Sipariş Sil" (Edip, 2026-09-05: "yetkisi varsa her koşulda çalışsın") - istemcideki henüz
    // gönderilmemiş sepet zaten boşsa (Ödemeyi Al'dan Adisyona Dön sonrası HER ŞEY gönderilmiş
    // olur) bu uç nokta adisyondaki TÜM gönderilmiş satırları topluca iptal eder - bkz.
    // RestaurantPostingService.ClearOrderAsync. Adisyon kendiliğinden boşaldığı için redirect
    // Check(id) GET'in "artık açık değil" dalına düşer, bu da (madde 11 fix'i) otomatik temiz
    // Self Satış'a yönlendirir.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearOrder(int checkId, string reason, string? approverPin = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            await postingService.ClearOrderAsync(checkId, userId, reason, approverPin);
            TempData["Success"] = "Sipariş silindi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Check), new { id = checkId });
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
    public async Task<IActionResult> ApplyOrderLineDiscount(int lineId, int checkId, decimal amount, string? approverPin = null)
    {
        try
        {
            await postingService.ApplyOrderLineDiscountAsync(lineId, amount, CurrentUserId, approverPin);
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
        var moveError = await TerminalBranchErrorForSessionAsync(sessionId) ?? await TerminalBranchErrorForTableAsync(toTableId);
        if (moveError is not null)
        {
            TempData["Error"] = moveError;
            return RedirectToAction(nameof(Index));
        }

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
        var mergeError = await TerminalBranchErrorForSessionAsync(fromSessionId) ?? await TerminalBranchErrorForSessionAsync(intoSessionId);
        if (mergeError is not null)
        {
            TempData["Error"] = mergeError;
            return RedirectToAction(nameof(Index));
        }

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
