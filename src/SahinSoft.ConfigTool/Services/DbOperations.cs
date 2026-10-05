using System.IO;
using Microsoft.Data.SqlClient;

namespace SahinSoft.ConfigTool.Services;

// Bu sınıfın mantığı SahinSoft.DbSetup/DbOperations.cs'den BİREBİR taşındı (2026-10-02) - tehlikeli
// işlemlerin (Hareketleri Sil / Veritabanını Temizle) tablo listeleri orada ApplicationDbContext'in
// TÜM DbSet'leriyle elle karşılaştırılıp 2026-09-28'de bir birleştirme sırasında düzeltilmişti
// (eski bir araç restoran tablolarını hiç silmiyordu - gerçek bir hataydı). Burada YENİDEN İCAT
// EDİLMEDİ, aynen kopyalandı - yeni bir tablo eklenirse HER İKİ kopyayı da (burayı ve
// SahinSoft.DbSetup/DbOperations.cs'i) güncelleyin, otomatik senkron değil.
internal static class DbOperations
{
    public static bool TestConnection(string connectionString, out string message)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            message = "Bağlantı başarılı.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Bağlantı başarısız: {ex.Message}";
            return false;
        }
    }

    public static List<string> ListDatabases(string masterConnectionString)
    {
        var result = new List<string>();
        using var connection = new SqlConnection(masterConnectionString);
        connection.Open();
        using var command = new SqlCommand("SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(reader.GetString(0));
        }
        return result;
    }

    public static bool DatabaseExists(string masterConnectionString, string databaseName)
    {
        using var connection = new SqlConnection(masterConnectionString);
        connection.Open();
        using var command = new SqlCommand("SELECT 1 FROM sys.databases WHERE name = @name", connection);
        command.Parameters.AddWithValue("@name", databaseName);
        return command.ExecuteScalar() is not null;
    }

    public static void CreateDatabase(string masterConnectionString, string databaseName)
    {
        using var connection = new SqlConnection(masterConnectionString);
        connection.Open();
        using var command = new SqlCommand($"CREATE DATABASE [{databaseName.Replace("]", "]]")}]", connection) { CommandTimeout = 120 };
        command.ExecuteNonQuery();
    }

    public static void DropDatabase(string masterConnectionString, string databaseName)
    {
        using var connection = new SqlConnection(masterConnectionString);
        connection.Open();
        var escaped = databaseName.Replace("]", "]]");
        using var command = new SqlCommand(
            $"IF DB_ID(@name) IS NOT NULL BEGIN ALTER DATABASE [{escaped}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{escaped}]; END",
            connection) { CommandTimeout = 300 };
        command.Parameters.AddWithValue("@name", databaseName);
        command.ExecuteNonQuery();
    }

    public static void BackupDatabase(string masterConnectionString, string databaseName, string backupFilePath, Action<string> log)
    {
        log($"==> '{databaseName}' veritabanı yedekleniyor...");
        var dir = Path.GetDirectoryName(backupFilePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        using var connection = new SqlConnection(masterConnectionString);
        connection.Open();
        using var command = new SqlCommand("BACKUP DATABASE [" + databaseName.Replace("]", "]]") + "] TO DISK = @path WITH INIT, STATS = 10", connection) { CommandTimeout = 600 };
        command.Parameters.AddWithValue("@path", backupFilePath);
        command.ExecuteNonQuery();
        log($"    [OK] Yedek alındı: {backupFilePath}");
    }

    public static string? TryDetectBackupDirectory(string masterConnectionString)
    {
        try
        {
            using var connection = new SqlConnection(masterConnectionString);
            connection.Open();
            using var command = new SqlCommand(
                "DECLARE @path NVARCHAR(500); " +
                "EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\\Microsoft\\MSSQLServer\\MSSQLServer', N'BackupDirectory', @path OUTPUT; " +
                "SELECT @path;", connection);
            return command.ExecuteScalar() as string;
        }
        catch
        {
            return null;
        }
    }

    public static void CheckIntegrity(string connectionString, string databaseName, Action<string> log)
    {
        log("==> Veri bütünlüğü denetimi (DBCC CHECKDB) çalışıyor, bu biraz sürebilir...");
        using var connection = new SqlConnection(connectionString);
        connection.InfoMessage += (_, args) => log(args.Message);
        connection.Open();
        using var command = new SqlCommand($"DBCC CHECKDB ([{databaseName}]) WITH NO_INFOMSGS, ALL_ERRORMSGS;", connection) { CommandTimeout = 0 };
        command.ExecuteNonQuery();
        log("    [OK] Denetim tamamlandı. Yukarıda hata mesajı yoksa veritabanı sağlıklıdır (otomatik onarım YAPILMAZ).");
    }

    public static void CleanupTransactionLog(string connectionString, string databaseName, Action<string> log)
    {
        log("==> Log dosyası temizleniyor...");
        using var connection = new SqlConnection(connectionString);
        connection.InfoMessage += (_, args) => log(args.Message);
        connection.Open();

        using var modelCmd = new SqlCommand("SELECT recovery_model_desc FROM sys.databases WHERE name = @name", connection);
        modelCmd.Parameters.AddWithValue("@name", databaseName);
        var originalModel = (string?)modelCmd.ExecuteScalar() ?? "SIMPLE";

        using var logFileCmd = new SqlCommand("SELECT name FROM sys.master_files WHERE database_id = DB_ID(@name) AND type_desc = 'LOG'", connection);
        logFileCmd.Parameters.AddWithValue("@name", databaseName);
        var logFileName = (string?)logFileCmd.ExecuteScalar();
        if (logFileName is null)
        {
            log("    Log dosyası bulunamadı, atlanıyor.");
            return;
        }

        if (!string.Equals(originalModel, "SIMPLE", StringComparison.OrdinalIgnoreCase))
        {
            log($"    Kurtarma modeli geçici olarak SIMPLE yapılıyor (öncekiydi: {originalModel})...");
            using var setSimple = new SqlCommand($"ALTER DATABASE [{databaseName}] SET RECOVERY SIMPLE;", connection) { CommandTimeout = 0 };
            setSimple.ExecuteNonQuery();
        }

        using (var checkpoint = new SqlCommand("CHECKPOINT;", connection) { CommandTimeout = 0 })
        {
            checkpoint.ExecuteNonQuery();
        }

        using (var shrinkLog = new SqlCommand($"DBCC SHRINKFILE(N'{logFileName}', 1);", connection) { CommandTimeout = 0 })
        {
            shrinkLog.ExecuteNonQuery();
        }
        log($"    [OK] Log dosyası ({logFileName}) küçültüldü.");

        if (!string.Equals(originalModel, "SIMPLE", StringComparison.OrdinalIgnoreCase))
        {
            log($"    Kurtarma modeli eski haline döndürülüyor ({originalModel})...");
            using var restoreModel = new SqlCommand($"ALTER DATABASE [{databaseName}] SET RECOVERY {originalModel};", connection) { CommandTimeout = 0 };
            restoreModel.ExecuteNonQuery();
        }
    }

    public static void RunIndexMaintenance(string connectionString, Action<string> log)
    {
        log("==> Parçalanmış indexler taranıyor...");
        using var connection = new SqlConnection(connectionString);
        connection.InfoMessage += (_, args) => log(args.Message);
        connection.Open();

        var targets = new List<(string Schema, string Table, string Index, double Fragmentation)>();
        using (var scanCmd = new SqlCommand(
            "SELECT s.name AS SchemaName, t.name AS TableName, i.name AS IndexName, ps.avg_fragmentation_in_percent " +
            "FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps " +
            "JOIN sys.indexes i ON ps.object_id = i.object_id AND ps.index_id = i.index_id " +
            "JOIN sys.tables t ON t.object_id = i.object_id " +
            "JOIN sys.schemas s ON s.schema_id = t.schema_id " +
            "WHERE i.name IS NOT NULL AND ps.avg_fragmentation_in_percent > 5 AND ps.page_count > 50;", connection)
        { CommandTimeout = 0 })
        {
            using var reader = scanCmd.ExecuteReader();
            while (reader.Read())
            {
                targets.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetDouble(3)));
            }
        }

        if (targets.Count == 0)
        {
            log("    Parçalanmış (bakım gerektiren) index bulunamadı.");
            return;
        }

        foreach (var (schema, table, index, fragmentation) in targets)
        {
            var action = fragmentation >= 30 ? "REBUILD" : "REORGANIZE";
            using var cmd = new SqlCommand($"ALTER INDEX [{index}] ON [{schema}].[{table}] {action};", connection) { CommandTimeout = 0 };
            cmd.ExecuteNonQuery();
            log($"    {table}.{index}: %{fragmentation:N1} parçalanma -> {action} tamamlandı.");
        }

        log($"    [OK] Index bakımı tamamlandı ({targets.Count} index işlendi).");
    }

    public static void ShrinkDatabase(string connectionString, string databaseName, Action<string> log)
    {
        log("==> Veritabanı küçültülüyor (shrink)...");
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand($"DBCC SHRINKDATABASE(N'{databaseName}')", connection) { CommandTimeout = 300 };
        command.ExecuteNonQuery();
        log("    [OK] Küçültme tamamlandı.");
    }

    // "Hareketleri Sil": tanım/kart verilerini (stok, kategori, cari, şube, depo, KDV, ayarlar,
    // mutfak istasyonu, yazıcı tanımları) OLDUĞU GİBİ bırakıp yalnızca işlem/hareket tablolarını
    // boşaltır. SahinSoft.DbSetup/DbOperations.cs'deki MovementOnlyTables ile BİREBİR AYNI liste.
    internal static readonly string[] MovementOnlyTables =
    [
        "AuditLogs", "BusinessOrderLines", "BusinessOrders", "CurrentAccountTransactions",
        "DispatchNoteLines", "DispatchNotes", "ExchangeRates", "Expenses",
        "ExternalRecordMappings", "FinancialTransactions", "IntegrationOutboxMessages",
        "InventoryCountLines", "InventoryCounts", "InvoiceLines", "InvoicePaymentSchedules",
        "Invoices", "KitchenTicketLines", "KitchenTickets", "NegotiableInstruments",
        "PackageOrders", "PaymentReceiptLines", "PaymentReceipts", "PrintJobs",
        "PurchasePriceListItems", "PurchasePriceLists", "QuoteLines", "Quotes",
        "RestaurantCashShifts", "RestaurantCheckPendingPayments", "RestaurantChecks",
        "RestaurantOrderLineModifiers", "RestaurantOrderLines", "RestaurantOrders",
        "RestaurantPayments", "RestaurantPermissionAuditLogs", "RestaurantTableReservations",
        "RestaurantTableSessionMoves", "RestaurantTableSessions", "RestaurantZPeriods",
        "RetailSaleLines", "RetailSales", "SalesPriceListItems", "SalesPriceLists",
        "StockMovements", "StockReservations", "StockSlipLines", "StockSlips",
        "StockTransferLines", "StockTransfers"
    ];

    // "Veritabanını Temizle" (Hareket+Cari+Stok): yukarıdaki listeye EK olarak cari ve stok
    // KARTLARININ kendisi de silinir. SahinSoft.DbSetup/DbOperations.cs'deki FullWipeExtraTables
    // ile BİREBİR AYNI.
    internal static readonly string[] FullWipeExtraTables =
    [
        "ProductUnitConversions", "ProductSerialNumbers", "ProductImages", "ProductBarcodes",
        "ProductVariants", "ScaleProductSettings", "ProductPortions", "ProductRecipeLines",
        "ProductRecipeHeaders", "Products",
        "CustomerAddresses", "CustomerContacts", "Customers"
    ];

    public static void ClearTransactionalData(string connectionString, Action<string> log) =>
        RunWipe(connectionString, MovementOnlyTables, log,
            "Hareketler (işlem/movement kayıtları) temizleniyor - stok/cari/tanım kartları korunuyor...",
            "Hareketler temizlendi, numaratörler 1'e sıfırlandı, tanım/kart verileri korundu.");

    public static void ClearFullData(string connectionString, Action<string> log) =>
        RunWipe(connectionString, [.. MovementOnlyTables, .. FullWipeExtraTables], log,
            "Hareket + Cari + Stok tam temizleme başlıyor - TÜM cari/stok kartları da silinecek...",
            "Tam temizleme tamamlandı: hareketler + cari/stok kartları silindi, numaratörler 1'e sıfırlandı, program ayarları korundu.");

    private static void RunWipe(string connectionString, string[] tables, Action<string> log, string startMessage, string doneMessage)
    {
        log($"==> {startMessage}");
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            using (var disableFk = new SqlCommand("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'", connection, transaction) { CommandTimeout = 120 })
            {
                disableFk.ExecuteNonQuery();
            }

            foreach (var table in tables)
            {
                using var checkCommand = new SqlCommand("SELECT OBJECT_ID(@name)", connection, transaction);
                checkCommand.Parameters.AddWithValue("@name", $"[dbo].[{table}]");
                if (checkCommand.ExecuteScalar() is null or DBNull)
                {
                    continue;
                }

                using var deleteCommand = new SqlCommand($"DELETE FROM [dbo].[{table}]", connection, transaction) { CommandTimeout = 120 };
                var affected = deleteCommand.ExecuteNonQuery();

                using var reseedCommand = new SqlCommand(
                    $"IF OBJECTPROPERTY(OBJECT_ID(N'[dbo].[{table}]'), 'TableHasIdentity') = 1 DBCC CHECKIDENT('[dbo].[{table}]', RESEED, 0)",
                    connection, transaction);
                reseedCommand.ExecuteNonQuery();

                log($"    {table}: {affected} kayıt silindi.");
            }

            using (var enableFk = new SqlCommand("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'", connection, transaction) { CommandTimeout = 300 })
            {
                enableFk.ExecuteNonQuery();
            }

            // Hareketler silindiyse ürün kartındaki stok bakiyesi de sıfırlanmalı - bakiye hareketlerden
            // türetilmiyor, kartta saklanıyor (DbSetup'taki resetProductQuantities ile aynı davranış).
            if (tables.Contains("StockMovements", StringComparer.OrdinalIgnoreCase))
            {
                using var resetQty = new SqlCommand("IF OBJECT_ID(N'[dbo].[Products]') IS NOT NULL UPDATE [dbo].[Products] SET [StockQuantity] = 0", connection, transaction) { CommandTimeout = 120 };
                resetQty.ExecuteNonQuery();
                log("    Products: stok bakiyeleri 0'landı.");
            }

            using (var resetSequences = new SqlCommand(
                "IF OBJECT_ID(N'[dbo].[NumberSequences]') IS NOT NULL UPDATE [dbo].[NumberSequences] SET [NextNumber] = 1",
                connection, transaction))
            {
                resetSequences.ExecuteNonQuery();
            }

            transaction.Commit();
            log($"    [OK] {doneMessage}");
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
