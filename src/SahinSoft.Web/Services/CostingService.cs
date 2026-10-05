using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// Ağırlıklı ortalama maliyet: giriş hareketlerinin (açılış, alış, düzeltme girişi, sayım fazlası) miktar × birim maliyet toplamı / toplam miktar.
// Ters kayıtlar (aynı maliyetle eksi miktar) ortalamayı kendiliğinden düzeltir. Giriş yoksa son alış maliyeti, o da yoksa stok kartı alış fiyatının KDV hariç hali.
public sealed class CostingService(ApplicationDbContext db)
{
    private static readonly StockMovementType[] InboundCosted =
        [StockMovementType.Opening, StockMovementType.Purchase, StockMovementType.AdjustmentIn, StockMovementType.InventoryCountSurplus];

    public async Task<decimal> GetAverageCostAsync(int productId, CancellationToken ct = default)
    {
        var agg = await db.StockMovements.AsNoTracking()
            .Where(x => x.ProductId == productId && x.UnitCost > 0 && InboundCosted.Contains(x.MovementType))
            .GroupBy(x => 1)
            .Select(g => new { Qty = g.Sum(x => x.Quantity), Value = g.Sum(x => x.Quantity * x.UnitCost) })
            .SingleOrDefaultAsync(ct);
        if (agg is not null && agg.Qty > 0) { return Math.Round(agg.Value / agg.Qty, 4, MidpointRounding.AwayFromZero); }
        return await FallbackCostAsync(productId, ct);
    }

    public async Task<decimal> FallbackCostAsync(int productId, CancellationToken ct = default)
    {
        var last = await db.StockMovements.AsNoTracking()
            .Where(x => x.ProductId == productId && x.UnitCost > 0 && x.Quantity > 0 && InboundCosted.Contains(x.MovementType))
            .OrderByDescending(x => x.MovementDateUtc).ThenByDescending(x => x.Id)
            .Select(x => (decimal?)x.UnitCost).FirstOrDefaultAsync(ct);
        if (last is not null) { return last.Value; }
        var p = await db.Products.AsNoTracking().Where(x => x.Id == productId)
            .Select(x => new { x.PurchasePrice, Rate = x.TaxRate != null ? x.TaxRate.Rate : 0m }).SingleOrDefaultAsync(ct);
        if (p is null) { return 0m; }
        return Math.Round(p.PurchasePrice / (1 + p.Rate / 100), 4, MidpointRounding.AwayFromZero);
    }

    // Rapor için tüm ürünlerin ortalama maliyeti tek sorguda.
    public async Task<Dictionary<int, decimal>> GetAverageCostsAsync(CancellationToken ct = default)
    {
        var rows = await db.StockMovements.AsNoTracking()
            .Where(x => x.UnitCost > 0 && InboundCosted.Contains(x.MovementType))
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity), Value = g.Sum(x => x.Quantity * x.UnitCost) })
            .ToListAsync(ct);
        var result = rows.Where(x => x.Qty > 0).ToDictionary(x => x.ProductId, x => Math.Round(x.Value / x.Qty, 4, MidpointRounding.AwayFromZero));
        var missing = await db.Products.AsNoTracking().Select(x => new { x.Id, x.PurchasePrice, Rate = x.TaxRate != null ? x.TaxRate.Rate : 0m }).ToListAsync(ct);
        foreach (var p in missing.Where(p => !result.ContainsKey(p.Id)))
        {
            result[p.Id] = Math.Round(p.PurchasePrice / (1 + p.Rate / 100), 4, MidpointRounding.AwayFromZero);
        }
        return result;
    }
}
