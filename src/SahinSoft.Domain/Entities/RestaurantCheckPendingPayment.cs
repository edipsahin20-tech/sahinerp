using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

// Kısmi/parçalı ödeme - adisyon kapanmadan ÖNCE "şu tutar şu yöntemle alındı" kaydı (Edip,
// 2026-09-04, madde 4-8). Kapanışta CloseCheckAsync'in yazdığı RestaurantPayment/
// FinancialTransaction/CurrentAccountTransaction ile KARIŞTIRILMAMALI - bunlar MUHASEBEYE HENÜZ
// HİÇ İŞLENMEMİŞ, sadece kasiyerin ekranda "şimdiye kadar alınan" olarak görmesi ve adisyon
// kapatılırken CloseCheckAsync'e olduğu gibi devredilmesi için var. Adisyon kapanınca (veya
// "Ödeme İptal" ile) bu satırlar SİLİNİR - gerçek muhasebe hareketi hiç oluşmadıkları için
// silinmeleri güvenlidir (RestaurantOrderLine gibi hard-delete yasağı burada GEÇERLİ DEĞİL).
public sealed class RestaurantCheckPendingPayment : Common.EntityBase
{
    public int RestaurantCheckId { get; set; }
    public RestaurantCheck RestaurantCheck { get; set; } = null!;
    public RestaurantPaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public int? FinancialAccountId { get; set; }
    public FinancialAccount? FinancialAccount { get; set; }
    public string RecordedByUserId { get; set; } = string.Empty;
    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;
}
