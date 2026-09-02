using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class RestaurantSectionFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Salon adı")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Sıra")]
    public int DisplayOrder { get; set; }

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube")]
    public int? BranchId { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public string? BranchDisplay { get; set; }
}
