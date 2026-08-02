using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class DispatchNoteFormViewModel
{
    public int Id { get; set; }

    // Çift tıklama/mükerrer POST koruması — bkz. DispatchNote.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();

    public InvoiceType DispatchType { get; set; }

    public string? DispatchNumber { get; set; }

    [Required(ErrorMessage = "Cari seçilmelidir.")]
    [Display(Name = "Cari")]
    public int? CustomerId { get; set; }

    [Required(ErrorMessage = "Depo seçilmelidir.")]
    [Display(Name = "Depo")]
    public int? WarehouseId { get; set; }

    [Required]
    [Display(Name = "İrsaliye tarihi")]
    [DataType(DataType.Date)]
    public DateTime DispatchDateUtc { get; set; } = DateTime.UtcNow.Date;

    [StringLength(20)]
    [Display(Name = "Araç plakası")]
    public string? VehiclePlate { get; set; }

    [StringLength(150)]
    [Display(Name = "Nakliyeci")]
    public string? CarrierName { get; set; }

    [StringLength(500)]
    [Display(Name = "Notlar")]
    public string? Notes { get; set; }

    public List<DispatchNoteLineFormViewModel> Lines { get; set; } = [];

    public string? CustomerDisplay { get; set; }
    public string? WarehouseDisplay { get; set; }
}

public sealed class DispatchNoteLineFormViewModel
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    [Display(Name = "Ürün")]
    public int? ProductId { get; set; }

    public string? ProductDisplay { get; set; }

    [Range(0.001, 999999999)]
    [Display(Name = "Miktar")]
    public decimal Quantity { get; set; } = 1;
}
