using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SahinSoft.Web.Models;

// Toplu masa açma - Edip'in örneği (2026-09-03): Masa Adı "Salon-" + Aralık 1-20 -> Salon-1..
// Salon-20, seçilen Lokasyon (Branch) ve Grup'a (RestaurantSection) bağlı olarak.
public sealed class RestaurantTableBulkCreateViewModel
{
    [Required(ErrorMessage = "Lokasyon seçilmelidir.")]
    [Display(Name = "Lokasyon")]
    public int? BranchId { get; set; }

    [Required, StringLength(40)]
    [Display(Name = "Masa Adı")]
    public string NamePrefix { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Aralık Başlangıç")]
    [Range(1, 999)]
    public int RangeFrom { get; set; } = 1;

    [Required]
    [Display(Name = "Aralık Bitiş")]
    [Range(1, 999)]
    public int RangeTo { get; set; } = 1;

    [Range(1, 100)]
    [Display(Name = "Kapasite")]
    public int Capacity { get; set; } = 4;

    [Required(ErrorMessage = "Grup seçilmelidir.")]
    [Display(Name = "Grup")]
    public int? RestaurantSectionId { get; set; }

    public List<SelectListItem> Branches { get; set; } = [];
    public List<SelectListItem> Sections { get; set; } = [];
}
