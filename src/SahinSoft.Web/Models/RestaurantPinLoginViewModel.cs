namespace SahinSoft.Web.Models;

public sealed class RestaurantPinLoginViewModel
{
    public List<RestaurantStaffPickerItem> Staff { get; set; } = [];
    public string ReturnUrl { get; set; } = "/";
}

public sealed class RestaurantStaffPickerItem
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
