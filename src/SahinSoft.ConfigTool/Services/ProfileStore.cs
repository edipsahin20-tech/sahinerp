using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.SqlClient;
using SahinSoft.ConfigTool.Models;

namespace SahinSoft.ConfigTool.Services;

// Edip, 2026-10-02: "SahinSoftVeritabaniAyarlari ve [web'deki Bağlantı Modu ekranı] aynı yerden
// okusun" - web uygulamasının appsettings.json'dan SONRA okuduğu, makineye özel
// appsettings.Local.json (%ProgramData%\SahinSoft) dosyası HER ZAMAN KAZANIR - bu araç da AYNI
// dosyaya yazar, appsettings.json'a hiç dokunmaz.
public static class ProfileStore
{
    // Web uygulaması (giriş ekranındaki firma seçici) bu dosyayı IIS kimliğiyle okuduğu için
    // profil listesi kullanıcıya değil makineye ait (ProgramData) olmalı.
    private static readonly string ProfilesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "SahinSoft", "baglanti-profilleri.json");

    private static readonly string LegacyProfilesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SahinSoft", "baglanti-profilleri.json");

    public static readonly string LocalOverridePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "SahinSoft", "appsettings.Local.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static List<ConnectionProfile> LoadProfiles()
    {
        try
        {
            if (!File.Exists(ProfilesPath) && File.Exists(LegacyProfilesPath))
            {
                var dir = Path.GetDirectoryName(ProfilesPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.Copy(LegacyProfilesPath, ProfilesPath);
            }

            if (!File.Exists(ProfilesPath))
            {
                return [];
            }
            var json = File.ReadAllText(ProfilesPath);
            return string.IsNullOrWhiteSpace(json)
                ? []
                : JsonSerializer.Deserialize<List<ConnectionProfile>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static void SaveProfiles(List<ConnectionProfile> profiles)
    {
        var dir = Path.GetDirectoryName(ProfilesPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(ProfilesPath, JsonSerializer.Serialize(profiles, JsonOptions));
    }

    public static string BuildConnectionString(ConnectionProfile profile)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = profile.Server,
            InitialCatalog = profile.Database,
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        };
        if (profile.WindowsAuth)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = profile.UserId ?? string.Empty;
            builder.Password = profile.Password ?? string.Empty;
        }
        return builder.ConnectionString;
    }

    public static string BuildMasterConnectionString(ConnectionProfile profile)
    {
        var copy = new ConnectionProfile
        {
            Server = profile.Server,
            Database = "master",
            WindowsAuth = profile.WindowsAuth,
            UserId = profile.UserId,
            Password = profile.Password
        };
        return BuildConnectionString(copy);
    }

    // Seçili profili appsettings.Local.json'a ETKİNLEŞTİRİR - web'deki (artık kaldırılmış) Bağlantı
    // Modu sayfasıyla AYNI dosya/anahtarlar, bu yüzden "TerminalConnectionMode" alanı da yazılır.
    public static void ActivateProfile(ConnectionProfile profile)
    {
        var root = ReadRoot(LocalOverridePath);
        if (root["ConnectionStrings"] is not JsonObject connectionStrings)
        {
            connectionStrings = new JsonObject();
            root["ConnectionStrings"] = connectionStrings;
        }
        connectionStrings["DefaultConnection"] = BuildConnectionString(profile);
        root["TerminalConnectionMode"] = "Cloud";
        WriteRoot(LocalOverridePath, root);

        // Etkin veritabanı = varsayılan (giriş ekranında önceden seçili, masaüstünün kullandığı).
        var profiles = LoadProfiles();
        if (!profiles.Any(x => x.Name == profile.Name))
        {
            profiles.Add(profile);
        }
        foreach (var p in profiles)
        {
            p.IsDefault = p.Name == profile.Name;
        }
        SaveProfiles(profiles);
    }

    public static JsonObject ReadRoot(string path)
    {
        if (!File.Exists(path))
        {
            return new JsonObject();
        }
        var text = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(text))
        {
            return new JsonObject();
        }
        return JsonNode.Parse(text)?.AsObject() ?? new JsonObject();
    }

    public static void WriteRoot(string path, JsonObject root)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(path, root.ToJsonString(JsonOptions));
    }
}
