using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class RoleFormViewModel
{
    public string? Id { get; set; }

    [Required]
    [Display(Name = "Yetki Adı")]
    public string Name { get; set; } = string.Empty;
}
