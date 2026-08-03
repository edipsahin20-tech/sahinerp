using System.Data;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services;

// Çek/Senet, kabul edildiği anda kıymetli evrak olarak müşteri/tedarikçi borcunu gerçekten kapatır
// (Tek Düzen Hesap Planı'ndaki 101/103 kıymetli evrak hesaplarının muadili) — bu yüzden ilk cari
// netleştirme hareketi, kaydın kendisiyle AYNI transaction'da atomik olarak oluşturulur (bkz.
// CreateAsync). Sonraki durum geçişleri yalnızca kasa/banka hareketi veya devralan cariye yeni bir
// netleştirme oluşturur/geri alır — kayıt oluştuktan sonra Tutar/Cari/Yön hiçbir zaman değişmez
// (bkz. NegotiableInstrumentsController.Edit'in salt görüntüleme alanları).
public sealed class NegotiableInstrumentPostingService(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator)
{
    public Task<NegotiableInstrument> CreateAsync(
        NegotiableInstrumentType instrumentType,
        InstrumentDirection direction,
        int customerId,
        DateTime issueDateUtc,
        DateTime dueDateUtc,
        string currencyCode,
        decimal amount,
        string? bankName,
        string? branchName,
        string? accountNumber,
        string? drawerName,
        string? description,
        int? financialAccountId,
        string createdByUserId,
        Guid? submissionKey,
        CancellationToken cancellationToken = default)
    {
        var sequenceKey = instrumentType == NegotiableInstrumentType.Cheque ? "NEGOTIABLE_CHEQUE" : "NEGOTIABLE_NOTE";

        return DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var instrument = new NegotiableInstrument
                {
                    InstrumentType = instrumentType,
                    Direction = direction,
                    Status = InstrumentStatus.Portfolio,
                    InstrumentNumber = await documentNumberGenerator.GenerateWithinTransactionAsync(sequenceKey, cancellationToken),
                    CreatedByUserId = createdByUserId,
                    SubmissionKey = submissionKey,
                    CustomerId = customerId,
                    IssueDateUtc = issueDateUtc,
                    DueDateUtc = dueDateUtc,
                    CurrencyCode = currencyCode,
                    Amount = amount,
                    BankName = bankName,
                    BranchName = branchName,
                    AccountNumber = accountNumber,
                    DrawerName = drawerName,
                    Description = description,
                    FinancialAccountId = financialAccountId
                };
                dbContext.NegotiableInstruments.Add(instrument);

                // Alınan çek/senet: Tahsilat gibi davranır (müşteri borcu kapanır, Credit).
                // Verilen çek/senet: Tediye gibi davranır (tedarikçi borcu kapanır, Debit).
                dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
                {
                    TransactionDateUtc = issueDateUtc,
                    TransactionType = direction == InstrumentDirection.Received
                        ? CurrentAccountTransactionType.Collection
                        : CurrentAccountTransactionType.Payment,
                    DocumentNumber = instrument.InstrumentNumber,
                    CurrencyCode = currencyCode,
                    ExchangeRate = 1,
                    Debit = direction == InstrumentDirection.Issued ? amount : 0,
                    Credit = direction == InstrumentDirection.Received ? amount : 0,
                    CustomerId = customerId,
                    Description = $"{InstrumentTypeLabel(instrumentType)} kaydı - netleştirme",
                    NegotiableInstrument = instrument
                });

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return instrument;
            });
        }, cancellationToken);
    }

    // Tahsil Edildi (Alınan) / Ödendi (Verilen): cari zaten kayıt anında kapandığı için burada bir
    // daha dokunulmaz — yalnızca seçilen kasa/bankaya gerçek bir giriş/çıkış hareketi eklenir.
    public Task SettleAsync(
        int id,
        int settlementFinancialAccountId,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var instrument = await dbContext.NegotiableInstruments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                    ?? throw new InvalidOperationException("Kayıt bulunamadı.");

                if (instrument.Status != InstrumentStatus.Portfolio)
                {
                    throw new InvalidOperationException("Yalnızca portföydeki kayıtlar tahsil/ödeme durumuna geçirilebilir.");
                }

                dbContext.FinancialTransactions.Add(new FinancialTransaction
                {
                    TransactionDateUtc = DateTime.UtcNow,
                    TransactionType = instrument.Direction == InstrumentDirection.Received
                        ? FinancialTransactionType.Collection
                        : FinancialTransactionType.Payment,
                    DocumentNumber = instrument.InstrumentNumber,
                    Amount = instrument.Amount,
                    ExchangeRate = 1,
                    Description = $"{InstrumentTypeLabel(instrument.InstrumentType)} tahsilatı/ödemesi",
                    FinancialAccountId = settlementFinancialAccountId,
                    CustomerId = instrument.CustomerId,
                    NegotiableInstrumentId = instrument.Id
                });

                instrument.Status = instrument.Direction == InstrumentDirection.Received
                    ? InstrumentStatus.Collected
                    : InstrumentStatus.Paid;
                instrument.SettlementFinancialAccountId = settlementFinancialAccountId;
                instrument.SettledAtUtc = DateTime.UtcNow;
                instrument.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            });
        }, cancellationToken);

    // Ciro Edildi (yalnız Alınan): devralan gerçek bir cari karttır — devralanın borcu, çekin/senedin
    // tutarı kadar bir Tediye hareketiyle azalır. Kasa/banka hareketi oluşmaz.
    public Task EndorseAsync(
        int id,
        int endorsedToCustomerId,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var instrument = await dbContext.NegotiableInstruments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                    ?? throw new InvalidOperationException("Kayıt bulunamadı.");

                if (instrument.Direction != InstrumentDirection.Received)
                {
                    throw new InvalidOperationException("Yalnızca alınan çek/senetler ciro edilebilir.");
                }

                if (instrument.Status != InstrumentStatus.Portfolio)
                {
                    throw new InvalidOperationException("Yalnızca portföydeki kayıtlar ciro edilebilir.");
                }

                var endorsedCustomerExists = await dbContext.Customers
                    .AnyAsync(x => x.Id == endorsedToCustomerId && x.IsActive, cancellationToken);
                if (!endorsedCustomerExists)
                {
                    throw new InvalidOperationException("Geçerli, aktif bir cari seçilmelidir.");
                }

                dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
                {
                    TransactionDateUtc = DateTime.UtcNow,
                    TransactionType = CurrentAccountTransactionType.Payment,
                    DocumentNumber = instrument.InstrumentNumber,
                    CurrencyCode = instrument.CurrencyCode,
                    ExchangeRate = 1,
                    Debit = instrument.Amount,
                    Credit = 0,
                    CustomerId = endorsedToCustomerId,
                    Description = $"{InstrumentTypeLabel(instrument.InstrumentType)} cirosu ile borç kapatma",
                    NegotiableInstrumentId = instrument.Id
                });

                instrument.Status = InstrumentStatus.Endorsed;
                instrument.EndorsedToCustomerId = endorsedToCustomerId;
                instrument.EndorsedAtUtc = DateTime.UtcNow;
                instrument.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            });
        }, cancellationToken);

    // Karşılıksız/Protesto (yalnız Alınan): o ana kadar oluşmuş netleştirme(ler) ters kayıtla geri
    // alınır — hem asıl müşterinin borcu hem de (ciro edilmişse) devralanın kapanan borcu yeniden açılır.
    public Task ProtestAsync(int id, CancellationToken cancellationToken = default) =>
        ReverseNettingsAndSetStatusAsync(id, InstrumentStatus.Protested, "Karşılıksız/Protesto", requireReceivedDirection: true, cancellationToken);

    // İade Edildi (her iki yön): aynı geri alma mantığı, yön kısıtı yok.
    public Task ReturnAsync(int id, CancellationToken cancellationToken = default) =>
        ReverseNettingsAndSetStatusAsync(id, InstrumentStatus.Returned, "İade", requireReceivedDirection: false, cancellationToken);

    private Task ReverseNettingsAndSetStatusAsync(
        int id,
        InstrumentStatus targetStatus,
        string reasonLabel,
        bool requireReceivedDirection,
        CancellationToken cancellationToken) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var instrument = await dbContext.NegotiableInstruments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                    ?? throw new InvalidOperationException("Kayıt bulunamadı.");

                if (requireReceivedDirection && instrument.Direction != InstrumentDirection.Received)
                {
                    throw new InvalidOperationException("Bu işlem yalnızca alınan çek/senetler için geçerlidir.");
                }

                if (instrument.Status is not (InstrumentStatus.Portfolio or InstrumentStatus.Endorsed))
                {
                    throw new InvalidOperationException("Bu durum değişikliği yalnızca Portföyde veya Ciro Edildi kayıtlarda yapılabilir.");
                }

                await ReverseOpenCurrentAccountTransactionsAsync(instrument, reasonLabel, cancellationToken);

                instrument.Status = targetStatus;
                instrument.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            });
        }, cancellationToken);

    // İptal: zaten İptal edilmiş bir kayıt hariç HER durumdan (terminal Tahsil Edildi/Ödendi dahil)
    // çağrılabilir — yetki kontrolü (yalnızca Administrator) controller'da yapılır. O ana kadar
    // oluşmuş TÜM cari ve kasa/banka hareketleri ters kayıtla geri alınır.
    public Task CancelAsync(
        int id,
        string reason,
        string cancelledByUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        return DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var instrument = await dbContext.NegotiableInstruments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                    ?? throw new InvalidOperationException("Kayıt bulunamadı.");

                if (instrument.Status == InstrumentStatus.Cancelled)
                {
                    throw new InvalidOperationException("Bu kayıt zaten iptal edilmiş.");
                }

                await ReverseOpenCurrentAccountTransactionsAsync(instrument, $"İptal - {reason}", cancellationToken);
                await ReverseOpenFinancialTransactionsAsync(instrument, $"İptal - {reason}", cancellationToken);

                instrument.Status = InstrumentStatus.Cancelled;
                instrument.CancelledByUserId = cancelledByUserId;
                instrument.CancelledAtUtc = DateTime.UtcNow;
                instrument.CancellationReason = reason;
                instrument.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            });
        }, cancellationToken);
    }

    // Bu enstrümana bağlı, henüz ters kaydı yapılmamış CurrentAccountTransaction'ları bulur ve
    // her biri için bir ters kayıt (Debit/Credit yer değiştirmiş, ReversalOfId ile bağlı) oluşturur.
    // Karşılıksız/İade sırasında yalnızca netleştirme(ler) için, İptal sırasında ise (bu satırlar
    // zaten Karşılıksız/İade ile geri alınmışsa tekrar dokunulmadan) genel amaçlı kullanılır.
    private async Task ReverseOpenCurrentAccountTransactionsAsync(
        NegotiableInstrument instrument,
        string reasonLabel,
        CancellationToken cancellationToken)
    {
        var transactions = await dbContext.CurrentAccountTransactions
            .Where(x => x.NegotiableInstrumentId == instrument.Id)
            .ToListAsync(cancellationToken);

        var reversedOriginalIds = transactions
            .Where(x => x.ReversalOfId != null)
            .Select(x => x.ReversalOfId!.Value)
            .ToHashSet();

        foreach (var original in transactions.Where(x => x.ReversalOfId is null && !reversedOriginalIds.Contains(x.Id)))
        {
            dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
            {
                TransactionDateUtc = DateTime.UtcNow,
                TransactionType = original.TransactionType == CurrentAccountTransactionType.Collection
                    ? CurrentAccountTransactionType.Payment
                    : CurrentAccountTransactionType.Collection,
                DocumentNumber = $"IPTAL-{instrument.InstrumentNumber}",
                CurrencyCode = original.CurrencyCode,
                ExchangeRate = original.ExchangeRate,
                Debit = original.Credit,
                Credit = original.Debit,
                CustomerId = original.CustomerId,
                Description = $"{reasonLabel} - {instrument.InstrumentNumber}",
                NegotiableInstrumentId = instrument.Id,
                ReversalOfId = original.Id
            });
        }
    }

    // İptal'e özgü: bu enstrümana bağlı, henüz ters kaydı yapılmamış FinancialTransaction'ları
    // (Tahsil Edildi/Ödendi ile oluşmuş kasa/banka hareketi) bulur ve tersini oluşturur.
    private async Task ReverseOpenFinancialTransactionsAsync(
        NegotiableInstrument instrument,
        string reasonLabel,
        CancellationToken cancellationToken)
    {
        var transactions = await dbContext.FinancialTransactions
            .Where(x => x.NegotiableInstrumentId == instrument.Id)
            .ToListAsync(cancellationToken);

        var reversedOriginalIds = transactions
            .Where(x => x.ReversalOfId != null)
            .Select(x => x.ReversalOfId!.Value)
            .ToHashSet();

        foreach (var original in transactions.Where(x => x.ReversalOfId is null && !reversedOriginalIds.Contains(x.Id)))
        {
            dbContext.FinancialTransactions.Add(new FinancialTransaction
            {
                TransactionDateUtc = DateTime.UtcNow,
                TransactionType = original.TransactionType == FinancialTransactionType.Collection
                    ? FinancialTransactionType.Payment
                    : FinancialTransactionType.Collection,
                DocumentNumber = $"IPTAL-{instrument.InstrumentNumber}",
                Amount = original.Amount,
                ExchangeRate = original.ExchangeRate,
                Description = $"{reasonLabel} - {instrument.InstrumentNumber}",
                FinancialAccountId = original.FinancialAccountId,
                CustomerId = original.CustomerId,
                NegotiableInstrumentId = instrument.Id,
                ReversalOfId = original.Id
            });
        }
    }

    private static string InstrumentTypeLabel(NegotiableInstrumentType type) =>
        type == NegotiableInstrumentType.Cheque ? "Çek" : "Senet";
}
