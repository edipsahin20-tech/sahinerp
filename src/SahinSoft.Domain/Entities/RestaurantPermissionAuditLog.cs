using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// "Şifre sorulsun mu?" ikinci yetkili onayı verildiğinde bir kayıt (Edip, 2026-09-04, madde 21 -
// "bu onaylar MUTLAKA loglanmalı/denetlenebilir olmalı"). Sadece GERÇEKTEN bir ikinci onay
// istenip alındığı durumlarda yazılır - InventorySettings'teki ilgili "RequireSecondApprovalFor..."
// bayrağı kapalıysa (çoğunluk durum) hiç kayıt oluşmaz. UserId'ler string ve navigasyonsuz -
// ApplicationUser SahinSoft.Domain dışında yaşıyor (bkz. RestaurantCashShift.CashierUserId deseni).
public sealed class RestaurantPermissionAuditLog : EntityBase
{
    public string Action { get; set; } = string.Empty;
    public string PerformedByUserId { get; set; } = string.Empty;
    public string ApproverUserId { get; set; } = string.Empty;
    public int? RestaurantCheckId { get; set; }
    public int? RestaurantOrderLineId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
