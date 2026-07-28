using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class StockSlipFormViewModel
{
    public int Id { get; set; }

    public StockSlipType SlipType { get; set; }

    [Required(ErrorMessage = "Depo seçilmelidir.")]
    [Display(Name = "Depo")]
    public int? WarehouseId { get; set; }

    [Required]
    [Display(Name = "Fiş tarihi")]
    [DataType(DataType.Date)]
    public DateTime SlipDateUtc { get; set; } = DateTime.UtcNow.Date;

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    public List<StockSlipLineFormViewModel> Lines { get; set; } = [];

    public string? WarehouseDisplay { get; set; }
}

public sealed class StockSlipLineFormViewModel
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    [Display(Name = "Ürün")]
    public int? ProductId { get; set; }

    public string? ProductDisplay { get; set; }

    [Range(0.001, 999999999)]
    [Display(Name = "Miktar")]
    public decimal Quantity { get; set; } = 1;

    [Range(0, 999999999)]
    [Display(Name = "Birim Maliyet")]
    public decimal UnitCost { get; set; }

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
}
