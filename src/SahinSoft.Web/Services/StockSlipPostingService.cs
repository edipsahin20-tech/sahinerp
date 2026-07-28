using System.Data;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class StockSlipPostingService(
    ApplicationDbContext dbContext,
    InventoryBalanceService inventoryBalance)
{
    public Task ApproveAsync(
        int stockSlipId,
        string approvedByUserId,
        CancellationToken cancellationToken = default)
    {
        // EnableRetryOnFailure() (Program.cs) sets a retrying execution strategy; elle açılan
        // transaction'lar bununla uyumlu değil, tüm bloğun CreateExecutionStrategy() üzerinden
        // "tekrar denenebilir birim" olarak sarılması gerekiyor (aksi halde InvalidOperationException).
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await ApproveCoreAsync(stockSlipId, approvedByUserId, cancellationToken));
    }

    private async Task ApproveCoreAsync(
        int stockSlipId,
        string approvedByUserId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var slip = await dbContext.StockSlips
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == stockSlipId, cancellationToken)
            ?? throw new InvalidOperationException("Stok fişi bulunamadı.");

        if (slip.Status != StockSlipStatus.Draft || slip.Lines.Count == 0)
        {
            throw new InvalidOperationException("Yalnızca satırı bulunan taslak stok fişleri onaylanabilir.");
        }

        var settings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1, cancellationToken);

        foreach (var line in slip.Lines)
        {
            if (line.Quantity <= 0)
            {
                throw new InvalidOperationException("Stok fişi satır miktarı sıfırdan büyük olmalıdır.");
            }

            if (settings.RequireProductVariant &&
                line.ProductVariantId is null &&
                await dbContext.ProductVariants.AnyAsync(
                    x => x.ProductId == line.ProductId && x.IsActive,
                    cancellationToken))
            {
                throw new InvalidOperationException($"{line.Product.Name} için renk/varyant seçilmelidir.");
            }

            var quantity = slip.SlipType == StockSlipType.Receipt ? line.Quantity : -line.Quantity;
            if (quantity < 0 && settings.EnforceStockLevel && !settings.AllowNegativeStock)
            {
                var available = await inventoryBalance.GetAvailableAsync(
                    line.ProductId,
                    line.ProductVariantId,
                    slip.WarehouseId,
                    cancellationToken);
                if (available < line.Quantity)
                {
                    throw new InvalidOperationException($"{line.Product.Name} için yeterli stok yok.");
                }
            }

            dbContext.StockMovements.Add(new StockMovement
            {
                MovementDateUtc = slip.SlipDateUtc,
                MovementType = quantity > 0 ? StockMovementType.AdjustmentIn : StockMovementType.AdjustmentOut,
                Quantity = quantity,
                UnitCost = line.UnitCost,
                DocumentNumber = slip.SlipNumber,
                Description = line.Description ?? slip.Description,
                ProductId = line.ProductId,
                ProductVariantId = line.ProductVariantId,
                WarehouseId = slip.WarehouseId,
                CostCenterId = slip.CostCenterId,
                BusinessProjectId = slip.BusinessProjectId,
                StockSlipLineId = line.Id
            });

            line.Product.StockQuantity += quantity;
            line.Product.UpdatedAtUtc = DateTime.UtcNow;
        }

        slip.Status = StockSlipStatus.Approved;
        slip.ApprovedByUserId = approvedByUserId;
        slip.ApprovedAtUtc = DateTime.UtcNow;
        slip.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task CancelAsync(
        int stockSlipId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await CancelCoreAsync(stockSlipId, cancelledByUserId, reason, cancellationToken));
    }

    private async Task CancelCoreAsync(
        int stockSlipId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var slip = await dbContext.StockSlips
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == stockSlipId, cancellationToken)
            ?? throw new InvalidOperationException("Stok fişi bulunamadı.");

        if (slip.Status != StockSlipStatus.Approved)
        {
            throw new InvalidOperationException("Yalnızca onaylanmış fişler iptal edilebilir.");
        }

        var reversalDocumentNumber = $"IPTAL-{slip.SlipNumber}";

        foreach (var line in slip.Lines)
        {
            var originalMovement = await dbContext.StockMovements
                .SingleOrDefaultAsync(x => x.StockSlipLineId == line.Id, cancellationToken);
            if (originalMovement is null)
            {
                continue;
            }

            dbContext.StockMovements.Add(new StockMovement
            {
                MovementDateUtc = DateTime.UtcNow,
                MovementType = originalMovement.MovementType,
                Quantity = -originalMovement.Quantity,
                UnitCost = originalMovement.UnitCost,
                DocumentNumber = reversalDocumentNumber,
                Description = $"Stok fişi iptali - {reason}",
                ProductId = originalMovement.ProductId,
                ProductVariantId = originalMovement.ProductVariantId,
                WarehouseId = originalMovement.WarehouseId,
                StockSlipLineId = line.Id,
                ReversalOfId = originalMovement.Id
            });

            line.Product.StockQuantity -= originalMovement.Quantity;
            line.Product.UpdatedAtUtc = DateTime.UtcNow;
        }

        slip.Status = StockSlipStatus.Cancelled;
        slip.CancelledByUserId = cancelledByUserId;
        slip.CancelledAtUtc = DateTime.UtcNow;
        slip.CancellationReason = reason;
        slip.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
