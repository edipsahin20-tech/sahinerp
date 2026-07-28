using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum NegotiableInstrumentType
{
    [Display(Name = "Çek")]
    Cheque = 1,
    [Display(Name = "Senet")]
    PromissoryNote = 2
}
