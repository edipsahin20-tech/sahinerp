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
///
/// GERÇEK HATA (2026-09-26, Edip: "burası bir web tarayıcı gibi değil bir masaüstü
/// uygulama ekranı gibi gözüksün") - önceden üstte ◀/▶/⟳ gezinme butonları ve
/// bağlanılan URL'i gösteren bir durum etiketi (bir tarayıcı adres çubuğu gibi)
/// vardı. Web uygulamasının KENDİ üst çubuğu zaten var (RestaurantShell/Ayarlar
/// vb. içeriyor) - bu ikinci, WinForms tarafındaki çubuk sadece "bu bir tarayıcı"
/// izlenimi veriyordu. Artık pencere SADECE WebView2'yi doldurur, gezinme
/// kaldırıldı; bağlantı hatası artık kalıcı bir bar yerine tek seferlik bir
/// MessageBox ile bildirilir.
/// </summary>
public sealed class MainForm : Form
{
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private readonly ShellConfig _config;

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

        Controls.Add(_webView);

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
            await _webView.EnsureCoreWebView2Async(null);
            // Sunucu bu işareti görüp masaüstü penceresinde firma seçimini kilitler (bkz. DatabaseRouter).
            _webView.CoreWebView2.Settings.UserAgent += " SahinSoftDesktop";
            _webView.CoreWebView2.NavigationCompleted += (_, args) =>
            {
                if (!args.IsSuccess)
                {
                    MessageBox.Show(
                        this,
                        "Sayfa yüklenemedi - bağlantıyı kontrol edin.",
                        _config.Title,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            };
            // Çıkış butonu (2026-09-27, Edip: "giriş sayfasına... çıkış kırmızı buton") - PIN giriş
            // sayfasının kendisi bir Windows penceresini kapatamaz, bu yüzden web tarafı
            // window.chrome.webview.postMessage('exit-app') ile bu kabuğa haber verir. Onay sorusu
            // BİLEREK burada, native MessageBox ile soruluyor - eskiden web tarafında JS confirm()
            // kullanılıyordu, Edip: "çıkış için web tarayıcı sorusu değil masaüstü program gibi
            // sorsun" - WebView2'nin kendi JS confirm() kutusu bir tarayıcı iletişim kutusu gibi
            // görünüyordu, gerçek Windows programlarındaki MessageBox gibi değildi.
            _webView.CoreWebView2.WebMessageReceived += (_, args) =>
            {
                if (args.TryGetWebMessageAsString() != "exit-app")
                {
                    return;
                }
                var result = MessageBox.Show(
                    this,
                    "Programdan çıkmak istediğinize emin misiniz?",
                    _config.Title,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Close();
                }
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
            MessageBox.Show(this, $"Bağlantı hatası: {ex.Message}", _config.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
