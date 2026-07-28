using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class UnitOfMeasureFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Birim Kodu")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Birim Adı")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 6)]
    [Display(Name = "Ondalık Basamak")]
    public int DecimalPlaces { get; set; }

    [Display(Name = "Aktif mi?")]
    public bool IsActive { get; set; } = true;
}
