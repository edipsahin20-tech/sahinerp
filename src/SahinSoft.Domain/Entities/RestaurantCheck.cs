using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class RestaurantCheck : EntityBase
{
    public string CheckNumber { get; set; } = string.Empty;
    public RestaurantCheckStatus Status { get; set; } = RestaurantCheckStatus.Open;
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAtUtc { get; set; }

    // Kapanışta sunucuda yeniden hesaplanır — istemciden gelen değere güvenilmez.
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ServiceChargeAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }

    // Adisyon/Tutar indirimi (Edip, 2026-09-05 teknik doküman, madde 4) - satır indiriminden
    // AYRI bir motor. Bilinçli olarak satırlara DAĞITILMAZ, satırların UnitPriceSnapshot/
    // DiscountAmountSnapshot'ına HİÇ dokunmaz, satırlara "İndirim" etiketi bastırmaz - sadece
    // bu adisyonun toplamından düşülür (bkz. RestaurantController.ComputeCheckRunningTotal,
    // RestaurantPostingService.ApplyTicketDiscountAsync/CloseCheckAsync).
    public decimal TicketDiscountAmount { get; set; }

    public string? CancelledByUserId { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }

    // Fişi Beklet (2026-09-05 teknik doküman madde 8) - önceden bekletme SADECE istemci
    // localStorage'ında tutuluyordu (henüz gönderilmemiş satırlar dahil), bu yüzden başka bir
    // terminalden/kasiyerden "listede görünmüyor" hatası veriyordu. Artık check GERÇEKTEN
    // Status=Open kalır (veri bütünlüğü/ödeme akışı bozulmaz) ama bu iki alan doluysa "bekleyen"
    // sayılır - RestaurantSelfSaleController.Index() kendi açık check'ini ararken bunu HARİÇ
    // tutar, Bekleyen Fişler listesi ise TAM TERSİNE SADECE bunu sorgular (şube bazlı, tüm
    // terminallerden görünür - bkz. RestaurantSelfSaleController.HeldReceipts/Recall).
    public DateTime? HeldAtUtc { get; set; }
    public string? HeldByUserId { get; set; }

    // Müşteri hesap istediğinde işaretlenir (Edip, 2026-09-03: MASTER tasarımdaki "HESAP İSTENDİ"
    // rozeti) - kapanışta zaten Status=Closed olacağı için ayrıca temizlenmesine gerek yok, açık
    // adisyon listelerinde bu alan null olmayan her check zaten "hesap bekliyor" demektir.
    public DateTime? BillRequestedAtUtc { get; set; }

    // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
    public Guid? SubmissionKey { get; set; }

    // Fiş Notu - ürün bazlı KitchenNote'tan FARKLI, adisyonun TAMAMINI ilgilendiren serbest not
    // (Edip, 2026-09-03: eski POS ekranındaki "Fiş Notu" ikonu referansı).
    public string? Note { get; set; }

    // Fiş İkram (Edip, 2026-09-04, madde 11) - TÜM adisyonu ikram eder. Doldurulmuşsa (Complimentary
    // AtUtc not null) bu adisyonun TÜM aktif satırları IsComplimentary=true'dur - ciroya dahil
    // DEĞİLDİR (bkz. RestaurantPostingService.ApplyReceiptComplimentaryAsync). Yetkili kullanıcı
    // (Administrator) gerekçeyi atlayabilir, diğerleri Kime/Neden alanlarını doldurmak zorundadır.
    public DateTime? ComplimentaryAtUtc { get; set; }
    public string? ComplimentaryByUserId { get; set; }
    public string? ComplimentaryReasonFor { get; set; }
    public string? ComplimentaryReasonWhy { get; set; }
    public string? ComplimentaryNote { get; set; }

    // Cari Ekle (madde 13, Edip 2026-09-04, onaylı Self Satış tasarımındaki "Cari Ekle" butonu) -
    // adisyona bir müşteri bağlar. "Açık Hesap" ödeme yöntemi bu alan dolu olmadan KULLANILAMAZ
    // (CloseCheckAsync "Cari seçmelisiniz." ile reddeder) - bkz. RestaurantPostingService.
    public int? AttachedCustomerId { get; set; }
    public Customer? AttachedCustomer { get; set; }

    public int RestaurantTableSessionId { get; set; }
    public RestaurantTableSession RestaurantTableSession { get; set; } = null!;

    // Müşteri baştan kurumsal fatura isterse mevcut Satış Faturası akışı burada bağlanır.
    public int? LinkedInvoiceId { get; set; }
    public Invoice? LinkedInvoice { get; set; }

    // Fatura istenmezse kapanışta üretilen dahili perakende satış fişi (bkz. RetailSale).
    public int? LinkedRetailSaleId { get; set; }
    public RetailSale? LinkedRetailSale { get; set; }

    public ICollection<RestaurantOrder> Orders { get; set; } = new List<RestaurantOrder>();
    public ICollection<RestaurantPayment> Payments { get; set; } = new List<RestaurantPayment>();
}
