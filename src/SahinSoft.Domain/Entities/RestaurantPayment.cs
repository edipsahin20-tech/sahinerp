using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class RestaurantPayment : EntityBase
{
    public RestaurantPaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsReversal { get; set; }
    public int? ReversalOfId { get; set; }
    public RestaurantPayment? ReversalOf { get; set; }

    public Guid? SubmissionKey { get; set; }

    public int RestaurantCheckId { get; set; }
    public RestaurantCheck RestaurantCheck { get; set; } = null!;
    public int FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;

    // Kapanışta doldurulur.
    public int? FinancialTransactionId { get; set; }
    public FinancialTransaction? FinancialTransaction { get; set; }
}
