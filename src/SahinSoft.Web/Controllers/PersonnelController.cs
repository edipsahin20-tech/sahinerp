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
    DocumentNumberGeneratorService documentNumberGenerator,
    IPasswordHasher<ApplicationUser> passwordHasher) : Controller
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

    // Restoran personeli (garson/kasiyer) için e-posta ve Identity'nin karmaşık şifre şartı
    // ("Restoran personeli tanımlarında mail ya da karakter zorunluluğu olmayacak") anlamsız -
    // onlar PIN ile giriş yapacak (bkz. RestaurantAuthController). Bu yüzden: en az PIN veya
    // Şifre'den biri girilmeli, ikisi de zorunlu değil. E-posta boşsa UserName Personel Kodu'ndan
    // türetilir. Şifre boş ama PIN doluysa, Identity'nin global karmaşıklık politikasını
    // (Program.cs) hâlâ karşılamak için rastgele GÜÇLÜ bir şifre üretilir - kullanıcıya hiç
    // gösterilmez, çünkü zaten PIN ile giriş yapacak.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PersonnelFormViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.Password) && string.IsNullOrWhiteSpace(form.Pin))
        {
            ModelState.AddModelError(nameof(form.Password), "Şifre veya PIN'den en az biri girilmelidir.");
        }
        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        var personnelCode = string.IsNullOrWhiteSpace(form.PersonnelCode)
            ? await documentNumberGenerator.GenerateAsync("PERSONNEL")
            : form.PersonnelCode.Trim();
        var hasEmail = !string.IsNullOrWhiteSpace(form.Email);

        var user = new ApplicationUser
        {
            UserName = hasEmail ? form.Email.Trim() : personnelCode,
            Email = hasEmail ? form.Email.Trim() : null,
            EmailConfirmed = hasEmail,
            FullName = form.FullName.Trim(),
            IsActive = form.IsActive,
            PersonnelCode = personnelCode
        };
        MapOptionalFields(form, user);
        user.PersonnelCode = personnelCode;

        var effectivePassword = string.IsNullOrWhiteSpace(form.Password)
            ? GenerateStrongSystemPassword()
            : form.Password!;

        var createResult = await userManager.CreateAsync(user, effectivePassword);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await PopulateSelectionsAsync(form);
            return View("Form", form);
        }

        if (!string.IsNullOrWhiteSpace(form.Pin))
        {
            user.RestaurantPinHash = passwordHasher.HashPassword(user, form.Pin.Trim());
            await userManager.UpdateAsync(user);
        }

        await userManager.AddToRoleAsync(user, form.RoleName);

        TempData["Success"] = "Personel kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    // Identity'nin (Program.cs) Password.RequiredLength=10 + Require*=true politikasını
    // karşılayan, kimseye gösterilmeyecek rastgele bir şifre - restoran personeli sadece PIN
    // ile giriş yapacağı için bu şifrenin ne olduğu önemli değil, sadece CreateAsync'in
    // politikayı reddetmemesi için var.
    private static string GenerateStrongSystemPassword()
    {
        var randomPart = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(18))
            .Replace("+", "A").Replace("/", "b").Replace("=", "9");
        return $"Aa1!{randomPart}";
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
            DefaultPriceListId = user.DefaultPriceListId,
            DiscountLowerLimitPercent = user.DiscountLowerLimitPercent,
            DiscountUpperLimitPercent = user.DiscountUpperLimitPercent
            // Pin bilerek doldurulmuyor - Password gibi, mevcut PIN'i göstermeden korumak için
            // boş bırakılır; değiştirmek isteyen yeniden girer.
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

        var hasEmail = !string.IsNullOrWhiteSpace(form.Email);
        if (hasEmail && !string.Equals(user.Email, form.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            user.Email = form.Email.Trim();
            user.UserName = form.Email.Trim();
            await userManager.UpdateNormalizedEmailAsync(user);
            await userManager.UpdateNormalizedUserNameAsync(user);
        }
        else if (!hasEmail)
        {
            user.Email = null;
            user.EmailConfirmed = false;
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

        if (!string.IsNullOrWhiteSpace(form.Pin))
        {
            user.RestaurantPinHash = passwordHasher.HashPassword(user, form.Pin.Trim());
            await userManager.UpdateAsync(user);
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
