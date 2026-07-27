using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class InvoicePaymentSchedule : EntityBase
{
    public int InstallmentNumber { get; set; }
    public DateTime DueDateUtc { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
}
