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
    private async Task<IQueryable<RetailSale>> BuildFilteredQueryAsync(int? branchId, DateTime? dateFrom, DateTime? dateTo, string? channel, int? paymentMethod)
    {
        var query = dbContext.RetailSales
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x!.RestaurantTable)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(x => x.RestaurantCheck.RestaurantTableSession.BranchId == branchId.Value);
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
                "SelfSatis" => query.Where(x => x.RestaurantCheck.RestaurantTableSession.Channel == RestaurantSaleChannel.SelfSatis),
                "Paket" => query.Where(x => x.RestaurantCheck.RestaurantTableSession.Channel == RestaurantSaleChannel.Paket),
                "Masa" => query.Where(x => x.RestaurantCheck.RestaurantTableSession.Channel == RestaurantSaleChannel.Masa),
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

    private static string ResolveChannel(RestaurantSaleChannel channel) => channel switch
    {
        RestaurantSaleChannel.SelfSatis => "Self Satış",
        RestaurantSaleChannel.Paket => "Paket",
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
        var paymentAmountsByCheckId = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => checkIds.Contains(x.RestaurantCheckId) && !x.IsReversal)
            .GroupBy(x => new { x.RestaurantCheckId, x.PaymentMethod })
            .Select(g => new { g.Key.RestaurantCheckId, g.Key.PaymentMethod, Amount = g.Sum(p => p.Amount) })
            .ToListAsync();
        var paymentsLookup = paymentAmountsByCheckId
            .GroupBy(x => x.RestaurantCheckId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.PaymentMethod, x => x.Amount));

        // Filtreye uyan TÜM kayıtların (sayfalama olmadan) ödeme türü bazlı toplamları - ayrı,
        // hafif bir sorgu (yalnızca check id + ödeme türü + tutar, satır bazlı entity yüklenmez).
        var filteredCheckIdsQuery = query.Select(x => x.RestaurantCheckId);
        var totalsByMethod = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(p => !p.IsReversal && filteredCheckIdsQuery.Contains(p.RestaurantCheckId))
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.Method, x => x.Total);
        var discountTotal = await query.SumAsync(x => x.DiscountAmount);
        var grandTotalAll = await query.SumAsync(x => x.GrandTotal);
        var packageNumbersByCheckId = await dbContext.PackageOrders
            .AsNoTracking()
            .Where(x => checkIds.Contains(x.RestaurantCheckId))
            .ToDictionaryAsync(x => x.RestaurantCheckId, x => x.PackageNumber);

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
            Totals = new RetailSaleListTotalsViewModel
            {
                CashTotal = totalsByMethod.GetValueOrDefault(RestaurantPaymentMethod.Cash),
                CreditCardTotal = totalsByMethod.GetValueOrDefault(RestaurantPaymentMethod.CreditCard),
                MealCardTotal = totalsByMethod.GetValueOrDefault(RestaurantPaymentMethod.MealCard),
                UnpaidTotal = totalsByMethod.GetValueOrDefault(RestaurantPaymentMethod.Unpaid),
                OpenAccountTotal = totalsByMethod.GetValueOrDefault(RestaurantPaymentMethod.OpenAccount),
                DiscountTotal = discountTotal,
                GrandTotal = grandTotalAll
            },
            Items = sales.Select(x =>
            {
                var saleChannel = x.RestaurantCheck.RestaurantTableSession.Channel;
                var amounts = paymentsLookup.GetValueOrDefault(x.RestaurantCheckId, []);
                return new RetailSaleListItemViewModel
                {
                    RetailSaleId = x.Id,
                    DocumentNumber = x.DocumentNumber,
                    IssuedAtUtc = x.IssuedAtUtc,
                    BranchName = "", // aşağıda toplu doldurulur
                    Channel = ResolveChannel(saleChannel),
                    SourceLabel = saleChannel switch
                    {
                        RestaurantSaleChannel.SelfSatis => "Self Satış",
                        RestaurantSaleChannel.Paket => packageNumbersByCheckId.GetValueOrDefault(x.RestaurantCheckId, "Paket"),
                        _ => x.RestaurantCheck.RestaurantTableSession.RestaurantTable?.Name ?? ""
                    },
                    GrandTotal = x.GrandTotal,
                    CashAmount = amounts.GetValueOrDefault(RestaurantPaymentMethod.Cash),
                    CreditCardAmount = amounts.GetValueOrDefault(RestaurantPaymentMethod.CreditCard),
                    MealCardAmount = amounts.GetValueOrDefault(RestaurantPaymentMethod.MealCard),
                    UnpaidAmount = amounts.GetValueOrDefault(RestaurantPaymentMethod.Unpaid),
                    OpenAccountAmount = amounts.GetValueOrDefault(RestaurantPaymentMethod.OpenAccount),
                    DiscountAmount = x.DiscountAmount,
                    Status = x.Status == RetailSaleStatus.Cancelled ? "İptal Edildi" : "Tamamlandı",
                    CustomerName = x.Customer != null ? x.Customer.Name : null
                };
            }).ToList()
        };

        // Şube adı ayrı doldurulur - artık RestaurantTableSession.BranchId'den DOĞRUDAN (masa
        // zincirinden BAĞIMSIZ, 2026-09-29 mimari karar - masasız satışlarda da her zaman dolu).
        var branchNamesById = model.Branches.ToDictionary(x => x.Id, x => x.Name);
        var branchNameByCheckId = sales.ToDictionary(
            x => x.Id,
            x => branchNamesById.GetValueOrDefault(x.RestaurantCheck.RestaurantTableSession.BranchId, ""));
        foreach (var item in model.Items)
        {
            item.BranchName = branchNameByCheckId.GetValueOrDefault(item.RetailSaleId, "");
        }

        return View(model);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var sale = await dbContext.RetailSales
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x!.RestaurantTable)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x.Branch)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (sale is null)
        {
            return NotFound();
        }

        var channel = sale.RestaurantCheck.RestaurantTableSession.Channel;
        var branch = sale.RestaurantCheck.RestaurantTableSession.Branch;
        var payments = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => x.RestaurantCheckId == sale.RestaurantCheckId && !x.IsReversal)
            .ToListAsync();

        var sourceLabel = channel switch
        {
            RestaurantSaleChannel.SelfSatis => "Self Satış",
            RestaurantSaleChannel.Paket => await dbContext.PackageOrders.AsNoTracking().Where(x => x.RestaurantCheckId == sale.RestaurantCheckId).Select(x => x.PackageNumber).SingleOrDefaultAsync() ?? "Paket",
            _ => sale.RestaurantCheck.RestaurantTableSession.RestaurantTable?.Name ?? ""
        };

        var model = new RetailSaleDetailViewModel
        {
            RetailSaleId = sale.Id,
            DocumentNumber = sale.DocumentNumber,
            IssuedAtUtc = sale.IssuedAtUtc,
            BranchName = branch.Name,
            BranchAddress = branch.Address,
            BranchPhone = branch.Phone,
            Channel = ResolveChannel(channel),
            SourceLabel = sourceLabel,
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
                    RestaurantPaymentMethod.Unpaid => "Ödenmez",
                    RestaurantPaymentMethod.OpenAccount => "Açık Hesap",
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
        var paymentAmountsByCheckId = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => checkIds.Contains(x.RestaurantCheckId) && !x.IsReversal)
            .GroupBy(x => new { x.RestaurantCheckId, x.PaymentMethod })
            .Select(g => new { g.Key.RestaurantCheckId, g.Key.PaymentMethod, Amount = g.Sum(p => p.Amount) })
            .ToListAsync();
        var paymentsLookup = paymentAmountsByCheckId
            .GroupBy(x => x.RestaurantCheckId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.PaymentMethod, x => x.Amount));
        var branchNamesById = await dbContext.Branches.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name);
        var packageNumbersByCheckId = await dbContext.PackageOrders
            .AsNoTracking()
            .Where(x => checkIds.Contains(x.RestaurantCheckId))
            .ToDictionaryAsync(x => x.RestaurantCheckId, x => x.PackageNumber);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Perakende Fiş Listesi");
        var headers = new[] { "Fiş No", "Tarih", "Şube", "Kanal", "Masa/Kaynak", "Cari", "Nakit", "Kredi Kartı", "Yemek Kartı", "Ödenmez", "Açık Hesap", "İndirim/İkram", "Tutar", "Durum" };
        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var row = 2;
        decimal cashTotal = 0, creditCardTotal = 0, mealCardTotal = 0, unpaidTotal = 0, openAccountTotal = 0, discountTotal = 0, grandTotal = 0;
        foreach (var sale in sales)
        {
            var saleChannel = sale.RestaurantCheck.RestaurantTableSession.Channel;
            var amounts = paymentsLookup.GetValueOrDefault(sale.RestaurantCheckId, []);
            var cash = amounts.GetValueOrDefault(RestaurantPaymentMethod.Cash);
            var creditCard = amounts.GetValueOrDefault(RestaurantPaymentMethod.CreditCard);
            var mealCard = amounts.GetValueOrDefault(RestaurantPaymentMethod.MealCard);
            var unpaid = amounts.GetValueOrDefault(RestaurantPaymentMethod.Unpaid);
            var openAccount = amounts.GetValueOrDefault(RestaurantPaymentMethod.OpenAccount);
            sheet.Cell(row, 1).Value = sale.DocumentNumber;
            sheet.Cell(row, 2).Value = sale.IssuedAtUtc.ToLocalTime();
            sheet.Cell(row, 2).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
            sheet.Cell(row, 3).Value = branchNamesById.GetValueOrDefault(sale.RestaurantCheck.RestaurantTableSession.BranchId, "");
            sheet.Cell(row, 4).Value = ResolveChannel(saleChannel);
            sheet.Cell(row, 5).Value = saleChannel switch
            {
                RestaurantSaleChannel.SelfSatis => "Self Satış",
                RestaurantSaleChannel.Paket => packageNumbersByCheckId.GetValueOrDefault(sale.RestaurantCheckId, "Paket"),
                _ => sale.RestaurantCheck.RestaurantTableSession.RestaurantTable?.Name ?? ""
            };
            sheet.Cell(row, 6).Value = sale.Customer?.Name ?? "";
            sheet.Cell(row, 7).Value = cash;
            sheet.Cell(row, 8).Value = creditCard;
            sheet.Cell(row, 9).Value = mealCard;
            sheet.Cell(row, 10).Value = unpaid;
            sheet.Cell(row, 11).Value = openAccount;
            sheet.Cell(row, 12).Value = sale.DiscountAmount;
            sheet.Cell(row, 13).Value = sale.GrandTotal;
            sheet.Cell(row, 14).Value = sale.Status == RetailSaleStatus.Cancelled ? "İptal Edildi" : "Tamamlandı";
            cashTotal += cash;
            creditCardTotal += creditCard;
            mealCardTotal += mealCard;
            unpaidTotal += unpaid;
            openAccountTotal += openAccount;
            discountTotal += sale.DiscountAmount;
            grandTotal += sale.GrandTotal;
            row++;
        }

        sheet.Cell(row, 6).Value = "TOPLAM";
        sheet.Cell(row, 6).Style.Font.Bold = true;
        sheet.Cell(row, 7).Value = cashTotal;
        sheet.Cell(row, 8).Value = creditCardTotal;
        sheet.Cell(row, 9).Value = mealCardTotal;
        sheet.Cell(row, 10).Value = unpaidTotal;
        sheet.Cell(row, 11).Value = openAccountTotal;
        sheet.Cell(row, 12).Value = discountTotal;
        sheet.Cell(row, 13).Value = grandTotal;
        for (var i = 6; i <= 13; i++)
        {
            sheet.Cell(row, i).Style.Font.Bold = true;
        }
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"perakende-fis-listesi-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");
    }
}
