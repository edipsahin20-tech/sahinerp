namespace SahinSoft.DbSetup;

// Edip 3 araca (Kurulum, Veritabanı, Restoran programı) indirmek istedi - eskiden ayrı bir exe
// olan SahinSoft.ConfigTool'un (appsettings.json bağlantı dizesi ayarı) işlevi buraya taşındı,
// ConfigTool artık paketlenmiyor. 2026-09-28: ayrı bir exe olan SahinSoft.DataResetTool
// (SahinSoftVerileriSifirla.exe) da buraya birleştirildi - Edip'in "hepsini tek bir araçta
// birleştir" talebiyle; o proje kaldırıldı, tüm "Hareketleri Sil"/"Tam Temizle" işlevi (ve
// bakım: CHECKDB/log temizleme/index bakımı) artık burada. Bu exe kurulum paketinde zaten
// "Araclar/" klasörüne kopyalanıyor (bkz. SahinSoft.Setup/MainForm.cs InstallPermanentTools),
// ayrıca yeni bir yerleştirme adımı gerekmedi.
public sealed class MainForm : Form
{
    private const string ConfirmPhrase = "SİL";
    private static readonly Color BrandGold = Color.FromArgb(212, 164, 55);

    // ---- Bağlantı ----
    private readonly TextBox _serverBox = new() { Text = "localhost" };
    private readonly TextBox _databaseBox = new() { Text = "SahinSoftDb" };
    private readonly RadioButton _sqlAuthRadio = new() { Text = "SQL Server Kimlik Doğrulaması", Checked = true, AutoSize = true };
    private readonly RadioButton _windowsAuthRadio = new() { Text = "Windows Kimlik Doğrulaması", AutoSize = true };
    private readonly TextBox _userBox = new() { Text = "sa" };
    private readonly TextBox _passwordBox = new() { Text = "SahinSoft2026!Kurulum", UseSystemPasswordChar = true };
    private readonly Button _testButton = new() { Text = "Bağlantıyı Test Et", AutoSize = true, Padding = new Padding(10, 5, 10, 5) };
    private readonly Label _connectionStatusLabel = new() { AutoSize = true, ForeColor = Color.DimGray, MaximumSize = new Size(760, 0) };
    private readonly TextBox _appSettingsPathBox = new() { Text = @"C:\SitesSahinSoft\appsettings.json" };
    private readonly Button _saveConnectionButton = new() { Text = "appsettings.json'a Kaydet", AutoSize = true, Padding = new Padding(10, 5, 10, 5) };
    private readonly TextBox _backupPathBox = new() { Text = "(sunucudaki varsayılan yedek klasörü otomatik bulunur)", ForeColor = Color.DimGray };

    // ---- Tab 1: Oluştur / Bakım ----
    private Button _createIfMissingButton = null!;
    private Button _recreateFromScratchButton = null!;
    private Button _updateButton = null!;
    private Button _backupButton = null!;
    private readonly CheckBox _maintCheckDb = new() { Text = "Veri Bütünlüğü Denetimi (DBCC CHECKDB — sadece rapor, otomatik onarım yapmaz)", AutoSize = true };
    private readonly CheckBox _maintLogCleanup = new() { Text = "Log Temizleme (transaction log'u küçült)", AutoSize = true };
    private readonly CheckBox _maintIndex = new() { Text = "Index Bakımı (parçalanmış indexleri yeniden oluştur/düzenle)", AutoSize = true };
    private readonly CheckBox _maintShrink = new() { Text = "Shrink (veritabanı dosyasını küçült)", AutoSize = true };
    private readonly Button _maintRunButton = new() { Text = "Seçili Bakım İşlemlerini Çalıştır", AutoSize = true, Padding = new Padding(10, 6, 10, 6) };

    // ---- Tab 2 / Tab 3: Sil ----
    private readonly TextBox _movementConfirmBox = new() { Width = 120 };
    private readonly Button _movementRunButton = new();
    private readonly TextBox _fullConfirmBox = new() { Width = 120 };
    private readonly Button _fullRunButton = new();

    // ---- Tehlikeli bölge ----
    private Button _deleteDatabaseButton = null!;

    private readonly RichTextBox _logBox = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        BackColor = Color.White,
        Font = new Font("Consolas", 9F),
        BorderStyle = BorderStyle.FixedSingle
    };

    public MainForm()
    {
        Text = "ŞahinSoft Veritabanı Araçları";
        Width = 1000;
        Height = 920;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 700);
        Font = new Font("Segoe UI", 9F);
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // Kritik değil.
        }

        _windowsAuthRadio.CheckedChanged += (_, _) => UpdateAuthFieldsEnabled();
        _sqlAuthRadio.CheckedChanged += (_, _) => UpdateAuthFieldsEnabled();
        UpdateAuthFieldsEnabled();

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(16) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

        root.Controls.Add(BuildConnectionGroup(), 0, 0);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildMaintenanceTab());
        tabs.TabPages.Add(BuildMovementWipeTab());
        tabs.TabPages.Add(BuildFullWipeTab());
        root.Controls.Add(tabs, 0, 1);

        root.Controls.Add(BuildDangerZone(), 0, 2);

        var logLabel = new Label { Text = "İşlem Kaydı", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(0, 10, 0, 4) };
        var logHost = new Panel { Dock = DockStyle.Fill };
        logHost.Controls.Add(_logBox);
        var logPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        logPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        logPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        logPanel.Controls.Add(logLabel, 0, 0);
        logPanel.Controls.Add(logHost, 0, 1);
        root.Controls.Add(logPanel, 0, 3);

        Controls.Add(root);

        AppendLog("Hazır. Sunucu/veritabanı bilgilerini kontrol edip bir işlem seçin.", LogKind.Info);
        UpdateRunButtonsEnabled();
    }

    // ---------------------------------------------------------------- Bağlantı

    private GroupBox BuildConnectionGroup()
    {
        var group = new GroupBox { Text = "Veritabanı Bağlantısı", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));

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
            else
            {
                layout.SetColumnSpan(input, 2);
            }
        }

        AddRow("Sunucu Adresi", _serverBox);
        AddRow("Veritabanı Adı", _databaseBox);

        var authPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        authPanel.Controls.Add(_sqlAuthRadio);
        _windowsAuthRadio.Margin = new Padding(16, 3, 0, 0);
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

        var browseAppSettingsButton = new Button { Text = "Gözat...", AutoSize = true };
        browseAppSettingsButton.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "appsettings.json|appsettings.json|Tüm dosyalar (*.*)|*.*",
                FileName = "appsettings.json",
                CheckFileExists = false
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _appSettingsPathBox.Text = dialog.FileName;
            }
        };
        AddRow("appsettings.json Yolu", _appSettingsPathBox, browseAppSettingsButton);

        _saveConnectionButton.Click += (_, _) => SaveConnectionString();
        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.Controls.Add(_saveConnectionButton, 1, layout.RowCount - 1);

        AddRow("Yedek Dosya Yolu", _backupPathBox);
        var backupHint = new Label
        {
            Text = "Sadece hareket/tam sıfırlama işlemleri öncesi otomatik yedek için kullanılır. Boş/varsayılan bırakılırsa sunucunun SQL Server yedek klasörü otomatik tespit edilir.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Font = new Font("Segoe UI", 8F),
            MaximumSize = new Size(780, 0)
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

    private (string server, string database, string user, string password, bool useWindowsAuth)? ReadInputs()
    {
        var server = _serverBox.Text.Trim();
        var database = _databaseBox.Text.Trim();
        var useWindowsAuth = _windowsAuthRadio.Checked;
        var user = _userBox.Text.Trim();
        var password = _passwordBox.Text;

        if (server.Length == 0 || database.Length == 0)
        {
            AppendLog("Sunucu ve veritabanı adı boş olamaz.", LogKind.Error);
            return null;
        }
        if (!useWindowsAuth && user.Length == 0)
        {
            AppendLog("SQL Server kimlik doğrulamasında kullanıcı adı boş olamaz.", LogKind.Error);
            return null;
        }

        return (server, database, user, password, useWindowsAuth);
    }

    private async void TestButton_Click(object? sender, EventArgs e)
    {
        var inputs = ReadInputs();
        if (inputs is null)
        {
            UpdateRunButtonsEnabled();
            return;
        }

        SetConnectionStatus("Bağlantı deneniyor...", isError: false);
        Cursor = Cursors.WaitCursor;
        _testButton.Enabled = false;
        try
        {
            var (server, database, user, password, useWindowsAuth) = inputs.Value;
            var (ok, message) = await Task.Run(() =>
            {
                var success = DbOperations.TestConnection(server, database, user, password, useWindowsAuth, out var msg);
                return (success, msg);
            });
            SetConnectionStatus(message, isError: !ok);
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

    // Eskiden ayrı bir exe (SahinSoft.ConfigTool) olan işlev - web uygulamasının appsettings.json'daki
    // bağlantı dizesini, yukarıdaki Sunucu/Veritabanı/Kullanıcı/Şifre alanlarıyla günceller.
    private void SaveConnectionString()
    {
        var inputs = ReadInputs();
        if (inputs is null)
        {
            return;
        }

        var path = _appSettingsPathBox.Text.Trim();
        if (path.Length == 0)
        {
            AppendLog("appsettings.json yolu boş olamaz.", LogKind.Error);
            return;
        }

        try
        {
            var (server, database, user, password, useWindowsAuth) = inputs.Value;
            var connectionString = DbOperations.BuildConnectionString(server, database, user, password, useWindowsAuth);

            System.Text.Json.Nodes.JsonNode root;
            if (File.Exists(path))
            {
                var existingText = File.ReadAllText(path);
                root = string.IsNullOrWhiteSpace(existingText)
                    ? new System.Text.Json.Nodes.JsonObject()
                    : System.Text.Json.Nodes.JsonNode.Parse(existingText) ?? new System.Text.Json.Nodes.JsonObject();
            }
            else
            {
                root = new System.Text.Json.Nodes.JsonObject();
            }

            if (root["ConnectionStrings"] is not System.Text.Json.Nodes.JsonObject connectionStrings)
            {
                connectionStrings = new System.Text.Json.Nodes.JsonObject();
                root["ConnectionStrings"] = connectionStrings;
            }
            connectionStrings["DefaultConnection"] = connectionString;

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, root.ToJsonString(options));

            AppendLog($"appsettings.json güncellendi ({path}). Değişikliğin etkili olması için IIS uygulama havuzunu yeniden başlatın.", LogKind.Ok);
        }
        catch (Exception ex)
        {
            AppendLog($"HATA: appsettings.json kaydedilemedi: {ex.Message}", LogKind.Error);
        }
    }

    // ---------------------------------------------------------------- Tab 1: Oluştur / Bakım

    private TabPage BuildMaintenanceTab()
    {
        var page = new TabPage("1) Veritabanı Oluştur / Bakım");
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true, Padding = new Padding(10) };

        var createGroup = new GroupBox { Text = "Veritabanı Oluştur", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10) };
        var createPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };

        _createIfMissingButton = MakeActionButton("Veritabanı Oluştur\r\n(Yoksa)", BrandGold);
        _createIfMissingButton.Click += async (_, _) => await RunCreateIfMissingAsync();

        _recreateFromScratchButton = MakeActionButton("Sıfırdan Oluştur\r\n(MEVCUDU SİL)", Color.FromArgb(150, 40, 40));
        _recreateFromScratchButton.ForeColor = Color.White;
        _recreateFromScratchButton.Click += async (_, _) => await RunRecreateFromScratchAsync();

        _updateButton = MakeActionButton("Güncelle\r\n(Var Olanı Koru)", Color.FromArgb(70, 130, 100));
        _updateButton.ForeColor = Color.White;
        _updateButton.Click += async (_, _) => await RunUpdateAsync();

        _backupButton = MakeActionButton("Yedek Al...", Color.FromArgb(60, 90, 140));
        _backupButton.ForeColor = Color.White;
        _backupButton.Click += async (_, _) => await RunBackupAsync();

        createPanel.Controls.Add(_createIfMissingButton);
        createPanel.Controls.Add(_updateButton);
        createPanel.Controls.Add(_backupButton);
        createPanel.Controls.Add(_recreateFromScratchButton);
        createGroup.Controls.Add(createPanel);
        createGroup.Controls.Add(new Label
        {
            Text = "\"Oluştur (Yoksa)\": isim boşsa boş bir veritabanı açar, tabloları kurmak için ŞahinSoft uygulamasını bir kez çalıştırın.\n\"Güncelle\": bozuksa onarır, migration'ları uygular, küçültür - MEVCUT VERİYİ KORUR.\n\"Sıfırdan Oluştur\": veritabanını TAMAMEN SİLİP boş yeniden kurar.",
            Dock = DockStyle.Bottom,
            AutoSize = true,
            ForeColor = Color.DimGray,
            Font = new Font("Segoe UI", 8F),
            Padding = new Padding(0, 6, 0, 0)
        });
        layout.Controls.Add(createGroup);

        var maintGroup = new GroupBox { Text = "Bakım İşlemleri", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10), Margin = new Padding(0, 10, 0, 0) };
        var maintPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        maintPanel.Controls.Add(_maintCheckDb);
        maintPanel.Controls.Add(_maintLogCleanup);
        maintPanel.Controls.Add(_maintIndex);
        maintPanel.Controls.Add(_maintShrink);
        _maintRunButton.Margin = new Padding(0, 10, 0, 0);
        _maintRunButton.BackColor = Color.FromArgb(60, 90, 140);
        _maintRunButton.ForeColor = Color.White;
        _maintRunButton.FlatStyle = FlatStyle.Flat;
        _maintRunButton.Click += async (_, _) => await RunMaintenanceAsync();
        maintPanel.Controls.Add(_maintRunButton);
        maintGroup.Controls.Add(maintPanel);
        layout.Controls.Add(maintGroup);

        page.Controls.Add(layout);
        return page;
    }

    private static Button MakeActionButton(string text, Color backColor) => new()
    {
        Text = text,
        Width = 170,
        Height = 56,
        BackColor = backColor,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        Margin = new Padding(0, 0, 12, 12),
        Cursor = Cursors.Hand
    };

    private async Task RunCreateIfMissingAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null) return;

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password, useWindowsAuth) = inputs.Value;
            var created = DbOperations.CreateDatabaseIfNotExists(server, user, password, database, LogFromWorker, useWindowsAuth);
            if (created)
            {
                LogFromWorker("[OK] Oluşturuldu (boş). Tabloları kurmak için ŞahinSoft uygulamasını bir kez çalıştırın (migration otomatik uygulanır).");
            }
        }, "İşlem tamamlandı.");
    }

    private async Task RunRecreateFromScratchAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null) return;

        var confirm = MessageBox.Show(
            this,
            $"'{inputs.Value.database}' veritabanı (varsa) TAMAMEN SİLİNİP sıfırdan boş olarak yeniden oluşturulacak.\n\nİçindeki TÜM veri (stok, cari, hareketler) kaybolur. Devam edilsin mi?",
            "Veritabanını Sıfırdan Oluştur",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes) return;

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password, useWindowsAuth) = inputs.Value;
            DbOperations.DropDatabaseIfExists(server, user, password, database, LogFromWorker, useWindowsAuth);
            DbOperations.CreateDatabaseIfNotExists(server, user, password, database, LogFromWorker, useWindowsAuth);
            DbOperations.RunMigration(server, user, password, database, LogFromWorker, useWindowsAuth);
            DbOperations.RecycleIisAppPool(LogFromWorker);
        }, "Veritabanı sıfırdan oluşturuldu.");
    }

    private async Task RunUpdateAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null) return;

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password, useWindowsAuth) = inputs.Value;
            DbOperations.RepairIfCorrupt(server, user, password, database, LogFromWorker, useWindowsAuth);
            DbOperations.CreateDatabaseIfNotExists(server, user, password, database, LogFromWorker, useWindowsAuth);
            DbOperations.RunMigration(server, user, password, database, LogFromWorker, useWindowsAuth);
            DbOperations.ShrinkDatabase(server, user, password, database, LogFromWorker, useWindowsAuth);
            DbOperations.RecycleIisAppPool(LogFromWorker);
        }, "Veritabanı güncellendi, mevcut veri korundu.");
    }

    private async Task RunBackupAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null) return;

        var defaultDir = @"C:\SitesSahinSoft\Yedekler";
        var defaultName = $"{inputs.Value.database}_{DateTime.Now:yyyyMMdd_HHmm}.bak";

        using var dialog = new SaveFileDialog
        {
            Title = "Yedek Dosyası Kaydet",
            Filter = "SQL Server Yedeği (*.bak)|*.bak",
            FileName = defaultName,
            InitialDirectory = Directory.Exists(defaultDir) ? defaultDir : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password, useWindowsAuth) = inputs.Value;
            DbOperations.BackupDatabase(server, user, password, database, dialog.FileName, LogFromWorker, useWindowsAuth);
        }, "Yedek alma tamamlandı.");
    }

    private async Task RunMaintenanceAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null) return;
        if (!_maintCheckDb.Checked && !_maintLogCleanup.Checked && !_maintIndex.Checked && !_maintShrink.Checked)
        {
            MessageBox.Show(this, "En az bir bakım işlemi seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password, useWindowsAuth) = inputs.Value;
            if (_maintCheckDb.Checked)
            {
                DbOperations.CheckIntegrity(server, user, password, database, useWindowsAuth, LogFromWorker);
            }
            if (_maintLogCleanup.Checked)
            {
                DbOperations.CleanupTransactionLog(server, user, password, database, useWindowsAuth, LogFromWorker);
            }
            if (_maintIndex.Checked)
            {
                DbOperations.RunIndexMaintenance(server, user, password, database, useWindowsAuth, LogFromWorker);
            }
            if (_maintShrink.Checked)
            {
                DbOperations.ShrinkDatabase(server, user, password, database, LogFromWorker, useWindowsAuth);
            }
        }, "Bakım işlemleri tamamlandı.");
    }

    // ---------------------------------------------------------------- Tab 2 / Tab 3: Sil

    private TabPage BuildMovementWipeTab()
    {
        var page = new TabPage("2) Tüm Hareketleri Sil");
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true, Padding = new Padding(10) };

        layout.Controls.Add(new Label
        {
            Text = "Cari ve stok KARTLARI (isim/kod/tanımlar) silinmez; sadece geçmiş belge ve hareketleri temizlenir, bakiyeler ve stok miktarları sıfırlanır.",
            AutoSize = true,
            MaximumSize = new Size(900, 0),
            Margin = new Padding(0, 0, 0, 10)
        });

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            MaximumSize = new Size(900, 0),
            ForeColor = Color.FromArgb(178, 34, 34),
            Text = "SİLİNECEKLER: Faturalar, Teklifler, Siparişler, İrsaliyeler, Stok Fişleri/Transferleri/Sayımları,\n" +
                   "Tahsilat/Tediye Fişleri, Masraflar, Çek/Senetler, Stok/Cari Hareketleri, Fiyat Listesi hareketleri,\n" +
                   "Döviz kuru geçmişi, Denetim Kayıtları, Yazdırma Kuyruğu kayıtları, TÜM RESTORAN hareketleri\n" +
                   "(Adisyon/Sipariş/Mutfak Fişi/Ödeme/Vardiya/Z Dönemleri/Perakende Fişler/Paket Siparişleri).\n" +
                   "Ayrıca: tüm stok miktarları 0'a, numaralandırma sayaçları 1'e sıfırlanır.\n\n" +
                   "KORUNUR: Cari kartları, Stok kartları, Masa/Şube/Depo/Yazıcı/Mutfak İstasyonu tanımları, tüm program ayarları."
        });

        layout.Controls.Add(BuildConfirmRow(_movementConfirmBox, _movementRunButton, "YEDEK AL VE HAREKETLERİ SİL"));
        _movementConfirmBox.TextChanged += (_, _) => UpdateRunButtonsEnabled();
        _movementRunButton.Click += async (_, _) => await RunWipeFlowAsync(isFullWipe: false);

        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildFullWipeTab()
    {
        var page = new TabPage("3) Veritabanını Temizle (Hareket+Cari+Stok)");
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true, Padding = new Padding(10) };

        layout.Controls.Add(new Label
        {
            Text = "Tab 2'deki her şeye ek olarak cari ve stok KARTLARININ kendisi de kalıcı olarak silinir. Program ayarlarına dokunulmaz.",
            AutoSize = true,
            MaximumSize = new Size(900, 0),
            Margin = new Padding(0, 0, 0, 10)
        });

        var panel = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.Controls.Add(new Label
        {
            AutoSize = true,
            MaximumSize = new Size(420, 0),
            ForeColor = Color.FromArgb(178, 34, 34),
            Text = "KALICI OLARAK SİLİNECEKLER:\n" +
                   "• Tüm Cariler (müşteri/tedarikçi + hareketleri)\n" +
                   "• Tüm Stok Kartları (varyant/barkod/resim/reçete dahil)\n" +
                   "• Tüm Faturalar ve Stok Hareketleri\n" +
                   "• Tüm Tahsilat / Tediye Fişleri\n" +
                   "• Tüm Teklifler, Siparişler, İrsaliyeler\n" +
                   "• Tüm Stok Fişleri / Transferleri / Sayımları\n" +
                   "• Tüm Alış/Satış Fiyat Listesi hareketleri\n" +
                   "• Tüm Masraflar, Çek/Senetler\n" +
                   "• Döviz kuru geçmişi, Denetim Kayıtları, Yazdırma Kuyruğu\n" +
                   "• TÜM RESTORAN hareketleri (Adisyon/Sipariş/Mutfak/Ödeme/\n  Vardiya/Z Dönemi/Perakende Fiş/Paket Sipariş)"
        }, 0, 0);
        panel.Controls.Add(new Label
        {
            AutoSize = true,
            MaximumSize = new Size(420, 0),
            ForeColor = Color.FromArgb(31, 122, 77),
            Text = "KORUNACAKLAR (dokunulmaz):\n" +
                   "• Kullanıcılar ve Yetkiler (Roller)\n" +
                   "• Depo / Şube Tanımları\n" +
                   "• KDV Oranları, Birimler, Para Birimleri\n" +
                   "• Kasa/Banka Tanımları, Maliyet Merkezi/Proje\n" +
                   "• Stok Kategori/Renk Tanımları, Masraf Kategorileri\n" +
                   "• Fiyat Listesi Tanımları, Şirket Bilgileri\n" +
                   "• Masa/Salon, Mutfak İstasyonu, Yazıcı ve Çıktı Şablonu Tanımları\n" +
                   "• Stok Ayarları\n" +
                   "• Numaralandırma Ayarları (sayaçlar 1'e sıfırlanır)"
        }, 1, 0);
        layout.Controls.Add(panel);

        layout.Controls.Add(BuildConfirmRow(_fullConfirmBox, _fullRunButton, "YEDEK AL VE TÜM VERİLERİ SIFIRLA"));
        _fullConfirmBox.TextChanged += (_, _) => UpdateRunButtonsEnabled();
        _fullRunButton.Click += async (_, _) => await RunWipeFlowAsync(isFullWipe: true);

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
        runButton.FlatStyle = FlatStyle.Flat;
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

    private async Task RunWipeFlowAsync(bool isFullWipe)
    {
        var inputs = ReadInputs();
        if (inputs is null) return;

        var (server, database, user, password, useWindowsAuth) = inputs.Value;
        var scopeText = isFullWipe
            ? "TÜM cari, stok ve hareket verisi"
            : "TÜM hareket ve belge geçmişi (cari/stok kartları KALIR, bakiyeleri sıfırlanır)";
        var confirm = MessageBox.Show(this,
            $"\"{database}\" veritabanında {scopeText} kalıcı olarak silinecek.\n\n" +
            "Program ayarları (kullanıcılar, depo/şube/KDV/birim/kasa-banka tanımları vb.) korunacaktır.\n\n" +
            "İşlemden önce otomatik bir yedek alınacaktır. Devam edilsin mi?",
            "Son Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes) return;

        await RunWithButtonsDisabledAsync(() =>
        {
            var backupPath = ResolveBackupPath(server, database, user, password, useWindowsAuth);
            DbOperations.BackupDatabase(server, user, password, database, backupPath, LogFromWorker, useWindowsAuth);

            if (isFullWipe)
            {
                DbOperations.ClearFullData(server, user, password, database, LogFromWorker, useWindowsAuth);
            }
            else
            {
                DbOperations.ClearTransactionalData(server, user, password, database, LogFromWorker, useWindowsAuth);
            }
        }, "İşlem tamamlandı.");
    }

    private string ResolveBackupPath(string server, string database, string user, string password, bool useWindowsAuth)
    {
        var customPath = _backupPathBox.Text.Trim();
        var fileName = $"{database}_SifirlamaOncesiYedek_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

        if (customPath.Length > 0 && !customPath.StartsWith("(", StringComparison.Ordinal))
        {
            return customPath.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)
                ? customPath
                : Path.Combine(customPath, fileName);
        }

        var detectedDir = DbOperations.TryDetectBackupDirectory(server, user, password, useWindowsAuth)
            ?? throw new InvalidOperationException("Sunucunun varsayılan yedek klasörü tespit edilemedi. Lütfen 'Yedek Dosya Yolu' alanına elle bir yol girin.");
        return Path.Combine(detectedDir, fileName);
    }

    // ---------------------------------------------------------------- Tehlikeli bölge

    private FlowLayoutPanel BuildDangerZone()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 10, 0, 0) };

        var label = new Label
        {
            Text = "Tehlikeli bölge:",
            AutoSize = true,
            Margin = new Padding(0, 10, 12, 0),
            ForeColor = Color.Firebrick,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
        };

        _deleteDatabaseButton = MakeActionButton("Veritabanını Sil\r\n(Yeniden Oluşturmaz)", Color.FromArgb(90, 30, 30));
        _deleteDatabaseButton.Height = 44;
        _deleteDatabaseButton.ForeColor = Color.White;
        _deleteDatabaseButton.Click += async (_, _) => await RunDeleteDatabaseAsync();

        panel.Controls.Add(label);
        panel.Controls.Add(_deleteDatabaseButton);
        return panel;
    }

    private async Task RunDeleteDatabaseAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null) return;

        var confirm = MessageBox.Show(
            this,
            $"'{inputs.Value.database}' veritabanı TAMAMEN SİLİNECEK ve YENİDEN OLUŞTURULMAYACAK.\n\nBu işlemden sonra veritabanı hiç olmayacak - tekrar kullanmak için \"Veritabanı Oluştur\" gerekir.\n\nDevam edilsin mi?",
            "Veritabanını Sil",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes) return;

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password, useWindowsAuth) = inputs.Value;
            DbOperations.DropDatabaseIfExists(server, user, password, database, LogFromWorker, useWindowsAuth);
        }, "Veritabanı silindi.");
    }

    // ---------------------------------------------------------------- Ortak

    private async Task RunWithButtonsDisabledAsync(Action work, string successMessage)
    {
        SetButtonsEnabled(false);
        _logBox.Clear();
        try
        {
            await Task.Run(work);
            AppendLog(successMessage, LogKind.Ok);
        }
        catch (Exception ex)
        {
            AppendLog($"HATA: {ex.Message}", LogKind.Error);
        }
        finally
        {
            SetButtonsEnabled(true);
            UpdateRunButtonsEnabled();
        }
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _createIfMissingButton.Enabled = enabled;
        _recreateFromScratchButton.Enabled = enabled;
        _updateButton.Enabled = enabled;
        _backupButton.Enabled = enabled;
        _maintRunButton.Enabled = enabled;
        _deleteDatabaseButton.Enabled = enabled;
        _saveConnectionButton.Enabled = enabled;
        _testButton.Enabled = enabled;
        if (enabled)
        {
            UpdateRunButtonsEnabled();
        }
        else
        {
            _movementRunButton.Enabled = false;
            _fullRunButton.Enabled = false;
        }
    }

    private void UpdateRunButtonsEnabled()
    {
        _movementRunButton.Enabled = string.Equals(_movementConfirmBox.Text.Trim(), ConfirmPhrase, StringComparison.Ordinal);
        _fullRunButton.Enabled = string.Equals(_fullConfirmBox.Text.Trim(), ConfirmPhrase, StringComparison.Ordinal);
    }

    // DbOperations arka plan thread'inde (Task.Run içinde) çalışıyor - UI kontrollerine oradan
    // doğrudan dokunulamaz, Invoke ile ana thread'e geçiyoruz.
    private void LogFromWorker(string message)
    {
        var kind = message.Contains("[HATA]", StringComparison.Ordinal) ? LogKind.Error
            : message.Contains("[UYARI]", StringComparison.Ordinal) ? LogKind.Warn
            : message.Contains("[OK]", StringComparison.Ordinal) ? LogKind.Ok
            : message.StartsWith("==>", StringComparison.Ordinal) ? LogKind.Step
            : LogKind.Info;

        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLog(message, kind));
        }
        else
        {
            AppendLog(message, kind);
        }
    }

    private enum LogKind { Info, Ok, Warn, Error, Step }

    private void AppendLog(string text, LogKind kind)
    {
        var color = kind switch
        {
            LogKind.Ok => Color.SeaGreen,
            LogKind.Warn => Color.DarkOrange,
            LogKind.Error => Color.Firebrick,
            LogKind.Step => Color.FromArgb(150, 110, 20),
            _ => Color.Black
        };

        _logBox.SelectionStart = _logBox.TextLength;
        _logBox.SelectionLength = 0;
        _logBox.SelectionColor = color;
        _logBox.SelectionFont = kind == LogKind.Step ? new Font(_logBox.Font, FontStyle.Bold) : _logBox.Font;
        _logBox.AppendText(text + Environment.NewLine);
        _logBox.ScrollToCaret();
    }
}
