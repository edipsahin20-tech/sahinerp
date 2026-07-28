using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum InventoryCountStatus
{
    [Display(Name = "Taslak")]
    Draft = 1,
    [Display(Name = "Sayılıyor")]
    Counting = 2,
    [Display(Name = "Onaylı")]
    Approved = 3,
    [Display(Name = "İptal")]
    Cancelled = 4
}
