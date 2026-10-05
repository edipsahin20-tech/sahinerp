using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class FinancialAccount : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public FinancialAccountType AccountType { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? BankName { get; set; }
    // Bankanın kendi şube adı (örn. "NİLÜFER") - bizim satış şubemiz DEĞİL.
    public string? BranchName { get; set; }

    // Hesabın sahibi olan şube (örn. Nakit kasası). Null + IsShared=false = henüz eşlenmemiş.
    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    // Ortak hesap (örn. tek YAPI KREDİ, tek Trendyol kasası): tüm şubelerin kullanımına açıktır,
    // her hareket kaynak şubesini (FinancialTransaction.OriginBranchId) taşır.
    public bool IsShared { get; set; }
    public string? Iban { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    public ICollection<PaymentReceiptLine> PaymentReceiptLines { get; set; } = new List<PaymentReceiptLine>();
}
