using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class StockSlipFormViewModel
{
    public int Id { get; set; }

    // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();

    public StockSlipType SlipType { get; set; }

    public string? SlipNumber { get; set; }

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

    [Range(typeof(decimal), "0.001", "999999999", ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
    [Display(Name = "Miktar")]
    public decimal Quantity { get; set; } = 1;

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Birim maliyet 0 veya daha büyük olmalıdır.")]
    [Display(Name = "Birim Maliyet")]
    public decimal UnitCost { get; set; }

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
}
