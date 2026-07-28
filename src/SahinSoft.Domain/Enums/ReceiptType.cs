using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum ReceiptType
{
    [Display(Name = "Tahsilat")]
    Collection = 1,
    [Display(Name = "Tediye")]
    Payment = 2
}
