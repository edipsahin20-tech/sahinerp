using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class Branch : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsHeadOffice { get; set; }
    public bool IsActive { get; set; } = true;

    // Hibrit yerel/bulut senkron: şube merkezdeki sync API'sine bu anahtarla kimlik
    // doğrular (kullanıcı girişi değil, makine-makine bağlantısı). Sadece merkezde
    // dolu olması gerekir; şubenin appsettings.Local.json'ında MerkezSync:ApiKey
    // olarak eşleşen değer tutulur.
    public string? ApiKey { get; set; }

    public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
