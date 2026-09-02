namespace SahinSoft.Web.Models.Api;

/// <summary>
/// Merkez→Şube senkron yanıtı. Kayıtlar Id (int) değil RecordId (Guid) ile eşleşir,
/// çünkü şube ve merkez ayrı veritabanları - Id'ler çakışabilir, RecordId global olarak
/// benzersizdir (bkz. EntityBase, NEWSEQUENTIALID() ile üretiliyor).
/// </summary>
public sealed class CatalogSyncResponse
{
    public DateTime ServerTimeUtc { get; set; }
    public List<CategorySyncItem> Categories { get; set; } = [];
    public List<TaxRateSyncItem> TaxRates { get; set; } = [];
    public List<ProductSyncItem> Products { get; set; } = [];
}

public sealed class CategorySyncItem
{
    public Guid RecordId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class TaxRateSyncItem
{
    public Guid RecordId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsExempt { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ProductSyncItem
{
    public Guid RecordId { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public bool TrackStock { get; set; }
    public bool IsActive { get; set; }
    public Guid CategoryRecordId { get; set; }
    public Guid TaxRateRecordId { get; set; }
}

/// <summary>
/// Şube→Merkez yönü (Faz C): kapanan bir adisyonun IntegrationOutboxMessage.PayloadJson içeriği.
/// Hem yazan taraf (RestaurantPostingService.CloseCheckAsync) hem okuyan taraf (merkezdeki
/// SyncController) bu tipi paylaşır - serbest anonim obje yerine tek kaynak.
/// Not: satır kalemleri (RetailSaleLine) bilerek taşınmıyor - merkez yalnızca konsolide cari/rapor
/// amaçlı Satış+Tahsilat tutarına ihtiyaç duyar, satır detayı şubenin kendi RetailSale kaydında
/// zaten var. Kasa/banka hareketi de merkezde YENİDEN oluşturulmaz - nakit fiziksel olarak şubede
/// kalır, merkez sadece cari (Perakende Satışlar Carisi) tarafını konsolide eder.
/// </summary>
public sealed class RestaurantCheckClosedPayload
{
    public Guid RetailSaleRecordId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string? TradeType { get; set; }
    public string CheckNumber { get; set; } = string.Empty;
}

public sealed class TransactionSyncRequest
{
    public List<TransactionSyncEvent> Events { get; set; } = [];
}

public sealed class TransactionSyncEvent
{
    public Guid RecordId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class TransactionSyncResult
{
    public int AcceptedCount { get; set; }
    public int SkippedCount { get; set; }
}
