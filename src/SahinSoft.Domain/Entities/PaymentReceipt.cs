using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class PaymentReceipt : EntityBase
{
    public ReceiptType ReceiptType { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public DateTime ReceiptDateUtc { get; set; } = DateTime.UtcNow;
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public PaymentReceiptStatus Status { get; set; } = PaymentReceiptStatus.Draft;
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public int? BusinessProjectId { get; set; }
    public BusinessProject? BusinessProject { get; set; }
    // Kapalı Fatura onayında otomatik oluşturulan tahsilat/tediye fişini kaynak faturaya bağlar
    // (bkz. InvoicePostingService.ApproveCoreAsync). Elle oluşturulan serbest fişlerde boş kalır.
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public ICollection<PaymentReceiptLine> Lines { get; set; } = new List<PaymentReceiptLine>();
}
