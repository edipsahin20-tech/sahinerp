using SahinSoft.Domain.Entities;

namespace SahinSoft.Web.Services;

// Bir ürünün stok hareketi üretip üretmeyeceği: ürün "Stok takip edilsin" ise her zaman; Hizmet tipindeki ürün
// stok takibi kapalı olsa bile yalnızca "Hizmet ürünlerinde de stok hareketi oluştur" parametresi AÇIKSA.
public static class StockPolicy
{
    public static bool MovesStock(Product product, InventorySettings settings) =>
        product.TrackStock || (settings.StockMovementForServiceItems && string.Equals(product.ProductType, "Hizmet", StringComparison.OrdinalIgnoreCase));
}
