using System.Text.Json;

namespace SahinSoft.Web.Services;

/// <summary>
/// Şubenin merkezden son ne zaman veri çektiğini yerelde saklar (App_Data/sync-state.json).
/// Ayrı bir DB tablosu değil - bu tamamen operasyonel/yerel bir durum, merkeze senkron
/// edilmesi gereken bir iş verisi değil.
/// </summary>
public sealed class BranchSyncState
{
    public DateTime? LastCatalogSyncUtc { get; set; }

    private static string StatePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "sync-state.json");

    public static BranchSyncState Load()
    {
        try
        {
            if (File.Exists(StatePath))
            {
                var json = File.ReadAllText(StatePath);
                var state = JsonSerializer.Deserialize<BranchSyncState>(json);
                if (state is not null)
                {
                    return state;
                }
            }
        }
        catch
        {
            // Bozuk dosya olursa sıfırdan başla (bir sonraki sync tüm veriyi çeker, zararsız).
        }

        return new BranchSyncState();
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(StatePath)!;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(StatePath, JsonSerializer.Serialize(this));
    }
}
