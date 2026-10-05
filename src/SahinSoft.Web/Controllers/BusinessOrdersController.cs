using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Common;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class BusinessOrdersController(
    SahinSoft.Web.Services.DocumentHardDeleteService hardDeleteService,
    SahinSoft.Web.Services.BranchSelectionService branchSelection,
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator) : Controller
{
    public async Task<IActionResult> Index(InvoiceType? type, BusinessDocumentStatus? status, string? search)
    {
        var query = dbContext.BusinessOrders
            .AsNoTracking()
            .Include(x => x.Customer)
            .OrderByDescending(x => x.OrderDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(x => x.OrderType == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => EF.Functions.Like(x.OrderNumber, SahinSoft.Web.Services.SearchPattern.ToLike(search)) || EF.Functions.Like(x.Customer.Name, SahinSoft.Web.Services.SearchPattern.ToLike(search)));
        }

        ViewBag.Type = type;
        ViewBag.Status = status;
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create(InvoiceType type)
    {
        var model = new BusinessOrderFormViewModel
        {
            OrderType = type,
            Lines = [new BusinessOrderLineFormViewModel()]
        };
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Controller = "BusinessOrders",
            HardDelete = true,
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() }
        };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BusinessOrderFormViewModel form)
    {
        ValidateLines(form);
        form.BranchId = await branchSelection.ResolveAsync(form.BranchId, User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel
            {
                Controller = "BusinessOrders",
            HardDelete = true,
                CreateRouteValues = new Dictionary<string, string> { ["type"] = form.OrderType.ToString() }
            };
            return View("Form", form);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sequenceKey = form.OrderType == InvoiceType.Sales ? "SALES_ORDER" : "PURCHASE_ORDER";

        // Çift tıklama/mükerrer POST koruması ön kontrolü — bkz. BusinessOrder.SubmissionKey.
        var existingBySubmission = await dbContext.BusinessOrders
            .FirstOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
        if (existingBySubmission is not null)
        {
            TempData["Success"] = "Sipariş taslağı zaten oluşturulmuştu.";
            return RedirectToAction(nameof(Details), new { id = existingBySubmission.Id });
        }

        BusinessOrder order;
        try
        {
            order = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync();

                    var newOrder = new BusinessOrder
                    {
                        OrderType = form.OrderType,
                        Status = BusinessDocumentStatus.Draft,
                        OrderNumber = await documentNumberGenerator.GenerateWithinTransactionAsync(sequenceKey),
                        CreatedByUserId = userId,
                        SubmissionKey = form.SubmissionKey
                    };
                    MapHeader(form, newOrder);
                    await MapLinesAsync(form, newOrder);
                    ComputeTotals(newOrder);

                    dbContext.BusinessOrders.Add(newOrder);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return newOrder;
                });
            });
        }
        catch (DbUpdateException)
        {
            // Bu isteğin transaction'ı geri alındı. SQL Server tek bir INSERT'te birden fazla unique
            // index ihlalinden sadece birini raporlar — bu yüzden hata mesajının içeriğine güvenmek
            // yerine doğrudan "bu SubmissionKey ile zaten bir kayıt var mı?" kontrolü yapılır. Varsa:
            // diğer eşzamanlı istek başarıyla kaydetti, bu istek onun sonucuna yönlendirilir. Yoksa:
            // gerçekten farklı bir çakışma — kullanıcıya araç çubuğu korunarak tekrar deneme mesajı
            // gösterilir.
            var existing = await dbContext.BusinessOrders.AsNoTracking()
                .SingleOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
            if (existing is not null)
            {
                TempData["Success"] = "Sipariş taslağı zaten oluşturulmuştu.";
                return RedirectToAction(nameof(Details), new { id = existing.Id });
            }

            ModelState.AddModelError(string.Empty, "Kaydetme sırasında bir çakışma oluştu, lütfen tekrar deneyin.");
            await PopulateSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel
            {
                Controller = "BusinessOrders",
            HardDelete = true,
                CreateRouteValues = new Dictionary<string, string> { ["type"] = form.OrderType.ToString() }
            };
            return View("Form", form);
        }

        TempData["Success"] = "Sipariş taslağı oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var order = await dbContext.BusinessOrders
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status != BusinessDocumentStatus.Draft)
        {
            return BadRequest("Yalnızca taslak siparişler düzenlenebilir.");
        }

        var model = new BusinessOrderFormViewModel
        {
            Id = order.Id,
            OrderType = order.OrderType,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            BranchId = order.BranchId,
            OrderDateUtc = order.OrderDateUtc,
            RequestedDeliveryDateUtc = order.RequestedDeliveryDateUtc,
            CurrencyCode = order.CurrencyCode,
            ExchangeRate = order.ExchangeRate,
            Notes = order.Notes,
            Lines = order.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new BusinessOrderLineFormViewModel
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.ForeignUnitPrice ?? x.UnitPrice,
                    DiscountRate = x.DiscountRate,
                    TaxRate = x.TaxRate
                })
                .ToList()
        };
        if (model.Lines.Count == 0)
        {
            model.Lines.Add(new BusinessOrderLineFormViewModel());
        }

        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id, order.OrderType);
        return View("Form", model);
    }

    // Kalıcı Sil: belgeyi ve bağlı tüm stok/cari/kasa hareketlerini veritabanından siler (bkz. DocumentHardDeleteService).
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SahinSoft.Domain.Constants.AppRoles.Administrator)]
    public async Task<IActionResult> HardDelete(int id)
    {
        var result = await hardDeleteService.DeleteBusinessOrderAsync(id);
        TempData[result.Ok ? "Success" : "Error"] = result.Message;
        return result.Ok ? RedirectToAction(nameof(Index)) : RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await dbContext.BusinessOrders
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status != BusinessDocumentStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak siparişler silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.BusinessOrders.Remove(order);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Sipariş silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id, InvoiceType type)
    {
        var previousId = await dbContext.BusinessOrders.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.BusinessOrders.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "BusinessOrders",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() },
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = true,
            HardDelete = true
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BusinessOrderFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        ValidateLines(form);
        form.BranchId = await branchSelection.ResolveAsync(form.BranchId, User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            await SetToolbarAsync(id, form.OrderType);
            return View("Form", form);
        }

        var order = await dbContext.BusinessOrders
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status != BusinessDocumentStatus.Draft)
        {
            return BadRequest("Yalnızca taslak siparişler düzenlenebilir.");
        }

        MapHeader(form, order);
        order.Lines.Clear();
        await MapLinesAsync(form, order);
        ComputeTotals(order);
        order.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Sipariş taslağı güncellendi.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await dbContext.BusinessOrders
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        var model = new BusinessOrderDetailsViewModel
        {
            Id = order.Id,
            OrderType = order.OrderType,
            Status = order.Status,
            OrderNumber = order.OrderNumber,
            OrderDateUtc = order.OrderDateUtc,
            RequestedDeliveryDateUtc = order.RequestedDeliveryDateUtc,
            CustomerName = order.Customer.Name,
            CurrencyCode = order.CurrencyCode,
            Subtotal = order.Subtotal,
            DiscountTotal = order.DiscountTotal,
            TaxTotal = order.TaxTotal,
            GrandTotal = order.GrandTotal,
            Notes = order.Notes,
            Lines = order.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new BusinessOrderDetailsLineViewModel
                {
                    ProductNameSnapshot = x.ProductNameSnapshot,
                    UnitSnapshot = x.UnitSnapshot,
                    Quantity = x.Quantity,
                    FulfilledQuantity = x.FulfilledQuantity,
                    UnitPrice = x.UnitPrice,
                    ForeignUnitPrice = x.ForeignUnitPrice,
                    LineTotal = x.LineTotal
                })
                .ToList()
        };

        var linkedDispatches = await dbContext.DispatchNotes
            .AsNoTracking()
            .Where(x => x.BusinessOrderId == id)
            .OrderBy(x => x.Id)
            .ToListAsync();
        model.LinkedDispatchNotes = linkedDispatches
            .Select(x => new LinkedDocumentViewModel { Id = x.Id, Number = x.DispatchNumber, StatusText = x.Status.GetDisplayName() })
            .ToList();

        var orderLineIds = order.Lines.Select(x => x.Id).ToList();
        var linkedInvoices = await dbContext.InvoiceLines
            .AsNoTracking()
            .Where(x => x.BusinessOrderLineId != null && orderLineIds.Contains(x.BusinessOrderLineId!.Value))
            .Select(x => x.Invoice)
            .Distinct()
            .OrderBy(x => x.Id)
            .ToListAsync();
        model.LinkedInvoices = linkedInvoices
            .Select(x => new LinkedDocumentViewModel { Id = x.Id, Number = x.InvoiceNumber, StatusText = x.Status.GetDisplayName() })
            .ToList();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var order = await dbContext.BusinessOrders.SingleOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status != BusinessDocumentStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak siparişler onaylanabilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = BusinessDocumentStatus.Approved;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Sipariş onaylandı.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await dbContext.BusinessOrders.SingleOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status is BusinessDocumentStatus.Fulfilled or BusinessDocumentStatus.Cancelled)
        {
            TempData["Error"] = "Bu sipariş iptal edilemez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = BusinessDocumentStatus.Cancelled;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Sipariş iptal edildi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private void ValidateLines(BusinessOrderFormViewModel form)
    {
        // Boş bırakılan satırlar hata değildir, sessizce göz ardı edilir — ama ASP.NET Core'un
        // otomatik model doğrulaması bu satırlar için eklemiş olabileceği ModelState hatalarını da
        // (ör. ProductId [Required]) temizlemek gerekir, yoksa satır çıkarılmış olsa bile
        // ModelState.IsValid false kalmaya devam eder.
        for (var i = 0; i < form.Lines.Count; i++)
        {
            if (form.Lines[i].ProductId is null)
            {
                foreach (var key in ModelState.Keys.Where(k => k.StartsWith($"{nameof(form.Lines)}[{i}].", StringComparison.Ordinal)).ToList())
                {
                    ModelState.Remove(key);
                }
            }
        }

        form.CurrencyCode = string.IsNullOrWhiteSpace(form.CurrencyCode) ? "TRY" : form.CurrencyCode.Trim().ToUpperInvariant();
        if (form.CurrencyCode != "TRY" && form.ExchangeRate <= 0)
        {
            ModelState.AddModelError(nameof(form.ExchangeRate), "Döviz siparişi için geçerli bir kur giriniz.");
        }

        form.Lines = form.Lines.Where(x => x.ProductId is not null).ToList();
        if (form.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Siparişte en az bir satır bulunmalıdır.");
        }
    }

    private static void MapHeader(BusinessOrderFormViewModel source, BusinessOrder target)
    {
        target.CustomerId = source.CustomerId!.Value;
        target.BranchId = source.BranchId;
        target.OrderDateUtc = DateTime.SpecifyKind(source.OrderDateUtc, DateTimeKind.Utc);
        target.RequestedDeliveryDateUtc = source.RequestedDeliveryDateUtc.HasValue
            ? DateTime.SpecifyKind(source.RequestedDeliveryDateUtc.Value, DateTimeKind.Utc)
            : null;
        target.CurrencyCode = source.CurrencyCode.Trim().ToUpperInvariant();
        target.ExchangeRate = target.CurrencyCode == "TRY" ? 1 : source.ExchangeRate;
        target.Notes = source.Notes?.Trim();
    }

    private async Task MapLinesAsync(BusinessOrderFormViewModel source, BusinessOrder target)
    {
        var productIds = source.Lines.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToList();
        var products = await dbContext.Products.Where(x => productIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);

        var lineNumber = 1;
        foreach (var line in source.Lines)
        {
            if (line.ProductId is null || !products.TryGetValue(line.ProductId.Value, out var product))
            {
                continue;
            }

            // Döviz siparişinde satıra girilen fiyat sipariş para birimindedir (ör. USD); TL birim fiyat = döviz × kur
            // (faturadaki ile aynı model). Toplamlar ve stok/muhasebe hep TL üzerinden çalışır.
            var isForeign = target.CurrencyCode != "TRY";
            decimal? foreignPrice = isForeign ? line.UnitPrice : null;
            var unitPriceTl = isForeign ? RoundMoney(line.UnitPrice * target.ExchangeRate) : line.UnitPrice;

            var gross = RoundMoney(line.Quantity * unitPriceTl);
            var discountAmount = RoundMoney(gross * line.DiscountRate / 100);
            var net = gross - discountAmount;
            var taxAmount = RoundMoney(net * line.TaxRate / 100);

            target.Lines.Add(new BusinessOrderLine
            {
                LineNumber = lineNumber++,
                ProductId = product.Id,
                ProductCodeSnapshot = product.StockCode,
                ProductNameSnapshot = product.Name,
                UnitSnapshot = product.Unit,
                Quantity = line.Quantity,
                UnitPrice = unitPriceTl,
                ForeignUnitPrice = foreignPrice,
                DiscountRate = line.DiscountRate,
                TaxRate = line.TaxRate,
                LineTotal = net + taxAmount
            });
        }
    }

    private static void ComputeTotals(BusinessOrder order)
    {
        decimal subtotal = 0, discountTotal = 0, taxTotal = 0, grandTotal = 0;
        foreach (var line in order.Lines)
        {
            var gross = RoundMoney(line.Quantity * line.UnitPrice);
            var discountAmount = RoundMoney(gross * line.DiscountRate / 100);
            var net = gross - discountAmount;
            var taxAmount = RoundMoney(net * line.TaxRate / 100);
            subtotal += gross;
            discountTotal += discountAmount;
            taxTotal += taxAmount;
            grandTotal += net + taxAmount;
        }

        order.Subtotal = RoundMoney(subtotal);
        order.DiscountTotal = RoundMoney(discountTotal);
        order.TaxTotal = RoundMoney(taxTotal);
        order.GrandTotal = RoundMoney(grandTotal);
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private async Task PopulateSelectionsAsync(BusinessOrderFormViewModel model)
    {
        model.BranchId ??= await branchSelection.DefaultBranchIdAsync(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));
        model.BranchOptions = await branchSelection.OptionsAsync(model.BranchId);

        if (model.CustomerId is int customerId)
        {
            model.CustomerDisplay = await dbContext.Customers
                .Where(x => x.Id == customerId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        var productIds = model.Lines.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToList();
        var productDisplays = await dbContext.Products
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, Display = x.StockCode + " - " + x.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Display);

        foreach (var line in model.Lines)
        {
            if (line.ProductId is int productId && productDisplays.TryGetValue(productId, out var display))
            {
                line.ProductDisplay = display;
            }
        }
    }
}
