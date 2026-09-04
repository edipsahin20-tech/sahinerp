using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum RestaurantPaymentMethod
{
    [Display(Name = "Nakit")]
    Cash = 1,
    [Display(Name = "Kredi Kartı")]
    CreditCard = 2,
    [Display(Name = "Yemek Kartı")]
    MealCard = 3,

    // Ödenmez (madde 12, Edip 2026-09-04) - İkram'dan FARKLI: ürün/tutar tam fiyatla satılmış
    // sayılır (RetailSale.GrandTotal'e dahildir), ama fiilen TAHSİL EDİLMEMİŞTİR. CloseCheckAsync
    // bu yöntemle işaretlenen tutarı hem Sale (ciro) hem Collection (tahsilat) muhasebe
    // kayıtlarından NET OLARAK DIŞARIDA bırakır - cari hesap dengede kalır, gerçek gelir/tahsilat
    // asla oluşmaz. Yine de bir RestaurantPayment satırı olarak KAYDEDİLİR (izlenebilirlik/
    // raporlama için) - bkz. CloseCheckAsync.
    [Display(Name = "Ödenmez")]
    Unpaid = 4,

    // Açık Hesap (madde 13, Edip 2026-09-04) - Ödenmez'den FARKI: bu tutar bir CARIYE (gerçek
    // müşteri) borç yazılır, genel "Perakende Satışlar Carisi" değil - Sale (ciro) hareketi
    // NORMAL şekilde oluşur, sadece Collection (tahsilat) hareketi oluşmaz; bu, seçilen
    // carinin hesabında GERÇEK bir açık alacak bırakır (daha sonra normal Tahsilat ekranından
    // kapatılabilir). Cari seçimi ZORUNLUDUR - bkz. CloseCheckAsync.
    [Display(Name = "Açık Hesap")]
    OpenAccount = 5
}
