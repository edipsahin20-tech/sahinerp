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

        // Ödenmez iş kuralı (2026-09-06, Edip) - finansal karşılığı olmadığı için Net Ciro'ya
        // dahil edilmez (İkram GrandTotal=0 olduğu için zaten hariç); ürün müşteriye çıktığı için
        // fiş/Z bağlantısı ve stok hareketi korunur, PaymentBreakdown'da (aşağıda) kendi satırında
        // ayrıca gösterilir. Aynı kural RestaurantDashboardController/RestaurantPostingService'te de
        // (Z snapshot) uygulanır - tek ortak hesaplama.
        var dayUnpaidTotal = periodPaymentsRaw.Where(x => x.PaymentMethod == RestaurantPaymentMethod.Unpaid).Sum(x => x.Amount);

        var vm = new RestaurantReportsViewModel
        {
            ActiveTab = tab,
            SourceFilter = source,
            ReportDate = reportDate,
            PeriodFilter = period is "week" or "month" or "year" ? period : "day",
            PeriodRangeLabel = periodRangeLabel,
            NetRevenue = nonCancelled.Sum(x => x.GrandTotal) - dayUnpaidTotal,
            ReceiptCount = nonCancelled.Count,
            AverageReceipt = nonCancelled.Count == 0 ? 0 : nonCancelled.Sum(x => x.GrandTotal) / nonCancelled.Count,
            CancelRatePercent = daySales.Count == 0 ? 0 : Math.Round(daySales.Count(x => x.Status == RetailSaleStatus.Cancelled) * 100m / daySales.Count, 1)
        };

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

        // Onaylı Restoran Raporları mockup'ının 1. satırındaki raporlar (madde birebir-uygulama,
        // 2026-09-05) - En Çok Satanlar/Kategori Satışları/KDV Dökümü/İndirim & İkram. Hepsi AYNI
        // dönem filtresini (nonCancelled'ın kapsadığı RetailSale kümesi) kullanır - ayrı bir
        // sorgu penceresi İCAT EDİLMEDİ.
        var periodSaleIds = nonCancelled.Select(x => x.Id).ToList();
        if (periodSaleIds.Count > 0)
        {
            var periodLines = await dbContext.RetailSaleLines
                .AsNoTracking()
                .Where(x => periodSaleIds.Contains(x.RetailSaleId))
                .Select(x => new { x.ProductNameSnapshot, x.Quantity, x.LineTotal, x.TaxRateSnapshot, CategoryName = x.Product.Category.Name })
                .ToListAsync();

            vm.BestSellers = periodLines
                .GroupBy(x => x.ProductNameSnapshot)
                .Select(g => new RestaurantBestSellerRowViewModel(g.Key, g.Sum(x => x.Quantity), g.Sum(x => x.LineTotal)))
                .OrderByDescending(x => x.Total)
                .Take(15)
                .ToList();

            var categoryTotal = periodLines.Sum(x => x.LineTotal);
            vm.CategorySales = periodLines
                .GroupBy(x => x.CategoryName)
                .Select(g => new RestaurantCategorySalesRowViewModel(g.Key, g.Sum(x => x.Quantity), g.Sum(x => x.LineTotal), categoryTotal > 0 ? Math.Round(g.Sum(x => x.LineTotal) / categoryTotal * 100, 1) : 0))
                .OrderByDescending(x => x.Total)
                .ToList();

            vm.VatBreakdown = periodLines
                .GroupBy(x => x.TaxRateSnapshot)
                .Select(g =>
                {
                    var gross = g.Sum(x => x.LineTotal);
                    var (matrah, vatAmount) = RestaurantPricingCalculator.ExtractTax(gross, g.Key);
                    return new RestaurantVatRowViewModel(g.Key, matrah, vatAmount, gross);
                })
                .OrderBy(x => x.TaxRate)
                .ToList();
        }

        // İndirim & İkram - dönem genelinde (kasiyer bazlı DEĞİL, bkz. aşağıdaki Kasiyer Raporu
        // bunun kullanıcı filtreli hali) - RestaurantOrderLine.IsComplimentary madde 11'in kendi
        // ayrımı, satır satır (hangi fiş, ne zaman, ne kadar) listelenir.
        if (periodSaleIds.Count > 0)
        {
            var periodCheckIds = daySales.Where(x => periodSaleIds.Contains(x.Id)).Select(x => x.RestaurantCheckId).ToList();
            var discountLines = await dbContext.RestaurantOrderLines
                .AsNoTracking()
                .Where(x => periodCheckIds.Contains(x.RestaurantOrder.RestaurantCheckId) && x.Status != RestaurantOrderLineStatus.Cancelled && x.DiscountAmountSnapshot > 0)
                .Select(x => new
                {
                    x.IsComplimentary,
                    x.DiscountAmountSnapshot,
                    CheckId = x.RestaurantOrder.RestaurantCheckId
                })
                .ToListAsync();

            vm.DiscountTotal = discountLines.Where(x => !x.IsComplimentary).Sum(x => x.DiscountAmountSnapshot);
            vm.ComplimentaryTotal = discountLines.Where(x => x.IsComplimentary).Sum(x => x.DiscountAmountSnapshot);

            var saleByCheckId = daySales.Where(x => periodSaleIds.Contains(x.Id)).ToDictionary(x => x.RestaurantCheckId);
            vm.DiscountComplimentaryRows = discountLines
                .GroupBy(x => new { x.CheckId, x.IsComplimentary })
                .Where(g => saleByCheckId.ContainsKey(g.Key.CheckId))
                .Select(g =>
                {
                    var sale = saleByCheckId[g.Key.CheckId];
                    var sourceType = SourceTypeOf(sale.SectionName);
                    var isPackage = sourceType == "package";
                    var pkg = isPackage && packageNumbers.TryGetValue(sale.RestaurantCheckId, out var p) ? p : null;
                    var sourceLabel = isPackage ? pkg?.PackageNumber ?? sale.TableName : sourceType == "self" ? "Self Satış" : sale.TableName;
                    return new RestaurantDiscountComplimentaryRowViewModel(sale.IssuedAtUtc, sale.DocumentNumber, sourceLabel, g.Key.IsComplimentary, null, g.Sum(x => x.DiscountAmountSnapshot));
                })
                .OrderByDescending(x => x.IssuedAtUtc)
                .ToList();
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

        // Z Dönem Kapatma test talimatı (2026-09-06) - X/Z/Z Listesi artık RestaurantCashShift
        // ZAMAN ARALIĞI TAHMİNİ yerine RestaurantZPeriod'un GERÇEK FK ilişkisini kullanıyor
        // (bkz. RestaurantZPeriod.cs/GetOrCreateActiveZPeriodIdAsync yorumu). Vardiya (openShift,
        // yukarıda) BİLEREK dokunulmadı - o hâlâ RestaurantCashShift/kasiyer kasa açılışı.
        var lastClosedZPeriod = await dbContext.RestaurantZPeriods
            .AsNoTracking()
            .Where(x => x.Status == RestaurantZPeriodStatus.Closed)
            .OrderByDescending(x => x.ClosedAtUtc)
            .FirstOrDefaultAsync();
        var activeZPeriod = await dbContext.RestaurantZPeriods
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Status == RestaurantZPeriodStatus.Open);

        vm.IsShiftOpen = openShift is not null;
        vm.OpenShiftId = openShift?.Id;
        vm.ShiftOpenedAtUtc = openShift?.OpenedAtUtc;
        vm.FinancialAccountName = openShift?.FinancialAccount.Name;
        vm.LastZNumber = lastClosedZPeriod is null ? null : $"Z-{lastClosedZPeriod.Id:D6}";
        vm.LastZClosedAtUtc = lastClosedZPeriod?.ClosedAtUtc;

        // X Raporu artık AÇIK VARDİYA ŞART DEĞİL (Edip, 2026-09-03: "X ve Z raporu almak için
        // vardiya girilmesi zorunlu değil") - dönem sınırı artık aktif RestaurantZPeriod'un GERÇEK
        // açılış zamanı (tahmin değil). Açık bir vardiya varsa onun açılış saati bilgi amaçlı
        // gösterilir, dönemi BELİRLEMEZ.
        var xPeriodStartUtc = activeZPeriod?.OpenedAtUtc ?? DateTime.Now.Date.ToUniversalTime();

        var periodSalesForX = await dbContext.RetailSales
            .AsNoTracking()
            .Where(x => activeZPeriod != null ? x.RestaurantZPeriodId == activeZPeriod.Id : x.IssuedAtUtc >= xPeriodStartUtc)
            .Where(x => x.Status != RetailSaleStatus.Cancelled)
            .Select(x => new { x.RestaurantCheckId })
            .ToListAsync();
        var periodCheckIdsForX = periodSalesForX.Select(x => x.RestaurantCheckId).ToList();
        var periodPayments = periodCheckIdsForX.Count == 0
            ? []
            : await dbContext.RestaurantPayments
                .AsNoTracking()
                .Where(x => periodCheckIdsForX.Contains(x.RestaurantCheckId) && !x.IsReversal)
                .ToListAsync();
        var periodReceiptCount = periodSalesForX.Count;

        var (xBreakdown, _, _) = BuildPaymentBreakdown(periodPayments.Select(x => (x.PaymentMethod, x.Amount)));
        // Ödenmez iş kuralı (2026-09-06, Edip) - finansal karşılığı olmadığı için Net Ciro'ya
        // dahil edilmez, PaymentBreakdown'da (xBreakdown, üstte) kendi satırında ayrıca görünür.
        vm.XReport = new RestaurantXReportViewModel
        {
            OpenedAtUtc = openShift?.OpenedAtUtc ?? xPeriodStartUtc,
            ReceiptCount = periodReceiptCount,
            NetRevenue = periodPayments.Where(x => x.PaymentMethod != RestaurantPaymentMethod.Unpaid).Sum(x => x.Amount),
            PaymentBreakdown = xBreakdown
        };

        // Z Dönem Kapatma test talimatı (2026-09-06) - Z Listesi/Z Detay artık RestaurantZPeriod'un
        // GERÇEK FK ilişkisinden (RetailSale.RestaurantZPeriodId) okunuyor, zaman aralığı TAHMİNİ
        // değil. OpeningBalance/ExpectedBalance/CountedBalance (eski Vardiya-tabanlı kasa sayımı
        // alanları) Z dönemi için ANLAMSIZ - null bırakılır, view artık Fiş Sayısı/Net Ciro/ödeme
        // kırılımını (Summary) gösteriyor.
        var closedPeriodsRaw = await dbContext.RestaurantZPeriods
            .AsNoTracking()
            .Where(x => x.Status == RestaurantZPeriodStatus.Closed)
            .OrderByDescending(x => x.ClosedAtUtc)
            .Take(30)
            .ToListAsync();
        vm.ZList = closedPeriodsRaw.Select(p => new RestaurantZListRowViewModel(
            p.Id, $"Z-{p.Id:D6}", "", p.OpenedAtUtc, p.ClosedAtUtc!.Value, 0, null, null,
            new RestaurantZSummaryViewModel { ReceiptCount = p.ReceiptCount, GrossTotal = p.GrossTotal, DiscountTotal = p.DiscountTotal, NetTotal = p.NetTotal, TaxTotal = p.TaxTotal, ComplimentaryTotal = p.ComplimentaryTotal }))
            .ToList();

        // Z Listesi'nde bir Z'ye tıklanınca o dönemin GERÇEKTEN BAĞLI (FK) satış hareketleri -
        // Edip'in isteği (2026-09-03), artık RestaurantZPeriodId ile. Düzenleme/silme burada YOK -
        // reversal muhasebe kuralına aykırı olur; "Fişi Gör" modalındaki "Fişi Sil" (madde 10, Z
        // dönem kapatma test talimatı) AYRI, kendi onayıyla çalışan bir akış.
        if (zShiftId is not null)
        {
            var selectedPeriod = await dbContext.RestaurantZPeriods
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == zShiftId.Value && x.Status == RestaurantZPeriodStatus.Closed);

            if (selectedPeriod is not null)
            {
                var zSales = await dbContext.RetailSales
                    .AsNoTracking()
                    .Where(x => x.RestaurantZPeriodId == selectedPeriod.Id)
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
                vm.SelectedZNumber = $"Z-{selectedPeriod.Id:D6}";
                vm.SelectedZSummary = new RestaurantZSummaryViewModel
                {
                    ReceiptCount = selectedPeriod.ReceiptCount,
                    GrossTotal = selectedPeriod.GrossTotal,
                    DiscountTotal = selectedPeriod.DiscountTotal,
                    NetTotal = selectedPeriod.NetTotal,
                    TaxTotal = selectedPeriod.TaxTotal,
                    ComplimentaryTotal = selectedPeriod.ComplimentaryTotal
                };
                var zPaymentsForBreakdown = zCheckIds.Count == 0
                    ? []
                    : await dbContext.RestaurantPayments.AsNoTracking().Where(x => zCheckIds.Contains(x.RestaurantCheckId) && !x.IsReversal).Select(x => new { x.PaymentMethod, x.Amount }).ToListAsync();
                (vm.SelectedZSummary.PaymentBreakdown, _, _) = BuildPaymentBreakdown(zPaymentsForBreakdown.Select(x => (x.PaymentMethod, x.Amount)));
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
                        // GERÇEK HATA (2026-09-06, Z düzeltme kabul testinde bulundu) - bu satır içi
                        // switch yalnızca Nakit/Kredi Kartı'nı tanıyordu, geri kalan HER ödeme türünü
                        // (Açık Hesap, Ödenmez, Yemek Kartı) "Yemek Kartı" olarak etiketliyordu - Fiş
                        // Gör penceresinde bir Açık Hesap tahsilatı yanlışlıkla Yemek Kartı gösteriliyordu.
                        // Dosyanın kendi PaymentMethodLabel() yardımcısı (satır 32) zaten TÜM türleri
                        // doğru eşliyor, burada da o kullanılır.
                        PaymentMethodLabel(p.PaymentMethod),
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

        // Ödenmez iş kuralı (2026-09-06, Edip) - finansal karşılığı olmadığı için Net Ciro'ya
        // dahil edilmez ve "DİĞER TAHSİLATLAR" (Açık Hesap/Yemek Kartı) ile KARIŞTIRILMAZ, kendi
        // alanında ayrıca gösterilir. Aynı kural Dashboard/X Raporu/Z snapshot'ta da uygulanır.
        var kasiyerUnpaid = kasiyerPayments.Where(x => x.PaymentMethod == RestaurantPaymentMethod.Unpaid).Sum(x => x.Amount);

        vm.Kasiyer = new RestaurantKasiyerReportViewModel
        {
            DisplayName = string.IsNullOrEmpty(effectiveKasiyerUserId) ? "Tüm Kasiyerler" : openerNames.GetValueOrDefault(effectiveKasiyerUserId, "Kasiyer"),
            NetRevenue = kasiyerNonCancelled.Sum(x => x.GrandTotal) - kasiyerUnpaid,
            ReceiptCount = kasiyerNonCancelled.Count,
            Cash = kasiyerPayments.Where(x => x.PaymentMethod == RestaurantPaymentMethod.Cash).Sum(x => x.Amount),
            Card = kasiyerPayments.Where(x => x.PaymentMethod == RestaurantPaymentMethod.CreditCard).Sum(x => x.Amount),
            Unpaid = kasiyerUnpaid,
            OtherCollections = kasiyerPayments.Where(x => x.PaymentMethod is not (RestaurantPaymentMethod.Cash or RestaurantPaymentMethod.CreditCard or RestaurantPaymentMethod.Unpaid)).Sum(x => x.Amount),
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
    // bir gün sonu, gerçek bir kasa mutabakatı değil. 2026-09-06 Z Dönem Kapatma test talimatı:
    // eski RestaurantCashShift/zaman-aralığı tahminli CreateDirectZReportAsync YERİNE artık
    // RestaurantZPeriod'un GERÇEK FK ilişkisiyle çalışan CloseActiveZPeriodAsync çağrılıyor.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDirectZReport()
    {
        try
        {
            var period = await postingService.CloseActiveZPeriodAsync(CurrentUserId);
            TempData["Success"] = $"Z-{period.Id:D6} raporu oluşturuldu, gün sıfırlandı.";
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
