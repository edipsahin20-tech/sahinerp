using System.IO;
using System.Text.Json;
using SahinSoft.ConfigTool.Models;

namespace SahinSoft.ConfigTool.Services;

// "İşlem Geçmişi" sayfası - bu araçla yapılan her işlemi (bağlantı testi/etkinleştirme, veritabanı
// oluşturma, bakım, hareket silme/temizleme) basit bir yerel dosyaya kaydeder. Sunucu tarafında
// bir karşılığı yok - sadece bu makinede bu aracı kimin ne zaman çalıştırdığını izlemek için.
public static class OperationLogStore
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SahinSoft", "islem-gecmisi.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static List<OperationLogEntry> Load()
    {
        try
        {
            if (!File.Exists(LogPath))
            {
                return [];
            }
            var json = File.ReadAllText(LogPath);
            var entries = string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<List<OperationLogEntry>>(json) ?? [];
            return entries.OrderByDescending(x => x.AtUtc).ToList();
        }
        catch
        {
            return [];
        }
    }

    public static void Append(string operation, string database, bool success, string detail)
    {
        try
        {
            var entries = Load();
            entries.Insert(0, new OperationLogEntry
            {
                AtUtc = DateTime.UtcNow,
                Operation = operation,
                Database = database,
                Success = success,
                Detail = detail
            });

            // Dosya sınırsız büyümesin - son 200 işlem yeterli.
            if (entries.Count > 200)
            {
                entries = entries.Take(200).ToList();
            }

            var dir = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(LogPath, JsonSerializer.Serialize(entries, JsonOptions));
        }
        catch
        {
            // Geçmiş kaydı yazılamazsa sessizce geç - bu asıl işlemin başarısını etkilemez.
        }
    }
}
