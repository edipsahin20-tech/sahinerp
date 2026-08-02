using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class InvoiceFormViewModel
{
    public int Id { get; set; }

    // Çift tıklama/mükerrer POST koruması — form her (yeniden) açıldığında bir kez üretilir,
    // gizli alan olarak taşınır. Bkz. Invoice.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();

    public InvoiceType InvoiceType { get; set; }

    public string? InvoiceNumber { get; set; }

    // Görsel amaçlı: formun Onaylı bir faturayı mı düzenlediğini bilip uyarı göstermek için.
    // Sunucu tarafında durum geçişleri buna değil, veritabanındaki gerçek Invoice.Status'e göre karar verilir.
    public InvoiceStatus Status { get; set; }

    // Belge Seri + Sıra, formda elle değiştirilebilir (ör. farklı bir fiziksel evrak defteri/seri
    // kullanılıyorsa). Boş bırakılırsa sistemin ürettiği otomatik numara kullanılır.
    [StringLength(20)]
    [Display(Name = "Evrak Belge Seri")]
    public string? DocumentSeries { get; set; }

    [StringLength(30)]
    [Display(Name = "Evrak Belge Sıra")]
    public string? DocumentSequence { get; set; }

    [Required(ErrorMessage = "Cari seçilmelidir.")]
    [Display(Name = "Cari")]
    public int? CustomerId { get; set; }

    [Required(ErrorMessage = "Depo seçilmelidir.")]
    [Display(Name = "Depo")]
    public int? WarehouseId { get; set; }

    [Required]
    [Display(Name = "Fatura tarihi")]
    [DataType(DataType.Date)]
    public DateTime InvoiceDateUtc { get; set; } = DateTime.UtcNow.Date;

    [Display(Name = "Vade tarihi")]
    [DataType(DataType.Date)]
    public DateTime? DueDateUtc { get; set; }

    [Required, StringLength(3)]
    [Display(Name = "Para birimi")]
    public string CurrencyCode { get; set; } = "TRY";

    [Range(0.000001, 999999)]
    [Display(Name = "Döviz kuru")]
    public decimal ExchangeRate { get; set; } = 1;

    [StringLength(1000)]
    [Display(Name = "Notlar")]
    public string? Notes { get; set; }

    [StringLength(50)]
    [Display(Name = "Belge Numarası")]
    public string? ReferenceNumber { get; set; }

    [Display(Name = "Fiyatlara KDV Dahil")]
    public bool PricesIncludeTax { get; set; }

    [StringLength(100)]
    [Display(Name = "Ödeme Şekli")]
    public string? PaymentTerm { get; set; }

    [StringLength(100)]
    [Display(Name = "Ticaret Türü")]
    public string? TradeType { get; set; }

    [Display(Name = "İade Faturası mı?")]
    public bool IsReturn { get; set; }

    [Display(Name = "Plasiyer")]
    public string? SalespersonUserId { get; set; }

    [Display(Name = "Kapalı Fatura mı?")]
    public bool IsClosedInvoice { get; set; }

    [Display(Name = "Ödeme Yöntemi")]
    public PaymentMethod? SettlementPaymentMethod { get; set; }

    [Display(Name = "Kapayan Hesap Kodu")]
    public int? SettlementFinancialAccountId { get; set; }
    public string? SettlementFinancialAccountDisplay { get; set; }

    [Range(0, 999999999)]
    [Display(Name = "Tutar İskontosu")]
    public decimal AmountDiscount { get; set; }

    public List<InvoiceLineFormViewModel> Lines { get; set; } = [];

    public string? CustomerDisplay { get; set; }
    public string? WarehouseDisplay { get; set; }
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Salespeople { get; set; } = [];
}

public sealed class InvoiceLineFormViewModel
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    [Display(Name = "Ürün")]
    public int? ProductId { get; set; }

    public string? ProductDisplay { get; set; }

    [Range(0.001, 999999999)]
    [Display(Name = "Miktar")]
    public decimal Quantity { get; set; } = 1;

    [Range(0, 999999999)]
    [Display(Name = "Birim fiyat")]
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

    // Planlı İşlem (F5) ile bir irsaliyeden/siparişten çağrılan satırlarda set edilir; elle eklenen
    // satırlarda ikisi de null kalır. Bkz. InvoiceLine.DispatchNoteLineId / BusinessOrderLineId.
    public int? DispatchNoteLineId { get; set; }
    public int? BusinessOrderLineId { get; set; }
    public string? SourceDisplay { get; set; }
}
