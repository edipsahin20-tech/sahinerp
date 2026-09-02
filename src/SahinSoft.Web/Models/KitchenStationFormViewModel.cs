using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class KitchenStationFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "İstasyon Adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Yazıcı Adı")]
    public string? PrinterName { get; set; }

    [Display(Name = "Sıra")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
