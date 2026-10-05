using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

// Muhasebe raporları: stok değerleme (ağırlıklı ortalama maliyet), brüt kâr, cari yaşlandırma, satış analizi.
[Authorize]
public sealed class AccountingReportsController(ApplicationDbContext db, CostingService costing, BranchSelectionService branches) : Controller
{
    public async Task<IActionResult> StockValuation(int? warehouseId, int? categoryId, bool onlyInStock = true)
    {
        var qtyQuery = db.StockMovements.AsNoTracking().AsQueryable();
        if (warehouseId is int w) { qtyQuery = qtyQuery.Where(x => x.WarehouseId == w); }
        var quantities = await qtyQuery.GroupBy(x => x.ProductId).Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) }).ToDictionaryAsync(x => x.ProductId, x => x.Qty);

        var products = db.Products.AsNoTracking().Where(x => x.TrackStock);
        if (categoryId is int c) { products = products.Where(x => x.CategoryId == c); }
        var list = await products.OrderBy(x => x.StockCode)
            .Select(x => new { x.Id, x.StockCode, x.Name, Category = x.Category.Name, x.SalePrice, Rate = x.TaxRate != null ? x.TaxRate.Rate : 0m })
            .ToListAsync();
        var avg = await costing.GetAverageCostsAsync();

        var rows = new List<StockValuationRow>();
        foreach (var p in list)
        {
            var qty = quantities.GetValueOrDefault(p.Id);
            if (onlyInStock && qty == 0) { continue; }
            var cost = avg.GetValueOrDefault(p.Id);
            var saleNet = Math.Round(p.SalePrice / (1 + p.Rate / 100), 4, MidpointRounding.AwayFromZero);
            rows.Add(new StockValuationRow(p.StockCode, p.Name, p.Category, qty, cost, Math.Round(qty * cost, 2, MidpointRounding.AwayFromZero), saleNet, Math.Round(qty * saleNet, 2, MidpointRounding.AwayFromZero)));
        }

        return View(new StockValuationViewModel
        {
            Rows = rows, WarehouseId = warehouseId, CategoryId = categoryId, OnlyInStock = onlyInStock,
            Warehouses = await db.Warehouses.AsNoTracking().OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync(),
            Categories = await db.ProductCategories.AsNoTracking().OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync()
        });
    }

    public async Task<IActionResult> GrossProfit(DateTime? from, DateTime? to, int? branchId, bool includeRestaurant = true)
    {
        var f = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var t = (to ?? DateTime.Today).Date;
        var avg = await costing.GetAverageCostsAsync();

        var lines = db.InvoiceLines.AsNoTracking()
            .Where(x => x.ProductId != null && x.Invoice.InvoiceType == InvoiceType.Sales && x.Invoice.Status == InvoiceStatus.Approved
                && x.Invoice.InvoiceDateUtc >= f && x.Invoice.InvoiceDateUtc < t.AddDays(1));
        if (branchId is int b) { lines = lines.Where(x => x.Invoice.BranchId == b); }
        var invLines = await lines.Select(x => new { x.Id, ProductId = x.ProductId!.Value, x.Quantity, Net = x.LineTotal - x.TaxAmount }).ToListAsync();
        var lineIds = invLines.Select(x => x.Id).ToList();
        var movementCost = (await db.StockMovements.AsNoTracking()
                .Where(x => x.InvoiceLineId != null && lineIds.Contains(x.InvoiceLineId.Value) && x.ReversalOfId == null && x.UnitCost > 0)
                .Select(x => new { LineId = x.InvoiceLineId!.Value, x.UnitCost }).ToListAsync())
            .GroupBy(x => x.LineId).ToDictionary(g => g.Key, g => g.First().UnitCost);

        var acc = new Dictionary<int, (decimal Qty, decimal Net, decimal Cost)>();
        foreach (var l in invLines)
        {
            var unit = movementCost.TryGetValue(l.Id, out var mc) ? mc : avg.GetValueOrDefault(l.ProductId);
            var cur = acc.GetValueOrDefault(l.ProductId);
            acc[l.ProductId] = (cur.Qty + l.Quantity, cur.Net + l.Net, cur.Cost + Math.Round(l.Quantity * unit, 2, MidpointRounding.AwayFromZero));
        }

        if (includeRestaurant)
        {
            var retail = db.RetailSaleLines.AsNoTracking()
                .Where(x => x.RetailSale.Status == RetailSaleStatus.Issued && !x.IsComplimentary && x.RetailSale.IssuedAtUtc >= f && x.RetailSale.IssuedAtUtc < t.AddDays(1));
            if (branchId is int rb) { retail = retail.Where(x => x.RetailSale.BranchId == rb); }
            foreach (var r in await retail.Select(x => new { x.ProductId, x.Quantity, x.LineTotal, x.TaxRateSnapshot }).ToListAsync())
            {
                var net = Math.Round(r.LineTotal / (1 + r.TaxRateSnapshot / 100), 2, MidpointRounding.AwayFromZero);
                var cur = acc.GetValueOrDefault(r.ProductId);
                acc[r.ProductId] = (cur.Qty + r.Quantity, cur.Net + net, cur.Cost + Math.Round(r.Quantity * avg.GetValueOrDefault(r.ProductId), 2, MidpointRounding.AwayFromZero));
            }
        }

        var ids = acc.Keys.ToList();
        var names = await db.Products.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.StockCode, x.Name }).ToDictionaryAsync(x => x.Id);
        var rows = acc.Select(kv => new GrossProfitRow(names[kv.Key].StockCode, names[kv.Key].Name, kv.Value.Qty, kv.Value.Net, kv.Value.Cost))
            .OrderByDescending(x => x.Profit).ToList();

        return View(new GrossProfitViewModel
        {
            Rows = rows, From = f, To = t, BranchId = branchId, IncludeRestaurant = includeRestaurant,
            Branches = await branches.OptionsAsync(branchId)
        });
    }

    public async Task<IActionResult> CustomerAging(DateTime? asOf)
    {
        var date = (asOf ?? DateTime.Today).Date;
        var customers = await db.Customers.AsNoTracking().Select(x => new { x.Id, x.Code, x.Name }).ToDictionaryAsync(x => x.Id);
        var transactions = await db.CurrentAccountTransactions.AsNoTracking()
            .OrderBy(x => x.TransactionDateUtc).ThenBy(x => x.Id)
            .Select(x => new { x.CustomerId, x.TransactionDateUtc, x.DueDateUtc, x.Debit, x.Credit })
            .ToListAsync();

        var receivables = new List<AgingRow>();
        var payables = new List<AgingRow>();
        foreach (var group in transactions.GroupBy(x => x.CustomerId))
        {
            var debits = new List<(DateTime Due, decimal Amount)>();
            var credits = new List<(DateTime Due, decimal Amount)>();
            foreach (var tr in group)
            {
                var due = (tr.DueDateUtc ?? tr.TransactionDateUtc).Date;
                if (tr.Debit > 0) { var rest = Consume(credits, tr.Debit); if (rest > 0) { debits.Add((due, rest)); } }
                if (tr.Credit > 0) { var rest = Consume(debits, tr.Credit); if (rest > 0) { credits.Add((due, rest)); } }
            }
            var c = customers[group.Key];
            if (debits.Sum(x => x.Amount) > 0) { receivables.Add(Bucket(c.Code, c.Name, debits, date)); }
            if (credits.Sum(x => x.Amount) > 0) { payables.Add(Bucket(c.Code, c.Name, credits, date)); }
        }

        return View(new CustomerAgingViewModel
        {
            AsOf = date,
            Receivables = receivables.OrderByDescending(x => x.D90Plus).ThenByDescending(x => x.Total).ToList(),
            Payables = payables.OrderByDescending(x => x.D90Plus).ThenByDescending(x => x.Total).ToList()
        });
    }

    // FIFO: karşı taraf hareketi en eski açık kalemlerden düşer; artanı döner.
    private static decimal Consume(List<(DateTime Due, decimal Amount)> open, decimal amount)
    {
        while (amount > 0 && open.Count > 0)
        {
            var first = open[0];
            if (first.Amount <= amount) { amount -= first.Amount; open.RemoveAt(0); }
            else { open[0] = (first.Due, first.Amount - amount); amount = 0; }
        }
        return amount;
    }

    private static AgingRow Bucket(string code, string name, List<(DateTime Due, decimal Amount)> items, DateTime asOf)
    {
        var row = new AgingRow { Code = code, Name = name };
        foreach (var (due, amount) in items)
        {
            var days = (asOf - due).Days;
            if (days <= 0) { row.NotDue += amount; }
            else if (days <= 30) { row.D1To30 += amount; }
            else if (days <= 60) { row.D31To60 += amount; }
            else if (days <= 90) { row.D61To90 += amount; }
            else { row.D90Plus += amount; }
        }
        return row;
    }

    public async Task<IActionResult> SalesAnalysis(DateTime? from, DateTime? to, string by = "customer", int? branchId = null)
    {
        var f = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var t = (to ?? DateTime.Today).Date;
        var lines = db.InvoiceLines.AsNoTracking()
            .Where(x => x.Invoice.InvoiceType == InvoiceType.Sales && x.Invoice.Status == InvoiceStatus.Approved
                && x.Invoice.InvoiceDateUtc >= f && x.Invoice.InvoiceDateUtc < t.AddDays(1));
        if (branchId is int b) { lines = lines.Where(x => x.Invoice.BranchId == b); }

        List<SalesAnalysisRow> rows;
        if (by == "product")
        {
            var data = await lines.Where(x => x.ProductId != null)
                .GroupBy(x => x.ProductId!.Value)
                .Select(g => new { ProductId = g.Key, Docs = g.Select(x => x.InvoiceId).Distinct().Count(), Qty = g.Sum(x => x.Quantity), Net = g.Sum(x => x.LineTotal - x.TaxAmount), Tax = g.Sum(x => x.TaxAmount) })
                .ToListAsync();
            var ids = data.Select(x => x.ProductId).ToList();
            var names = await db.Products.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.StockCode, x.Name }).ToDictionaryAsync(x => x.Id);
            rows = data.Select(x => new SalesAnalysisRow(names[x.ProductId].StockCode, names[x.ProductId].Name, x.Docs, x.Qty, x.Net, x.Tax)).ToList();
        }
        else
        {
            by = "customer";
            var data = await lines.GroupBy(x => x.Invoice.CustomerId)
                .Select(g => new { CustomerId = g.Key, Docs = g.Select(x => x.InvoiceId).Distinct().Count(), Qty = g.Sum(x => x.Quantity), Net = g.Sum(x => x.LineTotal - x.TaxAmount), Tax = g.Sum(x => x.TaxAmount) })
                .ToListAsync();
            var ids = data.Select(x => x.CustomerId).ToList();
            var names = await db.Customers.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Code, x.Name }).ToDictionaryAsync(x => x.Id);
            rows = data.Select(x => new SalesAnalysisRow(names[x.CustomerId].Code, names[x.CustomerId].Name, x.Docs, x.Qty, x.Net, x.Tax)).ToList();
        }

        return View(new SalesAnalysisViewModel
        {
            Rows = rows.OrderByDescending(x => x.NetSales).ToList(), From = f, To = t, By = by, BranchId = branchId,
            Branches = await branches.OptionsAsync(branchId)
        });
    }
}
