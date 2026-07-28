using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class NumberSequencesController(ApplicationDbContext dbContext) : Controller
{
    public static readonly Dictionary<string, string> KeyLabels = new()
    {
        ["STOCK"] = "Stok Kodu",
        ["SALES_INVOICE"] = "Satış Faturası",
        ["PURCHASE_INVOICE"] = "Alış Faturası",
        ["COLLECTION_RECEIPT"] = "Tahsilat Makbuzu",
        ["PAYMENT_RECEIPT"] = "Tediye Makbuzu",
        ["STOCK_RECEIPT"] = "Stok Giriş Fişi",
        ["STOCK_ISSUE"] = "Stok Çıkış Fişi",
        ["STOCK_COUNT"] = "Sayım Fişi",
        ["SALES_DISPATCH"] = "Satış İrsaliyesi",
        ["PURCHASE_DISPATCH"] = "Alış İrsaliyesi",
        ["STOCK_TRANSFER"] = "Depo Transferi",
        ["EXPENSE"] = "Masraf",
        ["NEGOTIABLE_CHEQUE"] = "Çek",
        ["NEGOTIABLE_NOTE"] = "Senet",
        ["SALES_ORDER"] = "Satış Siparişi",
        ["PURCHASE_ORDER"] = "Alış Siparişi",
        ["QUOTE"] = "Teklif",
        ["PERSONNEL"] = "Personel Kodu",
        ["CUSTOMER"] = "Cari Kodu",
        ["FINANCIAL_ACCOUNT_CASH"] = "Kasa Kodu",
        ["FINANCIAL_ACCOUNT_BANK"] = "Banka Kodu"
    };

    public async Task<IActionResult> Index()
    {
        var sequences = await dbContext.NumberSequences.AsNoTracking().OrderBy(x => x.Key).ToListAsync();
        return View(sequences);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var sequence = await dbContext.NumberSequences.SingleOrDefaultAsync(x => x.Id == id);
        if (sequence is null)
        {
            return NotFound();
        }

        return View("Form", new NumberSequenceFormViewModel
        {
            Id = sequence.Id,
            Key = sequence.Key,
            Prefix = sequence.Prefix,
            NextNumber = sequence.NextNumber,
            Padding = sequence.Padding
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NumberSequenceFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var sequence = await dbContext.NumberSequences.SingleOrDefaultAsync(x => x.Id == id);
        if (sequence is null)
        {
            return NotFound();
        }

        sequence.Prefix = form.Prefix.Trim();
        sequence.NextNumber = form.NextNumber;
        sequence.Padding = form.Padding;
        sequence.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Numaralandırma parametresi güncellendi.";
        return RedirectToAction(nameof(Edit), new { id = sequence.Id });
    }
}
