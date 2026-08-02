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
}
