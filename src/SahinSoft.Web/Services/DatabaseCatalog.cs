using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace SahinSoft.Web.Services;

// Kayıtlı veritabanı (firma) profilleri - ConfigTool (SahinSoftVeritabaniAyarlari.exe) ile AYNI dosya
// (%ProgramData%\SahinSoft\baglanti-profilleri.json). Giriş ekranı bu listeden seçim yaptırır.
public sealed class DatabaseProfile
{
    public string Name { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public bool WindowsAuth { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public string BuildConnectionString()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = Server,
            InitialCatalog = Database,
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        };
        if (WindowsAuth)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = UserId ?? string.Empty;
            builder.Password = Password ?? string.Empty;
        }
        return builder.ConnectionString;
    }
}

public sealed class DatabaseCatalog
{
    private static readonly string ProfilesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "SahinSoft", "baglanti-profilleri.json");

    public IReadOnlyList<DatabaseProfile> All()
    {
        try
        {
            if (!File.Exists(ProfilesPath))
            {
                return [];
            }
            var json = File.ReadAllText(ProfilesPath);
            return string.IsNullOrWhiteSpace(json)
                ? []
                : JsonSerializer.Deserialize<List<DatabaseProfile>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public DatabaseProfile? Find(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        return All().FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }

    public DatabaseProfile? Default()
    {
        var all = All();
        return all.FirstOrDefault(x => x.IsDefault) ?? all.FirstOrDefault();
    }
}

public sealed class PrepareDatabaseRequest
{
    public string? Profile { get; set; }
}
