using System.Data;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class DispatchNotePostingService(
    ApplicationDbContext dbContext,
    InventoryBalanceService inventoryBalance)
{
    public Task ApproveAsync(
        int dispatchNoteId,
        string approvedByUserId,
        CancellationToken cancellationToken = default)
    {
        // EnableRetryOnFailure() (Program.cs) sets a retrying execution strategy; elle açılan
        // transaction'lar bununla uyumlu değil, tüm bloğun CreateExecutionStrategy() üzerinden
        // "tekrar denenebilir birim" olarak sarılması gerekiyor (aksi halde InvalidOperationException).
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await ApproveCoreAsync(dispatchNoteId, approvedByUserId, cancellationToken));
    }

    private async Task ApproveCoreAsync(
        int dispatchNoteId,
        string approvedByUserId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var dispatch = await dbContext.DispatchNotes
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == dispatchNoteId, cancellationToken)
            ?? throw new InvalidOperationException("İrsaliye bulunamadı.");

        if (dispatch.Status != BusinessDocumentStatus.Draft)
        {
            throw new InvalidOperationException("Yalnızca taslak irsaliyeler onaylanabilir.");
        }

        if (dispatch.Lines.Count == 0)
        {
            throw new InvalidOperationException("İrsaliyede en az bir satır bulunmalıdır.");
        }

        var settings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1, cancellationToken);

        foreach (var line in dispatch.Lines)
        {
            if (line.Quantity <= 0)
            {
                throw new InvalidOperationException("İrsaliye satır miktarı sıfırdan büyük olmalıdır.");
            }

            if (!line.Product.TrackStock)
            {
                continue;
            }

            var signedQuantity = dispatch.DispatchType == InvoiceType.Sales
                ? -line.Quantity
                : line.Quantity;

            if (dispatch.DispatchType == InvoiceType.Sales &&
                settings.EnforceStockLevel &&
                !settings.AllowNegativeStock &&
                !settings.AllowSaleWhenOutOfStock)
            {
                var available = await inventoryBalance.GetAvailableAsync(
                    line.ProductId,
                    line.ProductVariantId,
                    dispatch.WarehouseId,
                    cancellationToken);

                if (available < line.Quantity)
                {
                    throw new InvalidOperationException(
                        $"{line.Product.Name} için yeterli stok yok. Mevcut: {available:N3}");
                }
            }

            dbContext.StockMovements.Add(new StockMovement
            {
                MovementDateUtc = dispatch.DispatchDateUtc,
                MovementType = dispatch.DispatchType == InvoiceType.Sales
                    ? StockMovementType.DispatchOut
                    : StockMovementType.DispatchIn,
                Quantity = signedQuantity,
                DocumentNumber = dispatch.DispatchNumber,
                Description = dispatch.Notes,
                ProductId = line.ProductId,
                ProductVariantId = line.ProductVariantId,
                WarehouseId = dispatch.WarehouseId,
                DispatchNoteLineId = line.Id
            });

            line.Product.StockQuantity += signedQuantity;
            line.Product.UpdatedAtUtc = DateTime.UtcNow;
        }

        dispatch.Status = BusinessDocumentStatus.Approved;
        dispatch.ApprovedByUserId = approvedByUserId;
        dispatch.ApprovedAtUtc = DateTime.UtcNow;
        dispatch.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task CancelAsync(
        int dispatchNoteId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await CancelCoreAsync(dispatchNoteId, cancelledByUserId, reason, cancellationToken));
    }

    private async Task CancelCoreAsync(
        int dispatchNoteId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var dispatch = await dbContext.DispatchNotes
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == dispatchNoteId, cancellationToken)
            ?? throw new InvalidOperationException("İrsaliye bulunamadı.");

        if (dispatch.Status != BusinessDocumentStatus.Approved)
        {
            throw new InvalidOperationException("Yalnızca onaylanmış irsaliyeler iptal edilebilir.");
        }

        var reversalDocumentNumber = $"IPTAL-{dispatch.DispatchNumber}";

        foreach (var line in dispatch.Lines)
        {
            var originalMovement = await dbContext.StockMovements
                .SingleOrDefaultAsync(x => x.DispatchNoteLineId == line.Id, cancellationToken);
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
                Description = $"İrsaliye iptali - {reason}",
                ProductId = originalMovement.ProductId,
                ProductVariantId = originalMovement.ProductVariantId,
                WarehouseId = originalMovement.WarehouseId,
                DispatchNoteLineId = line.Id,
                ReversalOfId = originalMovement.Id
            });

            line.Product.StockQuantity -= originalMovement.Quantity;
            line.Product.UpdatedAtUtc = DateTime.UtcNow;
        }

        dispatch.Status = BusinessDocumentStatus.Cancelled;
        dispatch.CancelledByUserId = cancelledByUserId;
        dispatch.CancelledAtUtc = DateTime.UtcNow;
        dispatch.CancellationReason = reason;
        dispatch.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
