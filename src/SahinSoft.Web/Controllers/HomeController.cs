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
    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var since = DateTime.UtcNow.Date.AddDays(-13);

        var dailyRaw = await dbContext.Invoices
            .Where(x => x.Status == InvoiceStatus.Approved && x.InvoiceDateUtc >= since)
            .GroupBy(x => new { Date = x.InvoiceDateUtc.Date, x.InvoiceType })
            .Select(g => new
            {
                g.Key.Date,
                g.Key.InvoiceType,
                Count = g.Count(),
                Total = g.Sum(x => x.GrandTotal)
            })
            .ToListAsync();

        var dailyStats = new List<DailyInvoiceStat>();
        for (var i = 0; i < 14; i++)
        {
            var date = since.AddDays(i);
            var sales = dailyRaw.SingleOrDefault(x => x.Date == date && x.InvoiceType == InvoiceType.Sales);
            var purchase = dailyRaw.SingleOrDefault(x => x.Date == date && x.InvoiceType == InvoiceType.Purchase);
            dailyStats.Add(new DailyInvoiceStat
            {
                DateUtc = date,
                SalesCount = sales?.Count ?? 0,
                SalesTotal = sales?.Total ?? 0,
                PurchaseCount = purchase?.Count ?? 0,
                PurchaseTotal = purchase?.Total ?? 0
            });
        }

        var topCustomers = await dbContext.CurrentAccountTransactions
            .GroupBy(x => x.CustomerId)
            .Select(g => new { CustomerId = g.Key, Balance = g.Sum(x => x.Debit) - g.Sum(x => x.Credit) })
            .Where(x => x.Balance > 0)
            .OrderByDescending(x => x.Balance)
            .Take(5)
            .Join(dbContext.Customers, x => x.CustomerId, c => c.Id, (x, c) => new TopCustomerStat
            {
                CustomerName = c.Name,
                Balance = x.Balance
            })
            .ToListAsync();

        var fromUtc = from.HasValue ? DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc) : (DateTime?)null;
        var toUtc = to.HasValue ? DateTime.SpecifyKind(to.Value.Date.AddDays(1), DateTimeKind.Utc) : (DateTime?)null;

        var invoiceTotalsQuery = dbContext.Invoices
            .Where(x => x.Status == InvoiceStatus.Approved);
        if (fromUtc.HasValue)
        {
            invoiceTotalsQuery = invoiceTotalsQuery.Where(x => x.InvoiceDateUtc >= fromUtc.Value);
        }
        if (toUtc.HasValue)
        {
            invoiceTotalsQuery = invoiceTotalsQuery.Where(x => x.InvoiceDateUtc < toUtc.Value);
        }

        var purchaseInvoiceTotal = await invoiceTotalsQuery
            .Where(x => x.InvoiceType == InvoiceType.Purchase)
            .SumAsync(x => (decimal?)x.GrandTotal) ?? 0;
        var salesInvoiceTotal = await invoiceTotalsQuery
            .Where(x => x.InvoiceType == InvoiceType.Sales)
            .SumAsync(x => (decimal?)x.GrandTotal) ?? 0;

        var balanceQuery = dbContext.CurrentAccountTransactions.AsQueryable();
        if (fromUtc.HasValue)
        {
            balanceQuery = balanceQuery.Where(x => x.TransactionDateUtc >= fromUtc.Value);
        }
        if (toUtc.HasValue)
        {
            balanceQuery = balanceQuery.Where(x => x.TransactionDateUtc < toUtc.Value);
        }

        var customerBalances = await balanceQuery
            .GroupBy(x => x.CustomerId)
            .Select(g => g.Sum(x => x.Debit) - g.Sum(x => x.Credit))
            .ToListAsync();
        var totalReceivable = customerBalances.Where(b => b > 0).Sum();
        var totalDebt = customerBalances.Where(b => b < 0).Sum(b => -b);

        var model = new DashboardViewModel
        {
            PurchaseInvoiceTotal = purchaseInvoiceTotal,
            SalesInvoiceTotal = salesInvoiceTotal,
            TotalDebt = totalDebt,
            TotalReceivable = totalReceivable,
            FilterFrom = from,
            FilterTo = to,
            DailyInvoiceStats = dailyStats,
            TopCustomers = topCustomers
        };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
