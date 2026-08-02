using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class DispatchNoteLine : EntityBase
{
    public int LineNumber { get; set; }
    public decimal Quantity { get; set; }

    // Bu irsaliye satırının ne kadarı faturaya dönüştürüldü — kısmi faturalamayı destekler.
    // Siparişin FulfilledQuantity'sinden AYRI bir havuzdur: irsaliye onaylandığı anda sipariş
    // havuzu zaten tüketildi, bu sayaç sadece "bu irsaliyenin kendisi ne kadar faturalandı"yı izler.
    public decimal InvoicedQuantity { get; set; }

    public int DispatchNoteId { get; set; }
    public DispatchNote DispatchNote { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    // Bu irsaliye satırı hangi sipariş satırından sevk edildi (Sipariş → İrsaliye dönüşümü).
    // Bağımsız oluşturulan irsaliyelerde null kalır.
    public int? BusinessOrderLineId { get; set; }
    public BusinessOrderLine? BusinessOrderLine { get; set; }

    // Bu irsaliye satırından üretilmiş fatura satırları (dönüşüm izlenebilirliği).
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
}
