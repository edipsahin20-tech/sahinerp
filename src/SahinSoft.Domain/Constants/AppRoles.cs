namespace SahinSoft.Domain.Constants;

public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string Staff = "Staff";

    // Restoran modülü rolleri — Administrator her işleme yetkili olmaya devam eder.
    public const string RestaurantManager = "RestaurantManager";
    public const string Cashier = "Cashier";
    public const string Waiter = "Waiter";
    public const string Kitchen = "Kitchen";

    public static readonly string[] All = [Administrator, Staff, RestaurantManager, Cashier, Waiter, Kitchen];
}
