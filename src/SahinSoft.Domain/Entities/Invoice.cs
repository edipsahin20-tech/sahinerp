using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class Invoice : EntityBase
{
    public InvoiceType InvoiceType { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DueDateUtc { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }

    public string? ReferenceNumber { get; set; }
    public string? PaymentTerm { get; set; }
    public string? TradeType { get; set; }
    public bool IsReturn { get; set; }
    public string? SalespersonUserId { get; set; }
    public int? SettlementFinancialAccountId { get; set; }
    public FinancialAccount? SettlementFinancialAccount { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int? QuoteId { get; set; }
    public Quote? Quote { get; set; }
    public int? PurchasePriceListId { get; set; }
    public PurchasePriceList? PurchasePriceList { get; set; }
    public int? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public int? BusinessProjectId { get; set; }
    public BusinessProject? BusinessProject { get; set; }
    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public ICollection<CurrentAccountTransaction> AccountTransactions { get; set; } = new List<CurrentAccountTransaction>();
    public ICollection<InvoicePaymentSchedule> PaymentSchedules { get; set; } = new List<InvoicePaymentSchedule>();
    public ICollection<DispatchNote> DispatchNotes { get; set; } = new List<DispatchNote>();
}
