using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class HomeController(ApplicationDbContext dbContext, OverdueScheduleService overdueScheduleService) : Controller
{
    public async Task<IActionResult> Index(string? period, DateTime? from, DateTime? to)
    {
        var today = DateTime.UtcNow.Date;
        var selectedPeriod = string.IsNullOrWhiteSpace(period) ? "today" : period.ToLowerInvariant();
        var (filterFrom, filterTo, periodLabel) = ResolvePeriod(selectedPeriod, from, to, today);
        var filterToExclusive = filterTo.AddDays(1);

        var invoiceQuery = dbContext.Invoices.AsNoTracking()
            .Where(x => x.Status == InvoiceStatus.Approved &&
                        x.InvoiceDateUtc >= filterFrom &&
                        x.InvoiceDateUtc < filterToExclusive);

        var salesInvoiceTotal = await invoiceQuery
            .Where(x => x.InvoiceType == InvoiceType.Sales)
            .SumAsync(x => (decimal?)x.GrandTotal) ?? 0;
        var purchaseInvoiceTotal = await invoiceQuery
            .Where(x => x.InvoiceType == InvoiceType.Purchase)
            .SumAsync(x => (decimal?)x.GrandTotal) ?? 0;

        var receiptQuery = dbContext.PaymentReceipts.AsNoTracking()
            .Where(x => x.Status == PaymentReceiptStatus.Approved &&
                        x.ReceiptDateUtc >= filterFrom &&
                        x.ReceiptDateUtc < filterToExclusive);
        var collectionTotal = await receiptQuery
            .Where(x => x.ReceiptType == ReceiptType.Collection)
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;
        var paymentTotal = await receiptQuery
            .Where(x => x.ReceiptType == ReceiptType.Payment)
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

        // Çek/Senet Tahsil Edildi/Ödendi (bkz. NegotiableInstrumentPostingService.SettleAsync) bir
        // PaymentReceipt oluşturmuyor — bu yüzden yukarıdaki PaymentReceipts sorgusuna hiç girmez.
        // Diğer kartlar gibi (Fatura/Sipariş) "şu an geçerli durum" mantığıyla: yalnızca hâlâ
        // Tahsil Edildi/Ödendi durumunda olan (İptal edilmemiş) kayıtlar, SettledAtUtc'ye göre sayılır.
        var instrumentSettlementQuery = dbContext.NegotiableInstruments.AsNoTracking()
            .Where(x => x.SettledAtUtc != null &&
                        x.SettledAtUtc >= filterFrom &&
                        x.SettledAtUtc < filterToExclusive);
        var instrumentCollectionTotal = await instrumentSettlementQuery
            .Where(x => x.Status == InstrumentStatus.Collected)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;
        var instrumentPaymentTotal = await instrumentSettlementQuery
            .Where(x => x.Status == InstrumentStatus.Paid)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;
        collectionTotal += instrumentCollectionTotal;
        paymentTotal += instrumentPaymentTotal;

        // Borç/alacak kartları tarih filtresinden bağımsız olarak güncel cari pozisyonu gösterir.
        var customerBalances = await dbContext.CurrentAccountTransactions.AsNoTracking()
            .GroupBy(x => x.CustomerId)
            .Select(g => g.Sum(x => x.Debit) - g.Sum(x => x.Credit))
            .ToListAsync();
        var totalReceivable = customerBalances.Where(x => x > 0).Sum();
        var totalDebt = customerBalances.Where(x => x < 0).Sum(x => -x);

        var chartSince = today.AddDays(-13);
        var invoiceChartRaw = await dbContext.Invoices.AsNoTracking()
            .Where(x => x.Status == InvoiceStatus.Approved && x.InvoiceDateUtc >= chartSince)
            .GroupBy(x => new { Date = x.InvoiceDateUtc.Date, x.InvoiceType })
            .Select(g => new { g.Key.Date, g.Key.InvoiceType, Total = g.Sum(x => x.GrandTotal) })
            .ToListAsync();

        var cashChartRaw = await dbContext.PaymentReceipts.AsNoTracking()
            .Where(x => x.Status == PaymentReceiptStatus.Approved && x.ReceiptDateUtc >= chartSince)
            .GroupBy(x => new { Date = x.ReceiptDateUtc.Date, x.ReceiptType })
            .Select(g => new { g.Key.Date, g.Key.ReceiptType, Total = g.Sum(x => x.TotalAmount) })
            .ToListAsync();

        var instrumentChartRaw = await dbContext.NegotiableInstruments.AsNoTracking()
            .Where(x => x.SettledAtUtc != null && x.SettledAtUtc >= chartSince &&
                        (x.Status == InstrumentStatus.Collected || x.Status == InstrumentStatus.Paid))
            .GroupBy(x => new { Date = x.SettledAtUtc!.Value.Date, x.Status })
            .Select(g => new { g.Key.Date, g.Key.Status, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var dailyInvoiceStats = new List<DailyInvoiceStat>(14);
        var dailyCashFlowStats = new List<DailyCashFlowStat>(14);
        for (var i = 0; i < 14; i++)
        {
            var date = chartSince.AddDays(i);
            dailyInvoiceStats.Add(new DailyInvoiceStat
            {
                DateUtc = date,
                SalesTotal = invoiceChartRaw.FirstOrDefault(x => x.Date == date && x.InvoiceType == InvoiceType.Sales)?.Total ?? 0,
                PurchaseTotal = invoiceChartRaw.FirstOrDefault(x => x.Date == date && x.InvoiceType == InvoiceType.Purchase)?.Total ?? 0
            });
            dailyCashFlowStats.Add(new DailyCashFlowStat
            {
                DateUtc = date,
                CollectionTotal = (cashChartRaw.FirstOrDefault(x => x.Date == date && x.ReceiptType == ReceiptType.Collection)?.Total ?? 0) +
                    (instrumentChartRaw.FirstOrDefault(x => x.Date == date && x.Status == InstrumentStatus.Collected)?.Total ?? 0),
                PaymentTotal = (cashChartRaw.FirstOrDefault(x => x.Date == date && x.ReceiptType == ReceiptType.Payment)?.Total ?? 0) +
                    (instrumentChartRaw.FirstOrDefault(x => x.Date == date && x.Status == InstrumentStatus.Paid)?.Total ?? 0)
            });
        }

        // Faturanın kendi PaidAmount'ına değil müşterinin GERÇEK cari bakiyesine göre kalan tutarı
        // hesaplar — elle girilen Tahsilat/Tediye fişleri belirli bir faturaya bağlanmadığından
        // (yalnızca Kapalı Fatura otomatik ödemesi bağlanır) PaidAmount tek başına güvenilir değil;
        // bkz. OverdueScheduleService.
        var upcomingUntil = today.AddDays(8);
        var nettedSchedules = await overdueScheduleService.GetNettedSchedulesAsync(upcomingUntil, invoiceType: null, HttpContext.RequestAborted);
        var upcomingPayments = nettedSchedules
            .Take(6)
            .Select(x => new UpcomingPaymentItem
            {
                InvoiceId = x.InvoiceId,
                InvoiceNumber = x.InvoiceNumber,
                CustomerName = x.CustomerName,
                DueDateUtc = x.DueDateUtc,
                RemainingAmount = x.RemainingAmount,
                DaysUntilDue = (x.DueDateUtc.Date - today).Days
            }).ToList();

        var stockSnapshot = await dbContext.Products.AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => new
            {
                x.Id,
                x.StockCode,
                x.Name,
                x.StockQuantity,
                x.MinimumStockQuantity,
                x.Unit,
                x.TrackStock
            })
            .ToListAsync();
        var trackedStocks = stockSnapshot.Where(x => x.TrackStock).ToList();
        var outOfStockCount = trackedStocks.Count(x => x.StockQuantity <= 0);
        var criticalStockCount = trackedStocks.Count(x => x.StockQuantity > 0 && x.StockQuantity <= x.MinimumStockQuantity);
        var normalStockCount = trackedStocks.Count - outOfStockCount - criticalStockCount;
        var criticalStocks = trackedStocks
            .Where(x => x.StockQuantity <= x.MinimumStockQuantity)
            .OrderBy(x => x.StockQuantity <= 0 ? 0 : 1)
            .ThenBy(x => x.StockQuantity)
            .Take(5)
            .Select(x => new CriticalStockItem
            {
                ProductId = x.Id,
                StockCode = x.StockCode,
                ProductName = x.Name,
                StockQuantity = x.StockQuantity,
                MinimumStockQuantity = x.MinimumStockQuantity,
                Unit = x.Unit
            })
            .ToList();

        var recentInvoices = await dbContext.Invoices.AsNoTracking()
            .Where(x => x.Status == InvoiceStatus.Approved)
            .OrderByDescending(x => x.ApprovedAtUtc ?? x.InvoiceDateUtc)
            .Take(5)
            .Select(x => new RecentActivityItem
            {
                DateUtc = x.ApprovedAtUtc ?? x.InvoiceDateUtc,
                Kind = "Fatura",
                Title = x.InvoiceType == InvoiceType.Sales ? "Satış faturası" : "Alış faturası",
                DocumentNumber = x.InvoiceNumber,
                Description = x.Customer.Name,
                Amount = x.GrandTotal,
                Controller = "Invoices",
                EntityId = x.Id,
                Tone = x.InvoiceType == InvoiceType.Sales ? "success" : "info"
            })
            .ToListAsync();
        var recentReceipts = await dbContext.PaymentReceipts.AsNoTracking()
            .Where(x => x.Status == PaymentReceiptStatus.Approved)
            .OrderByDescending(x => x.ApprovedAtUtc ?? x.ReceiptDateUtc)
            .Take(5)
            .Select(x => new RecentActivityItem
            {
                DateUtc = x.ApprovedAtUtc ?? x.ReceiptDateUtc,
                Kind = x.ReceiptType == ReceiptType.Collection ? "Tahsilat" : "Tediye",
                Title = x.ReceiptType == ReceiptType.Collection ? "Tahsilat kaydedildi" : "Tediye kaydedildi",
                DocumentNumber = x.ReceiptNumber,
                Description = x.Customer.Name,
                Amount = x.TotalAmount,
                Controller = "PaymentReceipts",
                EntityId = x.Id,
                Tone = x.ReceiptType == ReceiptType.Collection ? "success" : "danger"
            })
            .ToListAsync();
        var recentOrders = await dbContext.BusinessOrders.AsNoTracking()
            .Where(x => x.Status != BusinessDocumentStatus.Draft && x.Status != BusinessDocumentStatus.Cancelled)
            .OrderByDescending(x => x.UpdatedAtUtc ?? x.OrderDateUtc)
            .Take(4)
            .Select(x => new RecentActivityItem
            {
                DateUtc = x.UpdatedAtUtc ?? x.OrderDateUtc,
                Kind = "Sipariş",
                Title = x.OrderType == InvoiceType.Sales ? "Satış siparişi" : "Alış siparişi",
                DocumentNumber = x.OrderNumber,
                Description = x.Customer.Name,
                Amount = x.GrandTotal,
                Controller = "BusinessOrders",
                EntityId = x.Id,
                Tone = "warning"
            })
            .ToListAsync();
        var recentInstrumentsRaw = await dbContext.NegotiableInstruments.AsNoTracking()
            .OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
            .Take(4)
            .Select(x => new
            {
                x.Id,
                DateUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                x.InstrumentType,
                x.Status,
                x.InstrumentNumber,
                CustomerName = x.Customer.Name,
                x.Amount
            })
            .ToListAsync();
        var recentInstruments = recentInstrumentsRaw.Select(x => new RecentActivityItem
        {
            DateUtc = x.DateUtc,
            Kind = x.InstrumentType == NegotiableInstrumentType.Cheque ? "Çek" : "Senet",
            Title = InstrumentActivityTitle(x.InstrumentType, x.Status),
            DocumentNumber = x.InstrumentNumber,
            Description = x.CustomerName,
            Amount = x.Amount,
            Controller = "NegotiableInstruments",
            EntityId = x.Id,
            Tone = InstrumentActivityTone(x.Status)
        }).ToList();
        var recentActivities = recentInvoices
            .Concat(recentReceipts)
            .Concat(recentOrders)
            .Concat(recentInstruments)
            .OrderByDescending(x => x.DateUtc)
            .Take(6)
            .ToList();

        var draftInvoiceCount = await dbContext.Invoices.AsNoTracking()
            .CountAsync(x => x.Status == InvoiceStatus.Draft);
        var pendingOrderCount = await dbContext.BusinessOrders.AsNoTracking()
            .CountAsync(x => x.Status == BusinessDocumentStatus.Draft);
        var overdueReceivableCount = nettedSchedules.Count(x => x.InvoiceType == InvoiceType.Sales && x.DueDateUtc < today);
        var overduePayableCount = nettedSchedules.Count(x => x.InvoiceType == InvoiceType.Purchase && x.DueDateUtc < today);

        // Portföyde bekleyen (henüz Tahsil Edildi/Ödendi/İptal olmamış) çek/senetler — dashboard'da
        // hiç görünmüyordu, bkz. "dashbord çek ile ilgili bölüm de yok" geri bildirimi.
        var portfolioInstruments = await dbContext.NegotiableInstruments.AsNoTracking()
            .Where(x => x.Status == InstrumentStatus.Portfolio || x.Status == InstrumentStatus.Endorsed)
            .Select(x => new { x.Amount, x.DueDateUtc })
            .ToListAsync();
        var portfolioInstrumentCount = portfolioInstruments.Count;
        var portfolioInstrumentTotal = portfolioInstruments.Sum(x => x.Amount);
        var dueSoonInstrumentCount = portfolioInstruments.Count(x => x.DueDateUtc < upcomingUntil);

        return View(new DashboardViewModel
        {
            PurchaseInvoiceTotal = purchaseInvoiceTotal,
            SalesInvoiceTotal = salesInvoiceTotal,
            CollectionTotal = collectionTotal,
            PaymentTotal = paymentTotal,
            TotalDebt = totalDebt,
            TotalReceivable = totalReceivable,
            FilterFrom = filterFrom,
            FilterTo = filterTo,
            Period = selectedPeriod,
            PeriodLabel = periodLabel,
            DailyInvoiceStats = dailyInvoiceStats,
            DailyCashFlowStats = dailyCashFlowStats,
            UpcomingPayments = upcomingPayments,
            NormalStockCount = normalStockCount,
            CriticalStockCount = criticalStockCount,
            OutOfStockCount = outOfStockCount,
            UntrackedStockCount = stockSnapshot.Count(x => !x.TrackStock),
            CriticalStocks = criticalStocks,
            RecentActivities = recentActivities,
            DraftInvoiceCount = draftInvoiceCount,
            PendingOrderCount = pendingOrderCount,
            OverdueReceivableCount = overdueReceivableCount,
            OverduePayableCount = overduePayableCount,
            PortfolioInstrumentCount = portfolioInstrumentCount,
            PortfolioInstrumentTotal = portfolioInstrumentTotal,
            DueSoonInstrumentCount = dueSoonInstrumentCount
        });
    }

    private static string InstrumentActivityTitle(NegotiableInstrumentType type, InstrumentStatus status)
    {
        var typeLabel = type == NegotiableInstrumentType.Cheque ? "Çek" : "Senet";
        return status switch
        {
            InstrumentStatus.Collected => $"{typeLabel} tahsil edildi",
            InstrumentStatus.Paid => $"{typeLabel} ödendi",
            InstrumentStatus.Endorsed => $"{typeLabel} ciro edildi",
            InstrumentStatus.Protested => $"{typeLabel} karşılıksız çıktı",
            InstrumentStatus.Returned => $"{typeLabel} iade edildi",
            InstrumentStatus.Cancelled => $"{typeLabel} iptal edildi",
            _ => $"{typeLabel} kaydedildi"
        };
    }

    private static string InstrumentActivityTone(InstrumentStatus status) => status switch
    {
        InstrumentStatus.Collected or InstrumentStatus.Endorsed => "success",
        InstrumentStatus.Paid => "info",
        InstrumentStatus.Protested or InstrumentStatus.Cancelled => "danger",
        InstrumentStatus.Returned => "warning",
        _ => "warning"
    };

    private static (DateTime From, DateTime To, string Label) ResolvePeriod(
        string period,
        DateTime? from,
        DateTime? to,
        DateTime today)
    {
        if (period == "custom" && from.HasValue && to.HasValue)
        {
            var customFrom = DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc);
            var customTo = DateTime.SpecifyKind(to.Value.Date, DateTimeKind.Utc);
            if (customFrom > customTo)
            {
                (customFrom, customTo) = (customTo, customFrom);
            }
            return (customFrom, customTo, $"{customFrom:dd.MM.yyyy} – {customTo:dd.MM.yyyy}");
        }

        return period switch
        {
            "week" => (today.AddDays(-6), today, "Son 7 Gün"),
            "month" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), today, "Bu Ay"),
            _ => (today, today, "Bugün")
        };
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
