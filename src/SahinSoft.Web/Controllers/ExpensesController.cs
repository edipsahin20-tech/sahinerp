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

        var expense = new Expense
        {
            DocumentNumber = await documentNumberGenerator.GenerateAsync("EXPENSE")
        };
        Map(form, expense);

        dbContext.Expenses.Add(expense);
        await dbContext.SaveChangesAsync();

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
