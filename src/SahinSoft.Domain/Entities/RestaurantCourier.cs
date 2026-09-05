using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

// Kurye Tanımları (madde birebir-uygulama, Paket Operasyon Merkezi, 2026-09-05) - "Kurye Takibi"
// panelindeki roster. "Restoran Kuryesi" (işletmenin kendi kuryesi) veya dış/platform kuryesi
// (Yemeksepeti/Trendyol/GetirYemek'in kendi kuryesi, sadece kayıt amaçlı) olabilir. Canlı GPS
// konumu YOK - Status alanı kasiyer/kurye tarafından elle güncellenir.
public sealed class RestaurantCourier : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsExternal { get; set; }
    public CourierStatus Status { get; set; } = CourierStatus.Available;
    public bool IsActive { get; set; } = true;
}
