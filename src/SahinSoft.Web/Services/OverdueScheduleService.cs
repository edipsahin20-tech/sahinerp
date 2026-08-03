using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

public sealed record NettedScheduleItem(
    int InvoiceId,
    string InvoiceNumber,
    int CustomerId,
    string CustomerName,
    InvoiceType InvoiceType,
    DateTime DueDateUtc,
    decimal RemainingAmount);

// Elle girilen normal Tahsilat/Tediye fişleri belirli bir faturaya bağlanmaz (yalnızca "Kapalı
// Fatura" otomatik ödemesi InvoiceId taşır) — bu yüzden bir faturanın kendi InvoicePaymentSchedule.
// PaidAmount'ı, müşteri o borcu genel bir ödemeyle zaten kapatmış olsa bile sıfır/eksik kalabilir
// (üretimde AF.00053 + TED.00006/TED.00007 ile doğrulandı: cari bakiye 0 ama fatura hâlâ "vadesi
// geçmiş" görünüyordu). Bu servis, faturanın kendi PaidAmount'ına değil müşterinin GERÇEK cari
// bakiyesine göre kalan tutarı hesaplar: her müşteri + fatura türü (Satış=alacak yönü, Alış=borç
// yönü) için bakiyeyi tek bir havuz sayar, vadesi en yakın faturadan başlayarak (FIFO) bu havuzdan
// düşer. Havuz sıfırsa (borç/alacak yok) o müşterinin hiçbir faturası sonuçta görünmez.
public sealed class OverdueScheduleService(ApplicationDbContext dbContext)
{
    public async Task<List<NettedScheduleItem>> GetNettedSchedulesAsync(
        DateTime windowUntilExclusive,
        InvoiceType? invoiceType = null,
        CancellationToken cancellationToken = default)
    {
        var rawQuery = dbContext.InvoicePaymentSchedules.AsNoTracking()
            .Where(x => x.Invoice.Status == InvoiceStatus.Approved && x.DueDateUtc < windowUntilExclusive);

        if (invoiceType.HasValue)
        {
            rawQuery = rawQuery.Where(x => x.Invoice.InvoiceType == invoiceType.Value);
        }

        var rawSchedules = await rawQuery
            .OrderBy(x => x.DueDateUtc)
            .ThenBy(x => x.InvoiceId)
            .Select(x => new
            {
                x.InvoiceId,
                x.Invoice.InvoiceNumber,
                x.Invoice.InvoiceType,
                x.Invoice.CustomerId,
                CustomerName = x.Invoice.Customer.Name,
                x.DueDateUtc,
                x.Amount,
                x.PaidAmount
            })
            // Kapalı Fatura otomatik ödemesi (veya legacy mutabakat) PaidAmount'ı zaten doğru
            // yazmış olabilir — bu satırın kendi bilinen kalanı (Amount-PaidAmount) sıfırsa fatura
            // gerçekten kapalıdır, havuzla hiç yarışmamalı (yoksa aynı müşterinin GERÇEKTEN açık
            // olan başka bir faturasının önüne geçip havuzu haksız yere tüketir).
            .Where(x => x.Amount > x.PaidAmount)
            .ToListAsync(cancellationToken);

        if (rawSchedules.Count == 0)
        {
            return [];
        }

        var customerIds = rawSchedules.Select(x => x.CustomerId).Distinct().ToList();
        var balances = await dbContext.CurrentAccountTransactions.AsNoTracking()
            .Where(x => customerIds.Contains(x.CustomerId))
            .GroupBy(x => x.CustomerId)
            .Select(g => new { CustomerId = g.Key, Balance = g.Sum(x => x.Debit) - g.Sum(x => x.Credit) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Balance, cancellationToken);

        var pools = new Dictionary<(int CustomerId, InvoiceType Type), decimal>();
        var result = new List<NettedScheduleItem>();
        foreach (var schedule in rawSchedules)
        {
            var key = (schedule.CustomerId, schedule.InvoiceType);
            if (!pools.TryGetValue(key, out var pool))
            {
                var balance = balances.GetValueOrDefault(schedule.CustomerId, 0m);
                // Satış: pozitif bakiye (Debit>Credit) müşterinin bize borcu — alacak yönü.
                // Alış: negatif bakiye (Credit>Debit) bizim müşteriye borcumuz — ödeme yönü.
                pool = schedule.InvoiceType == InvoiceType.Sales ? Math.Max(0, balance) : Math.Max(0, -balance);
                pools[key] = pool;
            }

            var ownRemaining = schedule.Amount - schedule.PaidAmount;
            var remaining = Math.Min(ownRemaining, pool);
            pools[key] = pool - remaining;

            if (remaining > 0)
            {
                result.Add(new NettedScheduleItem(
                    schedule.InvoiceId,
                    schedule.InvoiceNumber,
                    schedule.CustomerId,
                    schedule.CustomerName,
                    schedule.InvoiceType,
                    schedule.DueDateUtc,
                    remaining));
            }
        }

        return result;
    }
}
