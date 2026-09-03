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

    public string? CancelledByUserId { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }

    // Müşteri hesap istediğinde işaretlenir (Edip, 2026-09-03: MASTER tasarımdaki "HESAP İSTENDİ"
    // rozeti) - kapanışta zaten Status=Closed olacağı için ayrıca temizlenmesine gerek yok, açık
    // adisyon listelerinde bu alan null olmayan her check zaten "hesap bekliyor" demektir.
    public DateTime? BillRequestedAtUtc { get; set; }

    // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
    public Guid? SubmissionKey { get; set; }

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
