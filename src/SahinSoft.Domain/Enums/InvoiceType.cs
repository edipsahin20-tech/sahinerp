using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum InvoiceType
{
    [Display(Name = "Satış")]
    Sales = 1,
    [Display(Name = "Alış")]
    Purchase = 2
}
