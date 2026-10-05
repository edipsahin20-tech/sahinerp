using System.Text.Json.Nodes;
using SahinSoft.ConfigTool.Models;

namespace SahinSoft.ConfigTool.Services;

// Edip, 2026-10-02: "Merkez Bağlantı Ayarı" - web'deki eski "Bağlantı Modu" sayfasının Merkez Sync
// (HTTP+ApiKey tabanlı outbox senkronu) alanları KALDIRILMIŞTI (artık hiçbir yerden okunmuyor,
// yazılmıyor - bkz. RestaurantTerminalSettingsController). Bu ekran "MerkezSync" anahtarını YENİDEN
// TANIMLAR - artık HTTP API değil, DOĞRUDAN bir Merkez SQL bağlantısı (Sunucu/Port/Veritabanı/
// Kimlik/Şube bilgisi) saklar. appsettings.Local.json'daki AYNI "MerkezSync" anahtarı kullanılır,
// eski alanlar (BaseUrl/ApiKey/PollIntervalSeconds) artık hiçbir kod tarafından okunmuyor.
public static class CentralConnectionStore
{
    public static CentralConnectionSettings Load()
    {
        var root = ProfileStore.ReadRoot(ProfileStore.LocalOverridePath);
        var settings = new CentralConnectionSettings { Port = 1433 };

        if (root["MerkezSync"] is not JsonObject merkez)
        {
            return settings;
        }

        settings.Enabled = merkez["Enabled"]?.GetValue<bool>() ?? false;
        settings.Server = merkez["Server"]?.GetValue<string>() ?? string.Empty;
        settings.Port = merkez["Port"]?.GetValue<int>() ?? 1433;
        settings.Database = merkez["Database"]?.GetValue<string>() ?? string.Empty;
        settings.WindowsAuth = merkez["WindowsAuth"]?.GetValue<bool>() ?? false;
        settings.UserId = merkez["UserId"]?.GetValue<string>() ?? string.Empty;
        settings.Password = merkez["Password"]?.GetValue<string>() ?? string.Empty;
        settings.BranchCode = merkez["BranchCode"]?.GetValue<string>() ?? string.Empty;
        settings.BranchName = merkez["BranchName"]?.GetValue<string>() ?? string.Empty;
        return settings;
    }

    public static void Save(CentralConnectionSettings settings)
    {
        var root = ProfileStore.ReadRoot(ProfileStore.LocalOverridePath);
        root["MerkezSync"] = new JsonObject
        {
            ["Enabled"] = settings.Enabled,
            ["Server"] = settings.Server,
            ["Port"] = settings.Port,
            ["Database"] = settings.Database,
            ["WindowsAuth"] = settings.WindowsAuth,
            ["UserId"] = settings.UserId,
            ["Password"] = settings.Password,
            ["BranchCode"] = settings.BranchCode,
            ["BranchName"] = settings.BranchName
        };
        ProfileStore.WriteRoot(ProfileStore.LocalOverridePath, root);
    }

    public static string BuildConnectionString(CentralConnectionSettings settings)
    {
        var dataSource = settings.Port is > 0 and not 1433
            ? $"{settings.Server},{settings.Port}"
            : settings.Server;

        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
        {
            DataSource = dataSource,
            InitialCatalog = settings.Database,
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        };
        if (settings.WindowsAuth)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = settings.UserId ?? string.Empty;
            builder.Password = settings.Password ?? string.Empty;
        }
        return builder.ConnectionString;
    }
}
