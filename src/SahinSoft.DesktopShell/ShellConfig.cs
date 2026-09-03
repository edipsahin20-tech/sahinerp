using System.Text.Json;

namespace SahinSoft.DesktopShell;

/// <summary>
/// Şubenin masaüstü kabuğunun hangi adrese baktığını belirler. Edip'in isteği
/// (2026-09-03: "ayrı ayrı klasörler olmasın, ayarları aynı yerden alsın") üzerine
/// kendi ayrı shell.config.json'ı KALDIRILDI - artık web uygulamasıyla AYNI
/// appsettings.json'daki "RestaurantShell" bölümünden okunur (SahinSoft.exe
/// kurulumda C:\SitesSahinSoft\Araclar\MasaustuUygulama\'da durur, appsettings.json
/// ise iki üst dizinde, C:\SitesSahinSoft\'ta - tek kaynak, ayrı bir ayar dosyası yok).
/// </summary>
public sealed class ShellConfig
{
    public string Url { get; set; } = "http://localhost:1666/RestaurantDashboard";
    public string Title { get; set; } = "ŞahinSoft Restoran";

    // MasaustuUygulama/SahinSoft.exe -> Araclar -> SitesSahinSoft köküne çık, orada duran
    // appsettings.json'ı oku. Aynı klasörde de dursun diye önce kendi dizinine de bakılır
    // (yerel geliştirme/test senaryosu - shell exe web app ile aynı klasöre kopyalanmışsa).
    private static IEnumerable<string> CandidatePaths()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        yield return Path.Combine(baseDir, "appsettings.json");
        yield return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "appsettings.json"));
    }

    public static ShellConfig Load()
    {
        foreach (var path in CandidatePaths())
        {
            try
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                if (!doc.RootElement.TryGetProperty("RestaurantShell", out var section))
                {
                    continue;
                }

                var config = new ShellConfig();
                if (section.TryGetProperty("Url", out var urlProp) && urlProp.GetString() is { Length: > 0 } url)
                {
                    config.Url = url;
                }
                if (section.TryGetProperty("Title", out var titleProp) && titleProp.GetString() is { Length: > 0 } title)
                {
                    config.Title = title;
                }
                return config;
            }
            catch
            {
                // Bozuk/okunamayan appsettings.json olursa bir sonraki adayı dene, hepsi
                // başarısız olursa varsayılanla devam et.
            }
        }

        return new ShellConfig();
    }
}
