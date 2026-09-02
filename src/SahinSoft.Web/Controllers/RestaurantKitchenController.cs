using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager},{AppRoles.Kitchen}")]
public sealed class RestaurantKitchenController(ApplicationDbContext dbContext, RestaurantPostingService postingService) : RestaurantControllerBase(dbContext)
{
    public async Task<IActionResult> Index(int? stationId)
    {
        ActivePage = "kitchen";

        var activeTicketsQuery = dbContext.KitchenTickets
            .AsNoTracking()
            .Where(x => x.Status != KitchenTicketStatus.Served);

        var stations = await dbContext.KitchenStations
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new RestaurantKitchenStationFilter(
                x.Id,
                x.Name,
                activeTicketsQuery.Count(t => t.KitchenStationId == x.Id)))
            .ToListAsync();

        var ticketsQuery = activeTicketsQuery
            .Include(x => x.KitchenStation)
            .Include(x => x.Lines).ThenInclude(x => x.RestaurantOrderLine).ThenInclude(x => x.Modifiers)
            .Include(x => x.RestaurantOrder).ThenInclude(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable)
            .AsQueryable();

        if (stationId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(x => x.KitchenStationId == stationId.Value);
        }

        var tickets = await ticketsQuery
            .OrderBy(x => x.SentAtUtc)
            .ToListAsync();

        var vm = new RestaurantKitchenViewModel
        {
            Stations = stations,
            SelectedStationId = stationId,
            Tickets = tickets.Select(ticket => new RestaurantKitchenTicketViewModel
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                StationName = ticket.KitchenStation.Name,
                TableName = ticket.RestaurantOrder.RestaurantCheck.RestaurantTableSession.RestaurantTable.Name,
                CheckNumber = ticket.RestaurantOrder.RestaurantCheck.CheckNumber,
                Status = ticket.Status,
                SentAtUtc = ticket.SentAtUtc,
                Lines = ticket.Lines.Select(line => new RestaurantKitchenTicketLineViewModel(
                    line.RestaurantOrderLine.ProductNameSnapshot,
                    line.RestaurantOrderLine.PortionNameSnapshot,
                    line.RestaurantOrderLine.Quantity,
                    line.RestaurantOrderLine.KitchenNote,
                    line.RestaurantOrderLine.Modifiers.Select(m => $"{m.NameSnapshot} x{m.Quantity:0.##}").ToList(),
                    line.Status == KitchenTicketLineStatus.Cancelled)).ToList()
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Advance(int ticketId, int? stationId)
    {
        try
        {
            await postingService.AdvanceKitchenTicketAsync(ticketId);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index", new { stationId });
    }
}
