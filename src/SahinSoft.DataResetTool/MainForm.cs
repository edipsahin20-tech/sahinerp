using Microsoft.Data.SqlClient;

namespace SahinSoft.DataResetTool;

public sealed class MainForm : Form
{
    private const string ConfirmPhrase = "SİL";

    // Hareket / Cari / Stok verisi — bu tablolardaki TÜM SATIRLAR silinir.
    // Sıra, FK denetimleri işlem süresince kapatıldığı için önemli değildir; yine de
    // okunabilirlik için kabaca çocuktan-ebeveyne doğru sıralanmıştır.
    private static readonly string[] WipeTables =
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
        "ProductUnitConversions", "ProductSerialNumbers", "ProductImages", "ProductBarcodes", "ProductVariants", "ScaleProductSettings", "Products",
        "CustomerAddresses", "CustomerContacts", "Customers",
        "Expenses",
        "NegotiableInstruments",
        "ExchangeRates",
        "AuditLogs",
        "IntegrationOutboxMessages",
        "ExternalRecordMappings"
    ];

    private readonly TextBox _serverBox = new();
    private readonly TextBox _databaseBox = new() { Text = "SahinSoftDb" };
    private readonly RadioButton _windowsAuthRadio = new() { Text = "Windows Kimlik Doğrulaması", AutoSize = true };
    private readonly RadioButton _sqlAuthRadio = new() { Text = "SQL Server Kimlik Doğrulaması", Checked = true, AutoSize = true };
    private readonly TextBox _userBox = new() { Text = "sa" };
    private readonly TextBox _passwordBox = new() { UseSystemPasswordChar = true };
    private readonly Label _connectionStatusLabel = new() { AutoSize = true, ForeColor = Color.DimGray, MaximumSize = new Size(760, 0) };

    private readonly TextBox _backupPathBox = new() { Text = "(sunucudaki varsayılan yedek klasörü otomatik bulunur)" };
    private readonly TextBox _confirmBox = new();
    private readonly Button _testButton = new() { Text = "Bağlantıyı Test Et", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
    private readonly Button _runButton = new()
    {
        Text = "YEDEK AL VE TÜM VERİLERİ SIFIRLA",
        Enabled = false,
        AutoSize = true,
        Padding = new Padding(10, 8, 10, 8),
        BackColor = Color.FromArgb(178, 34, 34),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
    };
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

    public MainForm()
    {
        Text = "ŞahinSoft — Verileri Sıfırla (Cari / Stok / Hareket Temizleme)";
        Width = 900;
        Height = 900;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(820, 700);

        _windowsAuthRadio.CheckedChanged += (_, _) => UpdateAuthFieldsEnabled();
        _sqlAuthRadio.CheckedChanged += (_, _) => UpdateAuthFieldsEnabled();
        UpdateAuthFieldsEnabled();

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(16) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildConnectionGroup(), 0, 0);
        root.Controls.Add(BuildInfoGroup(), 0, 1);
        root.Controls.Add(BuildConfirmGroup(), 0, 2);
        root.Controls.Add(new Label { Text = "İşlem Kaydı", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(0, 10, 0, 4) }, 0, 3);
        root.Controls.Add(_logBox, 0, 4);

        Controls.Add(root);
    }

    private GroupBox BuildConnectionGroup()
    {
        var group = new GroupBox { Text = "Veritabanı Bağlantısı", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));

        void AddRow(string label, Control input, Control? extra = null)
        {
            var row = layout.RowCount;
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.Controls.Add(new Label { Text = label, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 6, 0, 0) }, 0, row);
            input.Dock = DockStyle.Fill;
            layout.Controls.Add(input, 1, row);
            if (extra is not null)
            {
                extra.Dock = DockStyle.Fill;
                layout.Controls.Add(extra, 2, row);
            }
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
            Text = "Boş/varsayılan bırakılırsa, sunucunun SQL Server yedek klasörü otomatik tespit edilip zaman damgalı bir dosya adıyla oraya yazılır.",
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

    private GroupBox BuildInfoGroup()
    {
        var group = new GroupBox { Text = "Bu İşlem Ne Yapar?", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10) };
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
        group.Controls.Add(panel);
        return group;
    }

    private GroupBox BuildConfirmGroup()
    {
        var group = new GroupBox { Text = "Onay", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true };
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
        _confirmBox.Dock = DockStyle.Fill;
        _confirmBox.TextChanged += (_, _) => UpdateRunButtonEnabled();
        _runButton.Click += RunButton_Click;

        layout.Controls.Add(promptLabel, 0, 0);
        layout.Controls.Add(_confirmBox, 1, 0);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        layout.Controls.Add(_runButton, 0, 1);
        layout.SetColumnSpan(_runButton, 3);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        group.Controls.Add(layout);
        return group;
    }

    private void UpdateAuthFieldsEnabled()
    {
        _userBox.Enabled = _sqlAuthRadio.Checked;
        _passwordBox.Enabled = _sqlAuthRadio.Checked;
    }

    private void UpdateRunButtonEnabled()
    {
        _runButton.Enabled = _connectionOk && string.Equals(_confirmBox.Text.Trim(), ConfirmPhrase, StringComparison.Ordinal);
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
        UpdateRunButtonEnabled();

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
            UpdateRunButtonEnabled();
        }
    }

    private void SetConnectionStatus(string message, bool isError)
    {
        _connectionStatusLabel.Text = message;
        _connectionStatusLabel.ForeColor = isError ? Color.Firebrick : Color.SeaGreen;
    }

    private void Log(string message)
    {
        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private async void RunButton_Click(object? sender, EventArgs e)
    {
        var database = _databaseBox.Text.Trim();
        if (!TryBuildConnectionString(database, out var targetConnectionString, out var error) ||
            !TryBuildConnectionString("master", out var masterConnectionString, out error))
        {
            MessageBox.Show(this, error, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var confirm = MessageBox.Show(this,
            $"\"{database}\" veritabanındaki TÜM cari, stok ve hareket verisi kalıcı olarak silinecek.\n\n" +
            "Program ayarları (kullanıcılar, depo/şube/KDV/birim/kasa-banka tanımları vb.) korunacaktır.\n\n" +
            "İşlemden önce otomatik bir yedek alınacaktır. Devam edilsin mi?",
            "Son Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes) return;

        _runButton.Enabled = false;
        _testButton.Enabled = false;
        Cursor = Cursors.WaitCursor;
        _logBox.Clear();

        try
        {
            var backupPath = await RunBackupAsync(masterConnectionString, database);
            Log($"Yedek tamamlandı: {backupPath}");
            Log("---");
            await RunWipeAsync(targetConnectionString);
            Log("---");
            Log("İŞLEM TAMAMLANDI. Cari/Stok/Hareket verisi silindi, program ayarları korundu.");
            MessageBox.Show(this,
                $"İşlem tamamlandı.\n\nYedek dosyası:\n{backupPath}",
                "Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            UpdateRunButtonEnabled();
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

    private async Task RunWipeAsync(string targetConnectionString)
    {
        await using var connection = new SqlConnection(targetConnectionString);
        connection.InfoMessage += (_, args) => Log(args.Message);
        await connection.OpenAsync();

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        try
        {
            Log("Yabancı anahtar denetimleri geçici olarak kapatılıyor...");
            await ExecuteAsync(connection, transaction, "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");

            foreach (var table in WipeTables)
            {
                var deleted = await ExecuteAsync(connection, transaction, $"DELETE FROM [{table}];");
                await ExecuteAsync(connection, transaction, $"IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'[{table}]')) DBCC CHECKIDENT ('[{table}]', RESEED, 0);");
                Log($"{table}: {deleted} kayıt silindi.");
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
