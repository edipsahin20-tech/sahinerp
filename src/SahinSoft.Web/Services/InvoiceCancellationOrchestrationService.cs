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

        // Her adım kendi başına commit ettiği için (yukarıdaki atomiklik notuna bakın), hangi
        // fişlerin gerçekten iptal edildiğini burada takip ediyoruz — bir sonraki adım hata verirse
        // kullanıcıya "hiçbir şey olmadı" ile "bir kısmı zaten iptal oldu, kalan işi tamamla" arasındaki
        // farkı net biçimde göstermek için.
        var cancelledReceiptNumbers = new List<string>();
        foreach (var receipt in activeReceipts)
        {
            try
            {
                await paymentReceiptPostingService.CancelAsync(receipt.Id, cancelledByUserId, reason, cancellationToken);
                cancelledReceiptNumbers.Add(receipt.ReceiptNumber);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ConcurrencyRetryExhaustedException)
            {
                var progress = cancelledReceiptNumbers.Count > 0
                    ? $"Şu fiş(ler) başarıyla iptal edildi: {string.Join(", ", cancelledReceiptNumbers)}. "
                    : "Henüz hiçbir fiş iptal edilmedi. ";
                throw new InvalidOperationException(
                    $"{progress}{receipt.ReceiptNumber} iptal edilirken hata oluştu: {ex.Message} " +
                    "Fatura HENÜZ İPTAL EDİLMEDİ. Kalan fiş(ler)i ve faturayı kontrol edip işlemi tekrar deneyin.", ex);
            }
        }

        try
        {
            await invoicePostingService.CancelAsync(invoiceId, cancelledByUserId, reason, cancellationToken);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ConcurrencyRetryExhaustedException)
        {
            throw new InvalidOperationException(
                $"Bağlı fiş(ler) ({string.Join(", ", cancelledReceiptNumbers)}) başarıyla iptal edildi, ancak faturanın " +
                $"kendisi iptal edilirken hata oluştu: {ex.Message} Fatura hâlâ ONAYLI durumda ve AÇIK kalmıştır " +
                "(bu tüm işlem tek bir veritabanı transaction'ı değildir); lütfen faturayı normal İptal Et akışıyla " +
                "tekrar deneyin.", ex);
        }
    }
}
