using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum PrinterPaperWidth
{
    [Display(Name = "58mm")]
    Mm58 = 58,
    [Display(Name = "80mm")]
    Mm80 = 80
}
