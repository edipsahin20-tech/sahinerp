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
    public Task ApproveAsync(
        int inventoryCountId,
        string approvedByUserId,
        CancellationToken cancellationToken = default)
    {
        // EnableRetryOnFailure() (Program.cs) sets a retrying execution strategy; elle açılan
        // transaction'lar bununla uyumlu değil, tüm bloğun CreateExecutionStrategy() üzerinden
        // "tekrar denenebilir birim" olarak sarılması gerekiyor (aksi halde InvalidOperationException).
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await ApproveCoreAsync(inventoryCountId, approvedByUserId, cancellationToken));
    }

    private async Task ApproveCoreAsync(
        int inventoryCountId,
        string approvedByUserId,
        CancellationToken cancellationToken)
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

    public Task CancelAsync(
        int inventoryCountId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await CancelCoreAsync(inventoryCountId, cancelledByUserId, reason, cancellationToken));
    }

    private async Task CancelCoreAsync(
        int inventoryCountId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var count = await dbContext.InventoryCounts
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == inventoryCountId, cancellationToken)
            ?? throw new InvalidOperationException("Sayım fişi bulunamadı.");

        if (count.Status != InventoryCountStatus.Approved)
        {
            throw new InvalidOperationException("Yalnızca onaylanmış sayımlar iptal edilebilir.");
        }

        var reversalDocumentNumber = $"IPTAL-{count.CountNumber}";

        foreach (var line in count.Lines)
        {
            var originalMovement = await dbContext.StockMovements
                .SingleOrDefaultAsync(x => x.InventoryCountLineId == line.Id, cancellationToken);
            if (originalMovement is null)
            {
                continue;
            }

            dbContext.StockMovements.Add(new StockMovement
            {
                MovementDateUtc = DateTime.UtcNow,
                MovementType = originalMovement.MovementType,
                Quantity = -originalMovement.Quantity,
                DocumentNumber = reversalDocumentNumber,
                Description = $"Sayım iptali - {reason}",
                ProductId = originalMovement.ProductId,
                ProductVariantId = originalMovement.ProductVariantId,
                WarehouseId = originalMovement.WarehouseId,
                InventoryCountLineId = line.Id,
                ReversalOfId = originalMovement.Id
            });

            line.Product.StockQuantity -= originalMovement.Quantity;
            line.Product.UpdatedAtUtc = DateTime.UtcNow;
        }

        count.Status = InventoryCountStatus.Cancelled;
        count.CancelledByUserId = cancelledByUserId;
        count.CancelledAtUtc = DateTime.UtcNow;
        count.CancellationReason = reason;
        count.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
