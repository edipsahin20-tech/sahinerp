using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum StockSlipType
{
    [Display(Name = "Giriş")]
    Receipt = 1,
    [Display(Name = "Çıkış")]
    Issue = 2
}
