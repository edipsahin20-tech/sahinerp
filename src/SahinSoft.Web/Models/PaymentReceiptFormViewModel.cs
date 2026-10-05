using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class PaymentReceiptFormViewModel
{
    public int Id { get; set; }

    // Çift tıklama/mükerrer POST koruması — bkz. PaymentReceipt.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();

    public ReceiptType ReceiptType { get; set; }

    // Restoran bölümünden açıldıysa true: fiş seçili TERMİNAL şubesine göre damgalanır ve hesaplar
    // o şubenin kasası/bankası ya da ortak hesap olmak zorunda.
    public bool FromRestaurant { get; set; }

    // Restoran ekranında "Makbuz yazdır" işaretliyse kayıttan sonra 80mm termal yazıcıya makbuz gönderilir.
    public bool PrintReceipt { get; set; }

    public string? ReceiptNumber { get; set; }

    [Display(Name = "Şube")]
    public int? BranchId { get; set; }

    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> BranchOptions { get; set; } = [];


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

    [Range(typeof(decimal), "0.000001", "999999", ParseLimitsInInvariantCulture = true)]
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

    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
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
