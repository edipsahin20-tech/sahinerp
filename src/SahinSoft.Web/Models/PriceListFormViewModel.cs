using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class PriceListFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Fiyat Kodu")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Fiyat Adı")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Aktif mi?")]
    public bool IsActive { get; set; } = true;
}
