using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.SqlClient;

namespace SahinSoft.ConfigTool;

public sealed class MainForm : Form
{
    private readonly TextBox _serverBox = new();
    private readonly TextBox _databaseBox = new() { Text = "SahinSoftDb" };
    private readonly RadioButton _windowsAuthRadio = new() { Text = "Windows Kimlik Doğrulaması", Checked = true, AutoSize = true };
    private readonly RadioButton _sqlAuthRadio = new() { Text = "SQL Server Kimlik Doğrulaması", AutoSize = true };
    private readonly TextBox _userBox = new() { Enabled = false };
    private readonly TextBox _passwordBox = new() { Enabled = false, UseSystemPasswordChar = true };
    private readonly TextBox _pathBox = new()
    {
        Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")
    };
    private readonly Label _statusLabel = new() { AutoSize = false, Height = 60, ForeColor = Color.DimGray };

    public MainForm()
    {
        Text = "ŞahinSoft Veritabanı Bağlantı Ayarları";
        Width = 520;
        Height = 480;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 9F);

        _windowsAuthRadio.CheckedChanged += (_, _) => UpdateAuthFieldsEnabled();
        _sqlAuthRadio.CheckedChanged += (_, _) => UpdateAuthFieldsEnabled();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            Padding = new Padding(16),
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
        }

        AddRow("Sunucu Adresi", _serverBox);
        AddRow("Veritabanı Adı", _databaseBox);

        var authPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        authPanel.Controls.Add(_windowsAuthRadio);
        authPanel.Controls.Add(_sqlAuthRadio);
        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.Controls.Add(new Label { Text = "Kimlik Doğrulama", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, layout.RowCount - 1);
        layout.Controls.Add(authPanel, 1, layout.RowCount - 1);
        layout.SetColumnSpan(authPanel, 2);

        AddRow("Kullanıcı Adı", _userBox);
        AddRow("Şifre", _passwordBox);

        var browseButton = new Button { Text = "Gözat..." };
        browseButton.Click += BrowseButton_Click;
        AddRow("appsettings.json Yolu", _pathBox, browseButton);

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        var testButton = new Button { Text = "Bağlantıyı Test Et", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
        testButton.Click += TestButton_Click;
        var saveButton = new Button { Text = "Kaydet", AutoSize = true, Padding = new Padding(8, 4, 8, 4), BackColor = Color.FromArgb(212, 164, 55) };
        saveButton.Click += SaveButton_Click;
        buttonPanel.Controls.Add(testButton);
        buttonPanel.Controls.Add(saveButton);

        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.Controls.Add(buttonPanel, 1, layout.RowCount - 1);
        layout.SetColumnSpan(buttonPanel, 2);

        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        layout.Controls.Add(_statusLabel, 0, layout.RowCount - 1);
        layout.SetColumnSpan(_statusLabel, 3);

        Controls.Add(layout);
    }

    private void UpdateAuthFieldsEnabled()
    {
        _userBox.Enabled = _sqlAuthRadio.Checked;
        _passwordBox.Enabled = _sqlAuthRadio.Checked;
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "appsettings.json|appsettings.json|Tüm dosyalar (*.*)|*.*",
            FileName = "appsettings.json",
            CheckFileExists = false
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _pathBox.Text = dialog.FileName;
        }
    }

    private bool TryBuildConnectionString(out string connectionString, out string error)
    {
        connectionString = string.Empty;
        error = string.Empty;

        var server = _serverBox.Text.Trim();
        var database = _databaseBox.Text.Trim();

        if (server.Length == 0)
        {
            error = "Sunucu adresi boş olamaz.";
            return false;
        }
        if (database.Length == 0)
        {
            error = "Veritabanı adı boş olamaz.";
            return false;
        }

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
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

    private void TestButton_Click(object? sender, EventArgs e)
    {
        if (!TryBuildConnectionString(out var connectionString, out var error))
        {
            SetStatus(error, isError: true);
            return;
        }

        SetStatus("Bağlantı deneniyor...", isError: false);
        Cursor = Cursors.WaitCursor;
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            SetStatus("Bağlantı başarılı.", isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Bağlantı başarısız: {ex.Message}", isError: true);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (!TryBuildConnectionString(out var connectionString, out var error))
        {
            SetStatus(error, isError: true);
            return;
        }

        var path = _pathBox.Text.Trim();
        if (path.Length == 0)
        {
            SetStatus("appsettings.json yolu boş olamaz.", isError: true);
            return;
        }

        try
        {
            JsonNode root;
            if (File.Exists(path))
            {
                var existingText = File.ReadAllText(path);
                root = string.IsNullOrWhiteSpace(existingText)
                    ? new JsonObject()
                    : JsonNode.Parse(existingText) ?? new JsonObject();
            }
            else
            {
                root = new JsonObject();
            }

            if (root["ConnectionStrings"] is not JsonObject connectionStrings)
            {
                connectionStrings = new JsonObject();
                root["ConnectionStrings"] = connectionStrings;
            }
            connectionStrings["DefaultConnection"] = connectionString;

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, root.ToJsonString(options));

            SetStatus("appsettings.json güncellendi. Değişikliğin etkili olması için IIS'i (iisreset) yeniden başlatın.", isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Kaydedilemedi: {ex.Message}", isError: true);
        }
    }

    private void SetStatus(string message, bool isError)
    {
        _statusLabel.Text = message;
        _statusLabel.ForeColor = isError ? Color.Firebrick : Color.SeaGreen;
    }
}
