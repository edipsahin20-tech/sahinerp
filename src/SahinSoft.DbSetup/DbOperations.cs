using System.Reflection;
using Microsoft.Data.SqlClient;

namespace SahinSoft.DbSetup;

// Program.cs (Kurulum.ps1'in sessizce çağırdığı konsol modu) ve MainForm.cs (Edip'in elle
// çalıştırdığı GUI modu) AYNI mantığı kullanıyor - kod burada tek yerde, ikisi de kendi
// log çıktısını (Console.WriteLine veya bir TextBox) bir callback ile alıyor.
internal static class DbOperations
{
    // SQL Server'ın kendi varsayılan sistem klasörü yerine burada, uygulamanın yanında dursun -
    // yedekleme/bulma/taşıma çok daha kolay olur. Sadece yerel (localhost) kurulumda anlamlı.
    private const string DataDirectory = @"C:\SitesSahinSoft\VeritabaniData";

    public static void DropDatabaseIfExists(string server, string user, string password, string databaseName, Action<string> log)
    {
        log($"==> '{databaseName}' veritabanı (varsa) siliniyor...");
        var masterConnStr = BuildConnectionString(server, "master", user, password);
        using var connection = new SqlConnection(masterConnStr);
        connection.Open();
        using var command = new SqlCommand(
            $"IF DB_ID('{databaseName}') IS NOT NULL BEGIN ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}]; END",
            connection);
        command.ExecuteNonQuery();
        log("    [OK] Silindi.");
    }

    // "Güncelle" akışında kullanılır: veritabanı zaten varsa DOKUNMAZ (mevcut veriyi korur),
    // yoksa sıfırdan oluşturur. Return değeri: true = yeni oluşturuldu, false = zaten vardı.
    public static bool CreateDatabaseIfNotExists(string server, string user, string password, string databaseName, Action<string> log)
    {
        var masterConnStr = BuildConnectionString(server, "master", user, password);
        using var connection = new SqlConnection(masterConnStr);
        connection.Open();

        using (var checkCommand = new SqlCommand($"SELECT DB_ID('{databaseName}')", connection))
        {
            if (checkCommand.ExecuteScalar() is not DBNull and not null)
            {
                log($"    '{databaseName}' zaten mevcut, olduğu gibi korunuyor.");
                return false;
            }
        }

        log($"==> '{databaseName}' veritabanı oluşturuluyor...");
        var isLocal = server.Equals("localhost", StringComparison.OrdinalIgnoreCase) || server == ".";
        string createSql;
        if (isLocal)
        {
            Directory.CreateDirectory(DataDirectory);
            GrantSqlServiceAccess(DataDirectory);
            var mdfPath = Path.Combine(DataDirectory, $"{databaseName}.mdf");
            var ldfPath = Path.Combine(DataDirectory, $"{databaseName}_log.ldf");
            createSql = $"""
                CREATE DATABASE [{databaseName}]
                ON PRIMARY (NAME = N'{databaseName}', FILENAME = N'{mdfPath}')
                LOG ON (NAME = N'{databaseName}_log', FILENAME = N'{ldfPath}')
                """;
        }
        else
        {
            createSql = $"CREATE DATABASE [{databaseName}]";
        }

        using var command = new SqlCommand(createSql, connection);
        command.ExecuteNonQuery();
        log(isLocal ? $"    [OK] Oluşturuldu ({DataDirectory})." : "    [OK] Oluşturuldu.");
        return true;
    }

    // SQL Server servisi kendi varsayılan veri klasörü dışındaki bir klasöre yazmak için o
    // klasörde açık izin ister - CREATE DATABASE burada "Access is denied" ile başarısız
    // olmasın diye önceden veriyoruz.
    private static void GrantSqlServiceAccess(string directory)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "icacls.exe",
                Arguments = $"\"{directory}\" /grant \"NT SERVICE\\MSSQLSERVER\":(OI)(CI)F /grant \"*S-1-5-18\":(OI)(CI)F",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var process = System.Diagnostics.Process.Start(psi);
            process?.WaitForExit(10000);
        }
        catch
        {
            // İzin verilemezse CREATE DATABASE zaten anlamlı bir hatayla başarısız olur.
        }
    }

    // Script `dotnet ef migrations script --idempotent` ile üretildi - her batch başında zaten
    // uygulanmış migration'ları IF NOT EXISTS ile atlıyor. Bu yüzden hem SIFIRDAN bir veritabanında
    // hem de bazı migration'ları zaten uygulanmış EN GÜNCEL OLMAYAN bir veritabanında (Güncelle
    // akışı) güvenle çalıştırılabilir - ikisi de aynı script, davranış farkı script'in kendi
    // idempotent kontrolünden geliyor.
    public static void RunMigration(string server, string user, string password, string databaseName, Action<string> log)
    {
        log("==> Migration script'i uygulanıyor...");
        var script = ReadEmbeddedScript();
        var batches = SplitIntoBatches(script);
        log($"    Toplam {batches.Count} batch bulundu.");

        var dbConnStr = BuildConnectionString(server, databaseName, user, password);
        using var connection = new SqlConnection(dbConnStr);
        connection.Open();

        for (var i = 0; i < batches.Count; i++)
        {
            var batch = batches[i];
            if (string.IsNullOrWhiteSpace(batch))
            {
                continue;
            }

            try
            {
                using var command = new SqlCommand(batch, connection) { CommandTimeout = 120 };
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Batch {i + 1}/{batches.Count} başarısız: {ex.Message}\n\nBatch içeriği (ilk 300 karakter):\n{batch[..Math.Min(300, batch.Length)]}", ex);
            }

            if ((i + 1) % 25 == 0 || i == batches.Count - 1)
            {
                log($"    ... {i + 1}/{batches.Count} batch tamamlandı");
            }
        }

        log("    [OK] Migration tamamlandı.");
    }

    // Edip'in isteği: "Güncelle" öncesi Emergency Mode / Suspect / Recovery Pending gibi bozulma
    // durumlarını otomatik onarsın, veritabanı sağlıklıysa hiç dokunmasın ("yoksa ellemesin").
    // Bu üç durum sys.databases.state_desc üzerinden görülebilir; standart SQL Server kurtarma
    // sırası: EMERGENCY moda al -> SINGLE_USER -> CHECKDB REPAIR_ALLOW_DATA_LOSS -> MULTI_USER.
    // master'a bağlanıyoruz çünkü SUSPECT/EMERGENCY durumundaki bir veritabanına normal
    // bağlantı zaten açılamaz.
    private static readonly string[] CorruptStates = ["SUSPECT", "EMERGENCY", "RECOVERY_PENDING"];

    public static void RepairIfCorrupt(string server, string user, string password, string databaseName, Action<string> log)
    {
        var masterConnStr = BuildConnectionString(server, "master", user, password);
        using var connection = new SqlConnection(masterConnStr);
        connection.Open();

        string? state;
        using (var checkCommand = new SqlCommand("SELECT state_desc FROM sys.databases WHERE name = @name", connection))
        {
            checkCommand.Parameters.AddWithValue("@name", databaseName);
            state = checkCommand.ExecuteScalar() as string;
        }

        if (state is null || !CorruptStates.Contains(state, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        log($"==> '{databaseName}' bozuk durumda tespit edildi ({state}) - onarılıyor...");
        var steps = new (string Sql, string Description)[]
        {
            ($"ALTER DATABASE [{databaseName}] SET EMERGENCY", "EMERGENCY moda alındı"),
            ($"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", "SINGLE_USER moda alındı"),
            ($"DBCC CHECKDB(N'{databaseName}', REPAIR_ALLOW_DATA_LOSS)", "CHECKDB onarımı çalıştırıldı"),
            ($"ALTER DATABASE [{databaseName}] SET MULTI_USER", "MULTI_USER moda geri alındı")
        };

        foreach (var (sql, description) in steps)
        {
            using var command = new SqlCommand(sql, connection) { CommandTimeout = 600 };
            command.ExecuteNonQuery();
            log($"    [OK] {description}.");
        }

        log($"    [OK] Onarım tamamlandı - bazı veriler kaybolmuş olabilir (REPAIR_ALLOW_DATA_LOSS), devam ediliyor.");
    }

    public static void ShrinkDatabase(string server, string user, string password, string databaseName, Action<string> log)
    {
        log("==> Veritabanı küçültülüyor (shrink)...");
        var connStr = BuildConnectionString(server, databaseName, user, password);
        using var connection = new SqlConnection(connStr);
        connection.Open();
        using var command = new SqlCommand($"DBCC SHRINKDATABASE(N'{databaseName}')", connection) { CommandTimeout = 300 };
        command.ExecuteNonQuery();
        log("    [OK] Küçültme tamamlandı.");
    }

    // "Hareketleri Sil": tanım/kart verilerini (stok, kategori, cari, şube, depo, KDV, ayarlar,
    // mutfak istasyonu) OLDUĞU GİBİ bırakıp yalnızca işlem/hareket tablolarını boşaltır - test
    // verisiyle uğraşmış bir kurulumu, yeniden kurmadan "sıfır hareketli" hale getirmek için.
    // FK'ları tabloları elle bağımlılık sırasına dizmek yerine (şema büyüdükçe kırılgan olur)
    // geçici olarak NOCHECK yapıp sonda tekrar doğruluyoruz.
    private static readonly string[] TransactionalTables =
    [
        "StockMovements", "Quotes", "QuoteLines", "AuditLogs",
        "CurrentAccountTransactions", "FinancialTransactions",
        "Invoices", "InvoiceLines", "PaymentReceipts", "PaymentReceiptLines",
        "StockTransfers", "StockTransferLines", "ProductSerialNumbers", "StockReservations",
        "InventoryCounts", "InventoryCountLines", "ExternalRecordMappings", "IntegrationOutboxMessages",
        "BusinessOrders", "BusinessOrderLines", "DispatchNotes", "DispatchNoteLines",
        "Expenses", "NegotiableInstruments", "InvoicePaymentSchedules",
        "StockSlips", "StockSlipLines",
        "RestaurantTableSessions", "RestaurantTableSessionMoves", "RestaurantChecks",
        "RestaurantOrders", "RestaurantOrderLines", "RestaurantOrderLineModifiers",
        "KitchenTickets", "KitchenTicketLines", "RestaurantPayments", "RestaurantCashShifts",
        "RetailSales", "RetailSaleLines"
    ];

    public static void ClearTransactionalData(string server, string user, string password, string databaseName, Action<string> log)
    {
        log("==> Hareketler (işlem/movement kayıtları) temizleniyor - stok/cari/tanım kartları korunuyor...");
        var connStr = BuildConnectionString(server, databaseName, user, password);
        using var connection = new SqlConnection(connStr);
        connection.Open();

        using (var disableFk = new SqlCommand("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'", connection) { CommandTimeout = 120 })
        {
            disableFk.ExecuteNonQuery();
        }

        foreach (var table in TransactionalTables)
        {
            using var checkCommand = new SqlCommand("SELECT OBJECT_ID(@name)", connection);
            checkCommand.Parameters.AddWithValue("@name", $"[dbo].[{table}]");
            if (checkCommand.ExecuteScalar() is null or DBNull)
            {
                continue;
            }

            using var deleteCommand = new SqlCommand($"DELETE FROM [dbo].[{table}]", connection) { CommandTimeout = 120 };
            var affected = deleteCommand.ExecuteNonQuery();

            using var reseedCommand = new SqlCommand(
                $"IF OBJECTPROPERTY(OBJECT_ID(N'[dbo].[{table}]'), 'TableHasIdentity') = 1 DBCC CHECKIDENT('[dbo].[{table}]', RESEED, 0)",
                connection);
            reseedCommand.ExecuteNonQuery();

            log($"    {table}: {affected} kayıt silindi.");
        }

        using (var enableFk = new SqlCommand("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'", connection) { CommandTimeout = 300 })
        {
            enableFk.ExecuteNonQuery();
        }

        using (var resetSequences = new SqlCommand(
            "IF OBJECT_ID(N'[dbo].[NumberSequences]') IS NOT NULL UPDATE [dbo].[NumberSequences] SET [NextNumber] = 1",
            connection))
        {
            resetSequences.ExecuteNonQuery();
        }

        log("    [OK] Hareketler temizlendi, numaratörler 1'e sıfırlandı, tanım/kart verileri korundu.");
    }

    public static void BackupDatabase(string server, string user, string password, string databaseName, string backupFilePath, Action<string> log)
    {
        log($"==> '{databaseName}' veritabanı yedekleniyor...");
        Directory.CreateDirectory(Path.GetDirectoryName(backupFilePath)!);

        var masterConnStr = BuildConnectionString(server, "master", user, password);
        using var connection = new SqlConnection(masterConnStr);
        connection.Open();
        using var command = new SqlCommand(
            $"BACKUP DATABASE [{databaseName}] TO DISK = @path WITH INIT, STATS = 10",
            connection) { CommandTimeout = 600 };
        command.Parameters.AddWithValue("@path", backupFilePath);
        command.ExecuteNonQuery();
        log($"    [OK] Yedek alındı: {backupFilePath}");
    }

    // Veritabanı sıfırdan kurulduktan/güncellendikten sonra IIS'te ZATEN ÇALIŞAN uygulama havuzu
    // hâlâ eski bağlantıları/durumu tutuyor olabilir. appcmd.exe IIS ile birlikte gelir, ekstra
    // modül/admin hakkı gerektirmeden havuzu resetler. IIS kurulu değilse veya havuz yoksa
    // (ör. bu makine henüz IIS kurulumu yapılmamış bir geliştirme makinesiyse) sessizce atlanır.
    public static void RecycleIisAppPool(Action<string> log, string appPoolName = "SahinSoft")
    {
        var appCmdPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "System32", "inetsrv", "appcmd.exe");

        if (!File.Exists(appCmdPath))
        {
            return;
        }

        log("==> IIS uygulama havuzu yeniden başlatılıyor...");
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = appCmdPath,
                Arguments = $"recycle apppool /apppool.name:\"{appPoolName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var process = System.Diagnostics.Process.Start(psi);
            process?.WaitForExit(15000);
            log(process?.ExitCode == 0
                ? "    [OK] Uygulama havuzu yeniden başlatıldı."
                : "    [UYARI] Uygulama havuzu bulunamadı/başlatılamadı - IIS Yöneticisi'nden havuzu elle Recycle edin.");
        }
        catch (Exception ex)
        {
            log($"    [UYARI] Uygulama havuzu yeniden başlatılamadı: {ex.Message}");
        }
    }

    public static string BuildConnectionString(string server, string database, string user, string password) =>
        $"Server={server};Database={database};User Id={user};Password={password};TrustServerCertificate=True;Encrypt=False;Connection Timeout=15";

    private static string ReadEmbeddedScript()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().Single(x => x.EndsWith("SahinSoftDb_Migration.sql", StringComparison.OrdinalIgnoreCase));
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        return reader.ReadToEnd();
    }

    // sqlcmd/SSMS'in "GO" satır ayracı gerçek T-SQL değildir - istemci tarafında script'i
    // parçalara bölmek için kullanılan bir kural. Burada aynısını elle yapıyoruz.
    private static List<string> SplitIntoBatches(string script)
    {
        var lines = script.Split('\n');
        var batches = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r');
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                batches.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.AppendLine(line);
            }
        }

        if (current.Length > 0)
        {
            batches.Add(current.ToString());
        }

        return batches;
    }
}
