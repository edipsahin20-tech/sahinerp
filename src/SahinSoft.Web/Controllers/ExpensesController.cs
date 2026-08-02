using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class ExpensesController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator) : Controller
{
    public async Task<IActionResult> Index(int? categoryId, string? search)
    {
        var query = dbContext.Expenses
            .AsNoTracking()
            .Include(x => x.ExpenseCategory)
            .OrderByDescending(x => x.ExpenseDateUtc)
            .ThenByDescending(x => x.Id)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.ExpenseCategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.DocumentNumber.Contains(search) || x.Description.Contains(search));
        }

        ViewBag.CategoryId = categoryId;
        ViewBag.Search = search;
        ViewBag.Categories = await dbContext.ExpenseCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();

        return View(await query.Take(500).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        var model = new ExpenseFormViewModel();
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "Expenses" };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "Expenses" };
            return View("Form", form);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // Çift tıklama/mükerrer POST koruması ön kontrolü — bkz. Expense.SubmissionKey.
        var existingBySubmission = await dbContext.Expenses
            .FirstOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
        if (existingBySubmission is not null)
        {
            TempData["Success"] = "Masraf zaten kaydedilmişti.";
            return RedirectToAction(nameof(Edit), new { id = existingBySubmission.Id });
        }

        Expense expense;
        try
        {
            expense = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync();

                    var newExpense = new Expense
                    {
                        DocumentNumber = await documentNumberGenerator.GenerateWithinTransactionAsync("EXPENSE"),
                        CreatedByUserId = userId,
                        SubmissionKey = form.SubmissionKey
                    };
                    Map(form, newExpense);

                    dbContext.Expenses.Add(newExpense);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return newExpense;
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
            var existing = await dbContext.Expenses.AsNoTracking()
                .SingleOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
            if (existing is not null)
            {
                TempData["Success"] = "Masraf zaten kaydedilmişti.";
                return RedirectToAction(nameof(Edit), new { id = existing.Id });
            }

            ModelState.AddModelError(string.Empty, "Kaydetme sırasında bir çakışma oluştu, lütfen tekrar deneyin.");
            await PopulateSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "Expenses" };
            return View("Form", form);
        }

        TempData["Success"] = "Masraf kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var expense = await dbContext.Expenses.SingleOrDefaultAsync(x => x.Id == id);
        if (expense is null)
        {
            return NotFound();
        }

        var model = new ExpenseFormViewModel
        {
            Id = expense.Id,
            DocumentNumber = expense.DocumentNumber,
            ExpenseCategoryId = expense.ExpenseCategoryId,
            ExpenseDateUtc = expense.ExpenseDateUtc,
            Description = expense.Description,
            CurrencyCode = expense.CurrencyCode,
            ExchangeRate = expense.ExchangeRate,
            NetAmount = expense.NetAmount,
            TaxAmount = expense.TaxAmount,
            CustomerId = expense.CustomerId,
            TaxRateId = expense.TaxRateId,
            FinancialAccountId = expense.FinancialAccountId
        };
        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var expense = await dbContext.Expenses.SingleOrDefaultAsync(x => x.Id == id);
        if (expense is null)
        {
            return NotFound();
        }

        dbContext.Expenses.Remove(expense);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Masraf silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.Expenses.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.Expenses.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "Expenses",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = false
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExpenseFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            await SetToolbarAsync(id);
            return View("Form", form);
        }

        var expense = await dbContext.Expenses.SingleOrDefaultAsync(x => x.Id == id);
        if (expense is null)
        {
            return NotFound();
        }

        Map(form, expense);
        expense.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Masraf güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    private static void Map(ExpenseFormViewModel source, Expense target)
    {
        target.ExpenseCategoryId = source.ExpenseCategoryId!.Value;
        target.ExpenseDateUtc = DateTime.SpecifyKind(source.ExpenseDateUtc, DateTimeKind.Utc);
        target.Description = source.Description.Trim();
        target.CurrencyCode = source.CurrencyCode.Trim().ToUpperInvariant();
        target.ExchangeRate = source.ExchangeRate;
        target.NetAmount = Math.Round(source.NetAmount, 2, MidpointRounding.AwayFromZero);
        target.TaxAmount = Math.Round(source.TaxAmount, 2, MidpointRounding.AwayFromZero);
        target.TotalAmount = target.NetAmount + target.TaxAmount;
        target.CustomerId = source.CustomerId;
        target.TaxRateId = source.TaxRateId;
        target.FinancialAccountId = source.FinancialAccountId;
    }

    private async Task PopulateSelectionsAsync(ExpenseFormViewModel model)
    {
        if (model.ExpenseCategoryId is int categoryId)
        {
            model.ExpenseCategoryDisplay = await dbContext.ExpenseCategories
                .Where(x => x.Id == categoryId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        if (model.CustomerId is int customerId)
        {
            model.CustomerDisplay = await dbContext.Customers
                .Where(x => x.Id == customerId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        if (model.TaxRateId is int taxRateId)
        {
            model.TaxRateDisplay = await dbContext.TaxRates
                .Where(x => x.Id == taxRateId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        if (model.FinancialAccountId is int financialAccountId)
        {
            model.FinancialAccountDisplay = await dbContext.FinancialAccounts
                .Where(x => x.Id == financialAccountId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }
    }
}
