using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum RestaurantPaymentMethod
{
    [Display(Name = "Nakit")]
    Cash = 1,
    [Display(Name = "Kredi Kartı")]
    CreditCard = 2,
    [Display(Name = "Yemek Kartı")]
    MealCard = 3
}
