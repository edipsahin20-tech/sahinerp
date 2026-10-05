using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class CompanySettings : EntityBase
{
    public string CompanyName { get; set; } = "ŞahinSoft";
    public string? TaxOffice { get; set; }
    public string? TaxNumber { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? BankName { get; set; }
    public string? Iban { get; set; }
    public string? LogoPath { get; set; }

    // Muhasebe evrak ekranlarında (fatura, irsaliye, sipariş, makbuz) varsayılan gelen şube/depo; evrakta değiştirilebilir.
    // Restoran/POS bunlardan etkilenmez.
    public int? DefaultBranchId { get; set; }
    public int? DefaultWarehouseId { get; set; }
}
