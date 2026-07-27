using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
public sealed class PaymentReceiptsController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator,
    PaymentReceiptPostingService paymentReceiptPostingService) : Controller
{
    public async Task<IActionResult> Index(
        ReceiptType? type,
        PaymentReceiptStatus? status,
        int? customerId,
        string? search)
    {
        var query = dbContext.PaymentReceipts
            .AsNoTracking()
            .Include(x => x.Customer)
            .OrderByDescending(x => x.ReceiptDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(x => x.ReceiptType == type.Value);
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
                x.ReceiptNumber.Contains(search) ||
                x.Customer.Name.Contains(search));
        }

        ViewBag.Type = type;
        ViewBag.Status = status;
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create(ReceiptType type)
    {
        var model = new PaymentReceiptFormViewModel
        {
            ReceiptType = type,
            Lines = [new PaymentReceiptLineFormViewModel()]
        };
        await PopulateSelectionsAsync(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentReceiptFormViewModel form)
    {
        ValidateLines(form);
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var receipt = new PaymentReceipt
        {
            ReceiptType = form.ReceiptType,
            Status = PaymentReceiptStatus.Draft,
            ReceiptNumber = await documentNumberGenerator.GenerateAsync(
                form.ReceiptType == ReceiptType.Collection ? "COLLECTION_RECEIPT" : "PAYMENT_RECEIPT"),
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
        };
        MapHeader(form, receipt);
        MapLines(form, receipt);

        dbContext.PaymentReceipts.Add(receipt);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Tahsilat/tediye taslağı oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = receipt.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var receipt = await dbContext.PaymentReceipts
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (receipt is null)
        {
            return NotFound();
        }

        if (receipt.Status != PaymentReceiptStatus.Draft)
        {
            return BadRequest("Yalnızca taslak fişler düzenlenebilir.");
        }

        var model = new PaymentReceiptFormViewModel
        {
            Id = receipt.Id,
            ReceiptType = receipt.ReceiptType,
            CustomerId = receipt.CustomerId,
            ReceiptDateUtc = receipt.ReceiptDateUtc,
            CurrencyCode = receipt.CurrencyCode,
            ExchangeRate = receipt.ExchangeRate,
            Description = receipt.Description,
            Lines = receipt.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new PaymentReceiptLineFormViewModel
                {
                    PaymentMethod = x.PaymentMethod,
                    ReferenceNumber = x.ReferenceNumber,
                    DueDateUtc = x.DueDateUtc,
                    Amount = x.Amount,
                    Description = x.Description,
                    FinancialAccountId = x.FinancialAccountId
                })
                .ToList()
        };
        if (model.Lines.Count == 0)
        {
            model.Lines.Add(new PaymentReceiptLineFormViewModel());
        }

        await PopulateSelectionsAsync(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PaymentReceiptFormViewModel form)
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

        var receipt = await dbContext.PaymentReceipts
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (receipt is null)
        {
            return NotFound();
        }

        if (receipt.Status != PaymentReceiptStatus.Draft)
        {
            return BadRequest("Yalnızca taslak fişler düzenlenebilir.");
        }

        MapHeader(form, receipt);
        receipt.Lines.Clear();
        MapLines(form, receipt);
        receipt.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Tahsilat/tediye taslağı güncellendi.";
        return RedirectToAction(nameof(Details), new { id = receipt.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var receipt = await dbContext.PaymentReceipts
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .ThenInclude(x => x.FinancialAccount)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (receipt is null)
        {
            return NotFound();
        }

        var model = new PaymentReceiptDetailsViewModel
        {
            Id = receipt.Id,
            ReceiptType = receipt.ReceiptType,
            Status = receipt.Status,
            ReceiptNumber = receipt.ReceiptNumber,
            ReceiptDateUtc = receipt.ReceiptDateUtc,
            CustomerName = receipt.Customer.Name,
            CurrencyCode = receipt.CurrencyCode,
            TotalAmount = receipt.TotalAmount,
            Description = receipt.Description,
            ApprovedByUserId = receipt.ApprovedByUserId,
            ApprovedAtUtc = receipt.ApprovedAtUtc,
            CancelledByUserId = receipt.CancelledByUserId,
            CancelledAtUtc = receipt.CancelledAtUtc,
            CancellationReason = receipt.CancellationReason,
            Lines = receipt.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new PaymentReceiptDetailsLineViewModel
                {
                    LineNumber = x.LineNumber,
                    PaymentMethod = x.PaymentMethod.ToString(),
                    ReferenceNumber = x.ReferenceNumber,
                    Amount = x.Amount,
                    FinancialAccountName = x.FinancialAccount.Name,
                    Description = x.Description
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
            await paymentReceiptPostingService.ApproveAsync(id, userId);
            TempData["Success"] = "Fiş onaylandı.";
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
            await paymentReceiptPostingService.CancelAsync(id, userId, reason);
            TempData["Success"] = "Fiş iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private static void ValidateLines(PaymentReceiptFormViewModel form)
    {
        form.Lines = form.Lines
            .Where(x => x.FinancialAccountId is not null && x.Amount > 0)
            .ToList();

        if (form.Lines.Count == 0)
        {
            throw new InvalidOperationException("Fişte en az bir satır bulunmalıdır.");
        }
    }

    private static void MapHeader(PaymentReceiptFormViewModel source, PaymentReceipt target)
    {
        target.CustomerId = source.CustomerId!.Value;
        target.ReceiptDateUtc = DateTime.SpecifyKind(source.ReceiptDateUtc, DateTimeKind.Utc);
        target.CurrencyCode = source.CurrencyCode.Trim().ToUpperInvariant();
        target.ExchangeRate = source.ExchangeRate;
        target.Description = source.Description?.Trim();
    }

    private static void MapLines(PaymentReceiptFormViewModel source, PaymentReceipt target)
    {
        var lineNumber = 1;
        foreach (var line in source.Lines)
        {
            target.Lines.Add(new PaymentReceiptLine
            {
                LineNumber = lineNumber++,
                PaymentMethod = line.PaymentMethod,
                ReferenceNumber = line.ReferenceNumber?.Trim(),
                DueDateUtc = line.DueDateUtc.HasValue
                    ? DateTime.SpecifyKind(line.DueDateUtc.Value, DateTimeKind.Utc)
                    : null,
                Amount = line.Amount,
                Description = line.Description?.Trim(),
                FinancialAccountId = line.FinancialAccountId!.Value
            });
        }
    }

    private async Task PopulateSelectionsAsync(PaymentReceiptFormViewModel model)
    {
        model.Customers = await dbContext.Customers
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem($"{x.Code} - {x.Name}", x.Id.ToString()))
            .ToListAsync();

        model.FinancialAccounts = await dbContext.FinancialAccounts
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();
    }
}
