using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager},{AppRoles.Waiter},{AppRoles.Cashier}")]
public sealed class RestaurantDashboardController(ApplicationDbContext dbContext) : RestaurantControllerBase(dbContext)
{
    public async Task<IActionResult> Index()
    {
        ActivePage = "dashboard";

        var todayStartUtc = DateTime.Now.Date.ToUniversalTime();
        var todayEndUtc = todayStartUtc.AddDays(1);

        var todaySales = await dbContext.RetailSales
            .AsNoTracking()
            .Where(x => x.IssuedAtUtc >= todayStartUtc && x.IssuedAtUtc < todayEndUtc && x.Status != RetailSaleStatus.Cancelled)
            .Select(x => new { x.GrandTotal, x.IssuedAtUtc })
            .ToListAsync();

        var todayPayments = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => x.PaidAtUtc >= todayStartUtc && x.PaidAtUtc < todayEndUtc && !x.IsReversal)
            .GroupBy(x => x.PaymentMethod)
            .Select(g => new { Method = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var pendingTickets = await dbContext.KitchenTicketLines
            .AsNoTracking()
            .Where(x => x.Status == KitchenTicketLineStatus.Sent || x.Status == KitchenTicketLineStatus.InProgress)
            .Select(x => x.KitchenTicket.SentAtUtc)
            .ToListAsync();

        var recentClosedChecks = await dbContext.RetailSales
            .AsNoTracking()
            .Where(x => x.Status != RetailSaleStatus.Cancelled)
            .OrderByDescending(x => x.IssuedAtUtc)
            .Take(6)
            .Select(x => new
            {
                x.IssuedAtUtc,
                x.GrandTotal,
                TableName = x.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name,
                Payments = x.RestaurantCheck.Payments.Where(p => !p.IsReversal).Select(p => p.PaymentMethod).Distinct().ToList()
            })
            .ToListAsync();

        static string PaymentSummary(List<RestaurantPaymentMethod> methods) => methods.Count switch
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

        var hourlyRevenue = new List<decimal>();
        var startHour = Math.Max(0, DateTime.Now.Hour - 11);
        for (var hour = startHour; hour <= DateTime.Now.Hour; hour++)
        {
            var hourTotal = todaySales
                .Where(x => x.IssuedAtUtc.ToLocalTime().Hour == hour)
                .Sum(x => x.GrandTotal);
            hourlyRevenue.Add(hourTotal);
        }

        var model = new RestaurantDashboardViewModel
        {
            NetRevenueToday = todaySales.Sum(x => x.GrandTotal),
            ClosedReceiptCountToday = todaySales.Count,
            CashCollectedToday = todayPayments.FirstOrDefault(x => x.Method == RestaurantPaymentMethod.Cash)?.Total ?? 0,
            CreditCardCollectedToday = todayPayments.FirstOrDefault(x => x.Method == RestaurantPaymentMethod.CreditCard)?.Total ?? 0,
            MealCardCollectedToday = todayPayments.FirstOrDefault(x => x.Method == RestaurantPaymentMethod.MealCard)?.Total ?? 0,
            KitchenPendingCount = pendingTickets.Count,
            KitchenLongestWaitMinutes = pendingTickets.Count == 0 ? 0 : (int)pendingTickets.Min(x => x).Subtract(DateTime.UtcNow).Duration().TotalMinutes,
            HourlyRevenue = hourlyRevenue,
            HourlyRevenueStartHour = startHour,
            RecentMovements = recentClosedChecks.Select(x => new RestaurantDashboardMovement
            {
                AtUtc = x.IssuedAtUtc,
                Title = $"{x.TableName} kapatıldı",
                Subtitle = PaymentSummary(x.Payments),
                Amount = x.GrandTotal
            }).ToList()
        };

        return View(model);
    }
}
