using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SahinSoft.Domain.Constants;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class RolesController(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var roles = new List<RoleListItemViewModel>();
        foreach (var role in roleManager.Roles.OrderBy(x => x.Name).ToList())
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
            roles.Add(new RoleListItemViewModel
            {
                Id = role.Id,
                Name = role.Name!,
                UserCount = usersInRole.Count
            });
        }

        return View(roles);
    }

    public IActionResult Create()
    {
        return View("Form", new RoleFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleFormViewModel form)
    {
        if (await roleManager.RoleExistsAsync(form.Name))
        {
            ModelState.AddModelError(nameof(form.Name), "Bu yetki adı zaten kullanılıyor.");
        }
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var newRole = new IdentityRole(form.Name.Trim());
        var result = await roleManager.CreateAsync(newRole);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("Form", form);
        }

        TempData["Success"] = "Yetki kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        return View("Form", new RoleFormViewModel { Id = role.Id, Name = role.Name! });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, RoleFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        if (string.Equals(role.Name, AppRoles.Administrator, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Administrator yetkisinin adı değiştirilemez.";
            return RedirectToAction(nameof(Index));
        }

        if (await roleManager.RoleExistsAsync(form.Name) &&
            !string.Equals(role.Name, form.Name, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(form.Name), "Bu yetki adı zaten kullanılıyor.");
        }
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        role.Name = form.Name.Trim();
        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("Form", form);
        }

        TempData["Success"] = "Yetki güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        if (string.Equals(role.Name, AppRoles.Administrator, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Administrator yetkisi silinemez.";
            return RedirectToAction(nameof(Index));
        }

        var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Count > 0)
        {
            TempData["Error"] = "Bu yetkiye atanmış personel var, silinemez.";
            return RedirectToAction(nameof(Index));
        }

        await roleManager.DeleteAsync(role);
        TempData["Success"] = "Yetki silindi.";
        return RedirectToAction(nameof(Index));
    }
}
