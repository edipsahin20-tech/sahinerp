using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum StockSlipStatus
{
    [Display(Name = "Taslak")]
    Draft = 1,
    [Display(Name = "Onaylı")]
    Approved = 2,
    [Display(Name = "İptal")]
    Cancelled = 3
}
