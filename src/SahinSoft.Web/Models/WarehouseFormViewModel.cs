using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class WarehouseFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Depo kodu")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Depo adı")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube")]
    public int? BranchId { get; set; }

    [Display(Name = "Varsayılan depo")]
    public bool IsDefault { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public string? BranchDisplay { get; set; }
}
