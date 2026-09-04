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
}
