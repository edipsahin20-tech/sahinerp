using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Controllers;

// Evrak kayıt bilgisi (Ctrl+D) ve yönetici günlük listesi.
[Authorize]
public sealed class DocumentLogsController(ApplicationDbContext db) : Controller
{
    private static string ActionText(string a) => a switch
    {
        "Created" => "Kayıt oluşturuldu",
        "Updated" => "Değiştirildi",
        "Approved" => "Onaylandı",
        "Cancelled" => "İptal edildi",
        "Deleted" => "Kalıcı silindi",
        _ => a
    };

    private static string EntityText(string e) => e switch
    {
        "Invoice" => "Fatura", "PaymentReceipt" => "Tahsilat/Tediye", "DispatchNote" => "İrsaliye",
        "BusinessOrder" => "Sipariş", "Expense" => "Masraf", "Quote" => "Teklif", _ => e
    };

    private static string Local(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZoneInfo.Local).ToString("dd.MM.yyyy HH:mm:ss");

    // Ctrl+D penceresi: o evrakın tüm kayıt geçmişi (en yeni üstte) + ilk giren/son değiştiren özeti.
    [HttpGet]
    public async Task<IActionResult> Info(string entity, int id)
    {
        var logs = await db.DocumentLogs.AsNoTracking()
            .Where(x => x.EntityName == entity && x.EntityId == id)
            .OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
            .ToListAsync();
        var created = logs.LastOrDefault(x => x.Action == "Created");
        var last = logs.FirstOrDefault(x => x.Action is "Updated" or "Created");
        return Json(new
        {
            title = EntityText(entity),
            number = logs.Select(x => x.DocumentNumber).FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? string.Empty,
            createdBy = created is null ? null : created.UserName,
            createdAt = created is null ? null : Local(created.CreatedAtUtc),
            lastBy = last?.UserName,
            lastAt = last is null ? null : Local(last.CreatedAtUtc),
            entries = logs.Select(x => new { action = ActionText(x.Action), user = x.UserName, time = Local(x.CreatedAtUtc), details = x.Details, ip = x.IpAddress })
        });
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Index(string? entity, string? q, string? user, DateTime? from, DateTime? to, int page = 1)
    {
        var query = db.DocumentLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(entity)) { query = query.Where(x => x.EntityName == entity); }
        if (!string.IsNullOrWhiteSpace(q)) { query = query.Where(x => EF.Functions.Like(x.DocumentNumber, SahinSoft.Web.Services.SearchPattern.ToLike(q))); }
        if (!string.IsNullOrWhiteSpace(user)) { query = query.Where(x => EF.Functions.Like(x.UserName, SahinSoft.Web.Services.SearchPattern.ToLike(user))); }
        if (from is DateTime f) { query = query.Where(x => x.CreatedAtUtc >= f.Date.ToUniversalTime()); }
        if (to is DateTime t) { query = query.Where(x => x.CreatedAtUtc < t.Date.AddDays(1).ToUniversalTime()); }
        const int size = 100;
        var total = await query.CountAsync();
        var rows = await query.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id).Skip((Math.Max(page, 1) - 1) * size).Take(size).ToListAsync();
        ViewBag.Total = total; ViewBag.Page = Math.Max(page, 1); ViewBag.Size = size;
        ViewBag.Entity = entity; ViewBag.Q = q; ViewBag.User = user; ViewBag.From = from; ViewBag.To = to;
        ViewBag.ActionText = (Func<string, string>)ActionText; ViewBag.EntityText = (Func<string, string>)EntityText; ViewBag.Local = (Func<DateTime, string>)Local;
        return View(rows);
    }
}
