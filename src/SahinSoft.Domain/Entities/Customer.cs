using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class Customer : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxOffice { get; set; }
    public string? TaxNumber { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Notes { get; set; }
    public bool IsCustomer { get; set; } = true;
    public bool IsSupplier { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<PurchasePriceList> PurchasePriceLists { get; set; } = new List<PurchasePriceList>();
    public ICollection<CurrentAccountTransaction> AccountTransactions { get; set; } = new List<CurrentAccountTransaction>();
    public ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
    public ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();
    public ICollection<SalesPriceList> SalesPriceLists { get; set; } = new List<SalesPriceList>();
}
