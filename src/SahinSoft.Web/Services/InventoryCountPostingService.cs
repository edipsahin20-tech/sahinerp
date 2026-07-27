using System.Data;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class InventoryCountPostingService(
    ApplicationDbContext dbContext,
    InventoryBalanceService inventoryBalance)
{
    public async Task ApproveAsync(
        int inventoryCountId,
        string approvedByUserId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var count = await dbContext.InventoryCounts
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == inventoryCountId, cancellationToken)
            ?? throw new InvalidOperationException("Sayım fişi bulunamadı.");

        if (count.Status is InventoryCountStatus.Approved or InventoryCountStatus.Cancelled ||
            count.Lines.Count == 0)
        {
            throw new InvalidOperationException("Bu sayım fişi onaylanamaz.");
        }

        var settings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1, cancellationToken);

        foreach (var line in count.Lines)
        {
            if (settings.RequireProductVariant &&
                line.ProductVariantId is null &&
                await dbContext.ProductVariants.AnyAsync(
                    x => x.ProductId == line.ProductId && x.IsActive,
                    cancellationToken))
            {
                throw new InvalidOperationException($"{line.Product.Name} için renk/varyant seçilmelidir.");
            }

            var currentQuantity = await inventoryBalance.GetOnHandAsync(
                line.ProductId,
                line.ProductVariantId,
                count.WarehouseId,
                cancellationToken);

            line.SystemQuantity = currentQuantity;
            var difference = line.CountedQuantity - currentQuantity;
            if (difference == 0)
            {
                continue;
            }

            dbContext.StockMovements.Add(new StockMovement
            {
                MovementDateUtc = count.CountDateUtc,
                MovementType = difference > 0
                    ? StockMovementType.InventoryCountSurplus
                    : StockMovementType.InventoryCountShortage,
                Quantity = difference,
                DocumentNumber = count.CountNumber,
                Description = difference > 0
                    ? $"Sayım fazlası - Sistem: {currentQuantity:N3}, Sayılan: {line.CountedQuantity:N3}, Fark: +{difference:N3}"
                    : $"Sayım noksanı - Sistem: {currentQuantity:N3}, Sayılan: {line.CountedQuantity:N3}, Fark: {difference:N3}",
                ProductId = line.ProductId,
                ProductVariantId = line.ProductVariantId,
                WarehouseId = count.WarehouseId,
                InventoryCountLineId = line.Id
            });

            line.Product.StockQuantity += difference;
            line.Product.UpdatedAtUtc = DateTime.UtcNow;
        }

        count.Status = InventoryCountStatus.Approved;
        count.ApprovedByUserId = approvedByUserId;
        count.ApprovedAtUtc = DateTime.UtcNow;
        count.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
