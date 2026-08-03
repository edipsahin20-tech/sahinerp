using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class BusinessOrderFormViewModel
{
    public int Id { get; set; }

    // Çift tıklama/mükerrer POST koruması — bkz. BusinessOrder.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();

    public InvoiceType OrderType { get; set; }

    public string? OrderNumber { get; set; }

    [Required(ErrorMessage = "Cari seçilmelidir.")]
    [Display(Name = "Cari")]
    public int? CustomerId { get; set; }

    [Required]
    [Display(Name = "Sipariş tarihi")]
    [DataType(DataType.Date)]
    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow.Date;

    [Display(Name = "Talep edilen teslim tarihi")]
    [DataType(DataType.Date)]
    public DateTime? RequestedDeliveryDateUtc { get; set; }

    [Required, StringLength(3)]
    [Display(Name = "Para birimi")]
    public string CurrencyCode { get; set; } = "TRY";

    [Range(typeof(decimal), "0.000001", "999999")]
    [Display(Name = "Döviz kuru")]
    public decimal ExchangeRate { get; set; } = 1;

    [StringLength(1000)]
    [Display(Name = "Notlar")]
    public string? Notes { get; set; }

    public List<BusinessOrderLineFormViewModel> Lines { get; set; } = [];

    public string? CustomerDisplay { get; set; }
}

public sealed class BusinessOrderLineFormViewModel
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    [Display(Name = "Ürün")]
    public int? ProductId { get; set; }

    public string? ProductDisplay { get; set; }

    [Range(typeof(decimal), "0.001", "999999999", ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
    [Display(Name = "Miktar")]
    public decimal Quantity { get; set; } = 1;

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Birim fiyat 0 veya daha büyük olmalıdır.")]
    [Display(Name = "Birim Fiyat")]
    public decimal UnitPrice { get; set; }

    [Range(typeof(decimal), "0", "100", ErrorMessage = "İskonto % 0 ile 100 arasında olmalıdır.")]
    [Display(Name = "İskonto %")]
    public decimal DiscountRate { get; set; }

    [Range(typeof(decimal), "0", "100", ErrorMessage = "KDV % 0 ile 100 arasında olmalıdır.")]
    [Display(Name = "KDV %")]
    public decimal TaxRate { get; set; }
}
