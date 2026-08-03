using System.Security.Claims;
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
            ViewBag.Toolbar = new EvrakToolbarViewModel
            {
                Controller = "NegotiableInstruments",
                CreateRouteValues = new Dictionary<string, string> { ["type"] = form.InstrumentType.ToString() }
            };
            return View("Form", form);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sequenceKey = form.InstrumentType == NegotiableInstrumentType.Cheque ? "NEGOTIABLE_CHEQUE" : "NEGOTIABLE_NOTE";

        // Çift tıklama/mükerrer POST koruması ön kontrolü — bkz. NegotiableInstrument.SubmissionKey.
        var existingBySubmission = await dbContext.NegotiableInstruments
            .FirstOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
        if (existingBySubmission is not null)
        {
            TempData["Success"] = "Kayıt zaten oluşturulmuştu.";
            return RedirectToAction(nameof(Details), new { id = existingBySubmission.Id });
        }

        NegotiableInstrument instrument;
        try
        {
            instrument = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync();

                    var newInstrument = new NegotiableInstrument
                    {
                        InstrumentType = form.InstrumentType,
                        Status = InstrumentStatus.Portfolio,
                        InstrumentNumber = await documentNumberGenerator.GenerateWithinTransactionAsync(sequenceKey),
                        CreatedByUserId = userId,
                        SubmissionKey = form.SubmissionKey
                    };
                    Map(form, newInstrument);

                    dbContext.NegotiableInstruments.Add(newInstrument);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return newInstrument;
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
            var existing = await dbContext.NegotiableInstruments.AsNoTracking()
                .SingleOrDefaultAsync(x => x.CreatedByUserId == userId && x.SubmissionKey == form.SubmissionKey);
            if (existing is not null)
            {
                TempData["Success"] = "Kayıt zaten oluşturulmuştu.";
                return RedirectToAction(nameof(Details), new { id = existing.Id });
            }

            ModelState.AddModelError(string.Empty, "Kaydetme sırasında bir çakışma oluştu, lütfen tekrar deneyin.");
            await PopulateSelectionsAsync(form);
            ViewBag.Toolbar = new EvrakToolbarViewModel
            {
                Controller = "NegotiableInstruments",
                CreateRouteValues = new Dictionary<string, string> { ["type"] = form.InstrumentType.ToString() }
            };
            return View("Form", form);
        }

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
            InstrumentNumber = instrument.InstrumentNumber,
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
            await SetToolbarAsync(id, form.InstrumentType);
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
        // Diğer evrak türlerindeki Approve/Cancel gibi, RowVersion çakışmasında (iki kullanıcının aynı
        // kaydı aynı anda durum değiştirmesi) ham DbUpdateConcurrencyException yerine taze bir okuma ile
        // otomatik tekrar dener — bkz. DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync.
        var result = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, async () =>
        {
            var instrument = await dbContext.NegotiableInstruments.SingleOrDefaultAsync(x => x.Id == id);
            if (instrument is null)
            {
                return "not-found";
            }

            if (instrument.Status is InstrumentStatus.Collected or InstrumentStatus.Paid or InstrumentStatus.Cancelled)
            {
                return "blocked";
            }

            instrument.Status = status;
            instrument.UpdatedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            return "ok";
        });

        if (result == "not-found")
        {
            return NotFound();
        }

        if (result == "blocked")
        {
            TempData["Error"] = "Bu kayıt artık durum değiştiremez.";
            return RedirectToAction(nameof(Details), new { id });
        }

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
