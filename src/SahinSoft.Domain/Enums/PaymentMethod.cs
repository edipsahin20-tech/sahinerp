using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum PaymentMethod
{
    [Display(Name = "Nakit")]
    Cash = 1,
    [Display(Name = "Havale/EFT")]
    BankTransfer = 2,
    [Display(Name = "Kredi Kartı")]
    CreditCard = 3,
    [Display(Name = "Çek")]
    Cheque = 4,
    [Display(Name = "Senet")]
    PromissoryNote = 5,
    [Display(Name = "Diğer")]
    Other = 6
}
