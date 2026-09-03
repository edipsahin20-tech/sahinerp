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
    private static readonly PackageOrderStatus[] ActivePackageStatuses =
        [PackageOrderStatus.Preparing, PackageOrderStatus.Ready, PackageOrderStatus.CourierWaiting, PackageOrderStatus.OnTheWay];

    public async Task<IActionResult> Index()
    {
        ActivePage = "dashboard";

        var todayStartUtc = DateTime.Now.Date.ToUniversalTime();
        var todayEndUtc = todayStartUtc.AddDays(1);
        var nowUtc = DateTime.UtcNow;

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
            .Select(x => new { x.KitchenTicket.SentAtUtc, x.KitchenTicket.RestaurantOrder.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name })
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

        // Ortalama masa devir süresi - bugün kapanan (RetailSale'e dönüşmüş) adisyonların
        // masa açılış → kapanış süresi.
        var closedSessionsToday = await dbContext.RestaurantChecks
            .AsNoTracking()
            .Where(x => x.Status == RestaurantCheckStatus.Closed && x.ClosedAtUtc != null
                     && x.ClosedAtUtc >= todayStartUtc && x.ClosedAtUtc < todayEndUtc)
            .Select(x => new { x.RestaurantTableSession.OpenedAtUtc, x.ClosedAtUtc, x.BillRequestedAtUtc })
            .ToListAsync();
        var avgTableTurnMinutes = closedSessionsToday.Count == 0
            ? (int?)null
            : (int)closedSessionsToday.Average(x => (x.ClosedAtUtc!.Value - x.OpenedAtUtc).TotalMinutes);

        // Hesap istenmesinden kapanışa kadar geçen ortalama süre - sadece bugün hesap istenip
        // kapanan adisyonlar üzerinden (BillRequestedAtUtc bugüne kadar hiç yoktu, bundan sonra
        // birikecek).
        var billRequestedClosedToday = closedSessionsToday.Where(x => x.BillRequestedAtUtc is not null).ToList();
        var avgPaymentCompletionMinutes = billRequestedClosedToday.Count == 0
            ? (int?)null
            : (int)billRequestedClosedToday.Average(x => (x.ClosedAtUtc!.Value - x.BillRequestedAtUtc!.Value).TotalMinutes);

        // Bugün teslim edilen paketlerin ortalama süresi.
        var deliveredPackagesToday = await dbContext.PackageOrders
            .AsNoTracking()
            .Where(x => x.Status == PackageOrderStatus.Delivered && x.DeliveredAtUtc != null
                     && x.DeliveredAtUtc >= todayStartUtc && x.DeliveredAtUtc < todayEndUtc)
            .Select(x => new { x.CreatedAtUtc, x.DeliveredAtUtc })
            .ToListAsync();
        var avgPackageDeliveryMinutes = deliveredPackagesToday.Count == 0
            ? (int?)null
            : (int)deliveredPackagesToday.Average(x => (x.DeliveredAtUtc!.Value - x.CreatedAtUtc).TotalMinutes);

        var billRequestedOpenChecks = await dbContext.RestaurantChecks
            .AsNoTracking()
            .Where(x => x.Status == RestaurantCheckStatus.Open && x.BillRequestedAtUtc != null)
            .Select(x => new { x.BillRequestedAtUtc, TableName = x.RestaurantTableSession.RestaurantTable.Name, x.GrandTotal })
            .ToListAsync();

        var activePackages = await dbContext.PackageOrders
            .AsNoTracking()
            .Where(x => ActivePackageStatuses.Contains(x.Status))
            .Select(x => new { x.Status, x.PackageNumber, x.CustomerName })
            .ToListAsync();

        var dailyTarget = await dbContext.InventorySettings
            .AsNoTracking()
            .Where(x => x.Id == 1)
            .Select(x => x.DailyRevenueTarget)
            .SingleOrDefaultAsync();

        // Öncelik Kuyruğu: mutfak kritik (15 dk+) + hesap isteyen masa + hazır paket, süreye göre
        // sıralı (Edip, 2026-09-03: MASTER tasarım referansı).
        var queue = new List<RestaurantDashboardQueueItem>();
        foreach (var t in pendingTickets)
        {
            var waitMin = (int)nowUtc.Subtract(t.SentAtUtc).TotalMinutes;
            if (waitMin >= 15)
            {
                queue.Add(new RestaurantDashboardQueueItem
                {
                    Kind = "kitchen",
                    TimeLabel = $"{waitMin} dk",
                    Title = $"Mutfak · {t.Name}",
                    Subtitle = "Sipariş bekliyor",
                    IsUrgent = true
                });
            }
        }
        foreach (var c in billRequestedOpenChecks)
        {
            var waitMin = (int)nowUtc.Subtract(c.BillRequestedAtUtc!.Value).TotalMinutes;
            queue.Add(new RestaurantDashboardQueueItem
            {
                Kind = "bill",
                TimeLabel = $"{waitMin} dk",
                Title = $"Hesap · {c.TableName}",
                Subtitle = c.GrandTotal.ToString("N2") + " ₺"
            });
        }
        foreach (var p in activePackages.Where(x => x.Status == PackageOrderStatus.Ready))
        {
            queue.Add(new RestaurantDashboardQueueItem
            {
                Kind = "package",
                TimeLabel = "HAZIR",
                Title = $"Paket · {p.PackageNumber}",
                Subtitle = p.CustomerName
            });
        }

        var model = new RestaurantDashboardViewModel
        {
            NetRevenueToday = todaySales.Sum(x => x.GrandTotal),
            ClosedReceiptCountToday = todaySales.Count,
            CashCollectedToday = todayPayments.FirstOrDefault(x => x.Method == RestaurantPaymentMethod.Cash)?.Total ?? 0,
            CreditCardCollectedToday = todayPayments.FirstOrDefault(x => x.Method == RestaurantPaymentMethod.CreditCard)?.Total ?? 0,
            MealCardCollectedToday = todayPayments.FirstOrDefault(x => x.Method == RestaurantPaymentMethod.MealCard)?.Total ?? 0,
            KitchenPendingCount = pendingTickets.Count,
            KitchenLongestWaitMinutes = pendingTickets.Count == 0 ? 0 : (int)pendingTickets.Max(x => nowUtc.Subtract(x.SentAtUtc).TotalMinutes),
            HourlyRevenue = hourlyRevenue,
            HourlyRevenueStartHour = startHour,
            BillRequestedCount = billRequestedOpenChecks.Count,
            ActivePackageCount = activePackages.Count,
            AvgTableTurnMinutes = avgTableTurnMinutes,
            AvgKitchenWaitMinutes = pendingTickets.Count == 0 ? null : (int)pendingTickets.Average(x => nowUtc.Subtract(x.SentAtUtc).TotalMinutes),
            AvgPackageDeliveryMinutes = avgPackageDeliveryMinutes,
            AvgPaymentCompletionMinutes = avgPaymentCompletionMinutes,
            DailyRevenueTarget = dailyTarget,
            PriorityQueue = queue.OrderByDescending(x => x.IsUrgent).Take(6).ToList(),
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
