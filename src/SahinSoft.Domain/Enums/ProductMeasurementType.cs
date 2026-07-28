using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum ProductMeasurementType
{
    [Display(Name = "Adet")]
    Unit = 1,
    [Display(Name = "Kilogram")]
    Kilogram = 2
}
