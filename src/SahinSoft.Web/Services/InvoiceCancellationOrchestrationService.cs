using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// Onaylı bir faturaya bağlı, hâlâ aktif (Onaylı) tahsilat/tediye fişi varsa normal Fatura İptali
// tek başına çalışmaz (bkz. InvoicePostingService.CancelCoreAsync — PaidAmount > 0 koruması). Bu
// servis, kullanıcının açık onayıyla önce bağlı fişleri, sonra faturayı — HER İKİSİNİ DE kendi
// mevcut, zaten test edilmiş Cancel akışları üzerinden — sırayla iptal eder. Status hiçbir yerde
// elle set edilmez; yalnızca PaymentReceiptPostingService.CancelAsync ve InvoicePostingService.
// CancelAsync çağrılır.
//
// Atomiklik notu: her adımın kendi Serializable transaction'ı + eşzamanlılık tekrar denemesi var
// (mevcut, kanıtlanmış davranış). Tüm adımları TEK bir veritabanı transaction'ında birleştirmek,
// zaten bağımsız çalışan iki posting servisinin transaction yönetimini yeniden yazmayı gerektirir
// — bu riskli bir refactor olur. Bunun yerine: her adım kendi başına ATOMİK ve GEÇERLİDİR (bir fişin
// iptali her zaman kendi başına doğru bir işlemdir); sıradaki bir adım başarısız olursa, o ana kadar
// başarıyla iptal edilmiş belgeler geçerli/doğru kalır (yarım/bozuk bir belge oluşmaz), yalnızca
// kalan adımlar tamamlanmamış olur ve kullanıcıya hangi adımda durduğu net şekilde bildirilir.
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

    public async Task CancelInvoiceWithLinkedPaymentsAsync(
        int invoiceId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        var activeReceipts = await GetActiveLinkedReceiptsAsync(invoiceId, cancellationToken);
        if (activeReceipts.Count == 0)
        {
            throw new InvalidOperationException(
                "Faturaya bağlı aktif bir tahsilat/tediye bulunamadı; normal İptal Et akışını kullanın.");
        }

        foreach (var receipt in activeReceipts)
        {
            await paymentReceiptPostingService.CancelAsync(receipt.Id, cancelledByUserId, reason, cancellationToken);
        }

        await invoicePostingService.CancelAsync(invoiceId, cancelledByUserId, reason, cancellationToken);
    }
}
