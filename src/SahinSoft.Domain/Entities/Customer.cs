using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class Customer : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CustomerAccountType AccountType { get; set; } = CustomerAccountType.Corporate;
    public string? TaxOffice { get; set; }
    public string? TaxNumber { get; set; }
    public string? IdentityNumber { get; set; }
    public string? CustomerGroup { get; set; }
    public decimal RiskLimit { get; set; }
    public int? DefaultPaymentTermDays { get; set; }
    public string? AuthorizedPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Notes { get; set; }
    public bool IsCustomer { get; set; } = true;
    public bool IsSupplier { get; set; }
    public bool IsActive { get; set; } = true;

    // Tahsilat Carisi (madde 14, Edip 2026-09-04) - açıkken bu cari restoran ödeme ekranında
    // KENDİ ADIYLA bir ödeme yöntemi butonu olarak belirir (Trendyol/Getir/Yemeksepeti/yemek
    // kartı vb. platform ödemeleri - "ödeme tipleri asla hard-code edilmeyecek"). Seçilince
    // Açık Hesap ile AYNI muhasebe mekaniğini kullanır (Sale normal oluşur, Collection oluşmaz -
    // bu cariden gerçek para geldiğinde normal Tahsilat ekranından kapatılır).
    public bool IsCollectionCari { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;

    // Çift tıklama/mükerrer POST koruması — bkz. DispatchNote.SubmissionKey.
    public Guid? SubmissionKey { get; set; }
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
