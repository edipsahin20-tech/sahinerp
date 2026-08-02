using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class InvoiceLine : EntityBase
{
    public int LineNumber { get; set; }
    public string ProductCodeSnapshot { get; set; } = string.Empty;
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string UnitSnapshot { get; set; } = "Adet";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Description { get; set; }

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    // Bu fatura satırı hangi irsaliye satırından kesildi (İrsaliye → Fatura dönüşümü).
    // Kaynak sipariş satırının FulfilledQuantity'sine BURADAN dokunulmaz — irsaliye onayında zaten
    // tüketildi (bkz. DispatchNoteLine.BusinessOrderLineId); çifte tüketimi önlemek için bu satırda
    // asla DispatchNoteLineId ile BusinessOrderLineId birlikte set edilmez.
    public int? DispatchNoteLineId { get; set; }
    public DispatchNoteLine? DispatchNoteLine { get; set; }

    // Bu fatura satırı hangi sipariş satırından DOĞRUDAN (irsaliyesiz) kesildi.
    public int? BusinessOrderLineId { get; set; }
    public BusinessOrderLine? BusinessOrderLine { get; set; }
}
