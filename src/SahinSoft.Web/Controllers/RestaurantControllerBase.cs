using SahinSoft.Web.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

// Master tasarımın ortak shell'ini (topbar/sidebar/statusbar) kullanan TÜM restoran
// controller'ları bundan türer - her action'da aynı sorguları tekrarlamak yerine
// ViewBag.Shell burada bir kere dolduruluyor. Alt sınıflar sadece kendi ActivePage
// değerini SetActivePage ile belirtir (Dashboard/Masa Satış/Paket/Self Satış/Raporlar/
// Mutfak/Vardiya).
public abstract class RestaurantControllerBase(ApplicationDbContext dbContext, RestaurantShellService shellService) : Controller
{
    protected string ActivePage { get; set; } = string.Empty;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ViewBag.Shell = await BuildShellAsync();
        await next();
    }

    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private async Task<RestaurantShellViewModel> BuildShellAsync()
    {
        return await shellService.BuildAsync(User, Request, ActivePage);
    }
}
