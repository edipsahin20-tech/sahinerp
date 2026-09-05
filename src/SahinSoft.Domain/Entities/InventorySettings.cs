using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class InventorySettings : EntityBase
{
    public bool RequireBarcode { get; set; } = true;
    public bool AutoGenerateBarcode { get; set; } = true;
    public string DefaultBarcodeType { get; set; } = "EAN13";
    public string DefaultScalePrefix { get; set; } = "27";
    public bool EnforceStockLevel { get; set; } = true;
    public bool AllowNegativeStock { get; set; }
    public bool AllowSaleWhenOutOfStock { get; set; }
    public bool EnableMinimumStockWarning { get; set; } = true;
    public bool RequireTransferApproval { get; set; } = true;
    public bool TrackStockByVariant { get; set; }
    public bool RequireProductVariant { get; set; }
    public bool AllowSaleBelowCost { get; set; }
    public bool IsRestaurantModuleEnabled { get; set; }

    // Restoran Dashboard'daki "Günün Hedefi" halkası için - boş/0 ise o bölüm hiç gösterilmez
    // (Edip, 2026-09-03: MASTER_SahinSoft_Restoran_POS_Premium.html referansı, "hedef" uydurma
    // bir sayı değil, gerçek ayarlanabilir bir değer).
    public decimal? DailyRevenueTarget { get; set; }

    // Mutfağa gönderilen bir sipariş, bu süre (dk) dolduğunda mutfak personeli hiç dokunmasa
    // bile otomatik "Hazır" durumuna geçer (Edip, 2026-09-03) - boş/0 ise otomatik geçiş kapalı,
    // her şey elle ilerletilir (bugünkü davranış). Bkz. KitchenAutoReadyBackgroundService.
    public int? KitchenAutoReadyMinutes { get; set; }

    // Kapalıyken (varsayılan - Edip, 2026-09-03: "default yapılmasın") mutfağa gönderilen
    // siparişler hiç KitchenTicket/KDS takibine girmez, doğrudan Servis Edildi sayılır - Mutfak
    // ekranında Hazır/Servis Edildi gibi tıklamalara gerek kalmaz. Açıkken bugünkü KDS akışı
    // (Sent→InProgress→Ready→Served) aynen çalışır. Bkz. RestaurantPostingService.
    // SendOrderToKitchenCoreAsync.
    public bool IsKitchenTrackingEnabled { get; set; }

    // Kapalıyken (varsayılan - Edip, 2026-09-03: "kapalı default olsun") yeni masa/self satış/
    // paket siparişi açmak için açık bir vardiya ŞART DEĞİL, bugünkü gibi çalışır. Açıkken hiç
    // açık RestaurantCashShift yoksa yeni satış başlatılamaz - bkz. RestaurantPostingService.
    // EnsureShiftOpenIfRequiredAsync. Vardiya'nın kendi FinancialAccountId bazlı kapsam hatası
    // AYRI ve HENÜZ DÜZELTİLMEDİ (Edip'in deyimiyle "sonra yapacağız") - bu parametre sadece
    // "satış başlatmak için açık vardiya şart mı" sorusuna cevap verir, o hatayı çözmez.
    public bool RequireOpenShiftForSales { get; set; }

    // Kapalıyken (varsayılan - Edip, 2026-09-03: "şimdilik default sorulmasın") sipariş satırı
    // iptalinde gerekçe sorulmaz, otomatik bir gerekçeyle direkt iptal edilir. Açıkken kasiyerden
    // gerekçe istenir (bkz. Check.cshtml #cancelLineModal, CancellationReasonPresets varsa hazır
    // seçenekler de gösterilir).
    public bool RequireCancellationReason { get; set; }

    // Sipariş satırı iptalinde hazır gerekçe seçenekleri - her satır bir gerekçe, boşsa hazır
    // seçenek gösterilmez sadece serbest metin kutusu kalır (Edip, 2026-09-03: "otomatik iptal
    // nedenleri girilecek alanlar ekle").
    public string? CancellationReasonPresets { get; set; }

    // Ürün notu (mutfağa iletilecek) için hazır not seçenekleri - her satır bir not (Edip,
    // 2026-09-03: "otomatik not girilecek alanlar ekle").
    public string? QuickNotePresets { get; set; }

    // Yok (None) iken restoran Kapat/Öde bugünkü gibi hiçbir fiskal cihaz çağrısı yapmadan
    // çalışır - bkz. SahinSoft.FiscalAgent projesi. Bir cihaz seçilip adres girildiğinde nakit/
    // kredi kartı/yemek çeki ödemeleri doğrudan yazarkasaya gönderilir (fatura kesilen satışlar
    // hariç - bkz. RestaurantController.ClosePayment).
    public FiscalDeviceType FiscalDeviceType { get; set; } = FiscalDeviceType.None;
    public string? FiscalAgentUrl { get; set; }

    // "Şifre sorulsun mu?" (Edip, 2026-09-04, madde 21) - açık olan kritik işlem, YETKİLİ
    // kullanıcı tarafından yapılsa BİLE ikinci bir yetkilinin PIN'ini ister; onaylayan kişi
    // MEVCUT oturumdaki kişi olmak zorunda değil (uzaktan müdür onayı senaryosu). Onaylayan,
    // ilgili yetkiye sahip olmalı (bkz. RestaurantPermissionService.VerifyApproverPinAsync).
    // Her onay RestaurantPermissionAuditLog'a kaydedilir. Kapalıyken (varsayılan) davranış
    // bugünküyle aynı - sadece RestaurantPermissionProfile'ın kendisi yeterli.
    public bool RequireSecondApprovalForCancelOrderLine { get; set; }
    public bool RequireSecondApprovalForCancelReceipt { get; set; }
    public bool RequireSecondApprovalForDiscount { get; set; }
    public bool RequireSecondApprovalForComplimentary { get; set; }
    public bool RequireSecondApprovalForEditKitchenSentLines { get; set; }
    public bool RequireSecondApprovalForAddNote { get; set; }

    // Self Satış Hızlı Ödeme (madde 3, Edip 2026-09-04) - Nakit/Kredi Kartı/Yemek Çeki tuşu TAM
    // tutarı anında alır ve satışı hiçbir onay istemeden kapatır. Bu parametre kapalıyken
    // (varsayılan) kapanışın ardından hiçbir şey sorulmadan doğrudan yeni/boş Self Satış ekranına
    // dönülür. Açıkken küçük bir "Fiş Yazdır | Kapat" diyaloğu gösterilir - Fiş Yazdır ödeme
    // yöntemi dahil fişi yazdırır, Kapat yazdırmadan kapatır; İKİSİ DE sonunda boş Self Satış
    // ekranına döner.
    public bool RequireReceiptPromptAfterQuickPay { get; set; }

    // Ödenmez ödeme tipi (madde 12) - kapalıyken (varsayılan) ödeme ekranında "Ödenmez" butonu
    // hiç gösterilmez, bugünkü gibi sadece Nakit/Kredi Kartı/Yemek Çeki vardır.
    public bool ShowUnpaidPaymentType { get; set; }

    // Sağ İşlem Menüsü (madde 22, Edip 2026-09-05) - İKİ KATMANLI kontrol: (1) burada sistem
    // genelinde açık/kapalı mı (varsayılan HEPSİ AÇIK - mevcut davranışı bozmamak için), (2)
    // RestaurantPermissionProfile'da hangi kullanıcı/profil görebilir. İkisi de açık olmalı ki
    // buton görünsün - bkz. RestaurantPermissionService.CanSeeMenuItemAsync.
    public bool EnableTicketNoteButton { get; set; } = true;
    public bool EnableTableTransferButton { get; set; } = true;
    public bool EnableSendToKitchenButton { get; set; } = true;
    public bool EnablePriceCheckButton { get; set; } = true;
    public bool EnableKeyboardButton { get; set; } = true;
    public bool EnableHoldReceiptButton { get; set; } = true;
    public bool EnableHeldReceiptsButton { get; set; } = true;
    public bool EnableProductListButton { get; set; } = true;
    public bool EnableComplimentaryReceiptButton { get; set; } = true;
    public bool EnableReceiptListButton { get; set; } = true;
    public bool EnableClearOrderButton { get; set; } = true;

    // Masa açılışında kişi sayısı sorulsun mu (2026-09-05 teknik doküman madde 13) - kapalıysa
    // masa doğrudan varsayılan (masanın kapasitesi) kişi sayısıyla açılır, hiç modal göstermez.
    // Açık bir masaya TEKRAR girildiğinde bu ayardan bağımsız olarak ZATEN hiç sorulmaz (bkz.
    // Restaurant/Index.cshtml - o akış sadece BOŞ masalarda tetiklenir).
    public bool AskGuestCountOnTableOpen { get; set; } = true;

    // Madde 26 (Ayarlar, 2026-09-05) - "Self Satış görünsün mü / Masa Satış görünsün mü / Paket
    // görünsün mü" - sol ana menüdeki bu 3 modülün kendisini gösterir/gizler (yukarıdaki
    // EnableXButton'lardan FARKLI katman - onlar bir modülÜN İÇİNDEKİ tek tek butonlar, bunlar
    // modülün TAMAMI). Varsayılan HEPSİ AÇIK - mevcut davranışı bozmamak için.
    public bool ShowTableSaleNav { get; set; } = true;
    public bool ShowSelfSaleNav { get; set; } = true;
    public bool ShowPackageNav { get; set; } = true;

    public bool OrderToDispatchPurchaseAutoApprove { get; set; }
    public bool OrderToDispatchSalesAutoApprove { get; set; }
    public bool OrderToInvoicePurchaseAutoApprove { get; set; }
    public bool OrderToInvoiceSalesAutoApprove { get; set; }
    public bool DispatchToInvoicePurchaseAutoApprove { get; set; }
    public bool DispatchToInvoiceSalesAutoApprove { get; set; }
}
