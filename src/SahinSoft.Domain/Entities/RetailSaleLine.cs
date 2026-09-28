using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class RetailSaleLine : EntityBase
{
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public decimal TaxRateSnapshot { get; set; }
    public decimal DiscountAmountSnapshot { get; set; }
    public decimal LineTotal { get; set; }

    // RestaurantOrderLine.IsComplimentary'den kapanışta kopyalanır (Çıktı Tasarımcısı X/Z
    // raporlarının İndirim ile İkram'ı GERÇEKTEN ayırabilmesi için - DiscountAmountSnapshot tek
    // başına ikisini ayırt edemiyordu, bkz. RestaurantPostingService.CloseCheckAsync).
    public bool IsComplimentary { get; set; }

    public int RetailSaleId { get; set; }
    public RetailSale RetailSale { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
