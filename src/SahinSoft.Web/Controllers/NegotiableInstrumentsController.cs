using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class NegotiableInstrumentsController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator) : Controller
{
    public async Task<IActionResult> Index(NegotiableInstrumentType? type, InstrumentStatus? status, string? search)
    {
        var query = dbContext.NegotiableInstruments
            .AsNoTracking()
            .Include(x => x.Customer)
            .OrderBy(x => x.DueDateUtc)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(x => x.InstrumentType == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.InstrumentNumber.Contains(search) || x.Customer.Name.Contains(search));
        }

        ViewBag.Type = type;
        ViewBag.Status = status;
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create(NegotiableInstrumentType type)
    {
        var model = new NegotiableInstrumentFormViewModel { InstrumentType = type };
        await PopulateSelectionsAsync(model);
        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Controller = "NegotiableInstruments",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() }
        };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NegotiableInstrumentFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var instrument = new NegotiableInstrument
        {
            InstrumentType = form.InstrumentType,
            Status = InstrumentStatus.Portfolio,
            InstrumentNumber = await documentNumberGenerator.GenerateAsync(
                form.InstrumentType == NegotiableInstrumentType.Cheque ? "NEGOTIABLE_CHEQUE" : "NEGOTIABLE_NOTE")
        };
        Map(form, instrument);

        dbContext.NegotiableInstruments.Add(instrument);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Kayıt oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = instrument.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var instrument = await dbContext.NegotiableInstruments.SingleOrDefaultAsync(x => x.Id == id);
        if (instrument is null)
        {
            return NotFound();
        }

        if (instrument.Status != InstrumentStatus.Portfolio)
        {
            return BadRequest("Yalnızca portföydeki kayıtlar düzenlenebilir.");
        }

        var model = new NegotiableInstrumentFormViewModel
        {
            Id = instrument.Id,
            InstrumentType = instrument.InstrumentType,
            Direction = instrument.Direction,
            CustomerId = instrument.CustomerId,
            IssueDateUtc = instrument.IssueDateUtc,
            DueDateUtc = instrument.DueDateUtc,
            CurrencyCode = instrument.CurrencyCode,
            Amount = instrument.Amount,
            BankName = instrument.BankName,
            BranchName = instrument.BranchName,
            AccountNumber = instrument.AccountNumber,
            DrawerName = instrument.DrawerName,
            Description = instrument.Description,
            FinancialAccountId = instrument.FinancialAccountId
        };
        await PopulateSelectionsAsync(model);
        await SetToolbarAsync(id, instrument.InstrumentType);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var instrument = await dbContext.NegotiableInstruments.SingleOrDefaultAsync(x => x.Id == id);
        if (instrument is null)
        {
            return NotFound();
        }

        if (instrument.Status != InstrumentStatus.Portfolio)
        {
            TempData["Error"] = "Yalnızca portföydeki kayıtlar silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.NegotiableInstruments.Remove(instrument);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Kayıt silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id, NegotiableInstrumentType type)
    {
        var previousId = await dbContext.NegotiableInstruments.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.NegotiableInstruments.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "NegotiableInstruments",
            CreateRouteValues = new Dictionary<string, string> { ["type"] = type.ToString() },
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = true,
            HasDetails = true
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NegotiableInstrumentFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var instrument = await dbContext.NegotiableInstruments.SingleOrDefaultAsync(x => x.Id == id);
        if (instrument is null)
        {
            return NotFound();
        }

        if (instrument.Status != InstrumentStatus.Portfolio)
        {
            return BadRequest("Yalnızca portföydeki kayıtlar düzenlenebilir.");
        }

        Map(form, instrument);
        instrument.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Kayıt güncellendi.";
        return RedirectToAction(nameof(Details), new { id = instrument.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var instrument = await dbContext.NegotiableInstruments
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.FinancialAccount)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (instrument is null)
        {
            return NotFound();
        }

        return View(instrument);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, InstrumentStatus status)
    {
        var instrument = await dbContext.NegotiableInstruments.SingleOrDefaultAsync(x => x.Id == id);
        if (instrument is null)
        {
            return NotFound();
        }

        if (instrument.Status is InstrumentStatus.Collected or InstrumentStatus.Paid or InstrumentStatus.Cancelled)
        {
            TempData["Error"] = "Bu kayıt artık durum değiştiremez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        instrument.Status = status;
        instrument.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Durum güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private static void Map(NegotiableInstrumentFormViewModel source, NegotiableInstrument target)
    {
        target.Direction = source.Direction;
        target.CustomerId = source.CustomerId!.Value;
        target.IssueDateUtc = DateTime.SpecifyKind(source.IssueDateUtc, DateTimeKind.Utc);
        target.DueDateUtc = DateTime.SpecifyKind(source.DueDateUtc, DateTimeKind.Utc);
        target.CurrencyCode = source.CurrencyCode.Trim().ToUpperInvariant();
        target.Amount = source.Amount;
        target.BankName = source.BankName?.Trim();
        target.BranchName = source.BranchName?.Trim();
        target.AccountNumber = source.AccountNumber?.Trim();
        target.DrawerName = source.DrawerName?.Trim();
        target.Description = source.Description?.Trim();
        target.FinancialAccountId = source.FinancialAccountId;
    }

    private async Task PopulateSelectionsAsync(NegotiableInstrumentFormViewModel model)
    {
        if (model.CustomerId is int customerId)
        {
            model.CustomerDisplay = await dbContext.Customers
                .Where(x => x.Id == customerId)
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
