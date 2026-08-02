using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

// Sipariş → Fatura (doğrudan, irsaliyesiz) dönüşümü: onaylı/kısmen karşılanmış bir siparişin kalan
// satırlarını, sipariş satırındaki fiyat/KDV/iskonto ile birlikte gösterir; kullanıcı her satır için
// "şimdi faturalanacak miktar"ı düzenleyebilir (varsayılan: tam kalan miktar).
public sealed class CreateInvoiceFromOrderViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public InvoiceType InvoiceType { get; set; }
    public int CustomerId { get; set; }
    public string CustomerDisplay { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;

    // Çift tıklama/mükerrer POST koruması — bkz. Invoice.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();

    // InventorySettings + kullanıcı rolüne göre GET anında hesaplanır — ekranda "Taslak
    // oluşturulacak" / "Kaydedilecek ve otomatik onaylanacak" bilgisini göstermek için.
    public bool WillAutoApprove { get; set; }

    [Required(ErrorMessage = "Depo seçilmelidir.")]
    [Display(Name = "Depo")]
    public int? WarehouseId { get; set; }
    public string? WarehouseDisplay { get; set; }

    [Required]
    [Display(Name = "Fatura tarihi")]
    [DataType(DataType.Date)]
    public DateTime InvoiceDateUtc { get; set; } = DateTime.UtcNow.Date;

    public List<CreateInvoiceFromOrderLineViewModel> Lines { get; set; } = [];
}

public sealed class CreateInvoiceFromOrderLineViewModel
{
    public int BusinessOrderLineId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string UnitSnapshot { get; set; } = "Adet";
    public decimal OrderedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal TaxRate { get; set; }

    [Range(0, 999999999)]
    [Display(Name = "Faturalanacak miktar")]
    public decimal QuantityToInvoice { get; set; }
}
