using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
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
public sealed class InvoicesController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator,
    InvoicePostingService invoicePostingService,
    InvoiceCancellationOrchestrationService invoiceCancellationOrchestration,
    OverdueScheduleService overdueScheduleService,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(
        InvoiceType? type,
        InvoiceStatus? status,
        int? customerId,
        string? search,
        string? period,
        DateTime? from,
        DateTime? to,
        bool? overdue)
    {
        var query = dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .OrderByDescending(x => x.InvoiceDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(x => x.InvoiceType == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        // Dashboard'daki "Gecikmiş Tahsilatlar"/"Gecikmiş Ödemeler" kartlarından gelir: faturanın
        // kendi PaidAmount'ı değil, müşterinin GERÇEK cari bakiyesi baz alınır (bkz.
        // OverdueScheduleService — elle girilen Tahsilat/Tediye faturaya bağlanmadığından PaidAmount
        // tek başına yanıltıcı olabiliyordu; aynı hesap HomeController.Index'te de kullanılıyor,
        // sayaçlar ve bu liste burada birebir eşleşir).
        if (overdue == true)
        {
            var overdueToday = DateTime.UtcNow.Date;
            var genuinelyOverdueIds = (await overdueScheduleService.GetNettedSchedulesAsync(overdueToday, type, HttpContext.RequestAborted))
                .Select(x => x.InvoiceId)
                .ToList();
            query = query.Where(x => genuinelyOverdueIds.Contains(x.Id));
        }

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.InvoiceNumber.Contains(search) ||
                x.Customer.Name.Contains(search));
        }

        // "Gelişmiş" panelindeki açık from/to tarihleri, üstteki hızlı "Dönem" seçiminden önceliklidir.
        var today = DateTime.UtcNow.Date;
        DateTime? periodStart = period switch
        {
            "today" => today,
            "week" => today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1)),
            "month" => new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => null
        };
        var effectiveFrom = from ?? periodStart;
        if (effectiveFrom.HasValue)
        {
            query = query.Where(x => x.InvoiceDateUtc >= effectiveFrom.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(x => x.InvoiceDateUtc < to.Value.AddDays(1));
        }

        ViewBag.Type = type;
        ViewBag.Status = status;
        ViewBag.CustomerId = customerId;
        ViewBag.Search = search;
        ViewBag.Period = period;
        ViewBag.From = from;
        ViewBag.To = to;
        ViewBag.Overdue = overdue;

        try
        {
            return View(await query.ToListAsync());
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Fatura listesi yüklenirken hata oluştu: " + (ex.InnerException?.Message ?? ex.Message);
            return View(new List<Invoice>());
        }
    }

    public async Task<IActionResult> Create(InvoiceType type)
    {
        var sequenceKey = type == InvoiceType.Sales ? "SALES_INVOICE" : "PURCHASE_INVOICE";
        var peek = await documentNumberGenerator.PeekAsync(sequenceKey);
        var defaultWarehouseId = await dbContext.Warehouses
            .Where(x => x.IsDefault && x.IsActive)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync();
        var model = new InvoiceFormViewModel
        {
            InvoiceType = type,
            DocumentSeries = peek.Prefix,
            DocumentSequence = peek.NextNumber.ToString($"D{peek.Padding}"),
            WarehouseId = defaultWarehouseId,
            Lines = [new InvoiceLineFormViewModel()]
        };
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Controller = "Invoices",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() }
        };
        var conversionSettings = await dbContext.InventorySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        ViewBag.DispatchToInvoiceAutoApprove = ConversionAutoApprovalPolicy.ShouldAutoApprove(
            conversionSettings, ConversionAutoApprovalKind.DispatchToInvoice, type, User);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceFormViewModel form)
    {
        ValidateLines(form);
        ValidateSettlement(form);
        // Belge numarası (Seri/Sıra) çözümlemesi gerçek bir DB yazımı ve bir sayaç tüketimidir —
        // bu yüzden yalnızca form BAŞKA hiçbir sebeple geçersiz DEĞİLSE denenir. Aksi halde (örn.
        // satır eksik, Kapalı Fatura için kasa/ödeme seçilmemiş) numara hiç üretilmeden hata gösterilir.
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            SetCreateToolbar(form.InvoiceType);
            return View("Form", form);
        }

        var sequenceKey = form.InvoiceType == InvoiceType.Sales ? "SALES_INVOICE" : "PURCHASE_INVOICE";
        var createdByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // Çift tıklama/mükerrer POST koruması: aynı SubmissionKey ile daha önce oluşturulmuş bir
        // fatura varsa (client tarafında buton pasifleştirme atlanmışsa, ağ tekrar denemesi vb.),
        // yeni bir numara/fatura ÜRETMEDEN aynı başarı sonucuna yönlendirilir. Bu ön kontrol sadece
        // hızlı yoldur — asıl garanti aşağıdaki veritabanı seviyesindeki unique index'tir (bkz.
        // Invoice.SubmissionKey), çünkü iki eşzamanlı istek bu ön kontrolü ikisi de geçebilir.
        var existingBySubmission = await dbContext.Invoices
            .FirstOrDefaultAsync(x => x.SubmissionKey == form.SubmissionKey);
        if (existingBySubmission is not null)
        {
            TempData["Success"] = $"{existingBySubmission.InvoiceNumber} numaralı fatura taslağı oluşturuldu.";
            return RedirectToAction(nameof(Create), new { type = form.InvoiceType });
        }

        // Numara üretimi ve fatura kaydı TEK transaction içinde: fatura satırları/toplamları
        // hesaplanırken veya SaveChangesAsync sırasında herhangi bir sebeple hata olursa, üretilen
        // numara da (sayaç dahil) geri alınır — hiçbir zaman "numara verildi ama evrak yok" durumu
        // oluşmaz. Dıştaki ExecuteWithConcurrencyRetryAsync, iki kullanıcının tam olarak aynı anda
        // kaydetmesi durumunda NumberSequence'ın RowVersion çakışmasını (DbUpdateConcurrencyException)
        // yakalayıp taze bir transaction'la otomatik tekrar dener — bkz. DocumentNumberGeneratorService.
        var isAutoNumbered = string.IsNullOrEmpty(form.DocumentSeries?.Trim()) && string.IsNullOrEmpty(form.DocumentSequence?.Trim());
        Invoice? invoice = null;
        for (var healAttempt = 1; ; healAttempt++)
        {
            try
            {
                invoice = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
                {
                    var strategy = dbContext.Database.CreateExecutionStrategy();
                    return await strategy.ExecuteAsync(async () =>
                    {
                        await using var transaction = await dbContext.Database.BeginTransactionAsync();

                        var invoiceNumber = await ResolveInvoiceNumberWithinTransactionAsync(form, sequenceKey, excludeInvoiceId: null);
                        if (invoiceNumber is null)
                        {
                            return null;
                        }

                        var newInvoice = new Invoice
                        {
                            InvoiceType = form.InvoiceType,
                            Status = InvoiceStatus.Draft,
                            InvoiceNumber = invoiceNumber,
                            CreatedByUserId = createdByUserId,
                            SubmissionKey = form.SubmissionKey
                        };
                        MapHeader(form, newInvoice);
                        if (!await MapLinesAsync(form, newInvoice))
                        {
                            ModelState.AddModelError(string.Empty, "İade satırları faturanın toplam tutarını negatife düşüremez.");
                            return null;
                        }

                        dbContext.Invoices.Add(newInvoice);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return newInvoice;
                    });
                });
                break;
            }
            catch (DbUpdateException ex) when (IsSubmissionKeyUniqueViolation(ex))
            {
                // Tam olarak aynı anda gönderilen ikinci istek: birinci istek kaydı zaten oluşturdu
                // (transaction commit oldu), bu istek SubmissionKey unique index'ine çarptı. Bu isteğin
                // kendi transaction'ı (dolayısıyla bu istekte üretilmiş olabilecek numara/sayaç artışı)
                // otomatik olarak geri alındı — ikinci bir fatura OLUŞMADI, birinci isteğin sonucuna
                // yönlendiriliyoruz.
                var existing = await dbContext.Invoices.SingleAsync(x => x.SubmissionKey == form.SubmissionKey);
                TempData["Success"] = $"{existing.InvoiceNumber} numaralı fatura taslağı oluşturuldu.";
                return RedirectToAction(nameof(Create), new { type = form.InvoiceType });
            }
            catch (DbUpdateException ex) when (IsInvoiceNumberUniqueViolation(ex) && isAutoNumbered && healAttempt < 3)
            {
                // Sayaç, bu türdeki gerçek veriden geride kalmış (ör. elle numara girilip sayaç
                // güncellenmeden önce oluşan eski bir tutarsızlık) — üretilen numara zaten dolu
                // çıktı. Başarısız denemenin ChangeTracker izleri temizlenir, sayaç ELDEKİ EN BÜYÜK
                // numaranın bir fazlasına (asla boşluk doldurmaya değil — Stok Kartı kodlarındaki
                // aynı kural) ileri alınır ve deneme tekrarlanır.
                foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
                {
                    entry.State = EntityState.Detached;
                }
                await HealSequenceDriftAsync(sequenceKey, form.InvoiceType);
            }
            catch (ConcurrencyRetryExhaustedException ex)
            {
                // Aşırı yoğun eşzamanlı yük (çok sayıda kullanıcı tam olarak aynı anda aynı belge
                // türünü kaydediyor) tüm tekrar deneme haklarını tüketti — ham teknik hata yerine
                // kullanıcıya anlaşılır bir mesaj gösterilip form (araç çubuğu dahil) korunuyor.
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectionsAsync(form);
                SetCreateToolbar(form.InvoiceType);
                return View("Form", form);
            }
        }

        if (invoice is null)
        {
            await PopulateSelectionsAsync(form);
            SetCreateToolbar(form.InvoiceType);
            return View("Form", form);
        }

        // "Planlı İşlem" (F5) ile bir veya daha fazla satır DispatchNoteLineId taşıyorsa, bu normal
        // Create aksiyonu üzerinden de İrsaliye→Fatura dönüşümü yapılmış demektir — otomatik onay
        // parametresi buraya da uygulanır. Kaynak irsaliyesi olmayan tamamen elle girilmiş (plansız)
        // faturalar bu kontrolden hiç etkilenmez.
        if (invoice.Lines.Any(x => x.DispatchNoteLineId is not null))
        {
            await TryAutoApproveAsync(invoice.Id, ConversionAutoApprovalKind.DispatchToInvoice, form.InvoiceType, createdByUserId,
                successMessage: $"{invoice.InvoiceNumber} numaralı fatura oluşturuldu ve otomatik onaylandı.",
                fallbackMessage: $"{invoice.InvoiceNumber} numaralı fatura taslağı oluşturuldu.");
        }
        else
        {
            TempData["Success"] = $"{invoice.InvoiceNumber} numaralı fatura taslağı oluşturuldu.";
        }

        // Mikro tarzı hızlı ardışık evrak girişi: yeni fatura kaydedilince Detay'a değil, aynı
        // türde (Satış/Alış) boş bir yeni fatura giriş ekranına dönülür — düzenlemede (Edit) bu
        // davranış farklı, orada kayıttan sonra Detay'a gidilmeye devam eder.
        return RedirectToAction(nameof(Create), new { type = form.InvoiceType });
    }

    // Sipariş → Fatura (doğrudan, irsaliyesiz): onaylı/kısmen karşılanmış siparişin kalan satırlarını,
    // sipariş satırındaki fiyat/KDV/iskonto ile birlikte gösterir.
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
            TempData["Error"] = "Yalnızca onaylı veya kısmen karşılanmış siparişlerden fatura oluşturulabilir.";
            return RedirectToAction("Details", "BusinessOrders", new { id = orderId });
        }

        var remainingLines = order.Lines
            .Where(x => x.Quantity - x.FulfilledQuantity > 0)
            .OrderBy(x => x.LineNumber)
            .Select(x => new CreateInvoiceFromOrderLineViewModel
            {
                BusinessOrderLineId = x.Id,
                ProductNameSnapshot = x.ProductNameSnapshot,
                UnitSnapshot = x.UnitSnapshot,
                OrderedQuantity = x.Quantity,
                RemainingQuantity = x.Quantity - x.FulfilledQuantity,
                UnitPrice = x.UnitPrice,
                DiscountRate = x.DiscountRate,
                TaxRate = x.TaxRate,
                QuantityToInvoice = x.Quantity - x.FulfilledQuantity
            })
            .ToList();

        if (remainingLines.Count == 0)
        {
            TempData["Error"] = "Siparişte kalan (faturalanmamış) satır yok.";
            return RedirectToAction("Details", "BusinessOrders", new { id = orderId });
        }

        var settings = await dbContext.InventorySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        var model = new CreateInvoiceFromOrderViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            InvoiceType = order.OrderType,
            CustomerId = order.CustomerId,
            CustomerDisplay = $"{order.Customer.Code} - {order.Customer.Name}",
            CurrencyCode = order.CurrencyCode,
            ExchangeRate = order.ExchangeRate,
            Lines = remainingLines,
            WillAutoApprove = ConversionAutoApprovalPolicy.ShouldAutoApprove(settings, ConversionAutoApprovalKind.OrderToInvoice, order.OrderType, User)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromOrder(CreateInvoiceFromOrderViewModel form)
    {
        form.Lines = form.Lines.Where(x => x.QuantityToInvoice > 0).ToList();
        if (form.WarehouseId is null)
        {
            ModelState.AddModelError(nameof(form.WarehouseId), "Depo seçilmelidir.");
        }

        if (form.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "En az bir satırda faturalanacak miktar girilmelidir.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateOrderConversionSelectionsAsync(form);
            return View(form);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sequenceKey = form.InvoiceType == InvoiceType.Sales ? "SALES_INVOICE" : "PURCHASE_INVOICE";

        var existingBySubmission = await dbContext.Invoices
            .FirstOrDefaultAsync(x => x.SubmissionKey == form.SubmissionKey);
        if (existingBySubmission is not null)
        {
            TempData["Success"] = $"{existingBySubmission.InvoiceNumber} numaralı fatura taslağı zaten oluşturulmuştu.";
            return RedirectToAction(nameof(Details), new { id = existingBySubmission.Id });
        }

        Invoice invoice;
        try
        {
            invoice = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync();

                    var orderLineIds = form.Lines.Select(x => x.BusinessOrderLineId).ToList();
                    var orderLines = await dbContext.BusinessOrderLines
                        .Where(x => orderLineIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id);

                    var newInvoice = new Invoice
                    {
                        InvoiceType = form.InvoiceType,
                        Status = InvoiceStatus.Draft,
                        InvoiceNumber = await documentNumberGenerator.GenerateWithinTransactionAsync(sequenceKey),
                        CreatedByUserId = userId,
                        SubmissionKey = form.SubmissionKey,
                        CustomerId = form.CustomerId,
                        WarehouseId = form.WarehouseId!.Value,
                        InvoiceDateUtc = DateTime.SpecifyKind(form.InvoiceDateUtc, DateTimeKind.Utc),
                        CurrencyCode = form.CurrencyCode,
                        ExchangeRate = form.ExchangeRate,
                        Notes = $"Sipariş {form.OrderNumber} üzerinden oluşturuldu."
                    };

                    var lineNumber = 1;
                    foreach (var line in form.Lines)
                    {
                        if (!orderLines.TryGetValue(line.BusinessOrderLineId, out var orderLine) || orderLine.ProductId is null)
                        {
                            continue;
                        }

                        newInvoice.Lines.Add(new InvoiceLine
                        {
                            LineNumber = lineNumber++,
                            ProductId = orderLine.ProductId.Value,
                            ProductVariantId = orderLine.ProductVariantId,
                            ProductCodeSnapshot = orderLine.ProductCodeSnapshot,
                            ProductNameSnapshot = orderLine.ProductNameSnapshot,
                            UnitSnapshot = orderLine.UnitSnapshot,
                            Quantity = line.QuantityToInvoice,
                            UnitPrice = orderLine.UnitPrice,
                            DiscountRate = orderLine.DiscountRate,
                            TaxRate = orderLine.TaxRate,
                            BusinessOrderLineId = orderLine.Id
                        });
                    }

                    if (newInvoice.Lines.Count == 0)
                    {
                        throw new InvalidOperationException("En az bir satırda faturalanacak miktar girilmelidir.");
                    }

                    CalculateDraftTotals(newInvoice);

                    dbContext.Invoices.Add(newInvoice);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return newInvoice;
                });
            });
        }
        catch (DbUpdateException)
        {
            var existing = await dbContext.Invoices.AsNoTracking()
                .SingleOrDefaultAsync(x => x.SubmissionKey == form.SubmissionKey);
            if (existing is not null)
            {
                TempData["Success"] = $"{existing.InvoiceNumber} numaralı fatura taslağı zaten oluşturulmuştu.";
                return RedirectToAction(nameof(Details), new { id = existing.Id });
            }

            ModelState.AddModelError(string.Empty, "Kaydetme sırasında bir çakışma oluştu, lütfen tekrar deneyin.");
            await PopulateOrderConversionSelectionsAsync(form);
            return View(form);
        }

        await TryAutoApproveAsync(invoice.Id, ConversionAutoApprovalKind.OrderToInvoice, form.InvoiceType, userId,
            successMessage: $"Siparişten {invoice.InvoiceNumber} numaralı fatura oluşturuldu ve otomatik onaylandı.",
            fallbackMessage: $"Siparişten {invoice.InvoiceNumber} numaralı fatura taslağı oluşturuldu. Gözden geçirip onaylayabilirsiniz.");
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    // Kaydettikten sonra otomatik onayla: taslak KAYDEDİLİR (yukarıdaki transaction zaten commit
    // edildi), ardından — ayrı, kendi atomik transaction'ına sahip — ApproveAsync çağrılır. Bu iki
    // adım TEK bir atomik işlem DEĞİLDİR (kasıtlı olarak); onay adımı başarısız olursa belge Taslak
    // olarak kalır, hiçbir kısmi hareket oluşmaz — ApproveAsync'in kendi Serializable transaction'ı
    // bunu garanti eder. Status hiçbir zaman elle set edilmez, yalnızca gerçek onay servisi üzerinden
    // değişir. (bkz. DispatchNotesController.TryAutoApproveAsync — aynı desen.)
    private async Task TryAutoApproveAsync(
        int invoiceId,
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
            await invoicePostingService.ApproveAsync(invoiceId, userId);
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

    private async Task PopulateOrderConversionSelectionsAsync(CreateInvoiceFromOrderViewModel form)
    {
        if (form.WarehouseId is int warehouseId)
        {
            form.WarehouseDisplay = await dbContext.Warehouses
                .Where(x => x.Id == warehouseId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }
    }

    // "Planlı İşlem" (F5): seçili cariye ait, aynı alış/satış yönündeki, onaylı ve henüz tamamen
    // faturalanmamış irsaliyeleri (satırlarıyla birlikte) döner — Invoices/Form.cshtml bunu JSON
    // olarak çekip picker'da gösterir. Fiyat/KDV/iskonto kaynağı: irsaliye satırı bir siparişten
    // geliyorsa (BusinessOrderLineId set) o sipariş satırının fiyatı; yoksa ürünün güncel fiyatı
    // (önerilen değer, kullanıcı düzenleyebilir).
    [HttpGet]
    public async Task<IActionResult> EligibleDispatchNotes(int customerId, InvoiceType invoiceType)
    {
        var dispatches = await dbContext.DispatchNotes
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .ThenInclude(x => x.TaxRate)
            .Include(x => x.Lines)
            .ThenInclude(x => x.BusinessOrderLine)
            .Where(x => x.CustomerId == customerId &&
                x.DispatchType == invoiceType &&
                (x.Status == BusinessDocumentStatus.Approved || x.Status == BusinessDocumentStatus.PartiallyFulfilled))
            .OrderByDescending(x => x.DispatchDateUtc)
            .ToListAsync();

        var result = dispatches
            .Select(dispatch => new
            {
                id = dispatch.Id,
                dispatchNumber = dispatch.DispatchNumber,
                dateUtc = dispatch.DispatchDateUtc.ToString("dd.MM.yyyy"),
                warehouseId = dispatch.WarehouseId,
                warehouseDisplay = $"{dispatch.Warehouse.Code} - {dispatch.Warehouse.Name}",
                lines = dispatch.Lines
                    .Where(x => x.Quantity - x.InvoicedQuantity > 0)
                    .OrderBy(x => x.LineNumber)
                    .Select(line => new
                    {
                        dispatchNoteLineId = line.Id,
                        productId = line.ProductId,
                        productDisplay = $"{line.Product.StockCode} - {line.Product.Name}",
                        unitSnapshot = line.Product.Unit,
                        remainingQuantity = line.Quantity - line.InvoicedQuantity,
                        unitPrice = line.BusinessOrderLine?.UnitPrice
                            ?? (invoiceType == InvoiceType.Sales ? line.Product.SalePrice : line.Product.PurchasePrice),
                        discountRate = line.BusinessOrderLine?.DiscountRate ?? 0,
                        taxRate = line.BusinessOrderLine?.TaxRate ?? line.Product.TaxRate.Rate
                    })
                    .ToList()
            })
            .Where(x => x.lines.Count > 0)
            .ToList();

        return Json(new { items = result });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var invoice = await dbContext.Invoices
            .Include(x => x.Lines)
            .ThenInclude(x => x.DispatchNoteLine!)
            .ThenInclude(x => x.DispatchNote!)
            .Include(x => x.Lines)
            .ThenInclude(x => x.BusinessOrderLine!)
            .ThenInclude(x => x.BusinessOrder!)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return BadRequest("İptal edilmiş faturalar düzenlenemez.");
        }

        if (invoice.Status == InvoiceStatus.Approved && !User.IsInRole(AppRoles.Administrator))
        {
            return Forbid();
        }

        var sequenceKey = invoice.InvoiceType == InvoiceType.Sales ? "SALES_INVOICE" : "PURCHASE_INVOICE";
        var currentPrefix = (await documentNumberGenerator.PeekAsync(sequenceKey)).Prefix;
        var (series, sequencePart) = SplitInvoiceNumber(invoice.InvoiceNumber, currentPrefix);

        var model = new InvoiceFormViewModel
        {
            Id = invoice.Id,
            InvoiceType = invoice.InvoiceType,
            InvoiceNumber = invoice.InvoiceNumber,
            Status = invoice.Status,
            DocumentSeries = series,
            DocumentSequence = sequencePart,
            CustomerId = invoice.CustomerId,
            WarehouseId = invoice.WarehouseId,
            InvoiceDateUtc = invoice.InvoiceDateUtc,
            DueDateUtc = invoice.DueDateUtc,
            CurrencyCode = invoice.CurrencyCode,
            ExchangeRate = invoice.ExchangeRate,
            Notes = invoice.Notes,
            ReferenceNumber = invoice.ReferenceNumber,
            PaymentTerm = invoice.PaymentTerm,
            TradeType = invoice.TradeType,
            IsReturn = invoice.IsReturn,
            SalespersonUserId = invoice.SalespersonUserId,
            IsClosedInvoice = invoice.IsClosedInvoice,
            SettlementPaymentMethod = invoice.SettlementPaymentMethod,
            SettlementFinancialAccountId = invoice.SettlementFinancialAccountId,
            AmountDiscount = invoice.AmountDiscount,
            Lines = invoice.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new InvoiceLineFormViewModel
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    DiscountRate = x.DiscountRate,
                    TaxRate = x.TaxRate,
                    Description = x.Description,
                    DispatchNoteLineId = x.DispatchNoteLineId,
                    BusinessOrderLineId = x.BusinessOrderLineId,
                    SourceDisplay = x.DispatchNoteLine is not null
                        ? $"İrsaliye {x.DispatchNoteLine.DispatchNote.DispatchNumber}"
                        : x.BusinessOrderLine is not null
                            ? $"Sipariş {x.BusinessOrderLine.BusinessOrder.OrderNumber}"
                            : null
                })
                .ToList()
        };
        if (model.Lines.Count == 0)
        {
            model.Lines.Add(new InvoiceLineFormViewModel());
        }

        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id, invoice.InvoiceType);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await dbContext.Invoices
            .Include(x => x.Lines)
            .Include(x => x.PaymentSchedules)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        // İptal edilen bir fatura da silinebilir — iptal sırasında zaten ters (reversal) stok/cari
        // hareketleri oluşturulmuştu; StockMovement/CurrentAccountTransaction kayıtları InvoiceId
        // SetNull ile korunduğu için silme bu geçmiş kayıtları etkilemez.
        if (invoice.Status is not (InvoiceStatus.Draft or InvoiceStatus.Cancelled))
        {
            TempData["Error"] = "Yalnızca taslak veya iptal edilmiş faturalar silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.Invoices.Remove(invoice);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Fatura silindi.";
        return RedirectToAction(nameof(Index));
    }

    // Create GET ile aynı toolbar'ı kurar — POST'ta doğrulama başarısız olup form yeniden
    // gösterildiğinde de ViewBag.Toolbar ayarlanmazsa _EvrakToolbar partial'ı (Model null olduğu
    // için) hiç render edilmez ve "Sakla" dahil TÜM araç çubuğu düğmeleri kaybolur — kullanıcı
    // hatayı düzeltip kaydedecek hiçbir buton bulamaz ("kayıt ediyorum ama evrak yok" hatasının
    // asıl sebeplerinden biri buydu).
    private void SetCreateToolbar(InvoiceType type)
    {
        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Controller = "Invoices",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() }
        };
    }

    private async Task SetToolbarAsync(int id, InvoiceType type)
    {
        var previousId = await dbContext.Invoices.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.Invoices.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "Invoices",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() },
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = true
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InvoiceFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        ValidateLines(form);
        ValidateSettlement(form);

        var invoice = await dbContext.Invoices
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return BadRequest("İptal edilmiş faturalar düzenlenemez.");
        }

        if (invoice.Status == InvoiceStatus.Approved && !User.IsInRole(AppRoles.Administrator))
        {
            return Forbid();
        }

        // Belge numarası çözümlemesi (Seri/Sıra) gerçek bir sayaç tüketimidir — form BAŞKA hiçbir
        // sebeple geçersiz DEĞİLSE denenir (bkz. Create'teki aynı gerekçe).
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            await SetToolbarAsync(id, invoice.InvoiceType);
            return View("Form", form);
        }

        var sequenceKey = invoice.InvoiceType == InvoiceType.Sales ? "SALES_INVOICE" : "PURCHASE_INVOICE";

        if (invoice.Status == InvoiceStatus.Draft)
        {
            // Numara üretimi ve fatura güncellemesi TEK transaction içinde (bkz. Create'teki gerekçe,
            // eşzamanlılık tekrar denemesi dahil).
            var negativeTotals = false;
            var saved = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync();

                    var invoiceNumber = await ResolveInvoiceNumberWithinTransactionAsync(form, sequenceKey, excludeInvoiceId: invoice.Id);
                    if (invoiceNumber is null)
                    {
                        return false;
                    }

                    invoice.InvoiceNumber = invoiceNumber;
                    MapHeader(form, invoice);
                    invoice.Lines.Clear();
                    if (!await MapLinesAsync(form, invoice))
                    {
                        negativeTotals = true;
                        return false;
                    }
                    invoice.UpdatedAtUtc = DateTime.UtcNow;

                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                });
            });

            if (!saved)
            {
                if (negativeTotals)
                {
                    ModelState.AddModelError(string.Empty, "İade satırları faturanın toplam tutarını negatife düşüremez.");
                }
                await PopulateSelectionsAsync(form);
                await SetToolbarAsync(id, invoice.InvoiceType);
                return View("Form", form);
            }

            TempData["Success"] = "Fatura taslağı güncellendi.";
            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }

        // Onaylı fatura düzenleniyor: eski stok/cari hareketleri ters kayıtla geri alınıp yeni
        // satırlarla yeniden postalanır (InvoicePostingService.EditApprovedAsync) — belge numarası
        // üretimi de aynı transaction içinde yapılır (callback'in kendi içinde), böylece numara
        // çakışması veya başka bir hata durumunda hiçbir şey (numara dahil) kalıcı olmaz.
        var editedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var numberConflict = false;
        try
        {
            await invoicePostingService.EditApprovedAsync(invoice.Id, editedByUserId, async inv =>
            {
                var invoiceNumber = await ResolveInvoiceNumberWithinTransactionAsync(form, sequenceKey, excludeInvoiceId: invoice.Id);
                if (invoiceNumber is null)
                {
                    numberConflict = true;
                    throw new InvalidOperationException("Bu belge numarası başka bir faturada kullanılıyor.");
                }
                inv.InvoiceNumber = invoiceNumber;
                MapHeader(form, inv);
                inv.Lines.Clear();
                if (!await MapLinesAsync(form, inv))
                {
                    throw new InvalidOperationException("İade satırları faturanın toplam tutarını negatife düşüremez.");
                }
            });
            TempData["Success"] = "Onaylı fatura düzenlendi; stok ve cari hareketleri güncellendi.";
        }
        catch (InvalidOperationException ex)
        {
            if (!numberConflict)
            {
                TempData["Error"] = ex.Message;
            }
        }

        if (numberConflict)
        {
            await PopulateSelectionsAsync(form);
            await SetToolbarAsync(id, invoice.InvoiceType);
            return View("Form", form);
        }

        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Warehouse)
            .Include(x => x.Lines)
            .ThenInclude(x => x.DispatchNoteLine!)
            .ThenInclude(x => x.DispatchNote!)
            .Include(x => x.Lines)
            .ThenInclude(x => x.BusinessOrderLine!)
            .ThenInclude(x => x.BusinessOrder!)
            .Include(x => x.PaymentSchedules)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        var settings = await dbContext.CompanySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        var salespersonName = invoice.SalespersonUserId is null
            ? null
            : (await userManager.FindByIdAsync(invoice.SalespersonUserId))?.FullName;
        var settlementAccountName = invoice.SettlementFinancialAccountId is null
            ? null
            : await dbContext.FinancialAccounts
                .Where(x => x.Id == invoice.SettlementFinancialAccountId)
                .Select(x => x.Name)
                .SingleOrDefaultAsync();
        var settlementReceipt = invoice.IsClosedInvoice
            ? await dbContext.PaymentReceipts
                .Where(x => x.InvoiceId == invoice.Id)
                .OrderByDescending(x => x.Id)
                .Select(x => new { x.Id, x.ReceiptNumber })
                .FirstOrDefaultAsync()
            : null;

        var model = new InvoiceDetailsViewModel
        {
            Id = invoice.Id,
            InvoiceType = invoice.InvoiceType,
            Status = invoice.Status,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDateUtc = invoice.InvoiceDateUtc,
            DueDateUtc = invoice.DueDateUtc,
            CustomerName = invoice.Customer.Name,
            CustomerCode = invoice.Customer.Code,
            CustomerTaxOffice = invoice.Customer.TaxOffice,
            CustomerTaxNumber = invoice.Customer.TaxNumber,
            CustomerPhone = invoice.Customer.Phone,
            CustomerEmail = invoice.Customer.Email,
            CustomerAddress = invoice.Customer.Address,
            WarehouseName = invoice.Warehouse.Name,
            CurrencyCode = invoice.CurrencyCode,
            ReferenceNumber = invoice.ReferenceNumber,
            PaymentTerm = invoice.PaymentTerm,
            TradeType = invoice.TradeType,
            IsReturn = invoice.IsReturn,
            SalespersonName = salespersonName,
            IsClosedInvoice = invoice.IsClosedInvoice,
            SettlementPaymentMethodName = invoice.SettlementPaymentMethod?.GetDisplayName(),
            SettlementFinancialAccountName = settlementAccountName,
            SettlementReceiptId = settlementReceipt?.Id,
            SettlementReceiptNumber = settlementReceipt?.ReceiptNumber,
            Company = new PdfCompanyInfoViewModel
            {
                CompanyName = settings.CompanyName,
                Website = settings.Website,
                Email = settings.Email,
                Phone = settings.Phone,
                BankName = settings.BankName,
                Iban = settings.Iban
            },
            Subtotal = invoice.Subtotal,
            DiscountTotal = invoice.DiscountTotal,
            AmountDiscount = invoice.AmountDiscount,
            TaxTotal = invoice.TaxTotal,
            GrandTotal = invoice.GrandTotal,
            Notes = invoice.Notes,
            ApprovedByUserId = invoice.ApprovedByUserId,
            ApprovedAtUtc = invoice.ApprovedAtUtc,
            CancelledByUserId = invoice.CancelledByUserId,
            CancelledAtUtc = invoice.CancelledAtUtc,
            CancellationReason = invoice.CancellationReason,
            Lines = invoice.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new InvoiceDetailsLineViewModel
                {
                    LineNumber = x.LineNumber,
                    ProductNameSnapshot = x.ProductNameSnapshot,
                    UnitSnapshot = x.UnitSnapshot,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    DiscountRate = x.DiscountRate,
                    DiscountAmount = x.DiscountAmount,
                    TaxRate = x.TaxRate,
                    LineTotal = x.LineTotal,
                    GrossTotal = Math.Round(x.Quantity * x.UnitPrice, 2, MidpointRounding.AwayFromZero),
                    NetTotal = Math.Round((x.Quantity * x.UnitPrice) - x.DiscountAmount, 2, MidpointRounding.AwayFromZero),
                    Description = x.Description,
                    SourceDisplay = x.DispatchNoteLine is not null
                        ? $"İrsaliye {x.DispatchNoteLine.DispatchNote.DispatchNumber}"
                        : x.BusinessOrderLine is not null
                            ? $"Sipariş {x.BusinessOrderLine.BusinessOrder.OrderNumber}"
                            : null
                })
                .ToList(),
            TaxBreakdown = invoice.Lines
                .GroupBy(x => x.TaxRate)
                .OrderBy(g => g.Key)
                .Select(g => new InvoiceTaxBreakdownViewModel { TaxRate = g.Key, TaxAmount = g.Sum(x => x.TaxAmount) })
                .ToList(),
            PaymentSchedules = invoice.PaymentSchedules
                .OrderBy(x => x.InstallmentNumber)
                .Select(x => new InvoiceDetailsScheduleViewModel
                {
                    InstallmentNumber = x.InstallmentNumber,
                    DueDateUtc = x.DueDateUtc,
                    Amount = x.Amount,
                    PaidAmount = x.PaidAmount
                })
                .ToList(),
            ActiveLinkedReceiptCount = invoice.Status == InvoiceStatus.Approved
                ? await dbContext.PaymentReceipts.CountAsync(x => x.InvoiceId == invoice.Id && x.Status == PaymentReceiptStatus.Approved)
                : 0
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        try
        {
            await invoicePostingService.ApproveAsync(id, userId);
            TempData["Success"] = "Fatura onaylandı.";
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
            await invoicePostingService.CancelAsync(id, userId, reason);
            TempData["Success"] = "Fatura iptal edildi.";
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

    // Faturaya bağlı aktif tahsilat/tediye fişi bulunduğu için normal İptal Et bloklanmışsa,
    // kullanıcıya hangi fişlerin de birlikte iptal edileceğini gösteren onay ekranı.
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> CancelWithPayments(int id)
    {
        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Status != InvoiceStatus.Approved)
        {
            TempData["Error"] = "Yalnızca onaylanmış faturalar bu şekilde iptal edilebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var activeReceipts = await invoiceCancellationOrchestration.GetActiveLinkedReceiptsAsync(id);
        if (activeReceipts.Count == 0)
        {
            TempData["Error"] = "Faturaya bağlı aktif bir tahsilat/tediye bulunamadı; normal İptal Et akışını kullanın.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new CancelInvoiceWithPaymentsViewModel
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceType = invoice.InvoiceType,
            CustomerName = invoice.Customer.Name,
            GrandTotal = invoice.GrandTotal,
            CurrencyCode = invoice.CurrencyCode,
            LinkedReceipts = activeReceipts.Select(x => new LinkedReceiptViewModel
            {
                Id = x.Id,
                ReceiptNumber = x.ReceiptNumber,
                ReceiptType = x.ReceiptType,
                TotalAmount = x.TotalAmount,
                ReceiptDateUtc = x.ReceiptDateUtc,
                FinancialAccountNames = string.Join(", ", x.Lines.Select(l => l.FinancialAccount.Name).Distinct())
            }).ToList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> CancelWithPayments(CancelInvoiceWithPaymentsViewModel form)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        try
        {
            await invoiceCancellationOrchestration.CancelInvoiceWithLinkedPaymentsAsync(form.InvoiceId, userId, form.Reason);
            TempData["Success"] = "Fatura, bağlı tahsilat/tediye fişleriyle birlikte iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (ConcurrencyRetryExhaustedException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = form.InvoiceId });
    }

    private void ValidateLines(InvoiceFormViewModel form)
    {
        // Kullanıcının doldurmadan bıraktığı tamamen boş satırlar (ör. Enter'a fazla basılması,
        // satır ekleyip vazgeçilmesi) gerçek bir hata değildir — sessizce göz ardı edilir. ASP.NET
        // Core'un otomatik model doğrulaması bu satırlar için ModelState hatası eklemiş olabilir
        // (ör. ProductId [Required]); satırı listeden çıkarmadan önce o hataları da temizlemek
        // gerekir, yoksa ModelState.IsValid, satır zaten çıkarılmış olsa bile false kalmaya devam eder.
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

        form.Lines = form.Lines
            .Where(x => x.ProductId is not null)
            .ToList();

        if (form.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Faturada en az bir satır bulunmalıdır.");
        }

        // Miktar aralığı negatifi (iade satırı) kabul eder — yalnızca tam sıfır anlamsızdır.
        for (var i = 0; i < form.Lines.Count; i++)
        {
            if (form.Lines[i].Quantity == 0)
            {
                ModelState.AddModelError($"{nameof(form.Lines)}[{i}].Quantity", "Miktar 0 olamaz.");
            }
        }
    }

    // Kapalı Fatura işaretliyse ödeme yöntemi ve kapanacak kasa/banka zorunludur — onay anında
    // InvoicePostingService.PostClosedInvoiceSettlementAsync otomatik tahsilat/tediye fişi
    // oluşturmak için bu ikisine ihtiyaç duyar.
    private void ValidateSettlement(InvoiceFormViewModel form)
    {
        if (!form.IsClosedInvoice)
        {
            return;
        }

        if (form.SettlementPaymentMethod is null)
        {
            ModelState.AddModelError(nameof(form.SettlementPaymentMethod), "Kapalı fatura için ödeme yöntemi seçilmelidir.");
        }

        if (form.SettlementFinancialAccountId is null)
        {
            ModelState.AddModelError(nameof(form.SettlementFinancialAccountId), "Kapalı fatura için kapanacak kasa/banka seçilmelidir.");
        }
    }

    private static void MapHeader(InvoiceFormViewModel source, Invoice target)
    {
        target.CustomerId = source.CustomerId!.Value;
        target.WarehouseId = source.WarehouseId!.Value;
        target.InvoiceDateUtc = DateTime.SpecifyKind(source.InvoiceDateUtc, DateTimeKind.Utc);
        target.DueDateUtc = source.DueDateUtc.HasValue
            ? DateTime.SpecifyKind(source.DueDateUtc.Value, DateTimeKind.Utc)
            : null;
        target.CurrencyCode = source.CurrencyCode.Trim().ToUpperInvariant();
        target.ExchangeRate = source.ExchangeRate;
        target.Notes = source.Notes?.Trim();
        target.ReferenceNumber = source.ReferenceNumber?.Trim();
        target.PaymentTerm = source.PaymentTerm?.Trim();
        target.TradeType = source.TradeType?.Trim();
        target.IsReturn = source.IsReturn;
        target.SalespersonUserId = string.IsNullOrWhiteSpace(source.SalespersonUserId) ? null : source.SalespersonUserId;
        target.IsClosedInvoice = source.IsClosedInvoice;
        target.SettlementPaymentMethod = source.IsClosedInvoice ? source.SettlementPaymentMethod : null;
        target.SettlementFinancialAccountId = source.IsClosedInvoice ? source.SettlementFinancialAccountId : null;
        target.AmountDiscount = source.AmountDiscount;
    }

    // İade satırı (negatif miktar) tek başına veya diğer satırları aşarak faturanın toplamlarını
    // negatife düşürebilir — DB'deki CK_Invoices_Totals check constraint'i (Subtotal/DiscountTotal/
    // TaxTotal/GrandTotal >= 0) bunu zaten engeller, ama ham SQL hatası yerine burada dostane mesajla
    // erken kesilsin diye dönüş değeri false olur (true = toplamlar geçerli, satırlar eklendi).
    private async Task<bool> MapLinesAsync(InvoiceFormViewModel source, Invoice target)
    {
        var productIds = source.Lines
            .Where(x => x.ProductId.HasValue)
            .Select(x => x.ProductId!.Value)
            .Distinct()
            .ToList();
        var products = await dbContext.Products
            .Where(x => productIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        var lineNumber = 1;
        foreach (var line in source.Lines)
        {
            if (line.ProductId is null || !products.TryGetValue(line.ProductId.Value, out var product))
            {
                continue;
            }

            target.Lines.Add(new InvoiceLine
            {
                LineNumber = lineNumber++,
                ProductId = product.Id,
                ProductCodeSnapshot = product.StockCode,
                ProductNameSnapshot = product.Name,
                UnitSnapshot = product.Unit,
                Quantity = line.Quantity,
                // "Fiyatlara KDV Dahil" işaretliyse girilen birim fiyat KDV dahildir; kayıt sırasında
                // KDV hariç birim fiyata çevrilir ki mevcut toplam/KDV hesap mantığı (InvoiceTotalsCalculator,
                // onay/iptal ters kayıtları) hiç değişmeden, hep KDV hariç fiyat üzerinden çalışmaya devam etsin.
                UnitPrice = source.PricesIncludeTax && line.TaxRate > 0
                    ? Math.Round(line.UnitPrice / (1 + line.TaxRate / 100), 6, MidpointRounding.AwayFromZero)
                    : line.UnitPrice,
                DiscountRate = line.DiscountRate,
                TaxRate = line.TaxRate,
                Description = line.Description?.Trim(),
                DispatchNoteLineId = line.DispatchNoteLineId,
                BusinessOrderLineId = line.DispatchNoteLineId is null ? line.BusinessOrderLineId : null
            });
        }

        CalculateDraftTotals(target);

        return target.Subtotal >= 0 && target.DiscountTotal >= 0 && target.TaxTotal >= 0 && target.GrandTotal >= 0;
    }

    // Onaydan önce (taslak haldeyken) satır ve fatura toplamlarının Detay ekranında 0.00 görünmemesi
    // için, InvoicePostingService.ApproveAsync'teki para hesabıyla aynı formül (InvoiceTotalsCalculator)
    // burada da uygulanır (stok hareketi/cari kaydı gibi onaya özel işlemler olmadan, sadece tutar hesabı).
    private static void CalculateDraftTotals(Invoice invoice) => InvoiceTotalsCalculator.Calculate(invoice);

    // Evrak Belge Seri/Sıra formda boş bırakılırsa sistem otomatik numara üretir (mevcut davranış).
    // Doldurulmuşsa kullanıcının girdiği numara aynen kullanılır — başka bir faturada kullanılmadığı
    // doğrulanır ve sayısal kısmı ayrıştırılabiliyorsa gelecekteki otomatik numaraların bununla
    // çakışmaması için sayaç ileri alınır (asla geri alınmaz).
    //
    // ÖNEMLİ: Bu metod her zaman çağıranın AÇIK OLAN transaction'ı içinde çalışır (GenerateAsync
    // yerine GenerateWithinTransactionAsync kullanır) — böylece numara üretimi, faturanın kendisi
    // kaydedilmeden önce herhangi bir sebeple (beklenmeyen hata, eşzamanlılık çakışması vb.) başarısız
    // olursa transaction'la birlikte GERİ ALINIR. Daha önce numara bağımsız commit edildiği için,
    // fatura kaydı başarısız olsa bile numara boşa harcanıyordu ("seri/sıra veriyor ama evrak yok"
    // hatası) — bu artık mümkün değil.
    private async Task<string?> ResolveInvoiceNumberWithinTransactionAsync(InvoiceFormViewModel form, string sequenceKey, int? excludeInvoiceId)
    {
        var series = form.DocumentSeries?.Trim();
        var sequence = form.DocumentSequence?.Trim();

        if (string.IsNullOrEmpty(series) || string.IsNullOrEmpty(sequence))
        {
            return await documentNumberGenerator.GenerateWithinTransactionAsync(sequenceKey);
        }

        var invoiceNumber = series + sequence;
        var isDuplicate = await dbContext.Invoices.AnyAsync(x =>
            x.InvoiceNumber == invoiceNumber && (excludeInvoiceId == null || x.Id != excludeInvoiceId));
        if (isDuplicate)
        {
            ModelState.AddModelError(nameof(form.DocumentSequence), "Bu belge numarası başka bir faturada kullanılıyor.");
            return null;
        }

        if (long.TryParse(sequence, out var parsedNumber))
        {
            await documentNumberGenerator.EnsureAtLeastForSeriesWithinTransactionAsync(sequenceKey, series, parsedNumber + 1);
        }

        return invoiceNumber;
    }

    // SQL Server hata kodları 2601/2627: unique index/constraint ihlali. Mesaj içinde index adını
    // (IX_Invoices_SubmissionKey) arayarak bunun spesifik olarak çift-gönderim koruması olduğunu,
    // başka bir unique index ihlali olmadığını doğruluyoruz.
    private static bool IsSubmissionKeyUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 } sqlEx &&
        sqlEx.Message.Contains("SubmissionKey", StringComparison.OrdinalIgnoreCase);

    private static bool IsInvoiceNumberUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 } sqlEx &&
        sqlEx.Message.Contains("IX_Invoices_InvoiceType_InvoiceNumber", StringComparison.OrdinalIgnoreCase);

    // Otomatik numaralandırılan bir fatura, üretilen numaranın ZATEN dolu olduğu bir çakışmayla
    // karşılaştığında (sayaç, gerçek veriden geride kalmışsa) çağrılır: ilgili tür + önek için
    // veritabanındaki en büyük mevcut numarayı bulup sayacı bunun bir fazlasına ileri alır — asla
    // geriye almaz, asla silinenlerin boşluğunu doldurmaya çalışmaz.
    private async Task HealSequenceDriftAsync(string sequenceKey, InvoiceType type)
    {
        var prefix = (await documentNumberGenerator.PeekAsync(sequenceKey)).Prefix;
        var existingNumbers = await dbContext.Invoices
            .Where(x => x.InvoiceType == type && x.InvoiceNumber.StartsWith(prefix))
            .Select(x => x.InvoiceNumber)
            .ToListAsync();

        var maxSuffix = 0L;
        foreach (var number in existingNumbers)
        {
            if (long.TryParse(number[prefix.Length..], out var parsed) && parsed > maxSuffix)
            {
                maxSuffix = parsed;
            }
        }

        await documentNumberGenerator.EnsureAtLeastForSeriesAsync(sequenceKey, prefix, maxSuffix + 1);
    }

    // "..." (Belge Sıra yanı) veya Seri alanında Enter'a basınca, o seriye özel önerilen sıra
    // numarasını döner (bkz. DocumentNumberGeneratorService.PeekForSeriesAsync) — Invoices/Form.cshtml.
    [HttpGet]
    public async Task<IActionResult> PeekSequence(InvoiceType type, string? series)
    {
        var sequenceKey = type == InvoiceType.Sales ? "SALES_INVOICE" : "PURCHASE_INVOICE";
        var peek = await documentNumberGenerator.PeekForSeriesAsync(sequenceKey, series?.Trim() ?? string.Empty);
        return Json(new { sequence = peek.NextNumber.ToString($"D{peek.Padding}") });
    }

    // Var olan bir faturanın numarasını (ör. "SF.00005") düzenleme formunda ayrı Seri/Sıra alanlarına
    // bölmek için — güncel seri önekiyle başlıyorsa oradan böler, başlamıyorsa (elle farklı bir seri
    // girilmiş olabilir) numaranın tamamını "Sıra" alanına koyar ki hiçbir bilgi kaybolmasın.
    private static (string Series, string Sequence) SplitInvoiceNumber(string invoiceNumber, string currentPrefix)
    {
        if (!string.IsNullOrEmpty(currentPrefix) && invoiceNumber.StartsWith(currentPrefix, StringComparison.Ordinal))
        {
            return (currentPrefix, invoiceNumber[currentPrefix.Length..]);
        }

        return (string.Empty, invoiceNumber);
    }

    private async Task PopulateSelectionsAsync(InvoiceFormViewModel model)
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

        if (model.SettlementFinancialAccountId is int settlementAccountId)
        {
            model.SettlementFinancialAccountDisplay = await dbContext.FinancialAccounts
                .Where(x => x.Id == settlementAccountId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        model.Salespeople = await userManager.Users
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new SelectListItem(x.FullName, x.Id))
            .ToListAsync();

        var productIds = model.Lines
            .Where(x => x.ProductId.HasValue)
            .Select(x => x.ProductId!.Value)
            .Distinct()
            .ToList();
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
