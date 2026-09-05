using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class RestaurantCashRegisterFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Kasa adı")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube")]
    public int? BranchId { get; set; }
    public string? BranchDisplay { get; set; }

    [Required(ErrorMessage = "Nakit hesabı seçilmelidir.")]
    [Display(Name = "Nakit hesabı")]
    public int? CashFinancialAccountId { get; set; }
    public string? CashFinancialAccountDisplay { get; set; }

    [Required(ErrorMessage = "Kredi kartı/banka hesabı seçilmelidir.")]
    [Display(Name = "Kredi kartı / banka hesabı")]
    public int? CreditCardFinancialAccountId { get; set; }
    public string? CreditCardFinancialAccountDisplay { get; set; }

    [Display(Name = "Yemek kartı hesabı (boş bırakılırsa kredi kartı hesabı kullanılır)")]
    public int? MealCardFinancialAccountId { get; set; }
    public string? MealCardFinancialAccountDisplay { get; set; }

    [Display(Name = "Not")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
