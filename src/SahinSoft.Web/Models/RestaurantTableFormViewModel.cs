using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SahinSoft.Web.Models;

public sealed class RestaurantTableFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Masa adı")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100)]
    [Display(Name = "Kapasite")]
    public int Capacity { get; set; } = 4;

    [Required(ErrorMessage = "Salon seçilmelidir.")]
    [Display(Name = "Salon")]
    public int? RestaurantSectionId { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public List<SelectListItem> Sections { get; set; } = [];
}
