using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class NegotiableInstrumentFormViewModel
{
    public int Id { get; set; }

    // Çift tıklama/mükerrer POST koruması — bkz. NegotiableInstrument.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();

    public NegotiableInstrumentType InstrumentType { get; set; }

    public string? InstrumentNumber { get; set; }

    [Display(Name = "Yön")]
    public InstrumentDirection Direction { get; set; } = InstrumentDirection.Received;

    [Required(ErrorMessage = "Cari seçilmelidir.")]
    [Display(Name = "Cari")]
    public int? CustomerId { get; set; }

    [Required]
    [Display(Name = "Düzenleme tarihi")]
    [DataType(DataType.Date)]
    public DateTime IssueDateUtc { get; set; } = DateTime.UtcNow.Date;

    [Required]
    [Display(Name = "Vade tarihi")]
    [DataType(DataType.Date)]
    public DateTime DueDateUtc { get; set; } = DateTime.UtcNow.Date.AddMonths(1);

    [Required, StringLength(3)]
    [Display(Name = "Para birimi")]
    public string CurrencyCode { get; set; } = "TRY";

    [Range(typeof(decimal), "0.01", "999999999")]
    [Display(Name = "Tutar")]
    public decimal Amount { get; set; }

    [StringLength(150)]
    [Display(Name = "Banka")]
    public string? BankName { get; set; }

    [StringLength(150)]
    [Display(Name = "Şube")]
    public string? BranchName { get; set; }

    [StringLength(80)]
    [Display(Name = "Hesap no")]
    public string? AccountNumber { get; set; }

    [StringLength(200)]
    [Display(Name = "Keşideci")]
    public string? DrawerName { get; set; }

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Kasa/Banka hesabı (opsiyonel)")]
    public int? FinancialAccountId { get; set; }

    public string? CustomerDisplay { get; set; }
    public string? FinancialAccountDisplay { get; set; }
}
