using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum QuoteStatus
{
    [Display(Name = "Taslak")]
    Draft = 1,
    [Display(Name = "Gönderildi")]
    Sent = 2,
    [Display(Name = "Onaylı")]
    Approved = 3,
    [Display(Name = "Reddedildi")]
    Rejected = 4
}
