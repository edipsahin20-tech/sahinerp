using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum FinancialAccountType
{
    [Display(Name = "Kasa")]
    Cash = 1,
    [Display(Name = "Banka")]
    Bank = 2,
    // Yemek Çeki: Yemek kartı ödemelerinin toplandığı hesap türü (örn. TRENDYOL KASASI). Ortak
    // hesaptır, hangi şubeden geldiği FinancialTransaction.OriginBranchId ile izlenir.
    [Display(Name = "Yemek Çeki")]
    MealVoucher = 3
}
