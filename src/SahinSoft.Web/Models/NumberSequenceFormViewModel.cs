using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class NumberSequenceFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Anahtar")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Ön Ek")]
    public string Prefix { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    [Display(Name = "Sıradaki Numara")]
    public long NextNumber { get; set; } = 1;

    [Range(1, 10)]
    [Display(Name = "Basamak Sayısı")]
    public int Padding { get; set; } = 5;
}
