using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Perakende Fiş Listesi (Edip, 2026-09-28: "rapor ekranını muhasebe programına ekleyeceksin") -
// ANA ERP'de yaşar (RestaurantController'ın [Authorize(Roles=...)] kısıtı YOK, ReportsController
// ile AYNI genel erişim deseni) - restoran modülünün ürettiği RetailSale kayıtlarını (kapanışta
// CloseCheckAsync'in yazdığı GERÇEK muhasebe fişi) şube/tarih/kanal/ödeme tipine göre süzer.
[Authorize]
public sealed class RetailSalesController(ApplicationDbContext dbContext) : Controller
{
    private const string SelfSaleSectionName = "Self Satış";
    private const string PackageSectionName = "Paket";

    private async Task<IQueryable<RetailSale>> BuildFilteredQueryAsync(int? branchId, DateTime? dateFrom, DateTime? dateTo, string? channel, int? paymentMethod)
    {
        var query = dbContext.RetailSales
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable).ThenInclude(x => x.RestaurantSection)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(x => x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.BranchId == branchId.Value);
        }
        if (dateFrom.HasValue)
        {
            query = query.Where(x => x.IssuedAtUtc >= dateFrom.Value.Date.ToUniversalTime());
        }
        if (dateTo.HasValue)
        {
            var toExclusive = dateTo.Value.Date.AddDays(1).ToUniversalTime();
            query = query.Where(x => x.IssuedAtUtc < toExclusive);
        }
        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = channel switch
            {
                "SelfSatis" => query.Where(x => x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name == SelfSaleSectionName),
                "Paket" => query.Where(x => x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name == PackageSectionName),
                "Masa" => query.Where(x => x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name != SelfSaleSectionName
                    && x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name != PackageSectionName),
                _ => query
            };
        }
        if (paymentMethod.HasValue)
        {
            var method = (RestaurantPaymentMethod)paymentMethod.Value;
            query = query.Where(x => dbContext.RestaurantPayments.Any(p => p.RestaurantCheckId == x.RestaurantCheckId && !p.IsReversal && p.PaymentMethod == method));
        }

        return query.OrderByDescending(x => x.IssuedAtUtc);
    }

    private static string ResolveChannel(RestaurantSection section) => section.Name switch
    {
        SelfSaleSectionName => "Self Satış",
        PackageSectionName => "Paket",
        _ => "Masa"
    };

    public async Task<IActionResult> Index(int? branchId, DateTime? dateFrom, DateTime? dateTo, string? channel, int? paymentMethod, int page = 1)
    {
        const int pageSize = 50;
        var query = await BuildFilteredQueryAsync(branchId, dateFrom, dateTo, channel, paymentMethod);
        var totalCount = await query.CountAsync();

        var sales = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var checkIds = sales.Select(x => x.RestaurantCheckId).ToList();
        var paymentsByCheckId = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => checkIds.Contains(x.RestaurantCheckId) && !x.IsReversal)
            .GroupBy(x => x.RestaurantCheckId)
            .Select(g => new { CheckId = g.Key, Methods = g.Select(p => p.PaymentMethod).Distinct().ToList() })
            .ToListAsync();
        var paymentsLookup = paymentsByCheckId.ToDictionary(x => x.CheckId, x => x.Methods);

        var model = new RetailSaleListViewModel
        {
            Branches = await dbContext.Branches.AsNoTracking().OrderBy(x => x.Name).ToListAsync(),
            BranchId = branchId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Channel = channel,
            PaymentMethod = paymentMethod,
            Page = page,
            TotalCount = totalCount,
            PageSize = pageSize,
            Items = sales.Select(x =>
            {
                var section = x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection;
                var methods = paymentsLookup.GetValueOrDefault(x.RestaurantCheckId, []);
                return new RetailSaleListItemViewModel
                {
                    RetailSaleId = x.Id,
                    DocumentNumber = x.DocumentNumber,
                    IssuedAtUtc = x.IssuedAtUtc,
                    BranchName = "", // aşağıda toplu doldurulur
                    Channel = ResolveChannel(section),
                    SourceLabel = x.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name,
                    GrandTotal = x.GrandTotal,
                    PaymentMethodsSummary = methods.Count == 0
                        ? "-"
                        : string.Join(", ", methods.Select(m => m switch
                        {
                            RestaurantPaymentMethod.Cash => "Nakit",
                            RestaurantPaymentMethod.CreditCard => "Kredi Kartı",
                            RestaurantPaymentMethod.MealCard => "Yemek Kartı",
                            _ => m.ToString()
                        })),
                    Status = x.Status == RetailSaleStatus.Cancelled ? "İptal Edildi" : "Tamamlandı",
                    CustomerName = x.Customer != null ? x.Customer.Name : null
                };
            }).ToList()
        };

        // Şube adı ayrı doldurulur - branch bilgisi RestaurantSection.BranchId üzerinden ayrı bir
        // sorguya ihtiyaç duyuyor (yukarıdaki projeksiyon zaten RestaurantSection nesnesine kadar
        // Include ile geldi, burada tek seferde toplu eşleniyor).
        var branchNamesById = model.Branches.ToDictionary(x => x.Id, x => x.Name);
        var sectionBranchByCheckId = sales.ToDictionary(
            x => x.Id,
            x => branchNamesById.GetValueOrDefault(x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.BranchId, ""));
        foreach (var item in model.Items)
        {
            item.BranchName = sectionBranchByCheckId.GetValueOrDefault(item.RetailSaleId, "");
        }

        return View(model);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var sale = await dbContext.RetailSales
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable).ThenInclude(x => x.RestaurantSection).ThenInclude(x => x.Branch)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (sale is null)
        {
            return NotFound();
        }

        var section = sale.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection;
        var payments = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => x.RestaurantCheckId == sale.RestaurantCheckId && !x.IsReversal)
            .ToListAsync();

        var model = new RetailSaleDetailViewModel
        {
            RetailSaleId = sale.Id,
            DocumentNumber = sale.DocumentNumber,
            IssuedAtUtc = sale.IssuedAtUtc,
            BranchName = section.Branch.Name,
            BranchAddress = section.Branch.Address,
            BranchPhone = section.Branch.Phone,
            Channel = ResolveChannel(section),
            SourceLabel = sale.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name,
            CustomerName = sale.Customer?.Name,
            SubtotalAmount = sale.SubtotalAmount,
            DiscountAmount = sale.DiscountAmount,
            TaxAmount = sale.TaxAmount,
            GrandTotal = sale.GrandTotal,
            Status = sale.Status == RetailSaleStatus.Cancelled ? "İptal Edildi" : "Tamamlandı",
            Lines = sale.Lines.Select(l => new RetailSaleDetailLineViewModel
            {
                ProductName = l.ProductNameSnapshot,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPriceSnapshot,
                DiscountAmount = l.DiscountAmountSnapshot,
                TaxRate = l.TaxRateSnapshot,
                LineTotal = l.LineTotal
            }).ToList(),
            Payments = payments.Select(p => new RetailSaleDetailPaymentViewModel
            {
                Method = p.PaymentMethod switch
                {
                    RestaurantPaymentMethod.Cash => "Nakit",
                    RestaurantPaymentMethod.CreditCard => "Kredi Kartı",
                    RestaurantPaymentMethod.MealCard => "Yemek Kartı",
                    _ => p.PaymentMethod.ToString()
                },
                Amount = p.Amount
            }).ToList()
        };

        return View(model);
    }

    // Excel'e Aktar (Edip, 2026-09-28: "excel veya pdf aktarabileyim") - filtreye uyan TÜM
    // kayıtları (sayfalama olmadan) tek bir çalışma sayfasına yazar. PDF için ayrı bir kütüphane
    // EKLENMEDİ - Detail ekranı zaten yazdırılabilir bir fiş görünümü, tarayıcının kendi
    // "PDF olarak kaydet" yazdırma hedefiyle karşılanıyor (Receipt.cshtml'deki AYNI desen).
    public async Task<IActionResult> ExportExcel(int? branchId, DateTime? dateFrom, DateTime? dateTo, string? channel, int? paymentMethod)
    {
        var query = await BuildFilteredQueryAsync(branchId, dateFrom, dateTo, channel, paymentMethod);
        var sales = await query.ToListAsync();

        var checkIds = sales.Select(x => x.RestaurantCheckId).ToList();
        var paymentsByCheckId = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => checkIds.Contains(x.RestaurantCheckId) && !x.IsReversal)
            .GroupBy(x => x.RestaurantCheckId)
            .Select(g => new { CheckId = g.Key, Methods = g.Select(p => p.PaymentMethod).Distinct().ToList() })
            .ToListAsync();
        var paymentsLookup = paymentsByCheckId.ToDictionary(x => x.CheckId, x => x.Methods);
        var branchNamesById = await dbContext.Branches.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Perakende Fiş Listesi");
        var headers = new[] { "Fiş No", "Tarih", "Şube", "Kanal", "Masa/Kaynak", "Cari", "Ödeme Türü", "Tutar", "Durum" };
        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var sale in sales)
        {
            var section = sale.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection;
            var methods = paymentsLookup.GetValueOrDefault(sale.RestaurantCheckId, []);
            sheet.Cell(row, 1).Value = sale.DocumentNumber;
            sheet.Cell(row, 2).Value = sale.IssuedAtUtc.ToLocalTime();
            sheet.Cell(row, 2).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
            sheet.Cell(row, 3).Value = branchNamesById.GetValueOrDefault(section.BranchId, "");
            sheet.Cell(row, 4).Value = ResolveChannel(section);
            sheet.Cell(row, 5).Value = sale.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name;
            sheet.Cell(row, 6).Value = sale.Customer?.Name ?? "";
            sheet.Cell(row, 7).Value = methods.Count == 0 ? "-" : string.Join(", ", methods.Select(m => m switch
            {
                RestaurantPaymentMethod.Cash => "Nakit",
                RestaurantPaymentMethod.CreditCard => "Kredi Kartı",
                RestaurantPaymentMethod.MealCard => "Yemek Kartı",
                _ => m.ToString()
            }));
            sheet.Cell(row, 8).Value = sale.GrandTotal;
            sheet.Cell(row, 9).Value = sale.Status == RetailSaleStatus.Cancelled ? "İptal Edildi" : "Tamamlandı";
            row++;
        }
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"perakende-fis-listesi-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");
    }
}
