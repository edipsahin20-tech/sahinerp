using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class CompanySettingsViewModel
{
    [Required]
    [Display(Name = "Şirket adı")]
    public string CompanyName { get; set; } = string.Empty;

    [Display(Name = "Vergi dairesi")]
    public string? TaxOffice { get; set; }

    [Display(Name = "Vergi numarası")]
    public string? TaxNumber { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [EmailAddress]
    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [Display(Name = "Web sitesi")]
    public string? Website { get; set; }

    [Display(Name = "Banka adı")]
    public string? BankName { get; set; }

    [Display(Name = "IBAN")]
    public string? Iban { get; set; }

    [Display(Name = "Logo yolu")]
    public string? LogoPath { get; set; }

    [Display(Name = "Kapalı dönem sonu (bu tarihe kadar işlem yapılamaz)")]
    [DataType(DataType.Date)]
    public DateTime? ClosedPeriodUntil { get; set; }

    [Display(Name = "Mikro uyumlu tutar hesabı")]
    public bool MikroCompatibleAmounts { get; set; }

    [Display(Name = "Varsayılan şube")]
    public int? DefaultBranchId { get; set; }

    [Display(Name = "Varsayılan depo")]
    public int? DefaultWarehouseId { get; set; }

    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> BranchOptions { get; set; } = [];
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> WarehouseOptions { get; set; } = [];
}
