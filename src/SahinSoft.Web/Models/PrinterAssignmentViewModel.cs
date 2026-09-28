using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

// "Ana Yazıcı Ataması" paneli (Edip, 2026-09-28: "Adisyon yazıcısı, X/Z raporu için tek
// seferlik bir yazıcı tanımı gerekiyor, kategoriye gerek yok - kategori sadece mutfak
// ürünleri için") - Mutfak burada BİLEREK yok, o zaten Kategori Tanımla ekranındaki
// istasyon bazlı yönlendirmeyle çalışıyor. Bu panel sadece Adisyon/X Raporu/Z Raporu için:
// bir şubede aynı rolde birden fazla Aktif yazıcı varsa PrintDispatchService.
// EnqueueForRoleAsync bunlardan rastgele/ilkini seçiyordu - bu panel o belirsizliği
// ortadan kaldırıp "hangisi şu an kullanılıyor" seçimini tek bir yerden, açıkça yapılabilir
// hale getiriyor. Seçim = o yazıcıyı Aktif yapıp aynı şube+roldeki DİĞERLERİNİ Pasif yapmak.
public sealed class RolePrinterAssignmentViewModel
{
    public PrinterRole Role { get; init; }
    public List<(int Id, string Name)> Options { get; init; } = [];
    public int? ActivePrinterId { get; init; }
}

public sealed class BranchPrinterAssignmentViewModel
{
    public int BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public List<RolePrinterAssignmentViewModel> Roles { get; init; } = [];
}
