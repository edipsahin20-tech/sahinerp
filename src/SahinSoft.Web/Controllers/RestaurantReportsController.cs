using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager},{AppRoles.Cashier}")]
public sealed class RestaurantReportsController(ApplicationDbContext dbContext, RestaurantPostingService postingService) : RestaurantControllerBase(dbContext)
{
    // Kaynak (Masa/Paket/Self) yeni bir alan/tablo değil - satışın bağlı olduğu sanal/gerçek
    // masanın salonu neyse odur (bkz. Self Satış/Paket'in gizli salon deseni). Dashboard'daki
    // PaymentSummary yardımcısıyla aynı mantık.
    private static string SourceTypeOf(string sectionName) => sectionName switch
    {
        RestaurantPostingService.SelfSaleSectionName => "self",
        "Paket" => "package",
        _ => "table"
    };

    private static string PaymentSummary(List<RestaurantPaymentMethod> methods) => methods.Distinct().Count() switch
    {
        0 => "-",
        1 => methods[0] switch
        {
            RestaurantPaymentMethod.Cash => "Nakit",
            RestaurantPaymentMethod.CreditCard => "Kredi Kartı",
            _ => "Yemek Kartı"
        },
        _ => "Karma Ödeme"
    };

    public async Task<IActionResult> Index(string tab = "daily", string source = "all", DateOnly? date = null, int? selected = null, int? zShiftId = null)
    {
        ActivePage = "reports";

        var reportDate = date ?? DateOnly.FromDateTime(DateTime.Now);
        var dayStartUtc = reportDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var dayEndUtc = dayStartUtc.AddDays(1);

        var daySales = await dbContext.RetailSales
            .AsNoTracking()
            .Where(x => x.IssuedAtUtc >= dayStartUtc && x.IssuedAtUtc < dayEndUtc)
            .Select(x => new
            {
                x.Id,
                x.IssuedAtUtc,
                x.DocumentNumber,
                x.GrandTotal,
                x.DiscountAmount,
                x.Status,
                x.RestaurantCheckId,
                TableName = x.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name,
                SectionName = x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name,
                OpenerName = x.RestaurantCheck.RestaurantTableSession.OpenedByUserId,
                Payments = x.RestaurantCheck.Payments.Where(p => !p.IsReversal).Select(p => p.PaymentMethod).ToList()
            })
            .ToListAsync();

        var openerIds = daySales.Select(x => x.OpenerName).Distinct().ToList();
        var openerNames = await dbContext.Users
            .AsNoTracking()
            .Where(x => openerIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName);

        var checkIds = daySales.Select(x => x.RestaurantCheckId).ToList();
        var packageNumbers = await dbContext.PackageOrders
            .AsNoTracking()
            .Where(x => checkIds.Contains(x.RestaurantCheckId))
            .ToDictionaryAsync(x => x.RestaurantCheckId, x => new { x.PackageNumber, x.CustomerName });

        var nonCancelled = daySales.Where(x => x.Status != RetailSaleStatus.Cancelled).ToList();

        var vm = new RestaurantReportsViewModel
        {
            ActiveTab = tab,
            SourceFilter = source,
            ReportDate = reportDate,
            NetRevenue = nonCancelled.Sum(x => x.GrandTotal),
            ReceiptCount = nonCancelled.Count,
            AverageReceipt = nonCancelled.Count == 0 ? 0 : nonCancelled.Sum(x => x.GrandTotal) / nonCancelled.Count,
            CancelRatePercent = daySales.Count == 0 ? 0 : Math.Round(daySales.Count(x => x.Status == RetailSaleStatus.Cancelled) * 100m / daySales.Count, 1)
        };

        // Ödeme yöntemi kırılımı - tutar bazında, RestaurantPayments'tan (RetailSale'de yok).
        var dayPayments = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => x.PaidAtUtc >= dayStartUtc && x.PaidAtUtc < dayEndUtc && !x.IsReversal)
            .GroupBy(x => x.PaymentMethod)
            .Select(g => new { Method = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();
        vm.Cash = dayPayments.FirstOrDefault(x => x.Method == RestaurantPaymentMethod.Cash)?.Total ?? 0;
        vm.Card = dayPayments.FirstOrDefault(x => x.Method == RestaurantPaymentMethod.CreditCard)?.Total ?? 0;
        vm.MealCard = dayPayments.FirstOrDefault(x => x.Method == RestaurantPaymentMethod.MealCard)?.Total ?? 0;

        var paymentTotal = vm.Cash + vm.Card + vm.MealCard;
        if (paymentTotal > 0)
        {
            vm.CashPercent = Math.Round(vm.Cash / paymentTotal * 100, 1);
            vm.CardPercent = Math.Round(vm.Card / paymentTotal * 100, 1);
            vm.MealCardPercent = Math.Round(100 - vm.CashPercent - vm.CardPercent, 1);
        }

        // Saatlik ciro akışı - Dashboard'daki Yoğunluk Haritası ile AYNI hesap deseni, rapor
        // tarihine göre (bugünle sınırlı değil). Veri yoksa varsayılan olarak tipik restoran
        // açılış saatleri (08-22) gösterilir, boş bir grafik yerine.
        var hourTotals = new decimal[24];
        foreach (var s in nonCancelled)
        {
            hourTotals[s.IssuedAtUtc.ToLocalTime().Hour] += s.GrandTotal;
        }
        var activeHours = Enumerable.Range(0, 24).Where(h => hourTotals[h] > 0).ToList();
        var chartStartHour = activeHours.Count > 0 ? Math.Max(0, activeHours.Min() - 1) : 8;
        var chartEndHour = activeHours.Count > 0 ? Math.Min(23, activeHours.Max() + 1) : 22;
        vm.HourlyRevenueStartHour = chartStartHour;
        vm.HourlyRevenue = Enumerable.Range(chartStartHour, chartEndHour - chartStartHour + 1)
            .Select(h => hourTotals[h])
            .ToList();

        var filtered = source switch
        {
            "table" => daySales.Where(x => SourceTypeOf(x.SectionName) == "table"),
            "package" => daySales.Where(x => SourceTypeOf(x.SectionName) == "package"),
            "self" => daySales.Where(x => SourceTypeOf(x.SectionName) == "self"),
            _ => daySales.AsEnumerable()
        };

        vm.Receipts = filtered
            .OrderByDescending(x => x.IssuedAtUtc)
            .Select(x =>
            {
                var sourceType = SourceTypeOf(x.SectionName);
                var isPackage = sourceType == "package";
                var pkg = isPackage && packageNumbers.TryGetValue(x.RestaurantCheckId, out var p) ? p : null;
                return new RestaurantReceiptRowViewModel(
                    x.Id,
                    x.IssuedAtUtc,
                    x.DocumentNumber,
                    isPackage ? pkg?.PackageNumber ?? x.TableName : sourceType == "self" ? "Self Satış" : x.TableName,
                    isPackage ? pkg?.CustomerName ?? "" : openerNames.GetValueOrDefault(x.OpenerName, ""),
                    sourceType,
                    PaymentSummary(x.Payments),
                    x.Status == RetailSaleStatus.Cancelled,
                    x.GrandTotal);
            })
            .ToList();

        vm.ListedCount = vm.Receipts.Count;
        vm.ListedTotal = vm.Receipts.Where(x => !x.IsCancelled).Sum(x => x.GrandTotal);
        vm.ListedDiscount = filtered.Sum(x => x.DiscountAmount);
        vm.ListedCancelledCount = vm.Receipts.Count(x => x.IsCancelled);

        // Vardiya/Z durumu - RestaurantShiftController ile AYNI kaynak (RestaurantCashShift),
        // burada tek bir "en son / şu an açık" özet olarak gösterilir.
        var openShift = await dbContext.RestaurantCashShifts
            .AsNoTracking()
            .Include(x => x.FinancialAccount)
            .Where(x => x.Status == RestaurantCashShiftStatus.Open)
            .OrderByDescending(x => x.OpenedAtUtc)
            .FirstOrDefaultAsync();

        var lastClosedShift = await dbContext.RestaurantCashShifts
            .AsNoTracking()
            .Where(x => x.Status == RestaurantCashShiftStatus.Closed)
            .OrderByDescending(x => x.ClosedAtUtc)
            .FirstOrDefaultAsync();

        vm.IsShiftOpen = openShift is not null;
        vm.OpenShiftId = openShift?.Id;
        vm.ShiftOpenedAtUtc = openShift?.OpenedAtUtc;
        vm.FinancialAccountName = openShift?.FinancialAccount.Name;
        vm.LastZNumber = lastClosedShift is null ? null : $"Z-{lastClosedShift.Id:D6}";
        vm.LastZClosedAtUtc = lastClosedShift?.ClosedAtUtc;

        if (openShift is not null)
        {
            var shiftPayments = await dbContext.RestaurantPayments
                .AsNoTracking()
                .Where(x => x.PaidAtUtc >= openShift.OpenedAtUtc && !x.IsReversal)
                .ToListAsync();
            var shiftReceiptCount = await dbContext.RetailSales
                .AsNoTracking()
                .CountAsync(x => x.IssuedAtUtc >= openShift.OpenedAtUtc && x.Status != RetailSaleStatus.Cancelled);

            vm.XReport = new RestaurantXReportViewModel
            {
                OpenedAtUtc = openShift.OpenedAtUtc,
                ReceiptCount = shiftReceiptCount,
                NetRevenue = shiftPayments.Sum(x => x.Amount),
                Cash = shiftPayments.Where(x => x.PaymentMethod == RestaurantPaymentMethod.Cash).Sum(x => x.Amount),
                Card = shiftPayments.Where(x => x.PaymentMethod == RestaurantPaymentMethod.CreditCard).Sum(x => x.Amount),
                MealCard = shiftPayments.Where(x => x.PaymentMethod == RestaurantPaymentMethod.MealCard).Sum(x => x.Amount)
            };
        }

        vm.ZList = await dbContext.RestaurantCashShifts
            .AsNoTracking()
            .Include(x => x.FinancialAccount)
            .Where(x => x.Status == RestaurantCashShiftStatus.Closed)
            .OrderByDescending(x => x.ClosedAtUtc)
            .Take(30)
            .Select(x => new RestaurantZListRowViewModel(
                x.Id, $"Z-{x.Id:D6}", x.FinancialAccount.Name, x.OpenedAtUtc, x.ClosedAtUtc!.Value, x.OpeningBalance, x.ClosingBalanceExpected, x.ClosingBalanceCounted))
            .ToListAsync();

        // Z Listesi'nde bir Z'ye tıklanınca o vardiyanın (Açılış→Kapanış) satış hareketleri -
        // Edip'in isteği (2026-09-03). Düzenleme/silme burada YOK - reversal muhasebe kuralına
        // aykırı olur (bkz. [[feedback_sahinsoft_conventions]]), Edip'ten ayrıca netleştirme
        // bekleniyor; şimdilik salt-okunur "Fişi Gör" ile aynı detay modalı.
        if (zShiftId is not null)
        {
            var selectedShift = await dbContext.RestaurantCashShifts
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == zShiftId.Value && x.Status == RestaurantCashShiftStatus.Closed);

            if (selectedShift is not null)
            {
                var zSales = await dbContext.RetailSales
                    .AsNoTracking()
                    .Where(x => x.IssuedAtUtc >= selectedShift.OpenedAtUtc && x.IssuedAtUtc < selectedShift.ClosedAtUtc!.Value)
                    .Select(x => new
                    {
                        x.Id,
                        x.IssuedAtUtc,
                        x.DocumentNumber,
                        x.GrandTotal,
                        x.Status,
                        x.RestaurantCheckId,
                        TableName = x.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name,
                        SectionName = x.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name,
                        OpenerName = x.RestaurantCheck.RestaurantTableSession.OpenedByUserId,
                        Payments = x.RestaurantCheck.Payments.Where(p => !p.IsReversal).Select(p => p.PaymentMethod).ToList()
                    })
                    .ToListAsync();

                var zOpenerIds = zSales.Select(x => x.OpenerName).Distinct().ToList();
                var zOpenerNames = await dbContext.Users
                    .AsNoTracking()
                    .Where(x => zOpenerIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.FullName);

                var zCheckIds = zSales.Select(x => x.RestaurantCheckId).ToList();
                var zPackageNumbers = await dbContext.PackageOrders
                    .AsNoTracking()
                    .Where(x => zCheckIds.Contains(x.RestaurantCheckId))
                    .ToDictionaryAsync(x => x.RestaurantCheckId, x => new { x.PackageNumber, x.CustomerName });

                vm.SelectedZShiftId = zShiftId;
                vm.SelectedZNumber = $"Z-{selectedShift.Id:D6}";
                vm.SelectedZReceipts = zSales
                    .OrderByDescending(x => x.IssuedAtUtc)
                    .Select(x =>
                    {
                        var sourceType = SourceTypeOf(x.SectionName);
                        var isPackage = sourceType == "package";
                        var pkg = isPackage && zPackageNumbers.TryGetValue(x.RestaurantCheckId, out var p) ? p : null;
                        return new RestaurantReceiptRowViewModel(
                            x.Id,
                            x.IssuedAtUtc,
                            x.DocumentNumber,
                            isPackage ? pkg?.PackageNumber ?? x.TableName : sourceType == "self" ? "Self Satış" : x.TableName,
                            isPackage ? pkg?.CustomerName ?? "" : zOpenerNames.GetValueOrDefault(x.OpenerName, ""),
                            sourceType,
                            PaymentSummary(x.Payments),
                            x.Status == RetailSaleStatus.Cancelled,
                            x.GrandTotal);
                    })
                    .ToList();
            }
        }

        if (selected is not null)
        {
            var sale = await dbContext.RetailSales
                .AsNoTracking()
                .Include(x => x.Lines)
                .Include(x => x.RestaurantCheck).ThenInclude(x => x.Payments)
                .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable).ThenInclude(x => x.RestaurantSection)
                .SingleOrDefaultAsync(x => x.Id == selected.Value);

            if (sale is not null)
            {
                var sectionName = sale.RestaurantCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name;
                var sourceType = SourceTypeOf(sectionName);
                var tableName = sale.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name;
                string sourceLabel;
                if (sourceType == "package")
                {
                    var pkgOrder = await dbContext.PackageOrders.AsNoTracking().SingleOrDefaultAsync(x => x.RestaurantCheckId == sale.RestaurantCheckId);
                    sourceLabel = pkgOrder is null ? tableName : $"{pkgOrder.PackageNumber} · {pkgOrder.CustomerName}";
                }
                else if (sourceType == "self")
                {
                    sourceLabel = "Self Satış";
                }
                else
                {
                    sourceLabel = tableName;
                }

                vm.SelectedReceiptId = sale.Id;
                vm.SelectedReceipt = new RestaurantReceiptDetailViewModel
                {
                    DocumentNumber = sale.DocumentNumber,
                    IssuedAtUtc = sale.IssuedAtUtc,
                    SourceLabel = sourceLabel,
                    SourceType = sourceType,
                    IsCancelled = sale.Status == RetailSaleStatus.Cancelled,
                    Lines = sale.Lines.Select(l => new RestaurantReceiptDetailLine(l.ProductNameSnapshot, l.Quantity, l.LineTotal)).ToList(),
                    SubtotalAmount = sale.SubtotalAmount,
                    DiscountAmount = sale.DiscountAmount,
                    TaxAmount = sale.TaxAmount,
                    GrandTotal = sale.GrandTotal,
                    Payments = sale.RestaurantCheck.Payments.Where(p => !p.IsReversal).Select(p => new RestaurantReceiptDetailPayment(
                        p.PaymentMethod switch { RestaurantPaymentMethod.Cash => "Nakit", RestaurantPaymentMethod.CreditCard => "Kredi Kartı", _ => "Yemek Kartı" },
                        p.Amount)).ToList()
                };
            }
        }

        return View(vm);
    }

    // Z Raporu = vardiya kapanışı - yeni bir "Z" kavramı İCAT EDİLMEDİ, mevcut
    // RestaurantPostingService.CloseShiftAsync (Vardiya ekranındakiyle AYNI metod) çağrılır.
    // Aynı vardiya için mükerrer Z, CloseShiftAsync'in "zaten kapalı" kontrolüyle engellenir.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseShift(int shiftId, decimal closingBalanceCounted)
    {
        try
        {
            var shift = await postingService.CloseShiftAsync(shiftId, closingBalanceCounted);
            TempData["Success"] = $"Z-{shift.Id:D6} raporu oluşturuldu, vardiya kapatıldı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = "zlist" });
    }
}
