using System.Text.Json;

namespace SahinSoft.DesktopShell;

/// <summary>
/// Şubenin masaüstü kabuğunun hangi adrese baktığını belirler - yerel kurulum,
/// "merkez" olarak çalışan bir PC, ya da gerçek bulut sunucusu, hepsi burada
/// sadece bir URL değişikliğidir. shell.config.json exe'nin yanında durur,
/// yeniden derleme gerekmeden Ayarlar penceresinden değiştirilebilir.
/// </summary>
public sealed class ShellConfig
{
    // Masaüstü kabuk = "restorant programı" (Edip, 2026-09-02: "restorant programi masaustu
    // program gibi acılsın exe olsun") - doğrudan restoran modülüne açılır, site köküne değil.
    // Muhasebe tarafı normal tarayıcıdan açılır, bu exe'yle hiç ilgisi yok.
    public string Url { get; set; } = "http://localhost:1666/RestaurantDashboard";
    public string Title { get; set; } = "ŞahinSoft Restoran";

    private static string ConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shell.config.json");

    public static ShellConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<ShellConfig>(json);
                if (config is not null)
                {
                    return config;
                }
            }
        }
        catch
        {
            // Bozuk/okunamayan config dosyası olursa varsayılanla devam et.
        }

        return new ShellConfig();
    }

    public void Save()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, options));
    }
}
