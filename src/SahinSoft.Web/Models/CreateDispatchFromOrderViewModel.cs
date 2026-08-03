using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

// Sipariş → İrsaliye dönüşümü: onaylı/kısmen karşılanmış bir siparişin kalan satırlarını gösterir,
// kullanıcı her satır için "şimdi sevk edilecek miktar"ı düzenleyebilir (varsayılan: tam kalan
// miktar) — böylece tam ve kısmi sevkiyat aynı formda desteklenir.
public sealed class CreateDispatchFromOrderViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public InvoiceType DispatchType { get; set; }
    public int CustomerId { get; set; }
    public string CustomerDisplay { get; set; } = string.Empty;

    // Çift tıklama/mükerrer POST koruması — bkz. DispatchNote.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();

    // InventorySettings + kullanıcı rolüne göre GET anında hesaplanır — ekranda "Taslak
    // oluşturulacak" / "Kaydedilecek ve otomatik onaylanacak" bilgisini göstermek için.
    public bool WillAutoApprove { get; set; }

    [Required(ErrorMessage = "Depo seçilmelidir.")]
    [Display(Name = "Depo")]
    public int? WarehouseId { get; set; }
    public string? WarehouseDisplay { get; set; }

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

    public List<CreateDispatchFromOrderLineViewModel> Lines { get; set; } = [];
}

public sealed class CreateDispatchFromOrderLineViewModel
{
    public int BusinessOrderLineId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string UnitSnapshot { get; set; } = "Adet";
    public decimal OrderedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    [Display(Name = "Sevk edilecek miktar")]
    public decimal QuantityToShip { get; set; }
}
