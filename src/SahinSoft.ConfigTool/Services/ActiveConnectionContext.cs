using SahinSoft.ConfigTool.Models;

namespace SahinSoft.ConfigTool.Services;

// Diğer sayfaların (Oluştur ve Bakım / Hareketleri Sil / Veritabanını Temizle) üst kısmında
// gösterdiği "Sunucu / Veritabanı / Bağlantı" özeti - Bağlantı Ayarları sayfasında hangi profil
// seçiliyse/etkinse o. Tek bir süreç içi paylaşılan durum (uygulama tek pencere, basit tutuldu).
public static class ActiveConnectionContext
{
    public static ConnectionProfile? Current { get; private set; }

    public static event Action? Changed;

    public static void Set(ConnectionProfile? profile)
    {
        Current = profile;
        Changed?.Invoke();
    }

    public static void EnsureLoaded()
    {
        if (Current is not null)
        {
            return;
        }
        var profiles = ProfileStore.LoadProfiles();
        Current = profiles.FirstOrDefault(x => x.IsDefault) ?? profiles.FirstOrDefault();
    }
}
