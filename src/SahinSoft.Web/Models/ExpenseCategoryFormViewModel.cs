using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class ExpenseCategoryFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Masraf kategori kodu")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Masraf kategori adı")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
