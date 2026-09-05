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
        1 => PaymentMethodLabel(methods[0]),
        _ => "Karma Ödeme"
    };

    private static string PaymentMethodLabel(RestaurantPaymentMethod method) => method switch
    {
        RestaurantPaymentMethod.Cash => "Nakit",
        RestaurantPaymentMethod.CreditCard => "Kredi Kartı",
        RestaurantPaymentMethod.MealCard => "Yemek Kartı",
        RestaurantPaymentMethod.Unpaid => "Ödenmez",
        RestaurantPaymentMethod.OpenAccount => "Açık Hesap",
        _ => method.ToString()
    };

    // Madde 19 (Edip 2026-09-04) - filtreleme için ödeme türü anahtarı. "mixed"/"none" hard-code
    // ödeme türü DEĞİL, birden fazla/hiç ödeme satırı olan durumlar için özel iki kova.
    private static string PaymentFilterKeyOf(List<RestaurantPaymentMethod> methods) => methods.Distinct().Count() switch
    {
        0 => "none",
        1 => methods[0].ToString().ToLowerInvariant(),
        _ => "mixed"
    };

    private static readonly string[] TurkishMonthNames = ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"];
    private static readonly string[] TurkishShortDayNames = ["Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz"];

    // Ortak dönem filtresi (madde 28, 2026-09-05) - "Günlük | Haftalık | Aylık | Yıllık". Hafta
    // Pazartesi'den başlar (ISO 8601, TR pratiği). period tanınmıyorsa "day" davranışına düşer.
    private static (DateTime StartUtc, DateTime EndUtc, string RangeLabel) ComputePeriodRange(string period, DateOnly reportDate)
    {
        switch (period)
        {
            case "week":
                var dow = (int)reportDate.DayOfWeek;
                var daysSinceMonday = dow == 0 ? 6 : dow - 1;
                var monday = reportDate.AddDays(-daysSinceMonday);
                var sunday = monday.AddDays(6);
                var weekStart = monday.ToDateTime(TimeOnly.MinValue);
                return (weekStart.ToUniversalTime(), weekStart.AddDays(7).ToUniversalTime(), $"{monday:dd.MM} - {sunday:dd.MM.yyyy}");
            case "month":
                var firstOfMonth = new DateOnly(reportDate.Year, reportDate.Month, 1);
                var monthStart = firstOfMonth.ToDateTime(TimeOnly.MinValue);
                return (monthStart.ToUniversalTime(), monthStart.AddMonths(1).ToUniversalTime(), $"{TurkishMonthNames[reportDate.Month - 1]} {reportDate.Year}");
            case "year":
                var yearStart = new DateTime(reportDate.Year, 1, 1);
                return (yearStart.ToUniversalTime(), yearStart.AddYears(1).ToUniversalTime(), reportDate.Year.ToString());
            default:
                var dayStart = reportDate.ToDateTime(TimeOnly.MinValue);
                return (dayStart.ToUniversalTime(), dayStart.AddDays(1).ToUniversalTime(), reportDate.ToString("dd.MM.yyyy"));
        }
    }

    // Ödeme dağılımı artık hard-code 3 yöntem değil - o dönemde GERÇEKTEN var olan ödeme
    // türlerinden dinamik (madde 30). Renk paleti sabit değil "yeter sayıda ayrışan renk" listesi.
    private static readonly string[] PaymentBreakdownColors = ["var(--rs-green)", "var(--rs-purple)", "var(--rs-gold)", "#C73E3E", "#4A7FC1", "#888888"];

    private static (List<RestaurantPaymentBreakdownItemViewModel> Items, decimal Total, string ConicGradient) BuildPaymentBreakdown(IEnumerable<(RestaurantPaymentMethod Method, decimal Amount)> payments)
    {
        var grouped = payments
            .GroupBy(x => x.Method)
            .Select(g => new { Method = g.Key, Amount = g.Sum(x => x.Amount) })
            .Where(x => x.Amount != 0)
            .OrderByDescending(x => x.Amount)
            .ToList();

        var total = grouped.Sum(x => x.Amount);
        var items = new List<RestaurantPaymentBreakdownItemViewModel>();
        var gradientParts = new List<string>();
        var cumulative = 0m;
        for (var i = 0; i < grouped.Count; i++)
        {
            var g = grouped[i];
            var percent = total > 0 ? Math.Round(g.Amount / total * 100, 1) : 0;
            var color = PaymentBreakdownColors[i % PaymentBreakdownColors.Length];
            items.Add(new RestaurantPaymentBreakdownItemViewModel(PaymentMethodLabel(g.Method), g.Amount, percent, color));
            var fromPct = total > 0 ? Math.Round(cumulative / total * 100, 2) : 0;
            cumulative += g.Amount;
            var toPct = total > 0 ? Math.Round(cumulative / total * 100, 2) : 0;
            gradientParts.Add($"{color} {fromPct.ToString(System.Globalization.CultureInfo.InvariantCulture)}% {toPct.ToString(System.Globalization.CultureInfo.InvariantCulture)}%");
        }
        var conicGradient = gradientParts.Count == 0 ? "" : $"conic-gradient({string.Join(", ", gradientParts)})";
        return (items, total, conicGradient);
    }

    public async Task<IActionResult> Index(string tab = "daily", string source = "all", DateOnly? date = null, int? selected = null, int? zShiftId = null, string? payment = null, string? status = null, string? q = null, string period = "day", string? kasiyerUserId = null)
    {
        ActivePage = "reports";

        var reportDate = date ?? DateOnly.FromDateTime(DateTime.Now);
        var (dayStartUtc, dayEndUtc, periodRangeLabel) = ComputePeriodRange(period, reportDate);

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
            PeriodFilter = period is "week" or "month" or "year" ? period : "day",
            PeriodRangeLabel = periodRangeLabel,
            NetRevenue = nonCancelled.Sum(x => x.GrandTotal),
            ReceiptCount = nonCancelled.Count,
            AverageReceipt = nonCancelled.Count == 0 ? 0 : nonCancelled.Sum(x => x.GrandTotal) / nonCancelled.Count,
            CancelRatePercent = daySales.Count == 0 ? 0 : Math.Round(daySales.Count(x => x.Status == RetailSaleStatus.Cancelled) * 100m / daySales.Count, 1)
        };

        // Ödeme dağılımı (madde 30) - tutar bazında, RestaurantPayments'tan (RetailSale'de yok),
        // artık HARD-CODE 3 yöntem değil, o dönemde GERÇEKTEN var olan tüm türlerden dinamik.
        // Ters kayıtlar (bkz. RestaurantPostingService.CancelRetailSaleAsync) DAHİL edilir ama
        // negatif işaretle - aksi halde iptal edilen bir fişin ödemesi bu toplamlarda hâlâ
        // "alınmış" gibi görünmeye devam ederdi (Edip, 2026-09-03: fiş iptali sonrası fark edildi).
        var periodPaymentsRaw = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => x.PaidAtUtc >= dayStartUtc && x.PaidAtUtc < dayEndUtc)
            .Select(x => new { x.PaymentMethod, Amount = x.IsReversal ? -x.Amount : x.Amount })
            .ToListAsync();
        (vm.PaymentBreakdown, vm.PaymentBreakdownTotal, vm.PaymentBreakdownConicGradient) =
            BuildPaymentBreakdown(periodPaymentsRaw.Select(x => (x.PaymentMethod, x.Amount)));

        // Ciro akışı (madde 28-29) - dönem büyüdükçe eksen kabalaşır: Günlük→saatlik,
        // Haftalık/Aylık→günlük, Yıllık→aylık. Dashboard'daki Yoğunluk Haritası ile AYNI mantık,
        // sadece eksen birimi seçilen döneme göre değişken.
        if (vm.PeriodFilter == "day")
        {
            var hourTotals = new decimal[24];
            foreach (var s in nonCancelled)
            {
                hourTotals[s.IssuedAtUtc.ToLocalTime().Hour] += s.GrandTotal;
            }
            var activeHours = Enumerable.Range(0, 24).Where(h => hourTotals[h] > 0).ToList();
            var chartStartHour = activeHours.Count > 0 ? Math.Max(0, activeHours.Min() - 1) : 8;
            var chartEndHour = activeHours.Count > 0 ? Math.Min(23, activeHours.Max() + 1) : 22;
            vm.HourlyRevenueStartHour = chartStartHour;
            vm.HourlyRevenue = Enumerable.Range(chartStartHour, chartEndHour - chartStartHour + 1).Select(h => hourTotals[h]).ToList();
            vm.HourlyRevenueLabels = Enumerable.Range(chartStartHour, chartEndHour - chartStartHour + 1).Select(h => $"{h}:00").ToList();
        }
        else if (vm.PeriodFilter == "week")
        {
            var weekTotals = new decimal[7];
            var weekStartLocal = dayStartUtc.ToLocalTime().Date;
            foreach (var s in nonCancelled)
            {
                var idx = (s.IssuedAtUtc.ToLocalTime().Date - weekStartLocal).Days;
                if (idx is >= 0 and < 7) { weekTotals[idx] += s.GrandTotal; }
            }
            vm.HourlyRevenue = weekTotals.ToList();
            vm.HourlyRevenueLabels = Enumerable.Range(0, 7).Select(i => $"{TurkishShortDayNames[i]} {weekStartLocal.AddDays(i):dd.MM}").ToList();
        }
        else if (vm.PeriodFilter == "month")
        {
            var daysInMonth = DateTime.DaysInMonth(reportDate.Year, reportDate.Month);
            var monthTotals = new decimal[daysInMonth];
            foreach (var s in nonCancelled)
            {
                var d = s.IssuedAtUtc.ToLocalTime().Day - 1;
                if (d is >= 0 && d < daysInMonth) { monthTotals[d] += s.GrandTotal; }
            }
            vm.HourlyRevenue = monthTotals.ToList();
            vm.HourlyRevenueLabels = Enumerable.Range(1, daysInMonth).Select(d => d.ToString()).ToList();
        }
        else // year
        {
            var yearTotals = new decimal[12];
            foreach (var s in nonCancelled)
            {
                yearTotals[s.IssuedAtUtc.ToLocalTime().Month - 1] += s.GrandTotal;
            }
            vm.HourlyRevenue = yearTotals.ToList();
            vm.HourlyRevenueLabels = TurkishMonthNames.Select(m => m[..3]).ToList();
        }

        var filtered = source switch
        {
            "table" => daySales.Where(x => SourceTypeOf(x.SectionName) == "table"),
            "package" => daySales.Where(x => SourceTypeOf(x.SectionName) == "package"),
            "self" => daySales.Where(x => SourceTypeOf(x.SectionName) == "self"),
            _ => daySales.AsEnumerable()
        };

        var sourceFilteredRows = filtered
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
                    PaymentFilterKeyOf(x.Payments),
                    x.Status == RetailSaleStatus.Cancelled,
                    x.GrandTotal);
            })
            .ToList();

        // Madde 19: ödeme filtresi seçenekleri SADECE bu gün/kaynak filtresinde GERÇEKTEN var
        // olan türlerden oluşur - hard-code bir liste değil.
        vm.AvailablePaymentFilters = sourceFilteredRows
            .Select(x => x.PaymentFilterKey)
            .Distinct()
            .OrderBy(x => x)
            .Select(k => new RestaurantPaymentFilterOptionViewModel(k, k switch
            {
                "mixed" => "Karma Ödeme",
                "none" => "Ödemesiz",
                _ => Enum.TryParse<RestaurantPaymentMethod>(k, true, out var m) ? PaymentMethodLabel(m) : k
            }))
            .ToList();

        vm.PaymentFilter = string.IsNullOrWhiteSpace(payment) ? null : payment;
        vm.StatusFilter = string.IsNullOrWhiteSpace(status) ? null : status;
        vm.SearchTerm = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var finalRows = sourceFilteredRows.AsEnumerable();
        if (vm.PaymentFilter is not null)
        {
            finalRows = finalRows.Where(x => x.PaymentFilterKey == vm.PaymentFilter);
        }
        if (vm.StatusFilter == "completed")
        {
            finalRows = finalRows.Where(x => !x.IsCancelled);
        }
        else if (vm.StatusFilter == "cancelled")
        {
            finalRows = finalRows.Where(x => x.IsCancelled);
        }
        if (vm.SearchTerm is not null)
        {
            finalRows = finalRows.Where(x =>
                x.DocumentNumber.Contains(vm.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                x.SourceLabel.Contains(vm.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                x.SourceSubtitle.Contains(vm.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }
        vm.Receipts = finalRows.ToList();

        vm.ListedCount = vm.Receipts.Count;
        vm.ListedTotal = vm.Receipts.Where(x => !x.IsCancelled).Sum(x => x.GrandTotal);
        var listedIds = vm.Receipts.Select(x => x.RetailSaleId).ToHashSet();
        vm.ListedDiscount = filtered.Where(x => listedIds.Contains(x.Id)).Sum(x => x.DiscountAmount);
        vm.ListedCancelledCount = vm.Receipts.Count(x => x.IsCancelled);

        // Madde 19: filtrelenmiş sonuçların ödeme türü bazında alt toplamları (Nakit toplamı,
        // Kredi Kartı toplamı, İkram/Ödenmez toplamı vb.) - sadece filtredeki fişlerin check'lerine
        // ait GERÇEK RestaurantPayment satırlarından, iptal edilmemiş fişler için.
        var listedCheckIds = daySales.Where(x => listedIds.Contains(x.Id) && x.Status != RetailSaleStatus.Cancelled).Select(x => x.RestaurantCheckId).ToList();
        if (listedCheckIds.Count > 0)
        {
            var subtotals = await dbContext.RestaurantPayments
                .AsNoTracking()
                .Where(x => listedCheckIds.Contains(x.RestaurantCheckId) && !x.IsReversal)
                .GroupBy(x => x.PaymentMethod)
                .Select(g => new { Method = g.Key, Total = g.Sum(x => x.Amount) })
                .ToListAsync();
            vm.PaymentSubtotals = subtotals
                .OrderBy(x => x.Method)
                .Select(x => new RestaurantPaymentSubtotalViewModel(PaymentMethodLabel(x.Method), x.Total))
                .ToList();
        }

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

        // X Raporu artık AÇIK VARDİYA ŞART DEĞİL (Edip, 2026-09-03: "X ve Z raporu almak için
        // vardiya girilmesi zorunlu değil") - Dashboard'daki AYNI dönem mantığı: bugün alınmış
        // son Z'nin kapanışından beri (yoksa takvim gece yarısından beri) satılan her şey.
        // Açık bir vardiya varsa onun açılış saati bilgi amaçlı gösterilir, dönemi BELİRLEMEZ.
        var todayStartForXUtc = DateTime.Now.Date.ToUniversalTime();
        var lastZClosedTodayForXUtc = await dbContext.RestaurantCashShifts
            .Where(x => x.Status == RestaurantCashShiftStatus.Closed && x.ClosedAtUtc >= todayStartForXUtc && x.ClosedAtUtc < todayStartForXUtc.AddDays(1))
            .OrderByDescending(x => x.ClosedAtUtc)
            .Select(x => (DateTime?)x.ClosedAtUtc)
            .FirstOrDefaultAsync();
        var xPeriodStartUtc = lastZClosedTodayForXUtc ?? todayStartForXUtc;

        var periodPayments = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => x.PaidAtUtc >= xPeriodStartUtc)
            .ToListAsync();
        var periodReceiptCount = await dbContext.RetailSales
            .AsNoTracking()
            .CountAsync(x => x.IssuedAtUtc >= xPeriodStartUtc && x.Status != RetailSaleStatus.Cancelled);

        var (xBreakdown, _, _) = BuildPaymentBreakdown(periodPayments.Select(x => (x.PaymentMethod, x.IsReversal ? -x.Amount : x.Amount)));
        vm.XReport = new RestaurantXReportViewModel
        {
            OpenedAtUtc = openShift?.OpenedAtUtc ?? xPeriodStartUtc,
            ReceiptCount = periodReceiptCount,
            NetRevenue = periodPayments.Sum(x => x.IsReversal ? -x.Amount : x.Amount),
            PaymentBreakdown = xBreakdown
        };

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
                            PaymentFilterKeyOf(x.Payments),
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

        // Kasiyer Raporu (madde 31, 2026-09-05) - "normal kasiyer yalnızca kendi işlemlerini
        // görmelidir ... yetkili yönetici ise başka kasiyerleri veya tüm kasiyerleri
        // seçebilmelidir". "Kasiyer" burada da diğer her yerdeki (Günlük Fişler'in
        // SourceSubtitle'ı, Z Listesi vb.) AYNI kavram - adisyonu açan kullanıcı (OpenerName).
        // Ortak dönem filtresi bu sekmede de geçerli (dayStartUtc/dayEndUtc zaten period'a göre).
        vm.CanPickAnyKasiyer = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.RestaurantManager);
        var effectiveKasiyerUserId = vm.CanPickAnyKasiyer ? kasiyerUserId : CurrentUserId;
        vm.SelectedKasiyerUserId = effectiveKasiyerUserId;

        if (vm.CanPickAnyKasiyer)
        {
            var kasiyerIds = daySales.Select(x => x.OpenerName).Distinct().ToList();
            vm.KasiyerOptions = kasiyerIds
                .Select(id => new RestaurantKasiyerOptionViewModel(id, openerNames.GetValueOrDefault(id, id)))
                .OrderBy(x => x.FullName)
                .ToList();
        }

        var kasiyerSales = string.IsNullOrEmpty(effectiveKasiyerUserId)
            ? daySales
            : daySales.Where(x => x.OpenerName == effectiveKasiyerUserId).ToList();
        var kasiyerNonCancelled = kasiyerSales.Where(x => x.Status != RetailSaleStatus.Cancelled).ToList();
        var kasiyerCheckIds = kasiyerNonCancelled.Select(x => x.RestaurantCheckId).ToList();

        var kasiyerPayments = kasiyerCheckIds.Count == 0
            ? []
            : await dbContext.RestaurantPayments
                .AsNoTracking()
                .Where(x => kasiyerCheckIds.Contains(x.RestaurantCheckId) && !x.IsReversal)
                .Select(x => new { x.PaymentMethod, x.Amount })
                .ToListAsync();

        // İndirim/İkram AYRI izlenir - RestaurantOrderLine.IsComplimentary madde 11'in kendi
        // ayrımı (ikram satırında DiscountAmountSnapshot = TAM brüt tutar). "İade" için bu
        // kodda ayrı bir kavram/tablo YOK (dönüş = mevcut iptal/reversal mekanizmasının kendisi,
        // bkz. RestaurantPostingService.CancelRetailSaleAsync) - uydurma bir sayı göstermek
        // yerine tek bir "İptal" alanında birleştirildi, ayrıca not düşüldü (bkz. tracking dosyası).
        var kasiyerLineAmounts = kasiyerCheckIds.Count == 0
            ? []
            : await dbContext.RestaurantOrderLines
                .AsNoTracking()
                .Where(x => kasiyerCheckIds.Contains(x.RestaurantOrder.RestaurantCheckId) && x.Status != RestaurantOrderLineStatus.Cancelled && x.DiscountAmountSnapshot > 0)
                .Select(x => new { x.IsComplimentary, x.DiscountAmountSnapshot })
                .ToListAsync();

        vm.Kasiyer = new RestaurantKasiyerReportViewModel
        {
            DisplayName = string.IsNullOrEmpty(effectiveKasiyerUserId) ? "Tüm Kasiyerler" : openerNames.GetValueOrDefault(effectiveKasiyerUserId, "Kasiyer"),
            NetRevenue = kasiyerNonCancelled.Sum(x => x.GrandTotal),
            ReceiptCount = kasiyerNonCancelled.Count,
            Cash = kasiyerPayments.Where(x => x.PaymentMethod == RestaurantPaymentMethod.Cash).Sum(x => x.Amount),
            Card = kasiyerPayments.Where(x => x.PaymentMethod == RestaurantPaymentMethod.CreditCard).Sum(x => x.Amount),
            OtherCollections = kasiyerPayments.Where(x => x.PaymentMethod is not (RestaurantPaymentMethod.Cash or RestaurantPaymentMethod.CreditCard)).Sum(x => x.Amount),
            DiscountAmount = kasiyerLineAmounts.Where(x => !x.IsComplimentary).Sum(x => x.DiscountAmountSnapshot),
            ComplimentaryAmount = kasiyerLineAmounts.Where(x => x.IsComplimentary).Sum(x => x.DiscountAmountSnapshot),
            CancelledCount = kasiyerSales.Count(x => x.Status == RetailSaleStatus.Cancelled)
        };

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

    // Vardiya açmadan doğrudan Z Raporu - Edip, 2026-09-03: "vardiya mantığı şu an kapalı olsun
    // Z raporunda direkt rapor alsın ve günü sıfırlasın herşeyi". Kasa sayımı YOK - bu "yumuşak"
    // bir gün sonu, gerçek bir kasa mutabakatı değil (bkz. CreateDirectZReportAsync yorumu).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDirectZReport()
    {
        try
        {
            var shift = await postingService.CreateDirectZReportAsync(CurrentUserId);
            TempData["Success"] = $"Z-{shift.Id:D6} raporu oluşturuldu, gün sıfırlandı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = "zlist" });
    }

    // Kapanmış (ödemesi alınmış) bir fişi "silmek" için - Edip'in isteği (2026-09-03: "restoranda
    // sil mantığı işlesin"). Gerçek hard-delete DEĞİL, ters kayıtlı iptal (bkz.
    // RestaurantPostingService.CancelRetailSaleAsync yorumu) - kullanıcı için fiş raporlarda/
    // toplamlarda kaybolur ama muhasebe denetim izi korunur. "Yetki bazlı" - sadece Administrator,
    // controller'ın genel yetki listesinden (Cashier/RestaurantManager de girebilir) daha dar.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> CancelReceipt(int retailSaleId, string reason, string returnTab = "daily", DateOnly? date = null, int? zShiftId = null)
    {
        try
        {
            await postingService.CancelRetailSaleAsync(retailSaleId, CurrentUserId, reason);
            TempData["Success"] = "Fiş iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = returnTab, date, zShiftId });
    }
}
