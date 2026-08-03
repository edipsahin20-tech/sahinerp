using System.Data;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// Onaylı bir faturaya bağlı, hâlâ aktif (Onaylı) tahsilat/tediye fişi varsa normal Fatura İptali
// tek başına çalışmaz (bkz. InvoicePostingService.CancelWithinTransactionAsync — PaidAmount > 0
// koruması). Bu servis, kullanıcının açık onayıyla önce bağlı fişleri, sonra faturayı iptal eder.
// Status hiçbir yerde elle set edilmez; yalnızca PaymentReceiptPostingService ve
// InvoicePostingService'in Cancel mantığı (CancelWithinTransactionAsync) çağrılır.
//
// Atomiklik: tüm adımlar TEK bir Serializable transaction'da çalışır (aynı scoped ApplicationDbContext
// üzerinden, aşağıdaki iki servisle paylaşılır). Herhangi bir adım hata verirse transaction hiç commit
// edilmeden dispose olur — o ana kadar yapılmış hiçbir cari/kasa/durum değişikliği kalıcı olmaz, fatura
// ve bağlı fiş(ler) tamamen başlangıçtaki (Onaylı) haliyle kalır. Yarım/bozuk bir sonuç artık mümkün
// değil: ya HEPSİ iptal olur ya da HİÇBİRİ.
public sealed class InvoiceCancellationOrchestrationService(
    ApplicationDbContext dbContext,
    InvoicePostingService invoicePostingService,
    PaymentReceiptPostingService paymentReceiptPostingService)
{
    // Onay ekranında göstermek üzere: faturaya AÇIKÇA (InvoiceId FK) bağlı, hâlâ aktif fişler.
    // Sezgisel/miktar eşleştirmesi yapılmaz — yalnızca doğrudan bağlantı esas alınır.
    public Task<List<PaymentReceipt>> GetActiveLinkedReceiptsAsync(int invoiceId, CancellationToken cancellationToken = default) =>
        dbContext.PaymentReceipts
            .AsNoTracking()
            .Include(x => x.Lines)
            .ThenInclude(x => x.FinancialAccount)
            .Where(x => x.InvoiceId == invoiceId && x.Status == PaymentReceiptStatus.Approved)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task CancelInvoiceWithLinkedPaymentsAsync(
        int invoiceId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        // Diğer Approve/Cancel akışlarıyla aynı desen: EnableRetryOnFailure() ile uyumlu olmak için
        // tüm blok CreateExecutionStrategy() üzerinden tekrar denenebilir birim olarak sarılır; bir
        // RowVersion çakışması olursa strategy TÜM bloğu (taze okuma + yeni transaction ile) baştan
        // çalıştırır — commit'ten önce hiçbir şey kalıcı olmadığından bu güvenlidir.
        return DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var activeReceiptIds = await dbContext.PaymentReceipts
                    .Where(x => x.InvoiceId == invoiceId && x.Status == PaymentReceiptStatus.Approved)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                if (activeReceiptIds.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Faturaya bağlı aktif bir tahsilat/tediye bulunamadı; normal İptal Et akışını kullanın.");
                }

                foreach (var receiptId in activeReceiptIds)
                {
                    await paymentReceiptPostingService.CancelWithinTransactionAsync(receiptId, cancelledByUserId, reason, cancellationToken);
                }

                await invoicePostingService.CancelWithinTransactionAsync(invoiceId, cancelledByUserId, reason, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return true;
            });
        }, cancellationToken);
    }
}
