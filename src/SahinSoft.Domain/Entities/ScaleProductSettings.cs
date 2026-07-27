using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

public sealed class ScaleProductSettings : EntityBase
{
    public ProductMeasurementType MeasurementType { get; set; }
    public string Prefix { get; set; } = "27";
    public string PluCode { get; set; } = string.Empty;
    public bool BarcodeContainsPrice { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string ScaleBarcode => $"{Prefix}{PluCode}";
}
