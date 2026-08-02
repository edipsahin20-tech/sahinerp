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
        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Controller = "PaymentReceipts",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() }
        };
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
            ViewBag.Toolbar = new EvrakToolbarViewModel
            {
                Controller = "PaymentReceipts",
                CreateRouteValues = new Dictionary<string, string> { ["type"] = form.ReceiptType.ToString() }
            };
            return View("Form", form);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sequenceKey = form.ReceiptType == ReceiptType.Collection ? "COLLECTION_RECEIPT" : "PAYMENT_RECEIPT";

        // Çift tıklama/mükerrer POST koruması ön kontrolü — bkz. PaymentReceipt.SubmissionKey.
        var existingBySubmission = await dbContext.PaymentReceipts
            .FirstOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
        if (existingBySubmission is not null)
        {
            TempData["Success"] = "Tahsilat/tediye taslağı zaten oluşturulmuştu.";
            return RedirectToAction(nameof(Details), new { id = existingBySubmission.Id });
        }

        PaymentReceipt receipt;
        try
        {
            receipt = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync();

                    var newReceipt = new PaymentReceipt
                    {
                        ReceiptType = form.ReceiptType,
                        Status = PaymentReceiptStatus.Draft,
                        ReceiptNumber = await documentNumberGenerator.GenerateWithinTransactionAsync(sequenceKey),
                        CreatedByUserId = userId,
                        SubmissionKey = form.SubmissionKey
                    };
                    MapHeader(form, newReceipt);
                    MapLines(form, newReceipt);

                    dbContext.PaymentReceipts.Add(newReceipt);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return newReceipt;
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
            var existing = await dbContext.PaymentReceipts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
            if (existing is not null)
            {
                TempData["Success"] = "Tahsilat/tediye taslağı zaten oluşturulmuştu.";
                return RedirectToAction(nameof(Details), new { id = existing.Id });
            }

            ModelState.AddModelError(string.Empty, "Kaydetme sırasında bir çakışma oluştu, lütfen tekrar deneyin.");
            await PopulateSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel
            {
                Controller = "PaymentReceipts",
                CreateRouteValues = new Dictionary<string, string> { ["type"] = form.ReceiptType.ToString() }
            };
            return View("Form", form);
        }

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
            ReceiptNumber = receipt.ReceiptNumber,
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
        await SetToolbarAsync(id, receipt.ReceiptType);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
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
            TempData["Error"] = "Yalnızca taslak fişler silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.PaymentReceipts.Remove(receipt);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Fiş silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id, ReceiptType type)
    {
        var previousId = await dbContext.PaymentReceipts.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.PaymentReceipts.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "PaymentReceipts",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() },
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = true
        };
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
            await SetToolbarAsync(id, form.ReceiptType);
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
                    PaymentMethod = x.PaymentMethod.GetDisplayName(),
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
            await paymentReceiptPostingService.CancelAsync(id, userId, reason);
            TempData["Success"] = "Fiş iptal edildi.";
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

    private void ValidateLines(PaymentReceiptFormViewModel form)
    {
        // Boş bırakılan satırlar hata değildir, sessizce göz ardı edilir — ama ASP.NET Core'un
        // otomatik model doğrulaması bu satırlar için eklemiş olabileceği ModelState hatalarını da
        // (ör. FinancialAccountId [Required]) temizlemek gerekir, yoksa satır çıkarılmış olsa bile
        // ModelState.IsValid false kalmaya devam eder.
        for (var i = 0; i < form.Lines.Count; i++)
        {
            if (form.Lines[i].FinancialAccountId is null || form.Lines[i].Amount <= 0)
            {
                foreach (var key in ModelState.Keys.Where(k => k.StartsWith($"{nameof(form.Lines)}[{i}].", StringComparison.Ordinal)).ToList())
                {
                    ModelState.Remove(key);
                }
            }
        }

        form.Lines = form.Lines
            .Where(x => x.FinancialAccountId is not null && x.Amount > 0)
            .ToList();

        if (form.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Fişte en az bir satır bulunmalıdır.");
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
        if (model.CustomerId is int customerId)
        {
            model.CustomerDisplay = await dbContext.Customers
                .Where(x => x.Id == customerId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        var accountIds = model.Lines.Where(x => x.FinancialAccountId.HasValue).Select(x => x.FinancialAccountId!.Value).Distinct().ToList();
        var accountDisplays = await dbContext.FinancialAccounts
            .Where(x => accountIds.Contains(x.Id))
            .Select(x => new { x.Id, Display = x.Code + " - " + x.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Display);

        foreach (var line in model.Lines)
        {
            if (line.FinancialAccountId is int accountId && accountDisplays.TryGetValue(accountId, out var display))
            {
                line.FinancialAccountDisplay = display;
            }
        }
    }
}
