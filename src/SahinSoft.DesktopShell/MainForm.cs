using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace SahinSoft.DesktopShell;

/// <summary>
/// ŞahinSoft'u gerçek bir Windows programı gibi açan ince bir kabuk. İçeride
/// tarayıcı, dışarıda kendi penceresi/görev çubuğu ikonu olan bir uygulama -
/// mevcut web uygulamasının UI'ı tekrar yazılmadan aynen kullanılıyor. Hangi
/// adrese (yerel kurulum / merkez PC / bulut) baktığı shell.config.json'da
/// tutulur ve Ayarlar penceresinden değiştirilebilir.
/// </summary>
public sealed class MainForm : Form
{
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private readonly Button _backButton = new() { Text = "◀", Width = 36 };
    private readonly Button _forwardButton = new() { Text = "▶", Width = 36 };
    private readonly Button _refreshButton = new() { Text = "⟳", Width = 36 };
    private readonly Button _settingsButton = new() { Text = "⚙ Ayarlar", AutoSize = true };
    private readonly Label _statusLabel = new() { AutoSize = true, ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft };

    private ShellConfig _config;

    public MainForm()
    {
        _config = ShellConfig.Load();

        Text = _config.Title;
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
            BackColor = Color.FromArgb(30, 30, 30)
        };

        _backButton.Click += (_, _) => { if (_webView.CanGoBack) _webView.GoBack(); };
        _forwardButton.Click += (_, _) => { if (_webView.CanGoForward) _webView.GoForward(); };
        _refreshButton.Click += (_, _) => _webView.Reload();
        _settingsButton.Click += SettingsButton_Click;

        toolbar.Controls.Add(_backButton);
        toolbar.Controls.Add(_forwardButton);
        toolbar.Controls.Add(_refreshButton);
        toolbar.Controls.Add(_settingsButton);
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

    private void SettingsButton_Click(object? sender, EventArgs e)
    {
        using var settingsForm = new SettingsForm(_config);
        if (settingsForm.ShowDialog(this) == DialogResult.OK)
        {
            _config.Url = settingsForm.ResultUrl;
            _config.Title = settingsForm.ResultTitle;
            _config.Save();

            Text = _config.Title;
            _ = InitializeWebViewAsync();
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
