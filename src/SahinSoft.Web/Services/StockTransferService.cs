using Microsoft.EntityFrameworkCore;
using System.Data;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class StockTransferService(
    ApplicationDbContext dbContext,
    InventoryBalanceService inventoryBalance)
{
    public Task ApproveAsync(int transferId, string approvedByUserId, CancellationToken cancellationToken = default)
    {
        // EnableRetryOnFailure() (Program.cs) sets a retrying execution strategy; elle açılan
        // transaction'lar bununla uyumlu değil, tüm bloğun CreateExecutionStrategy() üzerinden
        // "tekrar denenebilir birim" olarak sarılması gerekiyor (aksi halde InvalidOperationException).
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await ApproveCoreAsync(transferId, approvedByUserId, cancellationToken));
    }

    private async Task ApproveCoreAsync(int transferId, string approvedByUserId, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var transfer = await dbContext.StockTransfers
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == transferId, cancellationToken)
            ?? throw new InvalidOperationException("Sevk belgesi bulunamadı.");

        if (transfer.Status != StockTransferStatus.Draft)
        {
            throw new InvalidOperationException("Yalnızca taslak sevk belgesi onaylanabilir.");
        }

        if (transfer.FromWarehouseId == transfer.ToWarehouseId)
        {
            throw new InvalidOperationException("Çıkış ve giriş deposu aynı olamaz.");
        }

        if (transfer.Lines.Count == 0)
        {
            throw new InvalidOperationException("Sevk belgesinde en az bir ürün olmalıdır.");
        }

        var settings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1, cancellationToken);

        foreach (var line in transfer.Lines)
        {
            if (line.Quantity <= 0)
            {
                throw new InvalidOperationException("Sevk miktarı sıfırdan büyük olmalıdır.");
            }

            if (settings.RequireProductVariant &&
                line.ProductVariantId is null &&
                await dbContext.ProductVariants.AnyAsync(
                    x => x.ProductId == line.ProductId && x.IsActive,
                    cancellationToken))
            {
                throw new InvalidOperationException($"Ürün {line.ProductId} için renk/varyant seçilmelidir.");
            }

            var availableQuantity = await inventoryBalance.GetAvailableAsync(
                line.ProductId,
                line.ProductVariantId,
                transfer.FromWarehouseId,
                cancellationToken);

            if (settings.EnforceStockLevel &&
                !settings.AllowNegativeStock &&
                availableQuantity < line.Quantity)
            {
                throw new InvalidOperationException(
                    $"Ürün {line.ProductId} için kaynak depoda yeterli stok yok. Mevcut: {availableQuantity:N3}");
            }

            dbContext.StockMovements.AddRange(
                Movement(line, transfer, transfer.FromWarehouseId, -line.Quantity, StockMovementType.TransferOut),
                Movement(line, transfer, transfer.ToWarehouseId, line.Quantity, StockMovementType.TransferIn));
        }

        transfer.Status = StockTransferStatus.Approved;
        transfer.ApprovedByUserId = approvedByUserId;
        transfer.ApprovedAtUtc = DateTime.UtcNow;
        transfer.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task CancelAsync(
        int transferId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await CancelCoreAsync(transferId, cancelledByUserId, reason, cancellationToken));
    }

    private async Task CancelCoreAsync(
        int transferId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var transfer = await dbContext.StockTransfers
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == transferId, cancellationToken)
            ?? throw new InvalidOperationException("Sevk belgesi bulunamadı.");

        if (transfer.Status != StockTransferStatus.Approved)
        {
            throw new InvalidOperationException("Yalnızca onaylanmış transferler iptal edilebilir.");
        }

        var reversalDocumentNumber = $"IPTAL-{transfer.TransferNumber}";

        foreach (var line in transfer.Lines)
        {
            var originalMovements = await dbContext.StockMovements
                .Where(x => x.StockTransferLineId == line.Id)
                .ToListAsync(cancellationToken);

            foreach (var originalMovement in originalMovements)
            {
                dbContext.StockMovements.Add(new StockMovement
                {
                    MovementDateUtc = DateTime.UtcNow,
                    MovementType = originalMovement.MovementType,
                    Quantity = -originalMovement.Quantity,
                    DocumentNumber = reversalDocumentNumber,
                    Description = $"Transfer iptali - {reason}",
                    ProductId = originalMovement.ProductId,
                    ProductVariantId = originalMovement.ProductVariantId,
                    WarehouseId = originalMovement.WarehouseId,
                    StockTransferLineId = line.Id,
                    ReversalOfId = originalMovement.Id
                });
            }
        }

        transfer.Status = StockTransferStatus.Cancelled;
        transfer.CancelledByUserId = cancelledByUserId;
        transfer.CancelledAtUtc = DateTime.UtcNow;
        transfer.CancellationReason = reason;
        transfer.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static StockMovement Movement(
        StockTransferLine line,
        StockTransfer transfer,
        int warehouseId,
        decimal quantity,
        StockMovementType movementType) =>
        new()
        {
            ProductId = line.ProductId,
            ProductVariantId = line.ProductVariantId,
            WarehouseId = warehouseId,
            StockTransferLineId = line.Id,
            MovementDateUtc = transfer.TransferDateUtc,
            MovementType = movementType,
            Quantity = quantity,
            DocumentNumber = transfer.TransferNumber,
            Description = transfer.Description
        };
}
