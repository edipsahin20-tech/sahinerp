using Microsoft.Data.SqlClient;

namespace SahinSoft.DataResetTool;

public sealed class MainForm : Form
{
    private const string ConfirmPhrase = "SİL";

    // Tab 2 — SADECE hareketleri (belge + hareket geçmişini) siler. Cari ve Stok KARTLARI
    // (Customers/Products ve alt tanım tabloları) silinmez; sadece bakiye/miktarları sıfırlanır.
    private static readonly string[] MovementOnlyTables =
    [
        "StockReservations", "StockMovements",
        "CurrentAccountTransactions", "FinancialTransactions",
        "InvoicePaymentSchedules", "InvoiceLines", "Invoices",
        "PaymentReceiptLines", "PaymentReceipts",
        "QuoteLines", "Quotes",
        "BusinessOrderLines", "BusinessOrders",
        "DispatchNoteLines", "DispatchNotes",
        "StockSlipLines", "StockSlips",
        "StockTransferLines", "StockTransfers",
        "InventoryCountLines", "InventoryCounts",
        "PurchasePriceListItems", "PurchasePriceLists",
        "SalesPriceListItems", "SalesPriceLists",
        "Expenses",
        "NegotiableInstruments",
        "ExchangeRates",
        "AuditLogs",
        "IntegrationOutboxMessages",
        "ExternalRecordMappings"
    ];

    // Tab 3 — Hareket + Cari + Stok: yukarıdakine ek olarak cari ve stok KARTLARININ kendisi de silinir.
    private static readonly string[] FullWipeTables =
    [
        .. MovementOnlyTables,
        "ProductUnitConversions", "ProductSerialNumbers", "ProductImages", "ProductBarcodes", "ProductVariants", "ScaleProductSettings", "Products",
        "CustomerAddresses", "CustomerContacts", "Customers"
    ];

    private readonly TextBox _serverBox = new();
    private readonly TextBox _databaseBox = new() { Text = "SahinSoftDb" };
    private readonly RadioButton _windowsAuthRadio = new() { Text = "Windows Kimlik Doğrulaması", AutoSize = true };
    private readonly RadioButton _sqlAuthRadio = new() { Text = "SQL Server Kimlik Doğrulaması", Checked = true, AutoSize = true };
    private readonly TextBox _userBox = new() { Text = "sa" };
    private readonly TextBox _passwordBox = new() { UseSystemPasswordChar = true };
    private readonly Label _connectionStatusLabel = new() { AutoSize = true, ForeColor = Color.DimGray, MaximumSize = new Size(820, 0) };
    private readonly Button _testButton = new() { Text = "Bağlantıyı Test Et", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
    private readonly TextBox _backupPathBox = new() { Text = "(sunucudaki varsayılan yedek klasörü otomatik bulunur)" };

    private readonly TextBox _logBox = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Font = new Font("Consolas", 9F),
        BackColor = Color.Black,
        ForeColor = Color.Gainsboro,
        Dock = DockStyle.Fill
    };

    private bool _connectionOk;

    // Tab 1
    private readonly CheckBox _maintCheckDb = new() { Text = "Veri Bütünlüğü Denetimi (DBCC CHECKDB — sadece rapor, otomatik onarım yapmaz)", AutoSize = true };
    private readonly CheckBox _maintLogCleanup = new() { Text = "Log Temizleme (transaction log'u küçült)", AutoSize = true };
    private readonly CheckBox _maintIndex = new() { Text = "Index Bakımı (parçalanmış indexleri yeniden oluştur/düzenle)", AutoSize = true };
    private readonly CheckBox _maintShrink = new() { Text = "Shrink (veritabanı dosyasını küçült)", AutoSize = true };
    private readonly Button _createDbButton = new() { Text = "Veritabanını Oluştur", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
    private readonly Button _maintRunButton = new() { Text = "Seçili Bakım İşlemlerini Çalıştır", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };

    // Tab 2
    private readonly TextBox _movementConfirmBox = new();
    private readonly Button _movementRunButton = new();

    // Tab 3
    private readonly TextBox _fullConfirmBox = new();
    private readonly Button _fullRunButton = new();

    public MainForm()
    {
        Text = "ŞahinSoft — Veritabanı Araçları";
        Width = 940;
        Height = 940;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(860, 760);

        _windowsAuthRadio.CheckedChanged += (_, _) => UpdateAuthFieldsEnabled();
        _sqlAuthRadio.CheckedChanged += (_, _) => UpdateAuthFieldsEnabled();
        UpdateAuthFieldsEnabled();

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(16) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        root.Controls.Add(BuildConnectionGroup(), 0, 0);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildMaintenanceTab());
        tabs.TabPages.Add(BuildMovementWipeTab());
        tabs.TabPages.Add(BuildFullWipeTab());
        root.Controls.Add(tabs, 0, 1);

        root.Controls.Add(new Label { Text = "İşlem Kaydı", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(0, 10, 0, 4) }, 0, 2);
        root.Controls.Add(_logBox, 0, 3);

        Controls.Add(root);
        UpdateRunButtonsEnabled();
    }

    // ---------------------------------------------------------------- Bağlantı

    private GroupBox BuildConnectionGroup()
    {
        var group = new GroupBox { Text = "Veritabanı Bağlantısı", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));

        void AddRow(string label, Control input)
        {
            var row = layout.RowCount;
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.Controls.Add(new Label { Text = label, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 6, 0, 0) }, 0, row);
            input.Dock = DockStyle.Fill;
            layout.Controls.Add(input, 1, row);
        }

        AddRow("Sunucu Adresi", _serverBox);
        AddRow("Veritabanı Adı", _databaseBox);

        var authPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        authPanel.Controls.Add(_sqlAuthRadio);
        authPanel.Controls.Add(_windowsAuthRadio);
        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.Controls.Add(new Label { Text = "Kimlik Doğrulama", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, layout.RowCount - 1);
        layout.Controls.Add(authPanel, 1, layout.RowCount - 1);
        layout.SetColumnSpan(authPanel, 2);

        AddRow("Kullanıcı Adı", _userBox);
        AddRow("Şifre", _passwordBox);

        _testButton.Click += TestButton_Click;
        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.Controls.Add(_testButton, 1, layout.RowCount - 1);

        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_connectionStatusLabel, 1, layout.RowCount - 1);
        layout.SetColumnSpan(_connectionStatusLabel, 2);

        AddRow("Yedek Dosya Yolu", _backupPathBox);
        var backupHint = new Label
        {
            Text = "Sadece hareket/tam sıfırlama işlemleri öncesi kullanılır. Boş/varsayılan bırakılırsa sunucunun SQL Server yedek klasörü otomatik tespit edilir.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Font = new Font("Segoe UI", 8F)
        };
        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(backupHint, 1, layout.RowCount - 1);
        layout.SetColumnSpan(backupHint, 2);

        group.Controls.Add(layout);
        return group;
    }

    private void UpdateAuthFieldsEnabled()
    {
        _userBox.Enabled = _sqlAuthRadio.Checked;
        _passwordBox.Enabled = _sqlAuthRadio.Checked;
    }

    private bool TryBuildConnectionString(string database, out string connectionString, out string error)
    {
        connectionString = string.Empty;
        error = string.Empty;

        var server = _serverBox.Text.Trim();
        if (server.Length == 0)
        {
            error = "Sunucu adresi boş olamaz.";
            return false;
        }

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            TrustServerCertificate = true,
            ConnectTimeout = 15
        };

        if (_windowsAuthRadio.Checked)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            if (_userBox.Text.Trim().Length == 0)
            {
                error = "SQL kullanıcı adı boş olamaz.";
                return false;
            }
            builder.UserID = _userBox.Text.Trim();
            builder.Password = _passwordBox.Text;
        }

        connectionString = builder.ConnectionString;
        return true;
    }

    private async void TestButton_Click(object? sender, EventArgs e)
    {
        _connectionOk = false;
        UpdateRunButtonsEnabled();

        var database = _databaseBox.Text.Trim();
        if (database.Length == 0)
        {
            SetConnectionStatus("Veritabanı adı boş olamaz.", isError: true);
            return;
        }
        if (!TryBuildConnectionString(database, out var connectionString, out var error))
        {
            SetConnectionStatus(error, isError: true);
            return;
        }

        SetConnectionStatus("Bağlantı deneniyor...", isError: false);
        Cursor = Cursors.WaitCursor;
        _testButton.Enabled = false;
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            _connectionOk = true;
            SetConnectionStatus($"Bağlantı başarılı ({database}).", isError: false);
        }
        catch (Exception ex)
        {
            SetConnectionStatus($"Bağlantı başarısız: {ex.Message}", isError: true);
        }
        finally
        {
            Cursor = Cursors.Default;
            _testButton.Enabled = true;
            UpdateRunButtonsEnabled();
        }
    }

    private void SetConnectionStatus(string message, bool isError)
    {
        _connectionStatusLabel.Text = message;
        _connectionStatusLabel.ForeColor = isError ? Color.Firebrick : Color.SeaGreen;
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(Log, message);
            return;
        }
        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void UpdateRunButtonsEnabled()
    {
        _createDbButton.Enabled = _connectionOk;
        _maintRunButton.Enabled = _connectionOk;
        _movementRunButton.Enabled = _connectionOk && string.Equals(_movementConfirmBox.Text.Trim(), ConfirmPhrase, StringComparison.Ordinal);
        _fullRunButton.Enabled = _connectionOk && string.Equals(_fullConfirmBox.Text.Trim(), ConfirmPhrase, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- Tab 1: Oluştur / Bakım

    private TabPage BuildMaintenanceTab()
    {
        var page = new TabPage("1) Veritabanı Oluştur / Bakım");
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true, Padding = new Padding(10) };

        var createGroup = new GroupBox { Text = "Yeni Veritabanı Oluştur", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10) };
        var createPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        createPanel.Controls.Add(new Label
        {
            Text = "Yukarıda yazılan isimde veritabanı yoksa boş olarak oluşturulur.\nOluşturduktan sonra ŞahinSoft uygulamasını bir kez çalıştırın; tablolar (migration) otomatik kurulur.",
            AutoSize = true,
            MaximumSize = new Size(820, 0)
        });
        _createDbButton.Click += CreateDbButton_Click;
        createPanel.Controls.Add(_createDbButton);
        createGroup.Controls.Add(createPanel);
        layout.Controls.Add(createGroup);

        var maintGroup = new GroupBox { Text = "Bakım İşlemleri", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10) };
        var maintPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        maintPanel.Controls.Add(_maintCheckDb);
        maintPanel.Controls.Add(_maintLogCleanup);
        maintPanel.Controls.Add(_maintIndex);
        maintPanel.Controls.Add(_maintShrink);
        _maintRunButton.Margin = new Padding(0, 10, 0, 0);
        _maintRunButton.Click += MaintRunButton_Click;
        maintPanel.Controls.Add(_maintRunButton);
        maintGroup.Controls.Add(maintPanel);
        layout.Controls.Add(maintGroup);

        page.Controls.Add(layout);
        return page;
    }

    private async void CreateDbButton_Click(object? sender, EventArgs e)
    {
        var database = _databaseBox.Text.Trim();
        if (database.Length == 0)
        {
            MessageBox.Show(this, "Veritabanı adı boş olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (!TryBuildConnectionString("master", out var masterConnStr, out var error))
        {
            MessageBox.Show(this, error, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Cursor = Cursors.WaitCursor;
        _createDbButton.Enabled = false;
        try
        {
            await using var connection = new SqlConnection(masterConnStr);
            connection.InfoMessage += (_, args) => Log(args.Message);
            await connection.OpenAsync();

            await using var checkCommand = new SqlCommand("SELECT 1 FROM sys.databases WHERE name = @name", connection);
            checkCommand.Parameters.AddWithValue("@name", database);
            var exists = await checkCommand.ExecuteScalarAsync() is not null;
            if (exists)
            {
                Log($"'{database}' zaten mevcut, yeniden oluşturulmadı.");
                MessageBox.Show(this, $"'{database}' veritabanı zaten mevcut.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await using var createCommand = new SqlCommand($"CREATE DATABASE [{database}];", connection) { CommandTimeout = 0 };
            await createCommand.ExecuteNonQueryAsync();
            Log($"'{database}' veritabanı oluşturuldu (boş). Tabloları kurmak için ŞahinSoft uygulamasını bir kez çalıştırın.");
            MessageBox.Show(this, $"'{database}' veritabanı oluşturuldu.\n\nTabloları kurmak için ŞahinSoft uygulamasını bir kez çalıştırın (migration otomatik uygulanır).", "Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Log($"HATA: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            _createDbButton.Enabled = true;
        }
    }

    private async void MaintRunButton_Click(object? sender, EventArgs e)
    {
        var database = _databaseBox.Text.Trim();
        if (!TryBuildConnectionString(database, out var connectionString, out var error))
        {
            MessageBox.Show(this, error, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (!_maintCheckDb.Checked && !_maintLogCleanup.Checked && !_maintIndex.Checked && !_maintShrink.Checked)
        {
            MessageBox.Show(this, "En az bir bakım işlemi seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Cursor = Cursors.WaitCursor;
        _maintRunButton.Enabled = false;
        try
        {
            await using var connection = new SqlConnection(connectionString);
            connection.InfoMessage += (_, args) => Log(args.Message);
            await connection.OpenAsync();

            if (_maintCheckDb.Checked)
            {
                Log("Veri bütünlüğü denetimi (DBCC CHECKDB) çalışıyor, bu biraz sürebilir...");
                await using var cmd = new SqlCommand($"DBCC CHECKDB ([{database}]) WITH NO_INFOMSGS, ALL_ERRORMSGS;", connection) { CommandTimeout = 0 };
                await cmd.ExecuteNonQueryAsync();
                Log("Veri bütünlüğü denetimi tamamlandı. Yukarıda hata mesajı yoksa veritabanı sağlıklıdır. (Bu araç otomatik onarım YAPMAZ — hata görülürse elle inceleyin.)");
            }

            if (_maintLogCleanup.Checked)
            {
                await RunLogCleanupAsync(connection, database);
            }

            if (_maintIndex.Checked)
            {
                await RunIndexMaintenanceAsync(connection);
            }

            if (_maintShrink.Checked)
            {
                Log("Veritabanı küçültülüyor (DBCC SHRINKDATABASE)...");
                await using var cmd = new SqlCommand($"DBCC SHRINKDATABASE ([{database}]);", connection) { CommandTimeout = 0 };
                await cmd.ExecuteNonQueryAsync();
                Log("Shrink tamamlandı.");
            }

            Log("Bakım işlemleri tamamlandı.");
        }
        catch (Exception ex)
        {
            Log($"HATA: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            _maintRunButton.Enabled = true;
        }
    }

    private async Task RunLogCleanupAsync(SqlConnection connection, string database)
    {
        Log("Log dosyası temizleniyor...");

        await using var modelCmd = new SqlCommand("SELECT recovery_model_desc FROM sys.databases WHERE name = @name", connection);
        modelCmd.Parameters.AddWithValue("@name", database);
        var originalModel = (string?)await modelCmd.ExecuteScalarAsync() ?? "SIMPLE";

        await using var logFileCmd = new SqlCommand(
            "SELECT name FROM sys.master_files WHERE database_id = DB_ID(@name) AND type_desc = 'LOG'", connection);
        logFileCmd.Parameters.AddWithValue("@name", database);
        var logFileName = (string?)await logFileCmd.ExecuteScalarAsync();
        if (logFileName is null)
        {
            Log("Log dosyası bulunamadı, atlanıyor.");
            return;
        }

        if (!string.Equals(originalModel, "SIMPLE", StringComparison.OrdinalIgnoreCase))
        {
            Log($"Kurtarma modeli geçici olarak SIMPLE yapılıyor (öncekiydi: {originalModel})...");
            await using var setSimple = new SqlCommand($"ALTER DATABASE [{database}] SET RECOVERY SIMPLE;", connection) { CommandTimeout = 0 };
            await setSimple.ExecuteNonQueryAsync();
        }

        await using var checkpoint = new SqlCommand("CHECKPOINT;", connection) { CommandTimeout = 0 };
        await checkpoint.ExecuteNonQueryAsync();

        await using var shrinkLog = new SqlCommand($"DBCC SHRINKFILE(N'{logFileName}', 1);", connection) { CommandTimeout = 0 };
        await shrinkLog.ExecuteNonQueryAsync();
        Log($"Log dosyası ({logFileName}) küçültüldü.");

        if (!string.Equals(originalModel, "SIMPLE", StringComparison.OrdinalIgnoreCase))
        {
            Log($"Kurtarma modeli eski haline döndürülüyor ({originalModel})...");
            await using var restoreModel = new SqlCommand($"ALTER DATABASE [{database}] SET RECOVERY {originalModel};", connection) { CommandTimeout = 0 };
            await restoreModel.ExecuteNonQueryAsync();
        }
    }

    private async Task RunIndexMaintenanceAsync(SqlConnection connection)
    {
        Log("Parçalanmış indexler taranıyor...");
        var targets = new List<(string Schema, string Table, string Index, double Fragmentation)>();

        await using (var scanCmd = new SqlCommand(
            "SELECT s.name AS SchemaName, t.name AS TableName, i.name AS IndexName, ps.avg_fragmentation_in_percent " +
            "FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps " +
            "JOIN sys.indexes i ON ps.object_id = i.object_id AND ps.index_id = i.index_id " +
            "JOIN sys.tables t ON t.object_id = i.object_id " +
            "JOIN sys.schemas s ON s.schema_id = t.schema_id " +
            "WHERE i.name IS NOT NULL AND ps.avg_fragmentation_in_percent > 5 AND ps.page_count > 50;", connection)
        { CommandTimeout = 0 })
        {
            await using var reader = await scanCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                targets.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetDouble(3)));
            }
        }

        if (targets.Count == 0)
        {
            Log("Parçalanmış (bakım gerektiren) index bulunamadı.");
            return;
        }

        foreach (var (schema, table, index, fragmentation) in targets)
        {
            var action = fragmentation >= 30 ? "REBUILD" : "REORGANIZE";
            await using var cmd = new SqlCommand($"ALTER INDEX [{index}] ON [{schema}].[{table}] {action};", connection) { CommandTimeout = 0 };
            await cmd.ExecuteNonQueryAsync();
            Log($"{table}.{index}: %{fragmentation:N1} parçalanma -> {action} tamamlandı.");
        }

        Log($"Index bakımı tamamlandı ({targets.Count} index işlendi).");
    }

    // ---------------------------------------------------------------- Tab 2: Sadece Hareketleri Sil

    private TabPage BuildMovementWipeTab()
    {
        var page = new TabPage("2) Tüm Hareketleri Sil");
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true, Padding = new Padding(10) };

        layout.Controls.Add(new Label
        {
            Text = "Cari ve stok KARTLARI (isim/kod/tanımlar) silinmez; sadece geçmiş belge ve hareketleri temizlenir, bakiyeler ve stok miktarları sıfırlanır.",
            AutoSize = true,
            MaximumSize = new Size(860, 0),
            Margin = new Padding(0, 0, 0, 10)
        });

        var wipeLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(860, 0),
            ForeColor = Color.FromArgb(178, 34, 34),
            Text = "SİLİNECEKLER: Faturalar, Teklifler, Siparişler, İrsaliyeler, Stok Fişleri/Transferleri/Sayımları,\n" +
                   "Tahsilat/Tediye Fişleri, Masraflar, Çek/Senetler, Stok Hareketleri, Cari Hareketleri, Fiyat Listesi\n" +
                   "hareketleri, Döviz kuru geçmişi, Denetim Kayıtları. Ayrıca: tüm stok miktarları 0'a, numaralandırma\n" +
                   "sayaçları 1'e sıfırlanır.\n\nKORUNUR: Cari kartları, Stok kartları, tüm program ayarları."
        };
        layout.Controls.Add(wipeLabel);

        layout.Controls.Add(BuildConfirmRow(_movementConfirmBox, _movementRunButton, "YEDEK AL VE HAREKETLERİ SİL"));
        _movementConfirmBox.TextChanged += (_, _) => UpdateRunButtonsEnabled();
        _movementRunButton.Click += (_, _) => RunWipeFlowAsync(MovementOnlyTables, resetProductQuantities: true, deletesMasterData: false);

        page.Controls.Add(layout);
        return page;
    }

    // ---------------------------------------------------------------- Tab 3: Hareket + Cari + Stok

    private TabPage BuildFullWipeTab()
    {
        var page = new TabPage("3) Veritabanını Temizle (Hareket+Cari+Stok)");
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true, Padding = new Padding(10) };

        layout.Controls.Add(new Label
        {
            Text = "Tab 2'deki her şeye ek olarak cari ve stok KARTLARININ kendisi de kalıcı olarak silinir. Program ayarlarına dokunulmaz.",
            AutoSize = true,
            MaximumSize = new Size(860, 0),
            Margin = new Padding(0, 0, 0, 10)
        });

        var panel = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        var wipeLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(400, 0),
            ForeColor = Color.FromArgb(178, 34, 34),
            Text = "KALICI OLARAK SİLİNECEKLER:\n" +
                   "• Tüm Cariler (müşteri/tedarikçi + hareketleri)\n" +
                   "• Tüm Stok Kartları\n" +
                   "• Tüm Faturalar ve Stok Hareketleri\n" +
                   "• Tüm Tahsilat / Tediye Fişleri\n" +
                   "• Tüm Teklifler, Siparişler, İrsaliyeler\n" +
                   "• Tüm Stok Fişleri / Transferleri / Sayımları\n" +
                   "• Tüm Alış/Satış Fiyat Listesi hareketleri\n" +
                   "• Tüm Masraflar, Çek/Senetler\n" +
                   "• Döviz kuru geçmişi, Denetim Kayıtları"
        };
        var keepLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(400, 0),
            ForeColor = Color.FromArgb(31, 122, 77),
            Text = "KORUNACAKLAR (dokunulmaz):\n" +
                   "• Kullanıcılar ve Yetkiler (Roller)\n" +
                   "• Depo / Şube Tanımları\n" +
                   "• KDV Oranları, Birimler, Para Birimleri\n" +
                   "• Kasa/Banka Tanımları, Maliyet Merkezi/Proje\n" +
                   "• Stok Kategori/Renk Tanımları, Masraf Kategorileri\n" +
                   "• Fiyat Listesi Tanımları, Şirket Bilgileri\n" +
                   "• Stok Ayarları\n" +
                   "• Numaralandırma Ayarları (sayaçlar 1'e sıfırlanır)"
        };
        panel.Controls.Add(wipeLabel, 0, 0);
        panel.Controls.Add(keepLabel, 1, 0);
        layout.Controls.Add(panel);

        layout.Controls.Add(BuildConfirmRow(_fullConfirmBox, _fullRunButton, "YEDEK AL VE TÜM VERİLERİ SIFIRLA"));
        _fullConfirmBox.TextChanged += (_, _) => UpdateRunButtonsEnabled();
        _fullRunButton.Click += (_, _) => RunWipeFlowAsync(FullWipeTables, resetProductQuantities: false, deletesMasterData: true);

        page.Controls.Add(layout);
        return page;
    }

    private TableLayoutPanel BuildConfirmRow(TextBox confirmBox, Button runButton, string runButtonText)
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var promptLabel = new Label
        {
            Text = $"Devam etmek için kutuya \"{ConfirmPhrase}\" yazın:",
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 8, 8, 0)
        };
        confirmBox.Dock = DockStyle.Fill;

        runButton.Text = runButtonText;
        runButton.AutoSize = true;
        runButton.Padding = new Padding(10, 8, 10, 8);
        runButton.BackColor = Color.FromArgb(178, 34, 34);
        runButton.ForeColor = Color.White;
        runButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        runButton.Enabled = false;

        layout.Controls.Add(promptLabel, 0, 0);
        layout.Controls.Add(confirmBox, 1, 0);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        layout.Controls.Add(runButton, 0, 1);
        layout.SetColumnSpan(runButton, 3);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        return layout;
    }

    // ---------------------------------------------------------------- Ortak: Yedekle + Sil akışı

    private async void RunWipeFlowAsync(string[] tables, bool resetProductQuantities, bool deletesMasterData)
    {
        var database = _databaseBox.Text.Trim();
        if (!TryBuildConnectionString(database, out var targetConnectionString, out var error) ||
            !TryBuildConnectionString("master", out var masterConnectionString, out error))
        {
            MessageBox.Show(this, error, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var scopeText = deletesMasterData
            ? "TÜM cari, stok ve hareket verisi"
            : "TÜM hareket ve belge geçmişi (cari/stok kartları KALIR, bakiyeleri sıfırlanır)";
        var confirm = MessageBox.Show(this,
            $"\"{database}\" veritabanında {scopeText} kalıcı olarak silinecek.\n\n" +
            "Program ayarları (kullanıcılar, depo/şube/KDV/birim/kasa-banka tanımları vb.) korunacaktır.\n\n" +
            "İşlemden önce otomatik bir yedek alınacaktır. Devam edilsin mi?",
            "Son Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes) return;

        _movementRunButton.Enabled = false;
        _fullRunButton.Enabled = false;
        _testButton.Enabled = false;
        Cursor = Cursors.WaitCursor;
        _logBox.Clear();

        try
        {
            var backupPath = await RunBackupAsync(masterConnectionString, database);
            Log($"Yedek tamamlandı: {backupPath}");
            Log("---");
            await RunWipeAsync(targetConnectionString, tables, resetProductQuantities);
            Log("---");
            Log("İŞLEM TAMAMLANDI.");
            MessageBox.Show(this, $"İşlem tamamlandı.\n\nYedek dosyası:\n{backupPath}", "Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Log($"HATA: {ex.Message}");
            MessageBox.Show(this,
                $"İşlem sırasında hata oluştu, hiçbir veri silinmedi (işlem geri alındı):\n\n{ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            _testButton.Enabled = true;
            UpdateRunButtonsEnabled();
        }
    }

    private async Task<string> RunBackupAsync(string masterConnectionString, string database)
    {
        Log($"'{database}' için yedek alınıyor...");

        await using var connection = new SqlConnection(masterConnectionString);
        connection.InfoMessage += (_, args) => Log(args.Message);
        await connection.OpenAsync();

        var customPath = _backupPathBox.Text.Trim();
        string backupPath;
        var fileName = $"{database}_SifirlamaOncesiYedek_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

        if (customPath.Length > 0 && !customPath.StartsWith("(", StringComparison.Ordinal))
        {
            backupPath = customPath.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)
                ? customPath
                : Path.Combine(customPath, fileName);
        }
        else
        {
            await using var dirCommand = new SqlCommand(
                "DECLARE @path NVARCHAR(500); " +
                "EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\\Microsoft\\MSSQLServer\\MSSQLServer', N'BackupDirectory', @path OUTPUT; " +
                "SELECT @path;", connection);
            var backupDir = (string?)await dirCommand.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Sunucunun varsayılan yedek klasörü tespit edilemedi. Lütfen 'Yedek Dosya Yolu' alanına elle bir yol girin.");
            backupPath = Path.Combine(backupDir, fileName);
        }

        Log($"Hedef yedek dosyası: {backupPath}");

        await using var backupCommand = new SqlCommand(
            $"BACKUP DATABASE [{database}] TO DISK = @path WITH INIT, STATS = 10;", connection)
        {
            CommandTimeout = 0
        };
        backupCommand.Parameters.AddWithValue("@path", backupPath);
        await backupCommand.ExecuteNonQueryAsync();

        return backupPath;
    }

    private async Task RunWipeAsync(string targetConnectionString, string[] tables, bool resetProductQuantities)
    {
        await using var connection = new SqlConnection(targetConnectionString);
        connection.InfoMessage += (_, args) => Log(args.Message);
        await connection.OpenAsync();

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        try
        {
            Log("Yabancı anahtar denetimleri geçici olarak kapatılıyor...");
            await ExecuteAsync(connection, transaction, "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");

            foreach (var table in tables)
            {
                var deleted = await ExecuteAsync(connection, transaction, $"DELETE FROM [{table}];");
                await ExecuteAsync(connection, transaction, $"IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'[{table}]')) DBCC CHECKIDENT ('[{table}]', RESEED, 0);");
                Log($"{table}: {deleted} kayıt silindi.");
            }

            if (resetProductQuantities)
            {
                Log("Stok kartlarındaki miktarlar 0'a sıfırlanıyor...");
                await ExecuteAsync(connection, transaction, "UPDATE [Products] SET [StockQuantity] = 0;");
            }

            Log("Numaralandırma sayaçları (Fatura/Fiş/Sıra No) 1'e sıfırlanıyor...");
            await ExecuteAsync(connection, transaction, "UPDATE [NumberSequences] SET [NextNumber] = 1;");

            Log("Yabancı anahtar denetimleri yeniden etkinleştiriliyor...");
            await ExecuteAsync(connection, transaction, "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task<int> ExecuteAsync(SqlConnection connection, SqlTransaction transaction, string sql)
    {
        await using var command = new SqlCommand(sql, connection, transaction) { CommandTimeout = 0 };
        return await command.ExecuteNonQueryAsync();
    }
}
