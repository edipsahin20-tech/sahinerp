using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public List<InvoiceLineFormViewModel> Lines { get; set; } = [];

    public IReadOnlyList<SelectListItem> Customers { get; set; } = [];
    public IReadOnlyList<SelectListItem> Warehouses { get; set; } = [];
    public IReadOnlyList<ProductOption> Products { get; set; } = [];
}

public sealed class InvoiceLineFormViewModel
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    [Display(Name = "Ürün")]
    public int? ProductId { get; set; }

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

public sealed class ProductOption
{
    public int Id { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "Adet";
    public decimal SalePrice { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal TaxRate { get; set; }
}
