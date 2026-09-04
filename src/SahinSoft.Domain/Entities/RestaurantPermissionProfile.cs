using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// Kullanıcı tanımlı yetki profili (Edip, 2026-09-04: "Yetki Mimarisi" - "X Yetkisi, Y Yetkisi, Z
// Yetkisi, kullanıcı tarafından oluşturulabilecek yeni yetkiler"). Admin serbest bir İSİM girer
// (ör. "Kasiyer", "Şef Garson"), kritik işlemler için aşağıdaki sabit bayrak listesinden hangileri
// açık olacağını seçer. Bir personele BİRDEN FAZLA profil atanabilir (bkz.
// RestaurantPersonnelPermissionProfile) - herhangi biri izin veriyorsa işlem serbesttir.
// Hiç profili olmayan kullanıcı için TÜM işlemler serbesttir (spec: "varsayılan bütün yetkiler
// açık başlayabilir") - Administrator rolü zaten her zaman tam yetkilidir, profile hiç bakılmaz.
public sealed class RestaurantPermissionProfile : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public bool CanCancelOrderLine { get; set; } = true;
    public bool CanCancelReceipt { get; set; } = true;
    public bool CanApplyDiscount { get; set; } = true;
    public bool CanApplyComplimentary { get; set; } = true;
    public bool CanEditKitchenSentLines { get; set; } = true;
    public bool CanAddNote { get; set; } = true;

    public ICollection<RestaurantPersonnelPermissionProfile> Assignments { get; set; } = new List<RestaurantPersonnelPermissionProfile>();
}
