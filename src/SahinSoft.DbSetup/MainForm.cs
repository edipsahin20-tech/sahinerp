namespace SahinSoft.DbSetup;

// Edip 3 araca (Kurulum, Veritabanı, Restoran programı) indirmek istedi - eskiden ayrı bir exe
// olan SahinSoft.ConfigTool'un (appsettings.json bağlantı dizesi ayarı) işlevi buraya taşındı,
// ConfigTool artık paketlenmiyor. Üç ayrı DB işlemi - Oluştur (sıfırdan, eskiyi siler), Güncelle
// (var olan veriyi koruyarak sadece bekleyen migration'ları uygular), Yedek Al.
public sealed class MainForm : Form
{
    private static readonly Color BrandGold = Color.FromArgb(212, 164, 55);

    private readonly TextBox _serverBox = new() { Text = "localhost" };
    private readonly TextBox _databaseBox = new() { Text = "SahinSoftDb" };
    private readonly TextBox _userBox = new() { Text = "sa" };
    private readonly TextBox _passwordBox = new() { Text = "SahinSoft2026!Kurulum", UseSystemPasswordChar = true };
    private readonly TextBox _appSettingsPathBox = new() { Text = @"C:\SitesSahinSoft\appsettings.json" };

    private readonly Button _createButton;
    private readonly Button _updateButton;
    private readonly Button _backupButton;
    private readonly Button _clearTransactionsButton;
    private readonly Button _deleteDatabaseButton;
    private readonly Button _saveConnectionButton;
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
        Text = "ŞahinSoft Veritabanı Aracı";
        Width = 640;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 9F);
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // Kritik değil.
        }

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            Padding = new Padding(16, 16, 16, 8),
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

        void AddRow(string label, Control input, Control? extra = null)
        {
            var row = layout.RowCount;
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.Controls.Add(new Label { Text = label, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.Left, Padding = new Padding(0, 6, 0, 0) }, 0, row);
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
        AddRow("Kullanıcı Adı", _userBox);
        AddRow("Şifre", _passwordBox);

        var browseAppSettingsButton = new Button { Text = "Gözat..." };
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

        _saveConnectionButton = new Button
        {
            Text = "Bağlantıyı appsettings.json'a Kaydet",
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 4),
            Margin = new Padding(146, 4, 0, 8)
        };
        _saveConnectionButton.Click += (_, _) => SaveConnectionString();
        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.Controls.Add(_saveConnectionButton, 1, layout.RowCount - 1);
        layout.SetColumnSpan(_saveConnectionButton, 2);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(16, 4, 16, 4)
        };

        _createButton = MakeActionButton("Veritabanı Oluştur\r\n(Sıfırdan)", BrandGold);
        _createButton.Click += async (_, _) => await RunCreateAsync();

        _updateButton = MakeActionButton("Güncelle\r\n(Var Olanı Koru)", Color.FromArgb(70, 130, 100));
        _updateButton.ForeColor = Color.White;
        _updateButton.Click += async (_, _) => await RunUpdateAsync();

        _backupButton = MakeActionButton("Yedek Al...", Color.FromArgb(60, 90, 140));
        _backupButton.ForeColor = Color.White;
        _backupButton.Click += async (_, _) => await RunBackupAsync();

        buttonPanel.Controls.Add(_createButton);
        buttonPanel.Controls.Add(_updateButton);
        buttonPanel.Controls.Add(_backupButton);

        var dangerLabel = new Label
        {
            Text = "Dikkatli kullanın:",
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(16, 4, 0, 2),
            ForeColor = Color.Firebrick,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
        };

        var dangerPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(16, 0, 16, 8)
        };

        _clearTransactionsButton = MakeActionButton("Hareketleri Sil\r\n(Kartlar Kalır)", Color.FromArgb(180, 110, 30));
        _clearTransactionsButton.ForeColor = Color.White;
        _clearTransactionsButton.Click += async (_, _) => await RunClearTransactionsAsync();

        _deleteDatabaseButton = MakeActionButton("Veritabanını Sil", Color.FromArgb(150, 40, 40));
        _deleteDatabaseButton.ForeColor = Color.White;
        _deleteDatabaseButton.Click += async (_, _) => await RunDeleteDatabaseAsync();

        dangerPanel.Controls.Add(_clearTransactionsButton);
        dangerPanel.Controls.Add(_deleteDatabaseButton);

        var logLabel = new Label
        {
            Text = "İşlem Kaydı",
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(16, 4, 0, 4),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        var logHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 16) };
        logHost.Controls.Add(_logBox);

        Controls.Add(logHost);
        Controls.Add(logLabel);
        Controls.Add(dangerPanel);
        Controls.Add(dangerLabel);
        Controls.Add(buttonPanel);
        Controls.Add(layout);

        AppendLog("Hazır. Sunucu/veritabanı bilgilerini kontrol edip bir işlem seçin.", LogKind.Info);
    }

    private static Button MakeActionButton(string text, Color backColor) => new()
    {
        Text = text,
        Width = 170,
        Height = 56,
        BackColor = backColor,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        Margin = new Padding(0, 0, 12, 0),
        Cursor = Cursors.Hand
    };

    private (string server, string database, string user, string password)? ReadInputs()
    {
        var server = _serverBox.Text.Trim();
        var database = _databaseBox.Text.Trim();
        var user = _userBox.Text.Trim();
        var password = _passwordBox.Text;

        if (server.Length == 0 || database.Length == 0 || user.Length == 0)
        {
            AppendLog("Sunucu, veritabanı adı ve kullanıcı adı boş olamaz.", LogKind.Error);
            return null;
        }

        return (server, database, user, password);
    }

    private async Task RunCreateAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null)
        {
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"'{inputs.Value.database}' veritabanı (varsa) TAMAMEN SİLİNİP sıfırdan boş olarak yeniden oluşturulacak.\n\nİçindeki TÜM veri (stok, cari, hareketler) kaybolur. Devam edilsin mi?",
            "Veritabanını Sıfırdan Oluştur",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password) = inputs.Value;
            DbOperations.DropDatabaseIfExists(server, user, password, database, LogFromWorker);
            DbOperations.CreateDatabaseIfNotExists(server, user, password, database, LogFromWorker);
            DbOperations.RunMigration(server, user, password, database, LogFromWorker);
            DbOperations.RecycleIisAppPool(LogFromWorker);
        }, "Veritabanı sıfırdan oluşturuldu.");
    }

    private async Task RunUpdateAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null)
        {
            return;
        }

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password) = inputs.Value;
            DbOperations.RepairIfCorrupt(server, user, password, database, LogFromWorker);
            DbOperations.CreateDatabaseIfNotExists(server, user, password, database, LogFromWorker);
            DbOperations.RunMigration(server, user, password, database, LogFromWorker);
            DbOperations.ShrinkDatabase(server, user, password, database, LogFromWorker);
            DbOperations.RecycleIisAppPool(LogFromWorker);
        }, "Veritabanı güncellendi, mevcut veri korundu.");
    }

    private async Task RunClearTransactionsAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null)
        {
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"'{inputs.Value.database}' veritabanındaki TÜM hareketler (fatura, sipariş, adisyon, stok/cari hareketi, kasa işlemleri vb.) silinecek.\n\nStok kartları, cari kartlar, kategori/KDV/şube/depo tanımları KORUNACAK. Numaratörler 1'e sıfırlanacak.\n\nDevam edilsin mi?",
            "Hareketleri Sil",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password) = inputs.Value;
            DbOperations.ClearTransactionalData(server, user, password, database, LogFromWorker);
            DbOperations.RecycleIisAppPool(LogFromWorker);
        }, "Hareketler temizlendi.");
    }

    private async Task RunDeleteDatabaseAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null)
        {
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"'{inputs.Value.database}' veritabanı TAMAMEN SİLİNECEK ve YENİDEN OLUŞTURULMAYACAK.\n\nBu işlemden sonra veritabanı hiç olmayacak - tekrar kullanmak için \"Veritabanı Oluştur\" gerekir.\n\nDevam edilsin mi?",
            "Veritabanını Sil",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password) = inputs.Value;
            DbOperations.DropDatabaseIfExists(server, user, password, database, LogFromWorker);
        }, "Veritabanı silindi.");
    }

    private async Task RunBackupAsync()
    {
        var inputs = ReadInputs();
        if (inputs is null)
        {
            return;
        }

        var defaultDir = @"C:\SitesSahinSoft\Yedekler";
        var defaultName = $"{inputs.Value.database}_{DateTime.Now:yyyyMMdd_HHmm}.bak";

        using var dialog = new SaveFileDialog
        {
            Title = "Yedek Dosyası Kaydet",
            Filter = "SQL Server Yedeği (*.bak)|*.bak",
            FileName = defaultName,
            InitialDirectory = Directory.Exists(defaultDir) ? defaultDir : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await RunWithButtonsDisabledAsync(() =>
        {
            var (server, database, user, password) = inputs.Value;
            DbOperations.BackupDatabase(server, user, password, database, dialog.FileName, LogFromWorker);
        }, "Yedek alma tamamlandı.");
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
            var (server, database, user, password) = inputs.Value;
            var connectionString = $"Server={server};Database={database};User Id={user};Password={password};TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";

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
        }
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _createButton.Enabled = enabled;
        _updateButton.Enabled = enabled;
        _backupButton.Enabled = enabled;
        _clearTransactionsButton.Enabled = enabled;
        _deleteDatabaseButton.Enabled = enabled;
        _saveConnectionButton.Enabled = enabled;
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
