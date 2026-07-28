using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class BranchFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Şube kodu")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Şube adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [StringLength(30)]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [Display(Name = "Merkez şube")]
    public bool IsHeadOffice { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
