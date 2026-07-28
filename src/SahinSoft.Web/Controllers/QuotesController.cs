using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class QuotesController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator) : Controller
{
    public async Task<IActionResult> Index(QuoteStatus? status, string? search)
    {
        var query = dbContext.Quotes
            .AsNoTracking()
            .Include(x => x.Customer)
            .OrderByDescending(x => x.QuoteDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.QuoteNumber.Contains(search) || x.Customer.Name.Contains(search));
        }

        ViewBag.Status = status;
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Studio()
    {
        var settings = await dbContext.CompanySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        return View(new QuoteStudioViewModel
        {
            CompanyName = settings.CompanyName,
            Website = settings.Website,
            Email = settings.Email,
            Phone = settings.Phone,
            BankName = settings.BankName,
            Iban = settings.Iban
        });
    }

    public async Task<IActionResult> GetCatalogDataApi()
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Category)
            .Include(x => x.TaxRate)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.StockCode,
                dbId = x.Id,
                name = x.Name,
                category = x.Category.Name,
                unit = x.Unit,
                price = x.SalePrice,
                stock = x.StockQuantity,
                kdv = x.TaxRate.Rate
            })
            .ToListAsync();

        return Json(new { products });
    }

    public async Task<IActionResult> GetCustomersApi()
    {
        var customers = await dbContext.Customers
            .AsNoTracking()
            .Where(x => x.IsActive && x.IsCustomer)
            .Include(x => x.Contacts)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                code = x.Code,
                name = x.Name,
                company = x.Name,
                contact = x.Contacts.Where(c => c.IsPrimary).Select(c => c.FullName).FirstOrDefault()
                    ?? x.Contacts.Select(c => c.FullName).FirstOrDefault(),
                phone = x.Phone,
                email = x.Email,
                taxOffice = x.TaxOffice != null && x.TaxNumber != null
                    ? x.TaxOffice + " " + x.TaxNumber
                    : x.TaxOffice ?? x.TaxNumber,
                address = x.Address,
                debit = dbContext.CurrentAccountTransactions.Where(t => t.CustomerId == x.Id).Sum(t => (decimal?)t.Debit) ?? 0,
                credit = dbContext.CurrentAccountTransactions.Where(t => t.CustomerId == x.Id).Sum(t => (decimal?)t.Credit) ?? 0
            })
            .ToListAsync();

        return Json(customers);
    }

    public async Task<IActionResult> GetQuoteMetricsApi()
    {
        var totalCount = await dbContext.Quotes.CountAsync();
        var approvedCount = await dbContext.Quotes.CountAsync(x => x.Status == QuoteStatus.Approved || x.Status == QuoteStatus.Sent);
        var totalVolume = await dbContext.Quotes.SumAsync(x => (decimal?)x.GrandTotal) ?? 0;

        return Json(new { totalCount, approvedCount, totalVolume });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomProductApi([FromBody] CustomProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Json(new { success = false, message = "Ürün/hizmet adı zorunludur." });
        }

        var category = await dbContext.ProductCategories.FirstOrDefaultAsync(x => x.Code == "YAZILIM" && x.IsActive)
            ?? await dbContext.ProductCategories.OrderBy(x => x.Id).FirstOrDefaultAsync(x => x.IsActive);
        var taxRate = await dbContext.TaxRates.FirstOrDefaultAsync(x => x.Code == "KDV20" && x.IsActive)
            ?? await dbContext.TaxRates.OrderBy(x => x.Id).FirstOrDefaultAsync(x => x.IsActive);

        if (category is null || taxRate is null)
        {
            return Json(new { success = false, message = "Varsayılan kategori veya KDV oranı bulunamadı." });
        }

        var product = new Product
        {
            StockCode = await documentNumberGenerator.GenerateAsync("STOCK"),
            Name = request.Name.Trim(),
            CategoryId = category.Id,
            TaxRateId = taxRate.Id,
            ProductType = "Hizmet",
            Unit = "Adet",
            TrackStock = false,
            SalePrice = request.Price,
            PurchasePrice = 0,
            IsActive = true
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return Json(new
        {
            success = true,
            id = product.Id,
            code = product.StockCode,
            name = product.Name,
            salePrice = product.SalePrice,
            purchasePrice = product.PurchasePrice,
            taxRate = taxRate.Rate,
            unit = product.Unit
        });
    }

    [HttpPost]
    public async Task<IActionResult> SaveQuoteApi([FromBody] QuoteStudioSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Company) || request.Items.Count == 0)
        {
            return Json(new { success = false, message = "Firma unvanı ve en az bir kalem zorunludur." });
        }

        Customer? customer = request.CustomerId is int customerId
            ? await dbContext.Customers.SingleOrDefaultAsync(x => x.Id == customerId)
            : null;

        // Aynı ünvanla daha önce otomatik kaydedilmiş bir cari varsa, mükerrer kayıt açmak yerine onu kullan.
        var companyName = request.Company.Trim();
        customer ??= await dbContext.Customers
            .FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "Turkish_CI_AI") == companyName);

        if (customer is null)
        {
            customer = new Customer
            {
                Code = await documentNumberGenerator.GenerateAsync("CUSTOMER"),
                Name = request.Company.Trim(),
                Phone = request.Phone,
                Email = request.Email,
                Address = request.Address,
                TaxOffice = request.TaxOffice,
                IsCustomer = true,
                IsActive = true
            };
            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var quoteDateUtc = request.QuoteDate.HasValue
            ? DateTime.SpecifyKind(request.QuoteDate.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;
        var quote = new Quote
        {
            QuoteNumber = await documentNumberGenerator.GenerateAsync("QUOTE"),
            QuoteDateUtc = quoteDateUtc,
            ValidUntilUtc = quoteDateUtc.AddDays(15),
            Status = request.Status.Contains("Onay", StringComparison.OrdinalIgnoreCase) ? QuoteStatus.Sent : QuoteStatus.Draft,
            CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
            ExchangeRate = request.ExchangeRate,
            Notes = request.Notes?.Trim(),
            CreatedByUserId = userId,
            CustomerId = customer.Id
        };

        var lineNumber = 1;
        decimal subtotal = 0, discountTotal = 0, taxTotal = 0, grandTotal = 0;
        foreach (var item in request.Items)
        {
            var gross = RoundMoney(item.Qty * item.Price);
            var discAmount = RoundMoney(gross * item.Discount / 100);
            var net = gross - discAmount;
            var taxAmount = RoundMoney(net * item.Kdv / 100);
            var lineTotal = net + taxAmount;

            subtotal += gross;
            discountTotal += discAmount;
            taxTotal += taxAmount;
            grandTotal += lineTotal;

            quote.Lines.Add(new QuoteLine
            {
                LineNumber = lineNumber++,
                ProductId = item.DbId,
                ProductCodeSnapshot = item.Code ?? string.Empty,
                ProductNameSnapshot = item.Name,
                UnitSnapshot = item.Unit,
                Quantity = item.Qty,
                UnitPrice = item.Price,
                DiscountRate = item.Discount,
                DiscountAmount = discAmount,
                TaxRate = item.Kdv,
                TaxAmount = taxAmount,
                LineTotal = lineTotal
            });
        }

        quote.Subtotal = RoundMoney(subtotal);
        quote.DiscountTotal = RoundMoney(discountTotal);
        quote.TaxTotal = RoundMoney(taxTotal);
        quote.GrandTotal = RoundMoney(grandTotal);

        dbContext.Quotes.Add(quote);
        await dbContext.SaveChangesAsync();

        return Json(new { success = true, id = quote.Id, quoteNumber = quote.QuoteNumber });
    }

    public IActionResult Create()
    {
        return RedirectToAction(nameof(Studio));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuoteFormViewModel form)
    {
        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var quote = new Quote
        {
            Status = QuoteStatus.Draft,
            QuoteNumber = await documentNumberGenerator.GenerateAsync("QUOTE"),
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
        };
        MapHeader(form, quote);
        await MapLinesAsync(form, quote);
        ComputeTotals(quote);

        dbContext.Quotes.Add(quote);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Teklif taslağı oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = quote.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var quote = await dbContext.Quotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status != QuoteStatus.Draft)
        {
            return BadRequest("Yalnızca taslak teklifler düzenlenebilir.");
        }

        var model = new QuoteFormViewModel
        {
            Id = quote.Id,
            CustomerId = quote.CustomerId,
            QuoteDateUtc = quote.QuoteDateUtc,
            ValidUntilUtc = quote.ValidUntilUtc,
            CurrencyCode = quote.CurrencyCode,
            ExchangeRate = quote.ExchangeRate,
            Notes = quote.Notes,
            Lines = quote.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new QuoteLineFormViewModel
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    DiscountRate = x.DiscountRate,
                    TaxRate = x.TaxRate,
                    Description = x.Description
                })
                .ToList()
        };
        if (model.Lines.Count == 0)
        {
            model.Lines.Add(new QuoteLineFormViewModel());
        }

        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var quote = await dbContext.Quotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status != QuoteStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak teklifler silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.Quotes.Remove(quote);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Teklif silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.Quotes.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.Quotes.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "Quotes",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = true
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, QuoteFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var quote = await dbContext.Quotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status != QuoteStatus.Draft)
        {
            return BadRequest("Yalnızca taslak teklifler düzenlenebilir.");
        }

        MapHeader(form, quote);
        quote.Lines.Clear();
        await MapLinesAsync(form, quote);
        ComputeTotals(quote);
        quote.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Teklif taslağı güncellendi.";
        return RedirectToAction(nameof(Details), new { id = quote.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var quote = await dbContext.Quotes
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .Include(x => x.Invoices)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        var model = new QuoteDetailsViewModel
        {
            Id = quote.Id,
            Status = quote.Status,
            QuoteNumber = quote.QuoteNumber,
            QuoteDateUtc = quote.QuoteDateUtc,
            ValidUntilUtc = quote.ValidUntilUtc,
            CustomerName = quote.Customer.Name,
            CurrencyCode = quote.CurrencyCode,
            Subtotal = quote.Subtotal,
            DiscountTotal = quote.DiscountTotal,
            TaxTotal = quote.TaxTotal,
            GrandTotal = quote.GrandTotal,
            Notes = quote.Notes,
            HasConvertedInvoice = quote.Invoices.Count > 0,
            Lines = quote.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new QuoteDetailsLineViewModel
                {
                    ProductNameSnapshot = x.ProductNameSnapshot,
                    UnitSnapshot = x.UnitSnapshot,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    DiscountRate = x.DiscountRate,
                    TaxRate = x.TaxRate,
                    LineTotal = x.LineTotal
                })
                .ToList(),
            Warehouses = await dbContext.Warehouses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id)
    {
        var quote = await dbContext.Quotes.SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status != QuoteStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak teklifler gönderilebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        quote.Status = QuoteStatus.Sent;
        quote.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Teklif gönderildi olarak işaretlendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var quote = await dbContext.Quotes.SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status is not (QuoteStatus.Draft or QuoteStatus.Sent))
        {
            TempData["Error"] = "Bu teklif onaylanamaz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        quote.Status = QuoteStatus.Approved;
        quote.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Teklif onaylandı.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var quote = await dbContext.Quotes.SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status is not (QuoteStatus.Draft or QuoteStatus.Sent))
        {
            TempData["Error"] = "Bu teklif reddedilemez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        quote.Status = QuoteStatus.Rejected;
        quote.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Teklif reddedildi olarak işaretlendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToInvoice(int id, int warehouseId)
    {
        var quote = await dbContext.Quotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status != QuoteStatus.Approved)
        {
            TempData["Error"] = "Yalnızca onaylanmış teklifler faturaya dönüştürülebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var invoice = new Invoice
        {
            InvoiceType = InvoiceType.Sales,
            Status = InvoiceStatus.Draft,
            InvoiceNumber = await documentNumberGenerator.GenerateAsync("SALES_INVOICE"),
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            CustomerId = quote.CustomerId,
            WarehouseId = warehouseId,
            InvoiceDateUtc = DateTime.UtcNow,
            CurrencyCode = quote.CurrencyCode,
            ExchangeRate = quote.ExchangeRate,
            QuoteId = quote.Id,
            Notes = quote.Notes
        };

        var lineNumber = 1;
        foreach (var line in quote.Lines.OrderBy(x => x.LineNumber))
        {
            invoice.Lines.Add(new InvoiceLine
            {
                LineNumber = lineNumber++,
                ProductId = line.ProductId,
                ProductCodeSnapshot = line.ProductCodeSnapshot,
                ProductNameSnapshot = line.ProductNameSnapshot,
                UnitSnapshot = line.UnitSnapshot,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                DiscountRate = line.DiscountRate,
                TaxRate = line.TaxRate,
                Description = line.Description
            });
        }

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Teklif satış faturasına dönüştürüldü. Taslağı gözden geçirip onaylayabilirsiniz.";
        return RedirectToAction("Details", "Invoices", new { id = invoice.Id });
    }

    private static void ValidateLines(QuoteFormViewModel form)
    {
        form.Lines = form.Lines.Where(x => x.ProductId is not null).ToList();
        if (form.Lines.Count == 0)
        {
            throw new InvalidOperationException("Teklifte en az bir satır bulunmalıdır.");
        }
    }

    private static void MapHeader(QuoteFormViewModel source, Quote target)
    {
        target.CustomerId = source.CustomerId!.Value;
        target.QuoteDateUtc = DateTime.SpecifyKind(source.QuoteDateUtc, DateTimeKind.Utc);
        target.ValidUntilUtc = source.ValidUntilUtc.HasValue
            ? DateTime.SpecifyKind(source.ValidUntilUtc.Value, DateTimeKind.Utc)
            : null;
        target.CurrencyCode = source.CurrencyCode.Trim().ToUpperInvariant();
        target.ExchangeRate = source.ExchangeRate;
        target.Notes = source.Notes?.Trim();
    }

    private async Task MapLinesAsync(QuoteFormViewModel source, Quote target)
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

            var gross = RoundMoney(line.Quantity * line.UnitPrice);
            var discountAmount = RoundMoney(gross * line.DiscountRate / 100);
            var net = gross - discountAmount;
            var taxAmount = RoundMoney(net * line.TaxRate / 100);

            target.Lines.Add(new QuoteLine
            {
                LineNumber = lineNumber++,
                ProductId = product.Id,
                ProductCodeSnapshot = product.StockCode,
                ProductNameSnapshot = product.Name,
                UnitSnapshot = product.Unit,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                DiscountRate = line.DiscountRate,
                DiscountAmount = discountAmount,
                TaxRate = line.TaxRate,
                TaxAmount = taxAmount,
                LineTotal = net + taxAmount,
                Description = line.Description?.Trim()
            });
        }
    }

    private static void ComputeTotals(Quote quote)
    {
        decimal subtotal = 0, discountTotal = 0, taxTotal = 0, grandTotal = 0;
        foreach (var line in quote.Lines)
        {
            var gross = RoundMoney(line.Quantity * line.UnitPrice);
            subtotal += gross;
            discountTotal += line.DiscountAmount;
            taxTotal += line.TaxAmount;
            grandTotal += line.LineTotal;
        }

        quote.Subtotal = RoundMoney(subtotal);
        quote.DiscountTotal = RoundMoney(discountTotal);
        quote.TaxTotal = RoundMoney(taxTotal);
        quote.GrandTotal = RoundMoney(grandTotal);
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private async Task PopulateSelectionsAsync(QuoteFormViewModel model)
    {
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
