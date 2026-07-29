using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class InvoiceFormViewModel
{
    public int Id { get; set; }

    public InvoiceType InvoiceType { get; set; }

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
    [Display(Name = "Belge No")]
    public string? ReferenceNumber { get; set; }

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
}
