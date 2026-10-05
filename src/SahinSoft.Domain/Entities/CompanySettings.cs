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
    // Kapalı dönem: bu tarihe (dahil) kadar fatura/makbuz/irsaliye eklenemez, değiştirilemez, onaylanamaz, iptal/silinemez. Boş = kilit yok.
    public DateTime? ClosedPeriodUntil { get; set; }

    // Mikro ile çalışan kurulumlarda faturalar Mikro'nun hesap kuralıyla hesaplanır (ekrandaki tutar Mikro'daki tutarla aynı olsun diye).
    public bool MikroCompatibleAmounts { get; set; }

    public int? DefaultBranchId { get; set; }
    public int? DefaultWarehouseId { get; set; }
}
