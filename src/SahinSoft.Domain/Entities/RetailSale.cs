using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

// Invoice'tan tamamen bağımsız, restoran modülünün ürettiği hafif "dahili satış kaydı" — hiçbir
// zaman Fatura listesinde görünmez. Fiş üzerinde FiscalizationStatus=NotFiscalized olduğu sürece
// "Dahili Perakende Satış Fişi — Resmî Mali Belge Değildir" ibaresi bulunur (bkz. §7, Karar 1:
// RetailSale/RetailSaleLine ayrı bir tablo ailesi, Invoice'a yeni tür EKLENMEDİ).
public sealed class RetailSale : EntityBase
{
    public string DocumentNumber { get; set; } = string.Empty;
    public RetailSaleStatus Status { get; set; } = RetailSaleStatus.Issued;
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;

    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ServiceChargeAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }

    public string? FiscalDeviceSerialNumber { get; set; }
    public string? FiscalReceiptNumber { get; set; }
    public string? ZReportNumber { get; set; }
    public RetailSaleFiscalizationStatus FiscalizationStatus { get; set; } = RetailSaleFiscalizationStatus.NotFiscalized;
    public string? FiscalTransactionId { get; set; }
    public string? EInvoiceUuid { get; set; }

    public string? CancelledByUserId { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }

    // Faz 3 kararı: restoran satışları her zaman "Perakende Satışlar Carisi" adlı sabit bir cariye
    // ve "Perakende yurtiçi ticaret" ticaret türüne postalanır (bkz. Invoice.TradeType ile aynı
    // GİB/e-fatura serbest metin alanı örüntüsü, burada RetailSale kendi kopyasını tutar çünkü
    // Invoice'a yeni tür eklenmedi - Karar 1).
    public string? TradeType { get; set; }

    public int RestaurantCheckId { get; set; }
    public RestaurantCheck RestaurantCheck { get; set; } = null!;

    // Opsiyonel — walk-in satışta boş kalabilir.
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public ICollection<RetailSaleLine> Lines { get; set; } = new List<RetailSaleLine>();
}
