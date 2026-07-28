using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum StockMovementType
{
    [Display(Name = "Açılış")]
    Opening = 1,
    [Display(Name = "Alış")]
    Purchase = 2,
    [Display(Name = "Satış")]
    Sale = 3,
    [Display(Name = "Düzeltme Giriş")]
    AdjustmentIn = 4,
    [Display(Name = "Düzeltme Çıkış")]
    AdjustmentOut = 5,
    [Display(Name = "İade Giriş")]
    ReturnIn = 6,
    [Display(Name = "İade Çıkış")]
    ReturnOut = 7,
    [Display(Name = "Transfer Giriş")]
    TransferIn = 8,
    [Display(Name = "Transfer Çıkış")]
    TransferOut = 9,
    [Display(Name = "Sayım Fazlası")]
    InventoryCountSurplus = 10,
    [Display(Name = "Sayım Eksiği")]
    InventoryCountShortage = 11,
    [Display(Name = "İrsaliye Giriş")]
    DispatchIn = 12,
    [Display(Name = "İrsaliye Çıkış")]
    DispatchOut = 13
}
