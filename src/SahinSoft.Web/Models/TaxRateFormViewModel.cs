using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class TaxRateFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "KDV kodu")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "KDV adı")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100)]
    [Display(Name = "Oran (%)")]
    public decimal Rate { get; set; }

    [Display(Name = "Muaf")]
    public bool IsExempt { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
