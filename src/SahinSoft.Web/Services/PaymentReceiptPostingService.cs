using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed class PaymentReceiptPostingService(ApplicationDbContext dbContext)
{
    public async Task ApproveAsync(
        int paymentReceiptId,
        string approvedByUserId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var receipt = await dbContext.PaymentReceipts
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == paymentReceiptId, cancellationToken)
            ?? throw new InvalidOperationException("Tahsilat/tediye fişi bulunamadı.");

        if (receipt.Status != PaymentReceiptStatus.Draft || receipt.Lines.Count == 0)
        {
            throw new InvalidOperationException("Yalnızca satırı bulunan taslak fişler onaylanabilir.");
        }

        if (receipt.Lines.Any(x => x.Amount <= 0))
        {
            throw new InvalidOperationException("Fiş satır tutarları sıfırdan büyük olmalıdır.");
        }

        receipt.TotalAmount = Math.Round(
            receipt.Lines.Sum(x => x.Amount),
            2,
            MidpointRounding.AwayFromZero);

        var accountTransaction = new CurrentAccountTransaction
        {
            TransactionDateUtc = receipt.ReceiptDateUtc,
            TransactionType = receipt.ReceiptType == ReceiptType.Collection
                ? CurrentAccountTransactionType.Collection
                : CurrentAccountTransactionType.Payment,
            DocumentNumber = receipt.ReceiptNumber,
            CurrencyCode = receipt.CurrencyCode,
            ExchangeRate = receipt.ExchangeRate,
            Debit = receipt.ReceiptType == ReceiptType.Payment ? receipt.TotalAmount : 0,
            Credit = receipt.ReceiptType == ReceiptType.Collection ? receipt.TotalAmount : 0,
            CustomerId = receipt.CustomerId,
            Description = receipt.Description
        };
        dbContext.CurrentAccountTransactions.Add(accountTransaction);

        foreach (var line in receipt.Lines.OrderBy(x => x.LineNumber))
        {
            var financialTransaction = new FinancialTransaction
            {
                TransactionDateUtc = receipt.ReceiptDateUtc,
                TransactionType = receipt.ReceiptType == ReceiptType.Collection
                    ? FinancialTransactionType.Collection
                    : FinancialTransactionType.Payment,
                DocumentNumber = receipt.ReceiptNumber,
                Amount = line.Amount,
                ExchangeRate = receipt.ExchangeRate,
                Description = line.Description ?? receipt.Description,
                FinancialAccountId = line.FinancialAccountId,
                CustomerId = receipt.CustomerId,
                CurrentAccountTransaction = accountTransaction
            };
            line.CurrentAccountTransaction = accountTransaction;
            line.FinancialTransaction = financialTransaction;
        }

        receipt.Status = PaymentReceiptStatus.Approved;
        receipt.ApprovedByUserId = approvedByUserId;
        receipt.ApprovedAtUtc = DateTime.UtcNow;
        receipt.UpdatedAtUtc = DateTime.UtcNow;
        dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
        {
            EventType = receipt.ReceiptType == ReceiptType.Collection
                ? "CollectionReceiptApproved"
                : "PaymentReceiptApproved",
            PayloadJson = JsonSerializer.Serialize(new
            {
                receipt.RecordId,
                receipt.ReceiptNumber,
                receipt.ReceiptType,
                receipt.CustomerId,
                receipt.TotalAmount
            })
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task CancelAsync(
        int paymentReceiptId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var receipt = await dbContext.PaymentReceipts
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == paymentReceiptId, cancellationToken)
            ?? throw new InvalidOperationException("Tahsilat/tediye fişi bulunamadı.");

        if (receipt.Status != PaymentReceiptStatus.Approved)
        {
            throw new InvalidOperationException("Yalnızca onaylanmış fişler iptal edilebilir.");
        }

        var reversalDocumentNumber = $"IPTAL-{receipt.ReceiptNumber}";
        var reversedTransactionType = receipt.ReceiptType == ReceiptType.Collection
            ? ReceiptType.Payment
            : ReceiptType.Collection;

        CurrentAccountTransaction? reversalAccountTransaction = null;

        foreach (var line in receipt.Lines)
        {
            if (line.FinancialTransactionId is null)
            {
                continue;
            }

            var originalFinancialTransaction = await dbContext.FinancialTransactions
                .SingleOrDefaultAsync(x => x.Id == line.FinancialTransactionId, cancellationToken);

            if (originalFinancialTransaction is null)
            {
                continue;
            }

            if (reversalAccountTransaction is null && line.CurrentAccountTransactionId is not null)
            {
                var originalAccountTransaction = await dbContext.CurrentAccountTransactions
                    .SingleOrDefaultAsync(x => x.Id == line.CurrentAccountTransactionId, cancellationToken);

                if (originalAccountTransaction is not null)
                {
                    reversalAccountTransaction = new CurrentAccountTransaction
                    {
                        TransactionDateUtc = DateTime.UtcNow,
                        TransactionType = reversedTransactionType == ReceiptType.Collection
                            ? CurrentAccountTransactionType.Collection
                            : CurrentAccountTransactionType.Payment,
                        DocumentNumber = reversalDocumentNumber,
                        CurrencyCode = originalAccountTransaction.CurrencyCode,
                        ExchangeRate = originalAccountTransaction.ExchangeRate,
                        Debit = originalAccountTransaction.Credit,
                        Credit = originalAccountTransaction.Debit,
                        CustomerId = originalAccountTransaction.CustomerId,
                        Description = $"Tahsilat/tediye iptali - {reason}",
                        ReversalOfId = originalAccountTransaction.Id
                    };
                    dbContext.CurrentAccountTransactions.Add(reversalAccountTransaction);
                }
            }

            dbContext.FinancialTransactions.Add(new FinancialTransaction
            {
                TransactionDateUtc = DateTime.UtcNow,
                TransactionType = reversedTransactionType == ReceiptType.Collection
                    ? FinancialTransactionType.Collection
                    : FinancialTransactionType.Payment,
                DocumentNumber = reversalDocumentNumber,
                Amount = originalFinancialTransaction.Amount,
                ExchangeRate = originalFinancialTransaction.ExchangeRate,
                Description = $"Tahsilat/tediye iptali - {reason}",
                FinancialAccountId = originalFinancialTransaction.FinancialAccountId,
                CustomerId = originalFinancialTransaction.CustomerId,
                CurrentAccountTransaction = reversalAccountTransaction,
                ReversalOfId = originalFinancialTransaction.Id
            });
        }

        receipt.Status = PaymentReceiptStatus.Cancelled;
        receipt.CancelledByUserId = cancelledByUserId;
        receipt.CancelledAtUtc = DateTime.UtcNow;
        receipt.CancellationReason = reason;
        receipt.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
        {
            EventType = receipt.ReceiptType == ReceiptType.Collection
                ? "CollectionReceiptCancelled"
                : "PaymentReceiptCancelled",
            PayloadJson = JsonSerializer.Serialize(new
            {
                receipt.RecordId,
                receipt.ReceiptNumber,
                receipt.ReceiptType,
                receipt.CustomerId,
                receipt.TotalAmount,
                Reason = reason
            })
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
