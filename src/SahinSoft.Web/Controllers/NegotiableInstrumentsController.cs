using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class NegotiableInstrumentsController(
    ApplicationDbContext dbContext,
    NegotiableInstrumentPostingService postingService) : Controller
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
            instrument = await postingService.CreateAsync(
                form.InstrumentType,
                form.Direction,
                form.CustomerId!.Value,
                DateTime.SpecifyKind(form.IssueDateUtc, DateTimeKind.Utc),
                DateTime.SpecifyKind(form.DueDateUtc, DateTimeKind.Utc),
                form.CurrencyCode.Trim().ToUpperInvariant(),
                form.Amount,
                form.BankName?.Trim(),
                form.BranchName?.Trim(),
                form.AccountNumber?.Trim(),
                form.DrawerName?.Trim(),
                form.Description?.Trim(),
                form.FinancialAccountId,
                userId,
                form.SubmissionKey);
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

    // Tutar/Cari/Yön, oluşturma anında atılan ilk cari netleştirme hareketiyle birlikte kalıcıdır —
    // yalnızca açıklayıcı alanlar (banka/şube/hesap no/keşideci/açıklama/tarihler) düzenlenebilir.
    // Bkz. NegotiableInstrumentPostingService üstündeki not.
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

    // Artık her kayıt oluşturulduğu anda kendi cari hareketini taşıdığından (bkz.
    // NegotiableInstrumentPostingService.CreateAsync) hard delete hiçbir zaman güvenli değildir —
    // bir hata varsa İptal Et kullanılmalı (ters kayıtla düzeltir, kaydı silmez).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        TempData["Error"] = "Çek/Senet kayıtları artık silinemez; hatalı bir kayıt için İptal Et kullanın.";
        return RedirectToAction(nameof(Details), new { id });
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
            CanDelete = false,
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

        // Tutar/Cari/Yön kasıtlı olarak buradan yazılmıyor — bkz. sınıf üstündeki not.
        instrument.IssueDateUtc = DateTime.SpecifyKind(form.IssueDateUtc, DateTimeKind.Utc);
        instrument.DueDateUtc = DateTime.SpecifyKind(form.DueDateUtc, DateTimeKind.Utc);
        instrument.BankName = form.BankName?.Trim();
        instrument.BranchName = form.BranchName?.Trim();
        instrument.AccountNumber = form.AccountNumber?.Trim();
        instrument.DrawerName = form.DrawerName?.Trim();
        instrument.Description = form.Description?.Trim();
        instrument.FinancialAccountId = form.FinancialAccountId;
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
            .Include(x => x.SettlementFinancialAccount)
            .Include(x => x.EndorsedToCustomer)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (instrument is null)
        {
            return NotFound();
        }

        return View(instrument);
    }

    // Tahsil Edildi (Alınan) / Ödendi (Verilen) — cariye dokunmaz, yalnızca seçilen kasa/bankaya
    // gerçek bir hareket ekler. Hedef durum enstrümanın kendi Direction'ına göre belirlenir.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settle(int id, int settlementFinancialAccountId)
    {
        try
        {
            await postingService.SettleAsync(id, settlementFinancialAccountId);
            TempData["Success"] = "Durum güncellendi.";
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

    // Ciro Edildi (yalnız Alınan) — devralan gerçek bir cari karttır.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Endorse(int id, int endorsedToCustomerId)
    {
        try
        {
            await postingService.EndorseAsync(id, endorsedToCustomerId);
            TempData["Success"] = "Durum güncellendi.";
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
    public async Task<IActionResult> Protest(int id)
    {
        try
        {
            await postingService.ProtestAsync(id);
            TempData["Success"] = "Durum güncellendi.";
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
    public async Task<IActionResult> Return(int id)
    {
        try
        {
            await postingService.ReturnAsync(id);
            TempData["Success"] = "Durum güncellendi.";
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

    // Yalnızca Administrator: terminal durumdaki (Tahsil Edildi/Ödendi dahil) hatalı bir kayıt da
    // iptal edilebilmeli — zaten İptal edilmiş bir kayıt hariç. Bkz.
    // NegotiableInstrumentPostingService.CancelAsync (o ana kadar oluşmuş tüm hareketleri geri alır).
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Cancel(int id, string reason)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        try
        {
            await postingService.CancelAsync(id, reason, userId);
            TempData["Success"] = "Kayıt iptal edildi.";
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
