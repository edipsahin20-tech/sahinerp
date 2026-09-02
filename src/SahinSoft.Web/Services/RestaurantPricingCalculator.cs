namespace SahinSoft.Web.Services;

// Restoran modülünde fiyatlar HER YERDE KDV DAHİL gösterilir (Product.SalePrice/ProductPortion.
// PriceOverride zaten KDV dahil — bkz. LookupController.Products'taki fatura/teklif fiyat
// politikası, tek kaynak olarak buraya da uygulanır). Ekranlarda (adisyon, masa durumu, mutfak
// fişi) KDV bir daha eklenmez ve matrah/KDV ayrımı GÖSTERİLMEZ — bu ayrıştırma yalnızca Faz 3'te
// (adisyon kapanışı, RetailSale/Fatura üretimi) muhasebe kaydı için kullanılır.
//
// Kural (kesin sıra): önce ikram/indirim KDV DAHİL tutardan düşülür, ayrıştırma bundan SONRA
// yapılır — matrah yuvarlanır, KDV tutarı kalan olarak hesaplanır ki ikisinin toplamı her zaman
// birebir KDV dahil tutara eşit kalsın (yuvarlama farkı oluşmaz).
public static class RestaurantPricingCalculator
{
    public static (decimal Matrah, decimal KdvTutari) ExtractTax(decimal kdvDahilTutar, decimal kdvOrani)
    {
        var matrah = Math.Round(kdvDahilTutar / (1 + kdvOrani / 100), 2, MidpointRounding.AwayFromZero);
        var kdvTutari = kdvDahilTutar - matrah;
        return (matrah, kdvTutari);
    }
}
