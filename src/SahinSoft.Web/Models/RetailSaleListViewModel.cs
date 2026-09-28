using SahinSoft.Domain.Entities;

namespace SahinSoft.Web.Models;

// Perakende Fiş Listesi (Edip, 2026-09-28: "arka tarafa muhasebe programına... perakende fiş
// olarak kayıt ediyor mu ve muhasebe programından görebiliyor muyum... rapor ekranını muhasebe
// programına ekleyeceksin") - restoran modülünün ürettiği RetailSale kayıtlarını (RestaurantCheck
// kapanınca CloseCheckAsync'in yazdığı GERÇEK muhasebe fişi) ana ERP tarafından, şube/tarih/kanal/
// ödeme tipine göre süzülebilir şekilde listeler. Yeni bir "restoran-only" kopya veri deposu
// İCAT EDİLMEDİ - doğrudan RetailSale/RetailSaleLine/RestaurantPayment'i okur.
public sealed class RetailSaleListViewModel
{
    public List<RetailSaleListItemViewModel> Items { get; set; } = [];
    public List<Branch> Branches { get; set; } = [];
    public RetailSaleListTotalsViewModel Totals { get; set; } = new();

    public int? BranchId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Channel { get; set; }
    public int? PaymentMethod { get; set; }
    public int Page { get; set; } = 1;
    public int TotalCount { get; set; }
    public int PageSize { get; set; } = 50;
}

public sealed class RetailSaleListItemViewModel
{
    public int RetailSaleId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string SourceLabel { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public decimal CashAmount { get; set; }
    public decimal CreditCardAmount { get; set; }
    public decimal MealCardAmount { get; set; }
    public decimal UnpaidAmount { get; set; }
    public decimal OpenAccountAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
}

// Filtreye uyan TÜM kayıtların (sayfalama olmadan) ödeme türü bazlı toplamları (Edip,
// 2026-09-28: "toplam rakamlar bir yerde toplam nakit kredi kartı butun ödeme tıplerın
// toplandıgı gösteren bir alan da olsun").
public sealed class RetailSaleListTotalsViewModel
{
    public decimal CashTotal { get; set; }
    public decimal CreditCardTotal { get; set; }
    public decimal MealCardTotal { get; set; }
    public decimal UnpaidTotal { get; set; }
    public decimal OpenAccountTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }
}

public sealed class RetailSaleDetailViewModel
{
    public int RetailSaleId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? BranchAddress { get; set; }
    public string? BranchPhone { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string SourceLabel { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<RetailSaleDetailLineViewModel> Lines { get; set; } = [];
    public List<RetailSaleDetailPaymentViewModel> Payments { get; set; } = [];
}

public sealed class RetailSaleDetailLineViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class RetailSaleDetailPaymentViewModel
{
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
