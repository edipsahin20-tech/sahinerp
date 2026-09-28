using SahinSoft.Domain.Entities;

namespace SahinSoft.Web.Models;

// Ürün Satış Raporu (Edip, 2026-09-28: "sonra ürün satış raporunu ekle filitre aynı mantıkta
// olsun odeme tipini kaldır oraya ketegori,marka,urun gibi filitreler ekle") - RetailSaleLine
// kayıtlarını (iptal edilmiş fişler HARİÇ) ürün bazında toplar; şube/tarih + kategori/marka/ürün
// filtrelenebilir, ödeme türü filtresi YOK (bu rapor ödeme değil ürün odaklı).
public sealed class ProductSalesReportViewModel
{
    public List<ProductSalesReportItemViewModel> Items { get; set; } = [];
    public List<Branch> Branches { get; set; } = [];
    public List<ProductCategory> Categories { get; set; } = [];
    public List<string> Brands { get; set; } = [];

    public int? BranchId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? ProductQuery { get; set; }

    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
}

public sealed class ProductSalesReportItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public decimal QuantitySold { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
