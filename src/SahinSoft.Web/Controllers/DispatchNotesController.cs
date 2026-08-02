using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Common;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class DispatchNotesController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator,
    DispatchNotePostingService dispatchNotePostingService) : Controller
{
    public async Task<IActionResult> Index(InvoiceType? type, BusinessDocumentStatus? status, string? search)
    {
        var query = dbContext.DispatchNotes
            .AsNoTracking()
            .Include(x => x.Customer)
            .OrderByDescending(x => x.DispatchDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(x => x.DispatchType == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.DispatchNumber.Contains(search) || x.Customer.Name.Contains(search));
        }

        ViewBag.Type = type;
        ViewBag.Status = status;
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create(InvoiceType type)
    {
        var model = new DispatchNoteFormViewModel
        {
            DispatchType = type,
            Lines = [new DispatchNoteLineFormViewModel()]
        };
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Controller = "DispatchNotes",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() }
        };
        var conversionSettings = await dbContext.InventorySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        ViewBag.OrderToDispatchAutoApprove = ConversionAutoApprovalPolicy.ShouldAutoApprove(
            conversionSettings, ConversionAutoApprovalKind.OrderToDispatch, type, User);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DispatchNoteFormViewModel form)
    {
        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel
            {
                Controller = "DispatchNotes",
                CreateRouteValues = new Dictionary<string, string> { ["type"] = form.DispatchType.ToString() }
            };
            return View("Form", form);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sequenceKey = form.DispatchType == InvoiceType.Sales ? "SALES_DISPATCH" : "PURCHASE_DISPATCH";

        // Çift tıklama/mükerrer POST koruması ön kontrolü — bkz. DispatchNote.SubmissionKey.
        var existingBySubmission = await dbContext.DispatchNotes
            .FirstOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
        if (existingBySubmission is not null)
        {
            TempData["Success"] = "İrsaliye taslağı zaten oluşturulmuştu.";
            return RedirectToAction(nameof(Details), new { id = existingBySubmission.Id });
        }

        DispatchNote dispatch;
        try
        {
            dispatch = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync();

                    var newDispatch = new DispatchNote
                    {
                        DispatchType = form.DispatchType,
                        Status = BusinessDocumentStatus.Draft,
                        DispatchNumber = await documentNumberGenerator.GenerateWithinTransactionAsync(sequenceKey),
                        CreatedByUserId = userId,
                        SubmissionKey = form.SubmissionKey
                    };
                    MapHeader(form, newDispatch);
                    await MapLinesAsync(form, newDispatch);

                    dbContext.DispatchNotes.Add(newDispatch);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return newDispatch;
                });
            });
        }
        catch (DbUpdateException)
        {
            // Bu isteğin transaction'ı geri alındı. SQL Server tek bir INSERT'te birden fazla unique
            // index ihlalinden sadece birini raporlar (ör. eşzamanlı üretilen aynı belge numarası VE
            // aynı SubmissionKey aynı anda çakışabilir) — bu yüzden hata mesajının içeriğine
            // güvenmek yerine doğrudan "bu SubmissionKey ile zaten bir kayıt var mı?" kontrolü
            // yapılır. Varsa: diğer eşzamanlı istek başarıyla kaydetti, bu istek onun sonucuna
            // yönlendirilir. Yoksa: gerçekten farklı bir çakışma — kullanıcıya araç çubuğu
            // korunarak tekrar deneme mesajı gösterilir.
            var existing = await dbContext.DispatchNotes.AsNoTracking()
                .SingleOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
            if (existing is not null)
            {
                TempData["Success"] = "İrsaliye taslağı zaten oluşturulmuştu.";
                return RedirectToAction(nameof(Details), new { id = existing.Id });
            }

            ModelState.AddModelError(string.Empty, "Kaydetme sırasında bir çakışma oluştu, lütfen tekrar deneyin.");
            await PopulateSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel
            {
                Controller = "DispatchNotes",
                CreateRouteValues = new Dictionary<string, string> { ["type"] = form.DispatchType.ToString() }
            };
            return View("Form", form);
        }

        // "Planlı İşlem" (F5) ile bir veya daha fazla satır BusinessOrderLineId taşıyorsa, bu normal
        // Create aksiyonu üzerinden de Sipariş→İrsaliye dönüşümü yapılmış demektir — otomatik onay
        // parametresi buraya da uygulanır. Kaynak siparişi olmayan tamamen elle girilmiş (plansız)
        // irsaliyeler bu kontrolden hiç etkilenmez.
        if (dispatch.Lines.Any(x => x.BusinessOrderLineId is not null))
        {
            await TryAutoApproveAsync(dispatch.Id, ConversionAutoApprovalKind.OrderToDispatch, dispatch.DispatchType, userId,
                successMessage: "İrsaliye oluşturuldu ve otomatik onaylandı.",
                fallbackMessage: "İrsaliye taslağı oluşturuldu.");
        }
        else
        {
            TempData["Success"] = "İrsaliye taslağı oluşturuldu.";
        }

        return RedirectToAction(nameof(Details), new { id = dispatch.Id });
    }

    // Sipariş → İrsaliye: onaylı/kısmen karşılanmış siparişin kalan satırlarını gösterir.
    public async Task<IActionResult> CreateFromOrder(int orderId)
    {
        var order = await dbContext.BusinessOrders
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == orderId);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status is not (BusinessDocumentStatus.Approved or BusinessDocumentStatus.PartiallyFulfilled))
        {
            TempData["Error"] = "Yalnızca onaylı veya kısmen karşılanmış siparişlerden irsaliye oluşturulabilir.";
            return RedirectToAction("Details", "BusinessOrders", new { id = orderId });
        }

        var remainingLines = order.Lines
            .Where(x => x.Quantity - x.FulfilledQuantity > 0)
            .OrderBy(x => x.LineNumber)
            .Select(x => new CreateDispatchFromOrderLineViewModel
            {
                BusinessOrderLineId = x.Id,
                ProductNameSnapshot = x.ProductNameSnapshot,
                UnitSnapshot = x.UnitSnapshot,
                OrderedQuantity = x.Quantity,
                RemainingQuantity = x.Quantity - x.FulfilledQuantity,
                QuantityToShip = x.Quantity - x.FulfilledQuantity
            })
            .ToList();

        if (remainingLines.Count == 0)
        {
            TempData["Error"] = "Siparişte kalan (sevk edilmemiş) satır yok.";
            return RedirectToAction("Details", "BusinessOrders", new { id = orderId });
        }

        var settings = await dbContext.InventorySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        var model = new CreateDispatchFromOrderViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            DispatchType = order.OrderType,
            CustomerId = order.CustomerId,
            CustomerDisplay = $"{order.Customer.Code} - {order.Customer.Name}",
            Lines = remainingLines,
            WillAutoApprove = ConversionAutoApprovalPolicy.ShouldAutoApprove(settings, ConversionAutoApprovalKind.OrderToDispatch, order.OrderType, User)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromOrder(CreateDispatchFromOrderViewModel form)
    {
        form.Lines = form.Lines.Where(x => x.QuantityToShip > 0).ToList();
        if (form.WarehouseId is null)
        {
            ModelState.AddModelError(nameof(form.WarehouseId), "Depo seçilmelidir.");
        }

        if (form.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "En az bir satırda sevk edilecek miktar girilmelidir.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateOrderConversionSelectionsAsync(form);
            return View(form);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sequenceKey = form.DispatchType == InvoiceType.Sales ? "SALES_DISPATCH" : "PURCHASE_DISPATCH";

        // Çift tıklama/mükerrer POST koruması ön kontrolü — bkz. DispatchNote.SubmissionKey.
        var existingBySubmission = await dbContext.DispatchNotes
            .FirstOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
        if (existingBySubmission is not null)
        {
            TempData["Success"] = "İrsaliye taslağı zaten oluşturulmuştu.";
            return RedirectToAction(nameof(Details), new { id = existingBySubmission.Id });
        }

        DispatchNote dispatch;
        try
        {
            dispatch = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync();

                    var orderLineIds = form.Lines.Select(x => x.BusinessOrderLineId).ToList();
                    var orderLines = await dbContext.BusinessOrderLines
                        .Where(x => orderLineIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id);

                    var newDispatch = new DispatchNote
                    {
                        DispatchType = form.DispatchType,
                        Status = BusinessDocumentStatus.Draft,
                        DispatchNumber = await documentNumberGenerator.GenerateWithinTransactionAsync(sequenceKey),
                        CreatedByUserId = userId,
                        SubmissionKey = form.SubmissionKey,
                        CustomerId = form.CustomerId,
                        WarehouseId = form.WarehouseId!.Value,
                        DispatchDateUtc = DateTime.SpecifyKind(form.DispatchDateUtc, DateTimeKind.Utc),
                        VehiclePlate = form.VehiclePlate?.Trim(),
                        CarrierName = form.CarrierName?.Trim(),
                        Notes = $"Sipariş {form.OrderNumber} üzerinden oluşturuldu.",
                        BusinessOrderId = form.OrderId
                    };

                    var lineNumber = 1;
                    foreach (var line in form.Lines)
                    {
                        if (!orderLines.TryGetValue(line.BusinessOrderLineId, out var orderLine) || orderLine.ProductId is null)
                        {
                            continue;
                        }

                        newDispatch.Lines.Add(new DispatchNoteLine
                        {
                            LineNumber = lineNumber++,
                            ProductId = orderLine.ProductId.Value,
                            ProductVariantId = orderLine.ProductVariantId,
                            Quantity = line.QuantityToShip,
                            BusinessOrderLineId = orderLine.Id
                        });
                    }

                    if (newDispatch.Lines.Count == 0)
                    {
                        throw new InvalidOperationException("En az bir satırda sevk edilecek miktar girilmelidir.");
                    }

                    dbContext.DispatchNotes.Add(newDispatch);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return newDispatch;
                });
            });
        }
        catch (DbUpdateException)
        {
            var existing = await dbContext.DispatchNotes.AsNoTracking()
                .SingleOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
            if (existing is not null)
            {
                TempData["Success"] = "İrsaliye taslağı zaten oluşturulmuştu.";
                return RedirectToAction(nameof(Details), new { id = existing.Id });
            }

            ModelState.AddModelError(string.Empty, "Kaydetme sırasında bir çakışma oluştu, lütfen tekrar deneyin.");
            await PopulateOrderConversionSelectionsAsync(form);
            return View(form);
        }

        await TryAutoApproveAsync(dispatch.Id, ConversionAutoApprovalKind.OrderToDispatch, form.DispatchType, userId,
            successMessage: "Siparişten irsaliye oluşturuldu ve otomatik onaylandı.",
            fallbackMessage: "Siparişten irsaliye taslağı oluşturuldu. Gözden geçirip onaylayabilirsiniz.");
        return RedirectToAction(nameof(Details), new { id = dispatch.Id });
    }

    // Kaydettikten sonra otomatik onayla: taslak KAYDEDİLİR (yukarıdaki transaction zaten commit
    // edildi), ardından — ayrı, kendi atomik transaction'ına sahip — ApproveAsync çağrılır. Bu iki
    // adım TEK bir atomik işlem DEĞİLDİR (kasıtlı olarak); onay adımı başarısız olursa (ör. eşzamanlı
    // bir başka onay kalan miktarı tüketmişse) belge Taslak olarak kalır, hiçbir kısmi hareket
    // oluşmaz — ApproveAsync'in kendi Serializable transaction'ı bunu garanti eder. Status hiçbir
    // zaman elle set edilmez, yalnızca gerçek onay servisi üzerinden değişir.
    private async Task TryAutoApproveAsync(
        int dispatchId,
        ConversionAutoApprovalKind kind,
        InvoiceType direction,
        string userId,
        string successMessage,
        string fallbackMessage)
    {
        var settings = await dbContext.InventorySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        if (!ConversionAutoApprovalPolicy.ShouldAutoApprove(settings, kind, direction, User))
        {
            TempData["Success"] = fallbackMessage;
            return;
        }

        try
        {
            await dispatchNotePostingService.ApproveAsync(dispatchId, userId);
            TempData["Success"] = successMessage;
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = $"Belge kaydedildi ancak otomatik onaylanamadı; Taslak olarak bırakıldı. Sebep: {ex.Message}";
        }
        catch (ConcurrencyRetryExhaustedException ex)
        {
            TempData["Error"] = $"Belge kaydedildi ancak otomatik onaylanamadı; Taslak olarak bırakıldı. Sebep: {ex.Message}";
        }
    }

    private async Task PopulateOrderConversionSelectionsAsync(CreateDispatchFromOrderViewModel form)
    {
        if (form.WarehouseId is int warehouseId)
        {
            form.WarehouseDisplay = await dbContext.Warehouses
                .Where(x => x.Id == warehouseId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }
    }

    // "Planlı İşlem" (F5) — mal kabul/sevkiyat: seçili cariye ait, aynı alış/satış yönündeki, onaylı
    // ve henüz tamamen karşılanmamış siparişleri (satırlarıyla birlikte) döner.
    [HttpGet]
    public async Task<IActionResult> EligibleOrders(int customerId, InvoiceType dispatchType)
    {
        var orders = await dbContext.BusinessOrders
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.CustomerId == customerId &&
                x.OrderType == dispatchType &&
                (x.Status == BusinessDocumentStatus.Approved || x.Status == BusinessDocumentStatus.PartiallyFulfilled))
            .OrderByDescending(x => x.OrderDateUtc)
            .ToListAsync();

        var result = orders
            .Select(order => new
            {
                id = order.Id,
                orderNumber = order.OrderNumber,
                dateUtc = order.OrderDateUtc.ToString("dd.MM.yyyy"),
                lines = order.Lines
                    .Where(x => x.Quantity - x.FulfilledQuantity > 0)
                    .OrderBy(x => x.LineNumber)
                    .Select(line => new
                    {
                        businessOrderLineId = line.Id,
                        productId = line.ProductId,
                        productDisplay = line.ProductCodeSnapshot + " - " + line.ProductNameSnapshot,
                        unitSnapshot = line.UnitSnapshot,
                        remainingQuantity = line.Quantity - line.FulfilledQuantity
                    })
                    .ToList()
            })
            .Where(x => x.lines.Count > 0)
            .ToList();

        return Json(new { items = result });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dispatch = await dbContext.DispatchNotes
            .Include(x => x.Lines)
            .ThenInclude(x => x.BusinessOrderLine!)
            .ThenInclude(x => x.BusinessOrder!)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (dispatch is null)
        {
            return NotFound();
        }

        if (dispatch.Status != BusinessDocumentStatus.Draft)
        {
            return BadRequest("Yalnızca taslak irsaliyeler düzenlenebilir.");
        }

        var model = new DispatchNoteFormViewModel
        {
            Id = dispatch.Id,
            DispatchType = dispatch.DispatchType,
            DispatchNumber = dispatch.DispatchNumber,
            CustomerId = dispatch.CustomerId,
            WarehouseId = dispatch.WarehouseId,
            DispatchDateUtc = dispatch.DispatchDateUtc,
            VehiclePlate = dispatch.VehiclePlate,
            CarrierName = dispatch.CarrierName,
            Notes = dispatch.Notes,
            Lines = dispatch.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new DispatchNoteLineFormViewModel
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    BusinessOrderLineId = x.BusinessOrderLineId,
                    SourceDisplay = x.BusinessOrderLine != null ? $"Sipariş {x.BusinessOrderLine.BusinessOrder.OrderNumber}" : null
                })
                .ToList()
        };
        if (model.Lines.Count == 0)
        {
            model.Lines.Add(new DispatchNoteLineFormViewModel());
        }

        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id, dispatch.DispatchType);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var dispatch = await dbContext.DispatchNotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (dispatch is null)
        {
            return NotFound();
        }

        if (dispatch.Status != BusinessDocumentStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak irsaliyeler silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.DispatchNotes.Remove(dispatch);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "İrsaliye silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id, InvoiceType type)
    {
        var previousId = await dbContext.DispatchNotes.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.DispatchNotes.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "DispatchNotes",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() },
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = true
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DispatchNoteFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            await SetToolbarAsync(id, form.DispatchType);
            return View("Form", form);
        }

        var dispatch = await dbContext.DispatchNotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (dispatch is null)
        {
            return NotFound();
        }

        if (dispatch.Status != BusinessDocumentStatus.Draft)
        {
            return BadRequest("Yalnızca taslak irsaliyeler düzenlenebilir.");
        }

        MapHeader(form, dispatch);
        dispatch.Lines.Clear();
        await MapLinesAsync(form, dispatch);
        dispatch.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "İrsaliye taslağı güncellendi.";
        return RedirectToAction(nameof(Details), new { id = dispatch.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var dispatch = await dbContext.DispatchNotes
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Warehouse)
            .Include(x => x.BusinessOrder)
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (dispatch is null)
        {
            return NotFound();
        }

        var settings = await dbContext.CompanySettings.AsNoTracking().SingleAsync(x => x.Id == 1);

        var model = new DispatchNoteDetailsViewModel
        {
            Id = dispatch.Id,
            DispatchType = dispatch.DispatchType,
            Status = dispatch.Status,
            DispatchNumber = dispatch.DispatchNumber,
            DispatchDateUtc = dispatch.DispatchDateUtc,
            CustomerName = dispatch.Customer.Name,
            CustomerPhone = dispatch.Customer.Phone,
            CustomerEmail = dispatch.Customer.Email,
            CustomerAddress = dispatch.Customer.Address,
            WarehouseName = dispatch.Warehouse.Name,
            Company = new PdfCompanyInfoViewModel
            {
                CompanyName = settings.CompanyName,
                Website = settings.Website,
                Email = settings.Email,
                Phone = settings.Phone,
                BankName = settings.BankName,
                Iban = settings.Iban
            },
            VehiclePlate = dispatch.VehiclePlate,
            CarrierName = dispatch.CarrierName,
            Notes = dispatch.Notes,
            ApprovedByUserId = dispatch.ApprovedByUserId,
            ApprovedAtUtc = dispatch.ApprovedAtUtc,
            Lines = dispatch.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new DispatchNoteDetailsLineViewModel { ProductName = x.Product.Name, Quantity = x.Quantity, InvoicedQuantity = x.InvoicedQuantity })
                .ToList(),
            SourceOrderId = dispatch.BusinessOrderId,
            SourceOrderNumber = dispatch.BusinessOrder?.OrderNumber
        };

        var dispatchLineIds = dispatch.Lines.Select(x => x.Id).ToList();
        var linkedInvoices = await dbContext.InvoiceLines
            .AsNoTracking()
            .Where(x => x.DispatchNoteLineId != null && dispatchLineIds.Contains(x.DispatchNoteLineId!.Value))
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        try
        {
            await dispatchNotePostingService.ApproveAsync(id, userId);
            TempData["Success"] = "İrsaliye onaylandı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (ConcurrencyRetryExhaustedException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Cancel(int id, string reason)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        try
        {
            await dispatchNotePostingService.CancelAsync(id, userId, reason);
            TempData["Success"] = "İrsaliye iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (ConcurrencyRetryExhaustedException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private void ValidateLines(DispatchNoteFormViewModel form)
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

        form.Lines = form.Lines.Where(x => x.ProductId is not null).ToList();
        if (form.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "İrsaliyede en az bir satır bulunmalıdır.");
        }
    }

    private static void MapHeader(DispatchNoteFormViewModel source, DispatchNote target)
    {
        target.CustomerId = source.CustomerId!.Value;
        target.WarehouseId = source.WarehouseId!.Value;
        target.DispatchDateUtc = DateTime.SpecifyKind(source.DispatchDateUtc, DateTimeKind.Utc);
        target.VehiclePlate = source.VehiclePlate?.Trim();
        target.CarrierName = source.CarrierName?.Trim();
        target.Notes = source.Notes?.Trim();
    }

    private async Task MapLinesAsync(DispatchNoteFormViewModel source, DispatchNote target)
    {
        var productIds = source.Lines.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToList();
        var validProductIds = await dbContext.Products.Where(x => productIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();

        var lineNumber = 1;
        foreach (var line in source.Lines)
        {
            if (line.ProductId is null || !validProductIds.Contains(line.ProductId.Value))
            {
                continue;
            }

            target.Lines.Add(new DispatchNoteLine
            {
                LineNumber = lineNumber++,
                ProductId = line.ProductId.Value,
                Quantity = line.Quantity,
                BusinessOrderLineId = line.BusinessOrderLineId
            });
        }
    }

    private async Task PopulateSelectionsAsync(DispatchNoteFormViewModel model)
    {
        if (model.CustomerId is int customerId)
        {
            model.CustomerDisplay = await dbContext.Customers
                .Where(x => x.Id == customerId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        if (model.WarehouseId is int warehouseId)
        {
            model.WarehouseDisplay = await dbContext.Warehouses
                .Where(x => x.Id == warehouseId)
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
