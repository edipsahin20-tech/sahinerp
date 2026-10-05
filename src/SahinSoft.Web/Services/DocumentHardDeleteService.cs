using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// "Sil" düğmesi (Edip, 2026-10-05): alış/satış faturası, tahsilat/tediye, irsaliye ve siparişi, bağlı BÜTÜN
// stok / cari / kasa hareketleriyle birlikte veritabanından KALICI siler (iptal/ters kayıt üretmez).
// Kaynak belgenin (sipariş/irsaliye) karşılanan miktarı geri alınır; belgeye bağlı BAŞKA bir belge varsa
// (ör. irsaliyeden kesilmiş fatura) silme reddedilir. Her şey tek transaction'dır, hata olursa hiçbir şey silinmez.
// Silinen belge numarası sayacı, kalan en büyük numaraya göre GERİ alınır (numara yeniden kullanılır).
public sealed class DocumentHardDeleteService(ApplicationDbContext db, PeriodLockService periodLock, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
{
    public sealed record Result(bool Ok, string Message);

    public Task<Result> DeleteInvoiceAsync(int id, CancellationToken ct = default) => RunAsync(async () =>
    {
        var invoice = await db.Invoices
            .Include(x => x.Lines).ThenInclude(l => l.DispatchNoteLine!).ThenInclude(d => d.DispatchNote).ThenInclude(d => d.Lines)
            .Include(x => x.Lines).ThenInclude(l => l.BusinessOrderLine!).ThenInclude(o => o.BusinessOrder).ThenInclude(o => o.Lines)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        if (invoice is null) { return new Result(false, "Fatura bulunamadı."); }
        if (await periodLock.CheckAsync(invoice.InvoiceDateUtc, ct) is { } lockMessage) { return new Result(false, lockMessage); }
        await WriteAuditAsync("Invoice", invoice.Id, invoice.InvoiceNumber, new { invoice.InvoiceType, invoice.Status, invoice.CustomerId, invoice.BranchId, invoice.WarehouseId, invoice.InvoiceDateUtc, invoice.GrandTotal, LineCount = invoice.Lines.Count }, ct);

        if (invoice.Status == InvoiceStatus.Approved)
        {
            ReverseFulfillment(invoice);
            await db.SaveChangesAsync(ct);
        }

        var lineIds = invoice.Lines.Select(x => x.Id).ToList();
        var nums = Numbers(invoice.InvoiceNumber);
        await PurgeLedgerAsync(nums, cariExtra: $"InvoiceId = {id}", stockExtra: Ids("InvoiceLineId", lineIds), ct);
        await Exec("UPDATE PaymentReceipts SET InvoiceId = NULL WHERE InvoiceId = {0}", id);
        await Exec("UPDATE DispatchNotes SET InvoiceId = NULL WHERE InvoiceId = {0}", id);
        await Exec("UPDATE RestaurantChecks SET LinkedInvoiceId = NULL WHERE LinkedInvoiceId = {0}", id);
        await Exec("DELETE FROM InvoicePaymentSchedules WHERE InvoiceId = {0}", id);
        await Exec("DELETE FROM InvoiceLines WHERE InvoiceId = {0}", id);
        await Exec("DELETE FROM Invoices WHERE Id = {0}", id);

        var type = invoice.InvoiceType;
        await ResetSequenceAsync(type == InvoiceType.Sales ? "SALES_INVOICE" : "PURCHASE_INVOICE",
            await db.Invoices.Where(x => x.InvoiceType == type).Select(x => x.InvoiceNumber).ToListAsync(ct), ct);
        return new Result(true, $"{invoice.InvoiceNumber} faturası ve bağlı tüm hareketleri veritabanından silindi.");
    });

    public Task<Result> DeletePaymentReceiptAsync(int id, CancellationToken ct = default) => RunAsync(async () =>
    {
        var receipt = await db.PaymentReceipts.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (receipt is null) { return new Result(false, "Makbuz bulunamadı."); }
        if (await periodLock.CheckAsync(receipt.ReceiptDateUtc, ct) is { } lockMessage) { return new Result(false, lockMessage); }
        await WriteAuditAsync("PaymentReceipt", receipt.Id, receipt.ReceiptNumber, new { receipt.ReceiptType, receipt.Status, receipt.CustomerId, receipt.OriginBranchId, receipt.ReceiptDateUtc, receipt.TotalAmount }, ct);

        var lineIds = await db.PaymentReceiptLines.Where(x => x.PaymentReceiptId == id).Select(x => x.Id).ToListAsync(ct);
        var nums = Numbers(receipt.ReceiptNumber);
        // Makbuz satırlarının bağlandığı hareketler (numara farklı olsa bile) da kapsama alınır.
        var linkedCari = lineIds.Count == 0 ? null : $"Id IN (SELECT CurrentAccountTransactionId FROM PaymentReceiptLines WHERE PaymentReceiptId = {id} AND CurrentAccountTransactionId IS NOT NULL)";
        await PurgeLedgerAsync(nums, cariExtra: linkedCari, stockExtra: null, ct,
            finExtra: lineIds.Count == 0 ? null : $"Id IN (SELECT FinancialTransactionId FROM PaymentReceiptLines WHERE PaymentReceiptId = {id} AND FinancialTransactionId IS NOT NULL)");
        await Exec("DELETE FROM PaymentReceiptLines WHERE PaymentReceiptId = {0}", id);
        await Exec("DELETE FROM PaymentReceipts WHERE Id = {0}", id);

        var type = receipt.ReceiptType;
        await ResetSequenceAsync(type == ReceiptType.Collection ? "COLLECTION_RECEIPT" : "PAYMENT_RECEIPT",
            await db.PaymentReceipts.Where(x => x.ReceiptType == type).Select(x => x.ReceiptNumber).ToListAsync(ct), ct);
        return new Result(true, $"{receipt.ReceiptNumber} makbuzu ve bağlı tüm hareketleri veritabanından silindi.");
    });

    public Task<Result> DeleteDispatchNoteAsync(int id, CancellationToken ct = default) => RunAsync(async () =>
    {
        var dispatch = await db.DispatchNotes
            .Include(x => x.Lines).ThenInclude(l => l.BusinessOrderLine!).ThenInclude(o => o.BusinessOrder).ThenInclude(o => o.Lines)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        if (dispatch is null) { return new Result(false, "İrsaliye bulunamadı."); }
        if (await periodLock.CheckAsync(dispatch.DispatchDateUtc, ct) is { } lockMessage) { return new Result(false, lockMessage); }
        await WriteAuditAsync("DispatchNote", dispatch.Id, dispatch.DispatchNumber, new { dispatch.DispatchType, dispatch.Status, dispatch.CustomerId, dispatch.BranchId, dispatch.WarehouseId, dispatch.DispatchDateUtc, LineCount = dispatch.Lines.Count }, ct);

        var lineIds = dispatch.Lines.Select(x => x.Id).ToList();
        var invoiced = dispatch.InvoiceId is not null
            || (lineIds.Count > 0 && await db.InvoiceLines.AnyAsync(x => x.DispatchNoteLineId != null && lineIds.Contains(x.DispatchNoteLineId.Value), ct));
        if (invoiced) { return new Result(false, "Bu irsaliyeden fatura kesilmiş. Önce faturayı silin."); }

        if (dispatch.Status != BusinessDocumentStatus.Draft && dispatch.Status != BusinessDocumentStatus.Cancelled)
        {
            var orders = new HashSet<BusinessOrder>();
            foreach (var line in dispatch.Lines.Where(x => x.BusinessOrderLine is not null))
            {
                line.BusinessOrderLine!.FulfilledQuantity = Math.Max(0, line.BusinessOrderLine.FulfilledQuantity - line.Quantity);
                orders.Add(line.BusinessOrderLine.BusinessOrder);
            }
            foreach (var order in orders)
            {
                order.Status = FulfillmentStatusCalculator.Calculate(order.Lines.Select(x => (x.Quantity, x.FulfilledQuantity)));
                order.UpdatedAtUtc = DateTime.UtcNow;
            }
            await db.SaveChangesAsync(ct);
        }

        var nums = Numbers(dispatch.DispatchNumber);
        await PurgeLedgerAsync(nums, cariExtra: null, stockExtra: Ids("DispatchNoteLineId", lineIds), ct);
        await Exec("DELETE FROM DispatchNoteLines WHERE DispatchNoteId = {0}", id);
        await Exec("DELETE FROM DispatchNotes WHERE Id = {0}", id);

        var type = dispatch.DispatchType;
        await ResetSequenceAsync(type == InvoiceType.Sales ? "SALES_DISPATCH" : "PURCHASE_DISPATCH",
            await db.DispatchNotes.Where(x => x.DispatchType == type).Select(x => x.DispatchNumber).ToListAsync(ct), ct);
        return new Result(true, $"{dispatch.DispatchNumber} irsaliyesi ve bağlı tüm hareketleri veritabanından silindi.");
    });

    public Task<Result> DeleteBusinessOrderAsync(int id, CancellationToken ct = default) => RunAsync(async () =>
    {
        var order = await db.BusinessOrders.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (order is null) { return new Result(false, "Sipariş bulunamadı."); }
        if (await periodLock.CheckAsync(order.OrderDateUtc, ct) is { } lockMessage) { return new Result(false, lockMessage); }
        await WriteAuditAsync("BusinessOrder", order.Id, order.OrderNumber, new { order.OrderType, order.Status, order.CustomerId, order.BranchId, order.OrderDateUtc, order.GrandTotal, LineCount = order.Lines.Count }, ct);

        var lineIds = order.Lines.Select(x => x.Id).ToList();
        if (lineIds.Count > 0)
        {
            var hasChild = await db.DispatchNoteLines.AnyAsync(x => x.BusinessOrderLineId != null && lineIds.Contains(x.BusinessOrderLineId.Value), ct)
                || await db.InvoiceLines.AnyAsync(x => x.BusinessOrderLineId != null && lineIds.Contains(x.BusinessOrderLineId.Value), ct);
            if (hasChild) { return new Result(false, "Bu siparişten irsaliye/fatura kesilmiş. Önce bağlı belgeleri silin."); }
        }

        await Exec("DELETE FROM BusinessOrderLines WHERE BusinessOrderId = {0}", id);
        await Exec("DELETE FROM BusinessOrders WHERE Id = {0}", id);

        var type = order.OrderType;
        await ResetSequenceAsync(type == InvoiceType.Sales ? "SALES_ORDER" : "PURCHASE_ORDER",
            await db.BusinessOrders.Where(x => x.OrderType == type).Select(x => x.OrderNumber).ToListAsync(ct), ct);
        return new Result(true, $"{order.OrderNumber} siparişi veritabanından silindi.");
    });

    private static void ReverseFulfillment(Invoice invoice)
    {
        var orders = new HashSet<BusinessOrder>();
        var dispatches = new HashSet<DispatchNote>();
        foreach (var line in invoice.Lines)
        {
            if (line.DispatchNoteLine is not null)
            {
                line.DispatchNoteLine.InvoicedQuantity = Math.Max(0, line.DispatchNoteLine.InvoicedQuantity - line.Quantity);
                dispatches.Add(line.DispatchNoteLine.DispatchNote);
            }
            else if (line.BusinessOrderLine is not null)
            {
                line.BusinessOrderLine.FulfilledQuantity = Math.Max(0, line.BusinessOrderLine.FulfilledQuantity - line.Quantity);
                orders.Add(line.BusinessOrderLine.BusinessOrder);
            }
        }
        foreach (var o in orders)
        {
            o.Status = FulfillmentStatusCalculator.Calculate(o.Lines.Select(x => (x.Quantity, x.FulfilledQuantity)));
            o.UpdatedAtUtc = DateTime.UtcNow;
        }
        foreach (var d in dispatches)
        {
            d.Status = FulfillmentStatusCalculator.Calculate(d.Lines.Select(x => (x.Quantity, x.InvoicedQuantity)));
            d.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    // Kalıcı silme denetim günlüğü: kim, neyi, ne zaman sildi (belge özeti JSON olarak saklanır). Silme geri alınırsa (hata) bu kayıt da geri alınır.
    private async Task WriteAuditAsync(string entityName, int id, string number, object summary, CancellationToken ct)
    {
        var http = httpContextAccessor.HttpContext;
        db.DocumentLogs.Add(new DocumentLog
        {
            EntityName = entityName, EntityId = id, DocumentNumber = number, Action = "Deleted",
            UserId = http?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            UserName = http?.User.Identity?.Name ?? "Sistem",
            Details = "Evrak ve bağlı tüm hareketleri kalıcı silindi",
            IpAddress = http?.Connection.RemoteIpAddress?.ToString()
        });
        db.AuditLogs.Add(new AuditLog
        {
            UserId = http?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            Action = "HardDelete",
            EntityName = entityName,
            EntityId = id.ToString(),
            OldValuesJson = JsonSerializer.Serialize(new { Number = number, Summary = summary }),
            IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = http?.Request.Headers.UserAgent.ToString()
        });
        await db.SaveChangesAsync(ct);
    }

    private static string[] Numbers(string number) => [number, "DUZ-" + number, "IPTAL-" + number];

    private static string? Ids(string column, List<int> ids) =>
        ids.Count == 0 ? null : $"{column} IN ({string.Join(",", ids)})";

    private async Task<Result> RunAsync(Func<Task<Result>> work)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync();
            try
            {
                var result = await work();
                if (!result.Ok) { await tx.RollbackAsync(); return result; }
                await tx.CommitAsync();
                return result;
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                return new Result(false, "Silinemedi: bu belgeye bağlı başka kayıtlar var. " + (ex.InnerException?.Message ?? ex.Message));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                await tx.RollbackAsync();
                return new Result(false, "Silinemedi: bu belgeye bağlı başka kayıtlar var. " + ex.Message);
            }
        });
    }

    private Task<int> Exec(string sql, params object[] args) => db.Database.ExecuteSqlRawAsync(sql, args);

    // Belgeye ait stok/cari/kasa hareketlerini (numara ve bağlantı ile bulunanlar, düzeltme/iptal/ters kayıtlar dahil) siler.
    private async Task PurgeLedgerAsync(string[] nums, string? cariExtra, string? stockExtra, CancellationToken ct, string? finExtra = null)
    {
        var inList = string.Join(",", nums.Select(n => "N'" + n.Replace("'", "''") + "'"));
        await Exec($@"
IF OBJECT_ID('tempdb..#cari') IS NOT NULL DROP TABLE #cari;
IF OBJECT_ID('tempdb..#fin') IS NOT NULL DROP TABLE #fin;
IF OBJECT_ID('tempdb..#stk') IS NOT NULL DROP TABLE #stk;
IF OBJECT_ID('tempdb..#prod') IS NOT NULL DROP TABLE #prod;
SELECT Id INTO #cari FROM CurrentAccountTransactions WHERE DocumentNumber IN ({inList}){(cariExtra is null ? "" : " OR " + cariExtra)};
SELECT Id INTO #fin FROM FinancialTransactions WHERE DocumentNumber IN ({inList}) OR CurrentAccountTransactionId IN (SELECT Id FROM #cari){(finExtra is null ? "" : " OR " + finExtra)};
SELECT Id INTO #stk FROM StockMovements WHERE DocumentNumber IN ({inList}){(stockExtra is null ? "" : " OR " + stockExtra)};
SELECT DISTINCT ProductId INTO #prod FROM StockMovements WHERE Id IN (SELECT Id FROM #stk);
DECLARE @sql nvarchar(max);
SET @sql = N'';
SELECT @sql += N'UPDATE [' + OBJECT_NAME(fkc.parent_object_id) + N'] SET [' + COL_NAME(fkc.parent_object_id, fkc.parent_column_id) + N'] = NULL WHERE [' + COL_NAME(fkc.parent_object_id, fkc.parent_column_id) + N'] IN (SELECT Id FROM #cari);'
FROM sys.foreign_key_columns fkc JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE fkc.referenced_object_id = OBJECT_ID('CurrentAccountTransactions') AND c.is_nullable = 1;
SELECT @sql += N'UPDATE [' + OBJECT_NAME(fkc.parent_object_id) + N'] SET [' + COL_NAME(fkc.parent_object_id, fkc.parent_column_id) + N'] = NULL WHERE [' + COL_NAME(fkc.parent_object_id, fkc.parent_column_id) + N'] IN (SELECT Id FROM #fin);'
FROM sys.foreign_key_columns fkc JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE fkc.referenced_object_id = OBJECT_ID('FinancialTransactions') AND c.is_nullable = 1;
SELECT @sql += N'UPDATE [' + OBJECT_NAME(fkc.parent_object_id) + N'] SET [' + COL_NAME(fkc.parent_object_id, fkc.parent_column_id) + N'] = NULL WHERE [' + COL_NAME(fkc.parent_object_id, fkc.parent_column_id) + N'] IN (SELECT Id FROM #stk);'
FROM sys.foreign_key_columns fkc JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE fkc.referenced_object_id = OBJECT_ID('StockMovements') AND c.is_nullable = 1;
EXEC sp_executesql @sql;
DELETE FROM FinancialTransactions WHERE Id IN (SELECT Id FROM #fin);
DELETE FROM CurrentAccountTransactions WHERE Id IN (SELECT Id FROM #cari);
DELETE FROM StockMovements WHERE Id IN (SELECT Id FROM #stk);
UPDATE p SET StockQuantity = ISNULL((SELECT SUM(m.Quantity) FROM StockMovements m WHERE m.ProductId = p.Id), 0) FROM Products p WHERE p.Id IN (SELECT ProductId FROM #prod);
DROP TABLE #cari; DROP TABLE #fin; DROP TABLE #stk; DROP TABLE #prod;");
    }

    // Numara sayacını kalan en büyük numaranın bir üstüne (gerekirse GERİ) çeker: silinen numara yeniden kullanılır.
    private async Task ResetSequenceAsync(string key, List<string> remainingNumbers, CancellationToken ct)
    {
        var seq = await db.NumberSequences.SingleOrDefaultAsync(x => x.Key == key, ct);
        if (seq is null) { return; }
        long max = 0;
        foreach (var number in remainingNumbers)
        {
            if (string.IsNullOrEmpty(seq.Prefix) || !number.StartsWith(seq.Prefix, StringComparison.Ordinal)) { continue; }
            var suffix = number[seq.Prefix.Length..];
            if (suffix.Length > 0 && suffix.All(char.IsDigit) && long.TryParse(suffix, out var n) && n > max) { max = n; }
        }
        seq.NextNumber = max + 1;
        await db.SaveChangesAsync(ct);
    }
}
