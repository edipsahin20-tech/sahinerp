using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class InventoryBalanceService(ApplicationDbContext dbContext)
{
    public async Task<decimal> GetOnHandAsync(
        int productId,
        int? productVariantId,
        int warehouseId,
        CancellationToken cancellationToken = default) =>
        await dbContext.StockMovements
            .Where(x =>
                x.ProductId == productId &&
                x.ProductVariantId == productVariantId &&
                x.WarehouseId == warehouseId)
            .SumAsync(x => (decimal?)x.Quantity, cancellationToken) ?? 0;

    public async Task<decimal> GetAvailableAsync(
        int productId,
        int? productVariantId,
        int warehouseId,
        CancellationToken cancellationToken = default)
    {
        var onHand = await GetOnHandAsync(
            productId,
            productVariantId,
            warehouseId,
            cancellationToken);
        var reserved = await dbContext.StockReservations
            .Where(x =>
                x.ProductId == productId &&
                x.ProductVariantId == productVariantId &&
                x.WarehouseId == warehouseId &&
                x.Status == ReservationStatus.Active &&
                x.ReservedUntilUtc > DateTime.UtcNow)
            .SumAsync(x => (decimal?)x.Quantity, cancellationToken) ?? 0;
        return onHand - reserved;
    }
}
