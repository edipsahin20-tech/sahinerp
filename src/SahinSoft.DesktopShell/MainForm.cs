using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace SahinSoft.DesktopShell;

/// <summary>
/// ŞahinSoft'u gerçek bir Windows programı gibi açan ince bir kabuk. İçeride
/// tarayıcı, dışarıda kendi penceresi/görev çubuğu ikonu olan bir uygulama -
/// mevcut web uygulamasının UI'ı tekrar yazılmadan aynen kullanılıyor. Hangi
/// adrese baktığı web uygulamasıyla AYNI appsettings.json'daki "RestaurantShell"
/// bölümünden okunur (bkz. ShellConfig) - Edip'in isteği (2026-09-03): ayrı bir
/// ayar dosyası/penceresi yok, tek kaynak.
/// </summary>
public sealed class MainForm : Form
{
    // Marka renkleri - web uygulamasındaki --rs-navy/--rs-gold ile aynı (Edip, 2026-09-03:
    // "üst bar daha düzgün gözüksün karanlık ve ne olduğu belli değil" - jenerik koyu gri yerine
    // uygulamanın kendi lacivert/altın kimliği, artı ne olduğu belli olsun diye bir uygulama adı).
    private static readonly Color NavyBg = Color.FromArgb(11, 34, 57);
    private static readonly Color GoldAccent = Color.FromArgb(226, 164, 0);

    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private readonly Button _backButton = NavButton("◀");
    private readonly Button _forwardButton = NavButton("▶");
    private readonly Button _refreshButton = NavButton("⟳");
    private readonly Label _titleLabel = new()
    {
        AutoSize = true,
        ForeColor = GoldAccent,
        Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleLeft,
        Margin = new Padding(10, 11, 16, 0)
    };
    private readonly Label _statusLabel = new()
    {
        AutoSize = true,
        ForeColor = Color.FromArgb(180, 195, 210),
        Font = new Font("Segoe UI", 8.5F),
        TextAlign = ContentAlignment.MiddleLeft,
        Margin = new Padding(0, 13, 0, 0)
    };

    private readonly ShellConfig _config;

    private static Button NavButton(string text) => new()
    {
        Text = text,
        Width = 34,
        Height = 30,
        Margin = new Padding(2, 4, 2, 4),
        FlatStyle = FlatStyle.Flat,
        BackColor = Color.FromArgb(22, 50, 78),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 10F),
        Cursor = Cursors.Hand,
        FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(34, 66, 99) }
    };

    public MainForm()
    {
        _config = ShellConfig.Load();

        Text = _config.Title;
        _titleLabel.Text = _config.Title;
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // İkon çıkarılamazsa varsayılan .NET ikonuyla devam - kritik değil.
        }
        Width = 1280;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        Font = new Font("Segoe UI", 9F);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(6, 4, 6, 4),
            BackColor = NavyBg
        };

        _backButton.Click += (_, _) => { if (_webView.CanGoBack) _webView.GoBack(); };
        _forwardButton.Click += (_, _) => { if (_webView.CanGoForward) _webView.GoForward(); };
        _refreshButton.Click += (_, _) => _webView.Reload();

        toolbar.Controls.Add(_backButton);
        toolbar.Controls.Add(_forwardButton);
        toolbar.Controls.Add(_refreshButton);
        toolbar.Controls.Add(_titleLabel);
        toolbar.Controls.Add(_statusLabel);

        Controls.Add(_webView);
        Controls.Add(toolbar);

        Load += MainForm_Load;
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        await InitializeWebViewAsync();
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            SetStatus($"Bağlanılıyor: {_config.Url}");
            await _webView.EnsureCoreWebView2Async(null);
            _webView.CoreWebView2.NavigationCompleted += (_, args) =>
            {
                SetStatus(args.IsSuccess ? _config.Url : "Sayfa yüklenemedi - bağlantıyı kontrol edin.");
            };
            _webView.Source = new Uri(_config.Url);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            var result = MessageBox.Show(
                this,
                "Bu bilgisayarda Microsoft Edge WebView2 bileşeni kurulu değil. Şimdi indirme sayfasını açayım mı?",
                "WebView2 Gerekli",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://developer.microsoft.com/microsoft-edge/webview2/",
                    UseShellExecute = true
                });
            }
            Close();
        }
        catch (Exception ex)
        {
            SetStatus($"Bağlantı hatası: {ex.Message}");
        }
    }

    private void SetStatus(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => _statusLabel.Text = message);
            return;
        }
        _statusLabel.Text = message;
    }
}
