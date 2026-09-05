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

    // Alt özet çubuğu (2026-09-05 teknik doküman madde 2) - Ara Toplam | İndirim | Ödenen | Kalan
    // HER ZAMAN görünür. AraToplam satırların net toplamı (kendi indirim/ikramları düşülmüş,
    // TicketDiscountAmount düşülmeMİŞ); TicketDiscountAmount SADECE adisyon/tutar indirimi (satır
    // indiriminden AYRI motor, madde 4) - PayableTotal = AraToplam - TicketDiscountAmount.
    public decimal AraToplam { get; set; }
    public decimal TicketDiscountAmount { get; set; }
    public decimal PaidTotal { get; set; }

    // Self Satış adisyonlarında "Masaya Aktar" butonu/modalı için doldurulur - bkz.
    // RestaurantController.Check ve RestaurantSelfSaleController.TransferToTable.
    public bool IsSelfSaleCheck { get; set; }
    public List<RestaurantTransferTableOptionViewModel> AvailableTables { get; set; } = [];

    // MASTER tasarımdaki HESAP İSTENDİ rozeti - masa/self satış ekranlarının ikisinde de
    // gösterilebilir olsun diye Self Satış'a da kısıtlanmadı.
    public bool BillRequested { get; set; }

    // Fiş Notu - adisyonun TAMAMINI ilgilendiren serbest not, ürün bazlı KitchenNote'tan ayrı
    // (Edip, 2026-09-03: eski POS ekranındaki "Fiş Notu" ikonu).
    public string? TicketNote { get; set; }

    // Yazar kasa entegrasyonu - Ayarlar > Stok Parametreleri'nde bir cihaz seçilip adres
    // girilmişse true, restaurant-close-payment.js ödemeyi önce buradaki adrese (yerel
    // SahinSoft.FiscalAgent) gönderir. Fatura kesilen satışlarda (CustomerId seçiliyse) bu
    // ekrandan hiç kullanılmaz - bkz. JS.
    public bool IsFiscalEnabled { get; set; }
    public string? FiscalAgentUrl { get; set; }

    // KDS takibi kapalıyken "Mutfağa Gönderilmiş Siparişler" paneli hiç gösterilmez - takip
    // edilecek bir şey yok, sadece kalabalık yapar (Edip, 2026-09-03).
    public bool IsKitchenTrackingEnabled { get; set; }

    // İptal ederken gerekçe sorulsun mu ve hazır gerekçe/not seçenekleri (Edip, 2026-09-03:
    // "iptal nedeni sorulsun mu diye parametre bağla ... otomatik iptal nedenleri/not girilecek
    // alanlar ekle"). Kapalıyken JS hiç modal açmadan otomatik bir gerekçeyle direkt iptal eder.
    public bool RequireCancellationReason { get; set; }
    public List<string> CancellationReasonPresets { get; set; } = [];
    public List<string> QuickNotePresets { get; set; } = [];

    // "Şifre sorulsun mu?" (madde 21) - açıksa JS ilgili işlemden önce ikinci bir yetkilinin
    // PIN'ini ister (bkz. approverPinModal, RestaurantPermissionService.RequiresSecondApproval*).
    public bool RequireSecondApprovalForCancelOrderLine { get; set; }
    public bool RequireSecondApprovalForEditKitchenSentLines { get; set; }
    public bool RequireSecondApprovalForComplimentary { get; set; }
    public bool RequireSecondApprovalForDiscount { get; set; }

    // Self Satış Hızlı Ödeme (madde 3) - açıkken kapanış sonrası "Fiş Yazdır | Kapat" diyaloğu
    // gösterilir, kapalıyken (varsayılan) hiç sorulmadan boş ekrana dönülür.
    public bool RequireReceiptPromptAfterQuickPay { get; set; }

    // Ödenmez ödeme tipi (madde 12) - kapalıyken (varsayılan) ödeme ekranında "Ödenmez" butonu
    // hiç gösterilmez.
    public bool ShowUnpaidPaymentType { get; set; }

    // Cari Ekle (madde 13) - adisyona bağlanmış müşteri, varsa. "Açık Hesap" ödeme yöntemi
    // bu olmadan kullanılamaz.
    public int? AttachedCustomerId { get; set; }
    public string? AttachedCustomerDisplay { get; set; }

    // Tahsilat Carileri (madde 14) - "Tahsilat Carisi" işaretli aktif müşteriler; ödeme
    // ekranında HER BİRİ kendi adıyla bir buton olarak belirir (hard-code yok, Trendyol/Getir/
    // Yemeksepeti/gelecekteki platformlar bu listeden gelir).
    public List<RestaurantCollectionCariViewModel> CollectionCaris { get; set; } = [];

    // Sağ İşlem Menüsü (madde 22) - İKİ katmanın (sistem geneli + kullanıcı yetkisi) BİRLEŞİMİ,
    // sunucuda hesaplanıp buraya konur - view sadece bu tek bool'a bakar.
    public bool ShowTicketNoteButton { get; set; } = true;
    public bool ShowTableTransferButton { get; set; } = true;
    public bool ShowSendToKitchenButton { get; set; } = true;
    public bool ShowPriceCheckButton { get; set; } = true;
    public bool ShowKeyboardButton { get; set; } = true;
    public bool ShowHoldReceiptButton { get; set; } = true;
    public bool ShowHeldReceiptsButton { get; set; } = true;
    public bool ShowProductListButton { get; set; } = true;
    public bool ShowComplimentaryReceiptButton { get; set; } = true;
    public bool ShowReceiptListButton { get; set; } = true;
    public bool ShowClearOrderButton { get; set; } = true;

    public List<RestaurantSentOrderViewModel> SentOrders { get; set; } = [];
    public List<RestaurantCatalogCategoryViewModel> Catalog { get; set; } = [];
    public List<RestaurantFinancialAccountViewModel> FinancialAccounts { get; set; } = [];

    // Kasa Tanımları (madde 27, 2026-09-05) - kasiyerin şubesine bağlı kasadan çözülen, ödeme
    // yöntemine göre GERÇEK hedef hesap. Öncesinde JS her zaman FinancialAccounts[0]'ı (alfabetik
    // ilk hesap) kullanıyordu - Nakit ve Kredi Kartı aynı hesaba yazılıyordu, hangi şubenin hangi
    // kasası kullanıldığı hiç izlenmiyordu. Kasa tanımlı değilse (henüz yapılandırılmamış şube)
    // null kalır, JS eski davranışa (ilk hesap) geri düşer.
    public int? CashRegisterCashAccountId { get; set; }
    public int? CashRegisterCreditCardAccountId { get; set; }
    public int? CashRegisterMealCardAccountId { get; set; }

    // Kısmi ödeme (madde 4-8) - sunucuda kalıcı olarak kayıtlı, henüz kapanmamış ödemeler. Ana
    // ekranda Ödenen/Kalan özetini ve "Ödemeyi Al" modalının ilk açılışını doldurur.
    public List<RestaurantPendingPaymentViewModel> PendingPayments { get; set; } = [];
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
    public string Unit { get; set; } = "Adet";
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
    public string Unit { get; set; } = "Adet";

    // Barkod okuyucu ile satış için - hem Stok Tanıtım Kartı'ndaki tekil Barcode alanı hem de
    // ProductBarcode'daki ek kodlar (farklı paket boyutları vb.) dahil (Edip, 2026-09-03: "tek
    // satırda barkod ve isimden satış yapabilsin").
    public List<string> Barcodes { get; set; } = [];
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

public sealed class RestaurantPendingPaymentRequest
{
    public int CheckId { get; set; }
    public int Method { get; set; }
    public int? FinancialAccountId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class RestaurantCollectionCariViewModel
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class RestaurantPendingPaymentViewModel
{
    public int PendingPaymentId { get; set; }
    public int Method { get; set; }
    public int? FinancialAccountId { get; set; }
    public decimal Amount { get; set; }
}
