using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum InstrumentDirection
{
    [Display(Name = "Alınan")]
    Received = 1,
    [Display(Name = "Verilen")]
    Issued = 2
}
