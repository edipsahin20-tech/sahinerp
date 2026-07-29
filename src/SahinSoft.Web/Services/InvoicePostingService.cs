using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class InvoicePostingService(
    ApplicationDbContext dbContext,
    InventoryBalanceService inventoryBalance)
{
    public Task ApproveAsync(
        int invoiceId,
        string approvedByUserId,
        CancellationToken cancellationToken = default)
    {
        // EnableRetryOnFailure() (Program.cs) sets a retrying execution strategy; elle açılan
        // transaction'lar bununla uyumlu değil, tüm bloğun CreateExecutionStrategy() üzerinden
        // "tekrar denenebilir birim" olarak sarılması gerekiyor (aksi halde InvalidOperationException).
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await ApproveCoreAsync(invoiceId, approvedByUserId, cancellationToken));
    }

    private async Task ApproveCoreAsync(
        int invoiceId,
        string approvedByUserId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var invoice = await dbContext.Invoices
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .Include(x => x.PaymentSchedules)
            .SingleOrDefaultAsync(x => x.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Fatura bulunamadı.");

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException("Yalnızca taslak faturalar onaylanabilir.");
        }

        if (invoice.Lines.Count == 0)
        {
            throw new InvalidOperationException("Faturada en az bir satır bulunmalıdır.");
        }

        var inventorySettings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1, cancellationToken);

        foreach (var line in invoice.Lines)
        {
            ValidateLine(line);
        }

        InvoiceTotalsCalculator.Calculate(invoice);

        foreach (var line in invoice.Lines.OrderBy(x => x.LineNumber))
        {
            if (line.ProductId is null || line.Product is null || !line.Product.TrackStock)
            {
                continue;
            }

            if (inventorySettings.RequireProductVariant &&
                line.ProductVariantId is null &&
                await dbContext.ProductVariants.AnyAsync(
                    x => x.ProductId == line.ProductId && x.IsActive,
                    cancellationToken))
            {
                throw new InvalidOperationException(
                    $"{line.ProductNameSnapshot} için renk/varyant seçilmelidir.");
            }

            var signedQuantity = invoice.InvoiceType == InvoiceType.Sales
                ? -line.Quantity
                : line.Quantity;

            if (invoice.InvoiceType == InvoiceType.Sales &&
                inventorySettings.EnforceStockLevel &&
                !inventorySettings.AllowNegativeStock &&
                !inventorySettings.AllowSaleWhenOutOfStock)
            {
                var available = await inventoryBalance.GetAvailableAsync(
                    line.ProductId.Value,
                    line.ProductVariantId,
                    invoice.WarehouseId,
                    cancellationToken);

                if (available < line.Quantity)
                {
                    throw new InvalidOperationException(
                        $"{line.ProductNameSnapshot} için yeterli stok yok. Mevcut: {available:N3}");
                }
            }

            dbContext.StockMovements.Add(new StockMovement
            {
                MovementDateUtc = invoice.InvoiceDateUtc,
                MovementType = invoice.InvoiceType == InvoiceType.Sales
                    ? StockMovementType.Sale
                    : StockMovementType.Purchase,
                Quantity = signedQuantity,
                UnitCost = invoice.InvoiceType == InvoiceType.Purchase ? line.UnitPrice : 0,
                DocumentNumber = invoice.InvoiceNumber,
                ProductId = line.ProductId.Value,
                ProductVariantId = line.ProductVariantId,
                WarehouseId = invoice.WarehouseId,
                InvoiceLineId = line.Id,
                Description = invoice.Notes
            });

            line.Product.StockQuantity += signedQuantity;
            line.Product.UpdatedAtUtc = DateTime.UtcNow;
        }

        invoice.Status = InvoiceStatus.Approved;
        invoice.ApprovedByUserId = approvedByUserId;
        invoice.ApprovedAtUtc = DateTime.UtcNow;
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        var accountTransaction = new CurrentAccountTransaction
        {
            TransactionDateUtc = invoice.InvoiceDateUtc,
            TransactionType = invoice.InvoiceType == InvoiceType.Sales
                ? CurrentAccountTransactionType.Sale
                : CurrentAccountTransactionType.Purchase,
            DocumentNumber = invoice.InvoiceNumber,
            CurrencyCode = invoice.CurrencyCode,
            ExchangeRate = invoice.ExchangeRate,
            Debit = invoice.InvoiceType == InvoiceType.Sales ? invoice.GrandTotal : 0,
            Credit = invoice.InvoiceType == InvoiceType.Purchase ? invoice.GrandTotal : 0,
            DueDateUtc = invoice.DueDateUtc,
            CustomerId = invoice.CustomerId,
            InvoiceId = invoice.Id,
            Description = invoice.Notes
        };
        dbContext.CurrentAccountTransactions.Add(accountTransaction);

        if (invoice.PaymentSchedules.Count == 0)
        {
            invoice.PaymentSchedules.Add(new InvoicePaymentSchedule
            {
                InstallmentNumber = 1,
                DueDateUtc = invoice.DueDateUtc ?? invoice.InvoiceDateUtc,
                Amount = invoice.GrandTotal
            });
        }

        dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
        {
            EventType = invoice.InvoiceType == InvoiceType.Sales
                ? "SalesInvoiceApproved"
                : "PurchaseInvoiceApproved",
            PayloadJson = JsonSerializer.Serialize(new
            {
                invoice.RecordId,
                invoice.InvoiceNumber,
                invoice.InvoiceType,
                invoice.CustomerId,
                invoice.GrandTotal
            })
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task CancelAsync(
        int invoiceId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () => await CancelCoreAsync(invoiceId, cancelledByUserId, reason, cancellationToken));
    }

    private async Task CancelCoreAsync(
        int invoiceId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var invoice = await dbContext.Invoices
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .Include(x => x.PaymentSchedules)
            .Include(x => x.AccountTransactions)
            .SingleOrDefaultAsync(x => x.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Fatura bulunamadı.");

        if (invoice.Status != InvoiceStatus.Approved)
        {
            throw new InvalidOperationException("Yalnızca onaylanmış faturalar iptal edilebilir.");
        }

        if (invoice.PaymentSchedules.Any(x => x.PaidAmount > 0))
        {
            throw new InvalidOperationException("Tahsilatı/ödemesi yapılmış fatura iptal edilemez.");
        }

        var reversalDocumentNumber = $"IPTAL-{invoice.InvoiceNumber}";

        foreach (var line in invoice.Lines)
        {
            if (line.ProductId is null || line.Product is null || !line.Product.TrackStock)
            {
                continue;
            }

            var originalMovement = await dbContext.StockMovements
                .SingleOrDefaultAsync(x => x.InvoiceLineId == line.Id, cancellationToken);

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
                Description = $"Fatura iptali - {reason}",
                ProductId = originalMovement.ProductId,
                ProductVariantId = originalMovement.ProductVariantId,
                WarehouseId = originalMovement.WarehouseId,
                InvoiceLineId = line.Id,
                ReversalOfId = originalMovement.Id
            });

            line.Product.StockQuantity -= originalMovement.Quantity;
            line.Product.UpdatedAtUtc = DateTime.UtcNow;
        }

        var originalAccountTransaction = invoice.AccountTransactions.SingleOrDefault();
        if (originalAccountTransaction is not null)
        {
            dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
            {
                TransactionDateUtc = DateTime.UtcNow,
                TransactionType = invoice.InvoiceType == InvoiceType.Sales
                    ? CurrentAccountTransactionType.CreditNote
                    : CurrentAccountTransactionType.DebitNote,
                DocumentNumber = reversalDocumentNumber,
                CurrencyCode = invoice.CurrencyCode,
                ExchangeRate = invoice.ExchangeRate,
                Debit = originalAccountTransaction.Credit,
                Credit = originalAccountTransaction.Debit,
                CustomerId = invoice.CustomerId,
                InvoiceId = invoice.Id,
                Description = $"Fatura iptali - {reason}",
                ReversalOfId = originalAccountTransaction.Id
            });
        }

        dbContext.InvoicePaymentSchedules.RemoveRange(invoice.PaymentSchedules);

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.CancelledByUserId = cancelledByUserId;
        invoice.CancelledAtUtc = DateTime.UtcNow;
        invoice.CancellationReason = reason;
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
        {
            EventType = invoice.InvoiceType == InvoiceType.Sales
                ? "SalesInvoiceCancelled"
                : "PurchaseInvoiceCancelled",
            PayloadJson = JsonSerializer.Serialize(new
            {
                invoice.RecordId,
                invoice.InvoiceNumber,
                invoice.InvoiceType,
                invoice.CustomerId,
                invoice.GrandTotal,
                Reason = reason
            })
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void ValidateLine(InvoiceLine line)
    {
        if (line.Quantity <= 0)
        {
            throw new InvalidOperationException("Fatura satır miktarı sıfırdan büyük olmalıdır.");
        }

        if (line.UnitPrice < 0 || line.DiscountRate is < 0 or > 100 || line.TaxRate is < 0 or > 100)
        {
            throw new InvalidOperationException("Fatura satır fiyat, iskonto veya KDV bilgisi geçersiz.");
        }
    }
}
