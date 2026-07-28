using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Kategori kodu")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Kategori adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Web sitesi yolu")]
    public string? WebsitePath { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
