using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(
        InvoiceType? type,
        InvoiceStatus? status,
        int? customerId,
        string? search)
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

        ViewBag.Type = type;
        ViewBag.Status = status;
        ViewBag.CustomerId = customerId;
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create(InvoiceType type)
    {
        var model = new InvoiceFormViewModel
        {
            InvoiceType = type,
            Lines = [new InvoiceLineFormViewModel()]
        };
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Controller = "Invoices",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() }
        };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceFormViewModel form)
    {
        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var invoice = new Invoice
        {
            InvoiceType = form.InvoiceType,
            Status = InvoiceStatus.Draft,
            InvoiceNumber = await documentNumberGenerator.GenerateAsync(
                form.InvoiceType == InvoiceType.Sales ? "SALES_INVOICE" : "PURCHASE_INVOICE"),
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
        };
        MapHeader(form, invoice);
        await MapLinesAsync(form, invoice);

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Fatura taslağı oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var invoice = await dbContext.Invoices
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            return BadRequest("Yalnızca taslak faturalar düzenlenebilir.");
        }

        var model = new InvoiceFormViewModel
        {
            Id = invoice.Id,
            InvoiceType = invoice.InvoiceType,
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
            SettlementFinancialAccountId = invoice.SettlementFinancialAccountId,
            Lines = invoice.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new InvoiceLineFormViewModel
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

        if (invoice.Status != InvoiceStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak faturalar silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.Invoices.Remove(invoice);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Fatura silindi.";
        return RedirectToAction(nameof(Index));
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
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var invoice = await dbContext.Invoices
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            return BadRequest("Yalnızca taslak faturalar düzenlenebilir.");
        }

        MapHeader(form, invoice);
        invoice.Lines.Clear();
        await MapLinesAsync(form, invoice);
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Fatura taslağı güncellendi.";
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Warehouse)
            .Include(x => x.Lines)
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
            SettlementFinancialAccountName = settlementAccountName,
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
                    TaxRate = x.TaxRate,
                    LineTotal = x.LineTotal
                })
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
                .ToList()
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

        return RedirectToAction(nameof(Details), new { id });
    }

    private static void ValidateLines(InvoiceFormViewModel form)
    {
        form.Lines = form.Lines
            .Where(x => x.ProductId is not null)
            .ToList();

        if (form.Lines.Count == 0)
        {
            throw new InvalidOperationException("Faturada en az bir satır bulunmalıdır.");
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
        target.SettlementFinancialAccountId = source.SettlementFinancialAccountId;
    }

    private async Task MapLinesAsync(InvoiceFormViewModel source, Invoice target)
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
                UnitPrice = line.UnitPrice,
                DiscountRate = line.DiscountRate,
                TaxRate = line.TaxRate,
                Description = line.Description?.Trim()
            });
        }

        CalculateDraftTotals(target);
    }

    // Onaydan önce (taslak haldeyken) satır ve fatura toplamlarının Detay ekranında 0.00 görünmemesi
    // için, InvoicePostingService.ApproveAsync'teki para hesabıyla aynı formül burada da uygulanır
    // (stok hareketi/cari kaydı gibi onaya özel işlemler olmadan, sadece tutar hesabı).
    private static void CalculateDraftTotals(Invoice invoice)
    {
        decimal subtotal = 0, discountTotal = 0, taxTotal = 0, grandTotal = 0;
        foreach (var line in invoice.Lines.OrderBy(x => x.LineNumber))
        {
            var gross = RoundMoney(line.Quantity * line.UnitPrice);
            line.DiscountAmount = RoundMoney(gross * line.DiscountRate / 100);
            var net = gross - line.DiscountAmount;
            line.TaxAmount = RoundMoney(net * line.TaxRate / 100);
            line.LineTotal = net + line.TaxAmount;

            subtotal += gross;
            discountTotal += line.DiscountAmount;
            taxTotal += line.TaxAmount;
            grandTotal += line.LineTotal;
        }

        invoice.Subtotal = RoundMoney(subtotal);
        invoice.DiscountTotal = RoundMoney(discountTotal);
        invoice.TaxTotal = RoundMoney(taxTotal);
        invoice.GrandTotal = RoundMoney(grandTotal);
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

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
