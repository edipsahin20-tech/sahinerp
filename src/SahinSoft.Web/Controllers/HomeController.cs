using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class HomeController(ApplicationDbContext dbContext) : Controller
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
                CollectionTotal = cashChartRaw.FirstOrDefault(x => x.Date == date && x.ReceiptType == ReceiptType.Collection)?.Total ?? 0,
                PaymentTotal = cashChartRaw.FirstOrDefault(x => x.Date == date && x.ReceiptType == ReceiptType.Payment)?.Total ?? 0
            });
        }

        var upcomingUntil = today.AddDays(7);
        var upcomingRaw = await dbContext.InvoicePaymentSchedules.AsNoTracking()
            .Where(x => x.Invoice.Status == InvoiceStatus.Approved &&
                        x.DueDateUtc < upcomingUntil.AddDays(1) &&
                        x.Amount > x.PaidAmount)
            .OrderBy(x => x.DueDateUtc)
            .ThenBy(x => x.Id)
            .Take(6)
            .Select(x => new
            {
                x.InvoiceId,
                x.Invoice.InvoiceNumber,
                CustomerName = x.Invoice.Customer.Name,
                x.DueDateUtc,
                RemainingAmount = x.Amount - x.PaidAmount
            })
            .ToListAsync();
        var upcomingPayments = upcomingRaw.Select(x => new UpcomingPaymentItem
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
        var recentActivities = recentInvoices
            .Concat(recentReceipts)
            .Concat(recentOrders)
            .OrderByDescending(x => x.DateUtc)
            .Take(6)
            .ToList();

        var draftInvoiceCount = await dbContext.Invoices.AsNoTracking()
            .CountAsync(x => x.Status == InvoiceStatus.Draft);
        var pendingOrderCount = await dbContext.BusinessOrders.AsNoTracking()
            .CountAsync(x => x.Status == BusinessDocumentStatus.Draft);
        var overdueReceivableCount = await dbContext.InvoicePaymentSchedules.AsNoTracking()
            .CountAsync(x => x.Invoice.Status == InvoiceStatus.Approved &&
                             x.Invoice.InvoiceType == InvoiceType.Sales &&
                             x.DueDateUtc < today && x.Amount > x.PaidAmount);
        var overduePayableCount = await dbContext.InvoicePaymentSchedules.AsNoTracking()
            .CountAsync(x => x.Invoice.Status == InvoiceStatus.Approved &&
                             x.Invoice.InvoiceType == InvoiceType.Purchase &&
                             x.DueDateUtc < today && x.Amount > x.PaidAmount);

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
            OverduePayableCount = overduePayableCount
        });
    }

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
