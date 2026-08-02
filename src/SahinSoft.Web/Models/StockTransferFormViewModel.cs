using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Web.Models;

public sealed class StockTransferFormViewModel
{
    public int Id { get; set; }

    // Çift tıklama/mükerrer POST koruması — bkz. StockTransfer.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();

    public string? TransferNumber { get; set; }

    [Required(ErrorMessage = "Çıkış deposu seçilmelidir.")]
    [Display(Name = "Çıkış Deposu")]
    public int? FromWarehouseId { get; set; }

    [Required(ErrorMessage = "Giriş deposu seçilmelidir.")]
    [Display(Name = "Giriş Deposu")]
    public int? ToWarehouseId { get; set; }

    [Required]
    [Display(Name = "Transfer tarihi")]
    [DataType(DataType.Date)]
    public DateTime TransferDateUtc { get; set; } = DateTime.UtcNow.Date;

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    public List<StockTransferLineFormViewModel> Lines { get; set; } = [];

    public string? FromWarehouseDisplay { get; set; }
    public string? ToWarehouseDisplay { get; set; }
}

public sealed class StockTransferLineFormViewModel
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    [Display(Name = "Ürün")]
    public int? ProductId { get; set; }

    public string? ProductDisplay { get; set; }

    [Range(0.001, 999999999)]
    [Display(Name = "Miktar")]
    public decimal Quantity { get; set; } = 1;
}
