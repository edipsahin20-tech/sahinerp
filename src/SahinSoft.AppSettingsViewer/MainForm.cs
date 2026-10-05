using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SahinSoft.AppSettingsViewer;

// Edip, 2026-10-02: "appsettings.json doğru şekilde okuyacak exe, içeriğini doğru göstersin" -
// appsettings.json'daki şifre gibi değerler .NET'in varsayılan JSON yazıcısı yüzünden
// "Ornek123+*" gibi Unicode-kaçışlı görünüyordu (bkz. ConfigTool'un kendi yazdığı dosya) -
// bu TEKNİK OLARAK doğru JSON ama insan gözüyle okuması kafa karıştırıcı. Bu araç dosyayı aynen
// PARSE edip, kaçışsız (UnsafeRelaxedJsonEscaping) ve girintili olarak geri yazdırır - yani
// "Ornek123+*" gibi gerçek haliyle gösterir. Sadece GÖRÜNTÜLER, dosyayı DEĞİŞTİRMEZ.
public sealed class MainForm : Form
{
    private readonly TextBox _contentBox = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Both,
        WordWrap = false,
        Dock = DockStyle.Fill,
        Font = new Font("Consolas", 10F),
        BackColor = Color.White
    };

    private readonly Label _pathLabel = new()
    {
        Dock = DockStyle.Top,
        Height = 28,
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(8, 0, 8, 0),
        ForeColor = Color.DimGray
    };

    private static readonly JsonSerializerOptions DisplayOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public MainForm(string? initialFilePath)
    {
        Text = "ŞahinSoft - Ayar Dosyası Görüntüleyici";
        Width = 900;
        Height = 700;
        MinimumSize = new Size(500, 350);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        AllowDrop = true;
        Font = new Font("Segoe UI", 9F);

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
        var openButton = new Button { Text = "Dosya Aç...", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
        openButton.Click += (_, _) => OpenViaDialog();
        var refreshButton = new Button { Text = "Yenile", AutoSize = true, Padding = new Padding(8, 4, 8, 4), Left = 100 };
        refreshButton.Click += (_, _) => { if (_currentPath is not null) LoadFile(_currentPath); };
        topPanel.Controls.Add(openButton);
        topPanel.Controls.Add(refreshButton);
        refreshButton.Location = new Point(openButton.Right + 8, 8);

        Controls.Add(_contentBox);
        Controls.Add(_pathLabel);
        Controls.Add(topPanel);

        DragEnter += (_, e) =>
        {
            e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
        };
        DragDrop += (_, e) =>
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
            {
                LoadFile(files[0]);
            }
        };

        if (!string.IsNullOrWhiteSpace(initialFilePath) && File.Exists(initialFilePath))
        {
            LoadFile(initialFilePath);
        }
        else
        {
            // Edip'in makinesindeki en olası konum - varsa otomatik açılır, kullanıcı hiç
            // "Dosya Aç" yapmak zorunda kalmaz; yoksa sessizce boş ekranla başlar.
            var likelyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(likelyPath))
            {
                LoadFile(likelyPath);
            }
            else
            {
                _pathLabel.Text = "Bir appsettings.json (veya herhangi bir .json) dosyası sürükleyip bırakın, ya da \"Dosya Aç...\" ile seçin.";
            }
        }
    }

    private string? _currentPath;

    private void OpenViaDialog()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "JSON dosyaları (*.json)|*.json|Tüm dosyalar (*.*)|*.*",
            FileName = "appsettings.json",
            InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadFile(dialog.FileName);
        }
    }

    private void LoadFile(string path)
    {
        try
        {
            var rawText = File.ReadAllText(path);
            var node = string.IsNullOrWhiteSpace(rawText) ? new JsonObject() : (JsonNode.Parse(rawText) ?? new JsonObject());
            _contentBox.Text = node.ToJsonString(DisplayOptions);
            _currentPath = path;
            _pathLabel.Text = path;
            _pathLabel.ForeColor = Color.DimGray;
        }
        catch (Exception ex)
        {
            _contentBox.Text = string.Empty;
            _pathLabel.Text = $"\"{path}\" okunamadı: {ex.Message}";
            _pathLabel.ForeColor = Color.Firebrick;
        }
    }
}
