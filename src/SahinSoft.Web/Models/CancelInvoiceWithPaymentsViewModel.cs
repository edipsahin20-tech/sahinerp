using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

// Faturaya bağlı aktif tahsilat/tediye fişleri olduğunda gösterilen onay ekranı — kullanıcı,
// hangi belgelerin ters çevrileceğini görüp bilerek onaylar.
public sealed class CancelInvoiceWithPaymentsViewModel
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public InvoiceType InvoiceType { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string CurrencyCode { get; set; } = "TRY";

    public List<LinkedReceiptViewModel> LinkedReceipts { get; set; } = [];

    [Required(ErrorMessage = "İptal gerekçesi zorunludur.")]
    [Display(Name = "İptal gerekçesi")]
    public string Reason { get; set; } = string.Empty;
}

public sealed class LinkedReceiptViewModel
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public ReceiptType ReceiptType { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime ReceiptDateUtc { get; set; }
    public string FinancialAccountNames { get; set; } = string.Empty;
}
