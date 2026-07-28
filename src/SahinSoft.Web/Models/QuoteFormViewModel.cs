using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class QuoteFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Cari seçilmelidir.")]
    [Display(Name = "Cari")]
    public int? CustomerId { get; set; }

    [Required]
    [Display(Name = "Teklif tarihi")]
    [DataType(DataType.Date)]
    public DateTime QuoteDateUtc { get; set; } = DateTime.UtcNow.Date;

    [Display(Name = "Geçerlilik tarihi")]
    [DataType(DataType.Date)]
    public DateTime? ValidUntilUtc { get; set; } = DateTime.UtcNow.Date.AddDays(15);

    [Required, StringLength(3)]
    [Display(Name = "Para birimi")]
    public string CurrencyCode { get; set; } = "TRY";

    [Range(0.000001, 999999)]
    [Display(Name = "Döviz kuru")]
    public decimal ExchangeRate { get; set; } = 1;

    [StringLength(1000)]
    [Display(Name = "Notlar")]
    public string? Notes { get; set; }

    public List<QuoteLineFormViewModel> Lines { get; set; } = [];

    public string? CustomerDisplay { get; set; }
}

public sealed class QuoteLineFormViewModel
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    [Display(Name = "Ürün")]
    public int? ProductId { get; set; }

    public string? ProductDisplay { get; set; }

    [Range(0.001, 999999999)]
    [Display(Name = "Miktar")]
    public decimal Quantity { get; set; } = 1;

    [Range(0, 999999999)]
    [Display(Name = "Birim Fiyat")]
    public decimal UnitPrice { get; set; }

    [Range(0, 100)]
    [Display(Name = "İskonto %")]
    public decimal DiscountRate { get; set; }

    [Range(0, 100)]
    [Display(Name = "KDV %")]
    public decimal TaxRate { get; set; }

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
}
