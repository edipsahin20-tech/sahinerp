using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

public sealed class HomeController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            ActiveCustomerCount = await dbContext.Customers.CountAsync(x => x.IsActive),
            ActiveProductCount = await dbContext.Products.CountAsync(x => x.IsActive),
            DraftInvoiceCount = await dbContext.Invoices.CountAsync(x => x.Status == InvoiceStatus.Draft),
            ApprovedInvoiceCount = await dbContext.Invoices.CountAsync(x => x.Status == InvoiceStatus.Approved)
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
