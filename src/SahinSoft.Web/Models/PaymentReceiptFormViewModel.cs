using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class PaymentReceiptFormViewModel
{
    public int Id { get; set; }

    public ReceiptType ReceiptType { get; set; }

    [Required(ErrorMessage = "Cari seçilmelidir.")]
    [Display(Name = "Cari")]
    public int? CustomerId { get; set; }

    [Required]
    [Display(Name = "Fiş tarihi")]
    [DataType(DataType.Date)]
    public DateTime ReceiptDateUtc { get; set; } = DateTime.UtcNow.Date;

    [Required, StringLength(3)]
    [Display(Name = "Para birimi")]
    public string CurrencyCode { get; set; } = "TRY";

    [Range(0.000001, 999999)]
    [Display(Name = "Döviz kuru")]
    public decimal ExchangeRate { get; set; } = 1;

    [StringLength(1000)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    public List<PaymentReceiptLineFormViewModel> Lines { get; set; } = [];

    public string? CustomerDisplay { get; set; }
}

public sealed class PaymentReceiptLineFormViewModel
{
    [Display(Name = "Ödeme yöntemi")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [StringLength(80)]
    [Display(Name = "Referans no")]
    public string? ReferenceNumber { get; set; }

    [Display(Name = "Vade tarihi")]
    [DataType(DataType.Date)]
    public DateTime? DueDateUtc { get; set; }

    [Range(0.01, 999999999)]
    [Display(Name = "Tutar")]
    public decimal Amount { get; set; }

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Kasa/banka hesabı seçilmelidir.")]
    [Display(Name = "Kasa/Banka")]
    public int? FinancialAccountId { get; set; }

    public string? FinancialAccountDisplay { get; set; }
}
