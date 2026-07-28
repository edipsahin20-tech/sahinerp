using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum CurrentAccountTransactionType
{
    [Display(Name = "Açılış")]
    Opening = 1,
    [Display(Name = "Satış")]
    Sale = 2,
    [Display(Name = "Alış")]
    Purchase = 3,
    [Display(Name = "Tahsilat")]
    Collection = 4,
    [Display(Name = "Tediye")]
    Payment = 5,
    [Display(Name = "Borç Dekontu")]
    DebitNote = 6,
    [Display(Name = "Alacak Dekontu")]
    CreditNote = 7
}
