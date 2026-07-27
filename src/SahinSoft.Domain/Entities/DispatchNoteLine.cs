using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class DispatchNoteLine : EntityBase
{
    public int LineNumber { get; set; }
    public decimal Quantity { get; set; }
    public int DispatchNoteId { get; set; }
    public DispatchNote DispatchNote { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
