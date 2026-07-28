using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum FinancialTransactionType
{
    [Display(Name = "Açılış")]
    Opening = 1,
    [Display(Name = "Tahsilat")]
    Collection = 2,
    [Display(Name = "Tediye")]
    Payment = 3,
    [Display(Name = "Devir Giriş")]
    TransferIn = 4,
    [Display(Name = "Devir Çıkış")]
    TransferOut = 5,
    [Display(Name = "Düzeltme")]
    Adjustment = 6
}
