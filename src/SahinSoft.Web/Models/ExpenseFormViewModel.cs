using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class ExpenseFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Masraf kategorisi seçilmelidir.")]
    [Display(Name = "Masraf kategorisi")]
    public int? ExpenseCategoryId { get; set; }

    [Required]
    [Display(Name = "Masraf tarihi")]
    [DataType(DataType.Date)]
    public DateTime ExpenseDateUtc { get; set; } = DateTime.UtcNow.Date;

    [Required, StringLength(500)]
    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Display(Name = "Para birimi")]
    public string CurrencyCode { get; set; } = "TRY";

    [Range(0.000001, 999999)]
    [Display(Name = "Döviz kuru")]
    public decimal ExchangeRate { get; set; } = 1;

    [Range(0, 999999999)]
    [Display(Name = "Tutar (KDV Hariç)")]
    public decimal NetAmount { get; set; }

    [Range(0, 999999999)]
    [Display(Name = "KDV Tutarı")]
    public decimal TaxAmount { get; set; }

    [Display(Name = "Cari (opsiyonel)")]
    public int? CustomerId { get; set; }

    [Display(Name = "KDV oranı (opsiyonel)")]
    public int? TaxRateId { get; set; }

    [Display(Name = "Ödeme hesabı (opsiyonel)")]
    public int? FinancialAccountId { get; set; }

    public string? ExpenseCategoryDisplay { get; set; }
    public string? CustomerDisplay { get; set; }
    public string? TaxRateDisplay { get; set; }
    public string? FinancialAccountDisplay { get; set; }
}
