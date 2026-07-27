using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class Product : EntityBase
{
    public string StockCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Barcode { get; set; }
    public string Unit { get; set; } = "Adet";
    public string ProductType { get; set; } = "Donanım";
    public string? Description { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal MinimumStockQuantity { get; set; }
    public bool TrackStock { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? ImagePath { get; set; }
    public string? WebsitePath { get; set; }

    public int CategoryId { get; set; }
    public ProductCategory Category { get; set; } = null!;
    public int TaxRateId { get; set; }
    public TaxRate TaxRate { get; set; } = null!;
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<QuoteLine> QuoteLines { get; set; } = new List<QuoteLine>();
    public ICollection<PurchasePriceListItem> PurchasePriceListItems { get; set; } = new List<PurchasePriceListItem>();
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<StockTransferLine> StockTransferLines { get; set; } = new List<StockTransferLine>();
    public ScaleProductSettings? ScaleSettings { get; set; }
    public bool TrackSerialNumbers { get; set; }
    public bool TrackLots { get; set; }
    public ICollection<ProductUnitConversion> UnitConversions { get; set; } = new List<ProductUnitConversion>();
    public ICollection<SalesPriceListItem> SalesPriceListItems { get; set; } = new List<SalesPriceListItem>();
}
