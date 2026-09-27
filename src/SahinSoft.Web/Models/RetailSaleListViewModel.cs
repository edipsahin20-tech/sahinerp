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
    public string PaymentMethodsSummary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
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
