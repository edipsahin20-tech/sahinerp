using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

// Masa–adisyon–mutfak akışı (Faz 2). Ödeme/kasa/perakende fiş/muhasebe posting burada YOK — bkz.
// CLEAN_ROOM_DEVELOPMENT.md. RestaurantManager/Waiter/Kitchen rolleri yalnızca menüde gizlenmekle
// kalmaz, her aksiyon burada [Authorize(Roles=...)] ile de zorunlu kılınır.
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager},{AppRoles.Waiter}")]
public sealed class RestaurantController(ApplicationDbContext dbContext, RestaurantPostingService postingService) : RestaurantControllerBase(dbContext)
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

        var model = new RestaurantFloorViewModel
        {
            Sections = sections.Select(section => new RestaurantFloorSectionViewModel
            {
                Name = section.Name,
                Tables = section.Tables.OrderBy(t => t.Name).Select(table =>
                {
                    var session = openSessions.SingleOrDefault(s => s.RestaurantTableId == table.Id);
                    var check = session?.Checks.SingleOrDefault(c => c.Status == RestaurantCheckStatus.Open);
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
                        RunningTotal = check is null ? 0 : ComputeCheckRunningTotal(check.Id)
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

    public async Task<IActionResult> Check(int id)
    {
        ActivePage = "tables";
        var check = await dbContext.RestaurantChecks
            .AsNoTracking()
            .Include(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable).ThenInclude(x => x.RestaurantSection)
            .Include(x => x.Orders).ThenInclude(x => x.Lines).ThenInclude(x => x.KitchenTicketLines)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (check is null)
        {
            return NotFound();
        }

        if (check.Status != RestaurantCheckStatus.Open)
        {
            TempData["Error"] = "Bu adisyon artık açık değil.";
            return RedirectToAction(nameof(Index));
        }

        var categories = await dbContext.Products
            .AsNoTracking()
            .Where(x => x.IsActive && x.ShowAsShortcut)
            .Include(x => x.Category)
            .Include(x => x.TaxRate)
            .Include(x => x.Portions.Where(p => p.IsActive))
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
            .Select(x => new { x.FiscalDeviceType, x.FiscalAgentUrl })
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
            IsFiscalEnabled = fiscalSettings is { FiscalDeviceType: not FiscalDeviceType.None } && !string.IsNullOrWhiteSpace(fiscalSettings.FiscalAgentUrl),
            FiscalAgentUrl = fiscalSettings?.FiscalAgentUrl,
            SentOrders = check.Orders.OrderBy(x => x.OrderedAtUtc).Select(order => new RestaurantSentOrderViewModel
            {
                OrderId = order.Id,
                OrderedAtUtc = order.OrderedAtUtc,
                Lines = order.Lines.OrderBy(x => x.Id).Select(line => new RestaurantSentOrderLineViewModel
                {
                    LineId = line.Id,
                    ProductName = line.ProductNameSnapshot,
                    PortionName = line.PortionNameSnapshot,
                    Quantity = line.Quantity,
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
                        ImagePath = p.ImagePath,
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
            PayableTotal = ComputeCheckRunningTotal(check.Id)
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

            return Ok(new { retailSale.DocumentNumber, retailSale.GrandTotal });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
    public async Task<IActionResult> CancelOrderLine(int lineId, int checkId, string reason)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            await postingService.CancelOrderLineAsync(lineId, userId, reason);
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
