namespace SahinSoft.FiscalAgent.Models;

// Ödeme yöntemi - SahinSoft.Web'deki RestaurantPaymentMethod (Cash/CreditCard/MealCard) ile
// birebir aynı isimlendirme, agent bunu Inpos.PaymentType'a çevirir (bkz. SaleOrchestrator).
public enum FiscalPaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    MealCard = 3
}

public sealed class FiscalSaleItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }

    // Cihazdaki PLU bölüm (section) numarası - yazarkasada tanımlı KDV/bölüm eşlemesine göre
    // ayarlanmalı, gerçek cihaz geldiğinde Edip ile birlikte doğru section numaraları
    // netleştirilecek. Şimdilik 1 (varsayılan/tek bölüm) kullanılıyor.
    public int Section { get; set; } = 1;
}

public sealed class FiscalPaymentDto
{
    public FiscalPaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
}

public sealed class FiscalSaleRequest
{
    public string CashierName { get; set; } = string.Empty;
    public List<FiscalSaleItemDto> Items { get; set; } = [];
    public List<FiscalPaymentDto> Payments { get; set; } = [];

    // SahinSoft tarafındaki adisyon/check numarası - cihazla hiç ilgisi yok, sadece agent
    // loglarında hangi SahinSoft satışının hangi fiskal işleme karşılık geldiğini izlemek için.
    public string? ReferenceCheckNumber { get; set; }
}

public sealed class FiscalSaleResult
{
    public bool Success { get; set; }
    public int? ReceiptNo { get; set; }
    public int? ZNo { get; set; }
    public int? EruNo { get; set; }
    public DateTime? DeviceDateTimeUtc { get; set; }
    public string? DeviceSerialNumber { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorDetail { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Simulated { get; set; }
}

public sealed class DeviceStatusResult
{
    public bool Connected { get; set; }
    public string EcrState { get; set; } = string.Empty;
    public string SaleState { get; set; } = string.Empty;
    public bool SimulationMode { get; set; }
    public string? ErrorMessage { get; set; }
}
