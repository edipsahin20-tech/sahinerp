using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class InventoryCountFormViewModel
{
    public int Id { get; set; }

    // Çift tıklama/mükerrer POST koruması — bkz. InventoryCount.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();

    public string? CountNumber { get; set; }

    [Required(ErrorMessage = "Depo seçilmelidir.")]
    [Display(Name = "Depo")]
    public int? WarehouseId { get; set; }

    [Required]
    [Display(Name = "Sayım tarihi")]
    [DataType(DataType.Date)]
    public DateTime CountDateUtc { get; set; } = DateTime.UtcNow.Date;

    public List<InventoryCountLineFormViewModel> Lines { get; set; } = [];

    public string? WarehouseDisplay { get; set; }
}

public sealed class InventoryCountLineFormViewModel
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    [Display(Name = "Ürün")]
    public int? ProductId { get; set; }

    public string? ProductDisplay { get; set; }

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Sayılan miktar 0 veya daha büyük olmalıdır.")]
    [Display(Name = "Sayılan Miktar")]
    public decimal CountedQuantity { get; set; }
}
