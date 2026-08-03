using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class NegotiableInstrument : EntityBase
{
    public NegotiableInstrumentType InstrumentType { get; set; }
    public InstrumentDirection Direction { get; set; }
    public InstrumentStatus Status { get; set; } = InstrumentStatus.Portfolio;
    public string InstrumentNumber { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;

    // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
    public Guid? SubmissionKey { get; set; }
    public DateTime IssueDateUtc { get; set; }
    public DateTime DueDateUtc { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public decimal Amount { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string? AccountNumber { get; set; }
    public string? DrawerName { get; set; }
    public string? Description { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? FinancialAccountId { get; set; }
    public FinancialAccount? FinancialAccount { get; set; }

    // Tahsil Edildi/Ödendi anında seçilen gerçek kasa/banka — FinancialAccountId'den (oluşturmada
    // girilen opsiyonel açıklayıcı alan) ayrı: bu, gerçek parasal hareketin hangi hesaba
    // işlendiğini taşır. Bkz. NegotiableInstrumentPostingService.SettleAsync.
    public int? SettlementFinancialAccountId { get; set; }
    public FinancialAccount? SettlementFinancialAccount { get; set; }
    public DateTime? SettledAtUtc { get; set; }

    // Ciro Edildi anında seçilen gerçek cari — devralan tarafın borcu bu kayıtla azaltılır.
    // Bkz. NegotiableInstrumentPostingService.EndorseAsync.
    public int? EndorsedToCustomerId { get; set; }
    public Customer? EndorsedToCustomer { get; set; }
    public DateTime? EndorsedAtUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }
    public string? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
}
