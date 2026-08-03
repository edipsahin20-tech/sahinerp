using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum KitchenTicketStatus
{
    [Display(Name = "Gönderildi")]
    Sent = 1,
    [Display(Name = "Hazırlanıyor")]
    InProgress = 2,
    [Display(Name = "Hazır")]
    Ready = 3,
    [Display(Name = "Servis Edildi")]
    Served = 4
}
