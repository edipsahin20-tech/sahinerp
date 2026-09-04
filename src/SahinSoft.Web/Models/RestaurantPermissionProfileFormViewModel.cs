namespace SahinSoft.Web.Models;

public sealed class RestaurantPermissionProfileFormViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public bool CanCancelOrderLine { get; set; } = true;
    public bool CanCancelReceipt { get; set; } = true;
    public bool CanApplyDiscount { get; set; } = true;
    public bool CanApplyComplimentary { get; set; } = true;
    public bool CanEditKitchenSentLines { get; set; } = true;
    public bool CanAddNote { get; set; } = true;

    // Sağ İşlem Menüsü görünürlüğü (madde 22) - 2. katman (kullanıcı/profil bazlı).
    public bool CanSeeTableTransfer { get; set; } = true;
    public bool CanSeeSendToKitchen { get; set; } = true;
    public bool CanSeePriceCheck { get; set; } = true;
    public bool CanSeeKeyboard { get; set; } = true;
    public bool CanSeeHoldReceipt { get; set; } = true;
    public bool CanSeeHeldReceipts { get; set; } = true;
    public bool CanSeeProductList { get; set; } = true;
    public bool CanSeeReceiptList { get; set; } = true;
    public bool CanClearOrder { get; set; } = true;
}
