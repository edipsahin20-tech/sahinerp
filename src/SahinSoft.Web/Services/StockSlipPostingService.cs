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
    public async Task ApproveAsync(
        int stockSlipId,
        string approvedByUserId,
        CancellationToken cancellationToken = default)
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
}
