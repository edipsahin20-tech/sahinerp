using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class PersonnelController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator) : Controller
{
    public static readonly string[] JobTitles = ["Personel", "Kasiyer", "Kullanıcı", "Paket", "Mobil Kullanıcı"];

    public async Task<IActionResult> Index()
    {
        var users = userManager.Users.OrderBy(x => x.FullName).ToList();
        var items = new List<PersonnelListItemViewModel>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            items.Add(new PersonnelListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                RoleName = roles.FirstOrDefault() ?? "-",
                IsActive = user.IsActive
            });
        }

        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        var model = new PersonnelFormViewModel();
        await PopulateSelectionsAsync(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PersonnelFormViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.Password))
        {
            ModelState.AddModelError(nameof(form.Password), "Yeni personel için şifre zorunludur.");
        }
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var user = new ApplicationUser
        {
            UserName = form.Email.Trim(),
            Email = form.Email.Trim(),
            EmailConfirmed = true,
            FullName = form.FullName.Trim(),
            IsActive = form.IsActive
        };
        MapOptionalFields(form, user);
        if (string.IsNullOrWhiteSpace(user.PersonnelCode))
        {
            user.PersonnelCode = await documentNumberGenerator.GenerateAsync("PERSONNEL");
        }

        var createResult = await userManager.CreateAsync(user, form.Password!);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        await userManager.AddToRoleAsync(user, form.RoleName);

        TempData["Success"] = "Personel kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        var model = new PersonnelFormViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            RoleName = roles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive,
            PersonnelCode = user.PersonnelCode,
            JobTitle = user.JobTitle,
            Address = user.Address,
            HireDateUtc = user.HireDateUtc,
            TerminationDateUtc = user.TerminationDateUtc,
            Salary = user.Salary,
            Deduction = user.Deduction,
            DeductionNote = user.DeductionNote,
            Iban = user.Iban,
            BankAccountNumber = user.BankAccountNumber,
            CommissionRate = user.CommissionRate,
            BreakDurationMinutes = user.BreakDurationMinutes,
            LicensePlate = user.LicensePlate,
            BranchId = user.BranchId,
            DefaultFinancialAccountId = user.DefaultFinancialAccountId,
            DefaultPriceListId = user.DefaultPriceListId
        };
        await PopulateSelectionsAsync(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, PersonnelFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.FullName = form.FullName.Trim();
        user.IsActive = form.IsActive;
        MapOptionalFields(form, user);

        if (!string.Equals(user.Email, form.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            user.Email = form.Email.Trim();
            user.UserName = form.Email.Trim();
            await userManager.UpdateNormalizedEmailAsync(user);
            await userManager.UpdateNormalizedUserNameAsync(user);
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        if (!string.IsNullOrWhiteSpace(form.Password))
        {
            await userManager.RemovePasswordAsync(user);
            var passwordResult = await userManager.AddPasswordAsync(user, form.Password);
            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError(nameof(form.Password), error.Description);
                }
                await PopulateSelectionsAsync(form);
                return View("Form", form);
            }
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(form.RoleName))
        {
            if (currentRoles.Count > 0)
            {
                await userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            await userManager.AddToRoleAsync(user, form.RoleName);
        }

        TempData["Success"] = "Personel güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    private static void MapOptionalFields(PersonnelFormViewModel source, ApplicationUser target)
    {
        target.PhoneNumber = source.PhoneNumber?.Trim();
        target.PersonnelCode = string.IsNullOrWhiteSpace(source.PersonnelCode) ? null : source.PersonnelCode.Trim();
        target.JobTitle = source.JobTitle?.Trim();
        target.Address = source.Address?.Trim();
        target.HireDateUtc = source.HireDateUtc.HasValue ? DateTime.SpecifyKind(source.HireDateUtc.Value, DateTimeKind.Utc) : null;
        target.TerminationDateUtc = source.TerminationDateUtc.HasValue ? DateTime.SpecifyKind(source.TerminationDateUtc.Value, DateTimeKind.Utc) : null;
        target.Salary = source.Salary;
        target.Deduction = source.Deduction;
        target.DeductionNote = source.DeductionNote?.Trim();
        target.Iban = source.Iban?.Trim();
        target.BankAccountNumber = source.BankAccountNumber?.Trim();
        target.CommissionRate = source.CommissionRate;
        target.BreakDurationMinutes = source.BreakDurationMinutes;
        target.LicensePlate = source.LicensePlate?.Trim();
        target.BranchId = source.BranchId;
        target.DefaultFinancialAccountId = source.DefaultFinancialAccountId;
        target.DefaultPriceListId = source.DefaultPriceListId;
    }

    private async Task PopulateSelectionsAsync(PersonnelFormViewModel model)
    {
        model.Roles = roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Name))
            .ToList();

        if (model.BranchId is int branchId)
        {
            model.BranchDisplay = await dbContext.Branches
                .Where(x => x.Id == branchId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        if (model.DefaultFinancialAccountId is int financialAccountId)
        {
            model.FinancialAccountDisplay = await dbContext.FinancialAccounts
                .Where(x => x.Id == financialAccountId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        if (model.DefaultPriceListId is int priceListId)
        {
            model.PriceListDisplay = await dbContext.PriceLists
                .Where(x => x.Id == priceListId)
                .Select(x => x.Code + " - " + x.Name)
                .SingleOrDefaultAsync();
        }

        model.JobTitleOptions = JobTitles
            .Select(x => new SelectListItem(x, x))
            .ToList();
    }
}
