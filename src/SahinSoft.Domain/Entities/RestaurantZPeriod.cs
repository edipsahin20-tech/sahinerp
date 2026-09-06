using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

// Z Dönem Kapatma test talimatı (2026-09-06, Edip: "bir satışın hangi Z'ye ait olduğu sonradan
// tahmin edilmemeli - satış finansal olarak kapanırken içinde bulunduğu aktif Z dönemiyle
// ilişkilendirilmeli") - RestaurantCashShift'ten (Vardiya, kasiyerin kendi kasa açılış/kapanışı,
// FinancialAccountId bazlı) KASITLI OLARAK AYRI bir tablo: ikisi aynı tabloyu paylaşsaydı, Z
// döneminin sürekli "açık" bir satır tutması Vardiya'nın "bu kasada zaten açık bir vardiya var"
// kontrolüyle çakışırdı. Her zaman TAM OLARAK BİR açık (Status=Open) dönem vardır - ilk satış
// kapanışında (RestaurantPostingService.GetOrCreateActiveZPeriodIdAsync) yoksa oluşturulur, Z
// alınınca bu satır kapanır ve hemen yenisi açılır. Snapshot alanları YALNIZCA kapanışta doldurulur
// ve bir daha DEĞİŞTİRİLMEZ (Z sonrası düzeltmeler yeni bir döneme düşer, bu dönemin özetini
// bozmaz).
public sealed class RestaurantZPeriod : EntityBase
{
    public RestaurantZPeriodStatus Status { get; set; } = RestaurantZPeriodStatus.Open;
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAtUtc { get; set; }
    public string? ClosedByUserId { get; set; }
    public int BranchId { get; set; }

    // Talimat 1 (2026-09-06) - otomatik (zamanlanmış) Z ile elle "Z Raporu Al" ayrımı, raporlama/
    // audit amaçlı. ClosedByUserId otomatik kapanışta null kalır (hiçbir kullanıcı tetiklemedi).
    public bool ClosedAutomatically { get; set; }

    // Kapanışta hesaplanıp donan özet (bkz. RestaurantReportsController.BuildZSummaryAsync ile
    // AYNI hesaplama, ama artık zaman-aralığı yerine RestaurantZPeriodId ile GERÇEK bağlantıdan).
    public int ReceiptCount { get; set; }
    public decimal GrossTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal NetTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal ComplimentaryTotal { get; set; }

    public ICollection<RetailSale> RetailSales { get; set; } = new List<RetailSale>();
}
