namespace SahinSoft.Web.Models;

public sealed class RestaurantCheckViewModel
{
    public int CheckId { get; set; }
    public string CheckNumber { get; set; } = string.Empty;
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public DateTime OpenedAtUtc { get; set; }
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();
    public Guid ClosePaymentSubmissionKey { get; set; } = Guid.NewGuid();
    public decimal PayableTotal { get; set; }

    // Self Satış adisyonlarında "Masaya Aktar" butonu/modalı için doldurulur - bkz.
    // RestaurantController.Check ve RestaurantSelfSaleController.TransferToTable.
    public bool IsSelfSaleCheck { get; set; }
    public List<RestaurantTransferTableOptionViewModel> AvailableTables { get; set; } = [];

    // MASTER tasarımdaki HESAP İSTENDİ rozeti - masa/self satış ekranlarının ikisinde de
    // gösterilebilir olsun diye Self Satış'a da kısıtlanmadı.
    public bool BillRequested { get; set; }

    // Yazar kasa entegrasyonu - Ayarlar > Stok Parametreleri'nde bir cihaz seçilip adres
    // girilmişse true, restaurant-close-payment.js ödemeyi önce buradaki adrese (yerel
    // SahinSoft.FiscalAgent) gönderir. Fatura kesilen satışlarda (CustomerId seçiliyse) bu
    // ekrandan hiç kullanılmaz - bkz. JS.
    public bool IsFiscalEnabled { get; set; }
    public string? FiscalAgentUrl { get; set; }

    public List<RestaurantSentOrderViewModel> SentOrders { get; set; } = [];
    public List<RestaurantCatalogCategoryViewModel> Catalog { get; set; } = [];
    public List<RestaurantFinancialAccountViewModel> FinancialAccounts { get; set; } = [];
}

public sealed record RestaurantTransferTableOptionViewModel(int TableId, string SectionName, string TableName, bool IsOccupied);

public sealed class RestaurantFinancialAccountViewModel
{
    public int FinancialAccountId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class RestaurantSentOrderViewModel
{
    public int OrderId { get; set; }
    public DateTime OrderedAtUtc { get; set; }
    public string OrderedByName { get; set; } = string.Empty;
    public List<RestaurantSentOrderLineViewModel> Lines { get; set; } = [];
}

public sealed class RestaurantSentOrderLineViewModel
{
    public int LineId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? PortionName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsComplimentary { get; set; }
    public string? KitchenNote { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool SentToKitchen { get; set; }
    public bool CanCancel { get; set; }
}

public sealed class RestaurantCatalogCategoryViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    // Kategori Tanımla'da seçilen renk (Edip, 2026-09-03: "renk seçebilsin seçtiği renk satış
    // programında o kategoride gözüksün") - kategori sekmesinde aynen kullanılır.
    public string Color { get; set; } = "#6c757d";
    public List<RestaurantCatalogProductViewModel> Products { get; set; } = [];
}

public sealed class RestaurantCatalogProductViewModel
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal TaxRate { get; set; }
    public bool HasKitchenStation { get; set; }
    public string? ImagePath { get; set; }
    public List<RestaurantCatalogPortionViewModel> Portions { get; set; } = [];
}

public sealed class RestaurantCatalogPortionViewModel
{
    public int PortionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? PriceOverride { get; set; }
    public bool IsDefault { get; set; }
}

// SendToKitchen'a gönderilen JSON gövdesi.
public sealed class RestaurantSendToKitchenRequest
{
    public int CheckId { get; set; }
    public Guid SubmissionKey { get; set; }
    public List<RestaurantSendToKitchenLineRequest> Lines { get; set; } = [];
}

public sealed class RestaurantSendToKitchenLineRequest
{
    public int ProductId { get; set; }
    public int? ProductPortionId { get; set; }
    public decimal Quantity { get; set; }
    public decimal DiscountAmount { get; set; }
    public bool IsComplimentary { get; set; }
    public string? KitchenNote { get; set; }
    public List<RestaurantSendToKitchenModifierRequest>? Modifiers { get; set; }
}

public sealed class RestaurantSendToKitchenModifierRequest
{
    public string NameSnapshot { get; set; } = string.Empty;
    public decimal PriceSnapshot { get; set; }
    public decimal Quantity { get; set; } = 1;
}

public sealed class RestaurantClosePaymentRequest
{
    public int CheckId { get; set; }
    public Guid SubmissionKey { get; set; }
    public int? CustomerId { get; set; }
    public List<RestaurantClosePaymentLineRequest> Payments { get; set; } = [];

    // Yazar kasa entegrasyonu açıkken JS tarafı satışı ÖNCE fiziksel cihaza gönderir, cihaz
    // başarılı dönerse bu alanları doldurup buraya iletir (bkz. restaurant-close-payment.js).
    // Entegrasyon kapalıyken veya fatura kesilen satışlarda hep null - RetailSale bugünkü gibi
    // hiçbir fiskal bilgi olmadan oluşur.
    public string? FiscalReceiptNumber { get; set; }
    public string? FiscalZNo { get; set; }
    public string? FiscalDeviceSerialNumber { get; set; }
}

public sealed class RestaurantClosePaymentLineRequest
{
    public int Method { get; set; }
    public int FinancialAccountId { get; set; }
    public decimal Amount { get; set; }
}
