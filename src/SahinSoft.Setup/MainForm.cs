using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace SahinSoft.Setup;

// Advanced Installer / Dinosoft'un kendi kurulumu gibi gerçek bir kurulum penceresi - kara
// PowerShell konsolu yerine logo, ilerleme çubuğu, canlı adım durumu ve (isteğe bağlı) detay
// logu. Arkadaki iş mantığı aynı: gömülü payload.zip'i %TEMP%'e çıkar, Kurulum.ps1'i çalıştır,
// çıktısını satır satır yakala ("==>" satırları adım ilerlemesini tetikler), kalıcı araçları
// kopyala, kısayol oluştur.
public sealed class MainForm : Form
{
    private static readonly Color BrandGold = Color.FromArgb(212, 164, 55);
    private static readonly Color BrandDark = Color.FromArgb(24, 24, 26);

    private static readonly string[] StepLabels =
    [
        "IIS (Web Server) rolü kontrol ediliyor...",
        ".NET 10 ASP.NET Core Hosting Bundle kontrol ediliyor...",
        "Yerel SQL Server bağlantısı kontrol ediliyor...",
        "Uygulama dosyaları açılıyor...",
        "Bağlantı dizesi ayarlanıyor...",
        "IIS Uygulama Havuzu ve Site kontrol ediliyor...",
        "Veritabanı migration script'i uygulanıyor...",
        "Masaüstü uygulaması ve araçlar kopyalanıyor..."
    ];

    private readonly Label _stepLabel = new()
    {
        Text = "Kuruluma hazırlanılıyor...",
        Dock = DockStyle.Top,
        Height = 40,
        Font = new Font("Segoe UI", 11F),
        ForeColor = Color.FromArgb(50, 50, 50),
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(0, 12, 0, 0)
    };

    private readonly GoldProgressBar _progressBar = new()
    {
        Dock = DockStyle.Top,
        Height = 14,
        Maximum = StepLabels.Length,
        Margin = new Padding(0, 6, 0, 10)
    };

    private readonly LinkLabel _detailsToggle = new()
    {
        Text = "Detayları göster",
        Dock = DockStyle.Top,
        Height = 28,
        Font = new Font("Segoe UI", 9F),
        Padding = new Padding(0, 8, 0, 0)
    };

    private readonly RichTextBox _logBox = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        BackColor = Color.White,
        Font = new Font("Consolas", 9F),
        Visible = false,
        BorderStyle = BorderStyle.FixedSingle
    };

    private readonly Button _closeButton = new()
    {
        Text = "Kapat",
        Dock = DockStyle.Bottom,
        Height = 46,
        Enabled = false,
        BackColor = BrandGold,
        ForeColor = Color.FromArgb(40, 30, 5),
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        FlatStyle = FlatStyle.Flat,
        Cursor = Cursors.Hand
    };

    private readonly Panel _detailsPanel;
    private int _currentStep;

    public MainForm()
    {
        Text = "ŞahinSoft Kurulum";
        Width = 640;
        Height = 460;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        // NOT: TopMost=true buradaydı ama TeamViewer/uzak masaüstü ile birleşince "Kapat"
        // butonunun hiç tıklanamaması gibi bilinen bir odak/giriş sorununa yol açtı - artık
        // SQL kurulumu tamamen sessiz çalıştığı için (başka pencereyle yarışma yok) kaldırıldı.
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // Kritik değil.
        }

        var header = BuildHeader();

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 20, 28, 16),
            BackColor = Color.FromArgb(250, 250, 251)
        };
        body.Controls.Add(_stepLabel);
        body.Controls.Add(_progressBar);

        _detailsToggle.LinkClicked += (_, _) => ToggleDetails();
        body.Controls.Add(_detailsToggle);

        _detailsPanel = new Panel { Dock = DockStyle.Fill, Visible = false };
        _detailsPanel.Controls.Add(_logBox);
        body.Controls.Add(_detailsPanel);

        // TableLayoutPanel yerine Dock sırası önemli - en son eklenen üstte görünür, bu yüzden
        // ters sırayla ekliyoruz (Panel.Controls'e eklenenler Dock.Top için ters istifleniyor).
        body.Controls.SetChildIndex(_detailsPanel, 0);
        body.Controls.SetChildIndex(_detailsToggle, 1);
        body.Controls.SetChildIndex(_progressBar, 2);
        body.Controls.SetChildIndex(_stepLabel, 3);

        _closeButton.FlatAppearance.BorderSize = 0;
        _closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 182, 76);
        _closeButton.Click += (_, _) => Close();

        Controls.Add(body);
        Controls.Add(_closeButton);
        Controls.Add(header);

        Load += async (_, _) => await RunInstallAsync();
    }

    private Panel BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = BrandDark };

        var logo = new PictureBox
        {
            Size = new Size(64, 64),
            Location = new Point(24, 16),
            SizeMode = PictureBoxSizeMode.Zoom
        };
        try
        {
            // ApplicationIcon (exe'nin Win32 simge kaynağı) ile .NET Embedded Resource FARKLI
            // mekanizmalar - GetManifestResourceStream ile okunamaz. Form'un kendi Icon'unu
            // ayarlarken kullandığımız ExtractAssociatedIcon burada da doğru yöntem.
            using var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (icon is not null)
            {
                logo.Image = icon.ToBitmap();
            }
        }
        catch
        {
            // İkon yüklenemezse logo boş kalır - kritik değil.
        }
        header.Controls.Add(logo);

        var title = new Label
        {
            Text = "ŞahinSoft Ön Muhasebe",
            ForeColor = BrandGold,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(100, 20)
        };
        header.Controls.Add(title);

        var subtitle = new Label
        {
            Text = "Kurulum Sihirbazı",
            ForeColor = Color.LightGray,
            Font = new Font("Segoe UI", 10F),
            AutoSize = true,
            Location = new Point(100, 54)
        };
        header.Controls.Add(subtitle);

        return header;
    }

    private void ToggleDetails()
    {
        _detailsPanel.Visible = !_detailsPanel.Visible;
        _logBox.Visible = _detailsPanel.Visible;
        _detailsToggle.Text = _detailsPanel.Visible ? "Detayları gizle" : "Detayları göster";
        Height = _detailsPanel.Visible ? 660 : 460;
    }

    private async Task RunInstallAsync()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "SahinSoftKurulum_" + Guid.NewGuid().ToString("N")[..8]);
        var success = false;

        try
        {
            AppendLog("Kurulum dosyaları hazırlanıyor...", LogKind.Info);
            Directory.CreateDirectory(tempRoot);
            ExtractEmbeddedPayload("payload.zip", tempRoot);
            AppendLog("Hazır.", LogKind.Ok);

            var exitCode = await RunKurulumScriptAsync(tempRoot);

            if (exitCode == 0)
            {
                SetStep(StepLabels.Length - 1);
                InstallPermanentTools(tempRoot);
                success = true;
            }
        }
        catch (Exception ex)
        {
            AppendLog($"Kurulum başarısız: {ex.Message}", LogKind.Error);
        }
        finally
        {
            TryDeleteDirectory(tempRoot);
        }

        if (success)
        {
            _stepLabel.Text = "Kurulum tamamlandı. Masaüstünde \"ŞahinSoft\" kısayolu oluşturuldu.";
            _progressBar.Value = _progressBar.Maximum;
        }
        else
        {
            _stepLabel.Text = "Kurulum tamamlanamadı - ayrıntılar için \"Detayları göster\"e bakın.";
            if (!_detailsPanel.Visible)
            {
                ToggleDetails();
            }
        }
        _closeButton.Enabled = true;
        _closeButton.Text = success ? "Kapat" : "Kapat (hatalarla)";
    }

    private async Task<int> RunKurulumScriptAsync(string tempRoot)
    {
        var tcs = new TaskCompletionSource<int>();
        var scriptPath = Path.Combine(tempRoot, "Kurulum.ps1");
        var logFilePath = Path.Combine(tempRoot, "kurulum.log");

        // Türkçe karakterler PowerShell'in konsol/pipe çıktısında güvenilir şekilde UTF-8
        // kalmıyordu - [Console]::OutputEncoding, SetOut, hatta "chcp 65001" bile denendi,
        // hiçbiri PowerShell 5.1'in yönlendirilmiş çıktısında tutarlı çalışmadı. Kesin çözüm:
        // konsolu/pipe'ı OKUMA KAYNAĞI olarak TAMAMEN TERK ETTİK - Kurulum.ps1 artık kendi
        // durumunu bir dosyaya (kurulum.log) UTF-8 ile yazıyor, biz de o dosyayı okuyoruz.
        // Dosya G/Ç'si her zaman istenen kodlamayla çalışır, konsol/pipe iç mekanizmasına bağlı
        // değildir. RedirectStandardOutput/CreateNoWindow burada sadece pencere/konsol
        // flaşlamasını önlemek için - içeriği artık kullanmıyoruz.
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = tempRoot,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.Exited += (_, _) => tcs.TrySetResult(process.ExitCode);

        process.Start();

        using var logCts = new CancellationTokenSource();
        var logTailTask = TailLogFileAsync(logFilePath, logCts.Token);

        var exitCode = await tcs.Task;

        // Süreç bitti ama dosyaya son yazılanları okumak için kısa bir son tarama daha yap,
        // sonra durdur.
        await Task.Delay(250);
        logCts.Cancel();
        try { await logTailTask; } catch (OperationCanceledException) { }

        process.Dispose();
        return exitCode;
    }

    // kurulum.log dosyasını periyodik olarak, en son okunan konumdan itibaren okuyup yeni
    // satırları HandleScriptLine'a besler - console/pipe encoding sorununu tamamen atlar.
    private async Task TailLogFileAsync(string logFilePath, CancellationToken cancellationToken)
    {
        long lastPosition = 0;
        var pendingPartialLine = string.Empty;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    using var stream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    if (stream.Length > lastPosition)
                    {
                        stream.Seek(lastPosition, SeekOrigin.Begin);
                        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                        var newContent = await reader.ReadToEndAsync(cancellationToken);
                        lastPosition = stream.Position;

                        var combined = pendingPartialLine + newContent;
                        var lines = combined.Split('\n');
                        // Son parça tam bir satır olmayabilir (dosyaya hâlâ yazılıyor olabilir) -
                        // bir sonraki turda tamamlanmak üzere sakla.
                        pendingPartialLine = lines[^1];
                        for (var i = 0; i < lines.Length - 1; i++)
                        {
                            HandleScriptLine(lines[i].TrimEnd('\r'));
                        }
                    }
                }
            }
            catch (IOException)
            {
                // Dosya o anda script tarafından yazılıyor olabilir (paylaşım kilidi) - bir
                // sonraki turda tekrar dene.
            }

            try
            {
                await Task.Delay(150, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        if (pendingPartialLine.Trim().Length > 0)
        {
            HandleScriptLine(pendingPartialLine.TrimEnd('\r'));
        }
    }

    private void HandleScriptLine(string line)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => HandleScriptLine(line));
            return;
        }

        var trimmed = line.Trim();
        if (trimmed.Length == 0)
        {
            return;
        }

        if (trimmed.StartsWith("==>", StringComparison.Ordinal))
        {
            var stepText = trimmed[3..].Trim();
            _stepLabel.Text = stepText;
            var stepIndex = Array.FindIndex(StepLabels, s => s.Equals(stepText, StringComparison.OrdinalIgnoreCase));
            if (stepIndex >= 0)
            {
                SetStep(stepIndex);
            }
            AppendLog(stepText, LogKind.Step);
        }
        else if (trimmed.Contains("[HATA]", StringComparison.Ordinal))
        {
            AppendLog(trimmed, LogKind.Error);
        }
        else if (trimmed.Contains("[UYARI]", StringComparison.Ordinal))
        {
            AppendLog(trimmed, LogKind.Warn);
        }
        else if (trimmed.Contains("[OK]", StringComparison.Ordinal))
        {
            AppendLog(trimmed, LogKind.Ok);
        }
        else
        {
            AppendLog(trimmed, LogKind.Info);
        }
    }

    private void SetStep(int index)
    {
        _currentStep = Math.Max(_currentStep, index);
        _progressBar.Value = Math.Min(_currentStep + 1, _progressBar.Maximum);
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

    // Kurulum.ps1 sadece IIS+SQL+ana uygulamayı kurar - masaüstü kabuk ve veritabanı aracı kalıcı
    // araçlar, bunlar Kurulum.ps1'in kurduğu C:\SitesSahinSoft'un yanına, ayrı bir "Araclar"
    // klasörüne kopyalanır ve masaüstüne kısayol bırakılır. Edip'in isteği: sadece 3 exe (Kurulum,
    // Veritabanı, Restoran programı) - eskiden ayrı bir 4. araç olan ConfigTool'un appsettings.json
    // bağlantı dizesi işlevi artık SahinSoftDbKur.exe'nin GUI'sinde, ayrıca paketlenmiyor.
    private void InstallPermanentTools(string tempRoot)
    {
        const string sitePath = @"C:\SitesSahinSoft";
        AppendLog("Masaüstü uygulaması ve araçlar kopyalanıyor...", LogKind.Step);

        var toolsDir = Path.Combine(sitePath, "Araclar");
        CopyDirectory(Path.Combine(tempRoot, "MasaustuUygulama"), Path.Combine(toolsDir, "MasaustuUygulama"));

        // Veritabanını tek başına (IIS/Hosting Bundle'a dokunmadan) yeniden kurabilmek için -
        // sorun çıkarsa müşteride/testte tekrar tüm kurulumu baştan başlatmak yerine bu tek
        // araç çalıştırılabilir.
        var dbKurSource = Path.Combine(tempRoot, "SahinSoftDbKur.exe");
        if (File.Exists(dbKurSource))
        {
            Directory.CreateDirectory(toolsDir);
            File.Copy(dbKurSource, Path.Combine(toolsDir, "SahinSoftDbKur.exe"), overwrite: true);
        }

        var shellExePath = Path.Combine(toolsDir, "MasaustuUygulama", "SahinSoft.exe");
        if (File.Exists(shellExePath))
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var shortcutPath = Path.Combine(desktop, "ŞahinSoft.lnk");
            CreateShortcut(shortcutPath, shellExePath, Path.Combine(toolsDir, "MasaustuUygulama"));
            AppendLog("Masaüstü kısayolu oluşturuldu.", LogKind.Ok);
        }
    }

    private static void ExtractEmbeddedPayload(string resourceFileName, string destination)
    {
        using var stream = GetEmbeddedStream(resourceFileName)
            ?? throw new InvalidOperationException($"Gömülü kurulum paketi ({resourceFileName}) bulunamadı.");
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        archive.ExtractToDirectory(destination, overwriteFiles: true);
    }

    private static Stream? GetEmbeddedStream(string resourceFileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(x => x.EndsWith(resourceFileName, StringComparison.OrdinalIgnoreCase));
        return resourceName is null ? null : assembly.GetManifestResourceStream(resourceName);
    }

    private static void CopyDirectory(string source, string destination)
    {
        if (!Directory.Exists(source))
        {
            return;
        }

        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);
            var targetPath = Path.Combine(destination, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.Copy(file, targetPath, overwrite: true);
        }
    }

    // Ekstra bir NuGet paketi gerekmeden Windows Script Host'un WScript.Shell COM nesnesi
    // üzerinden .lnk kısayolu oluşturur (late-bound COM - yalnızca Windows'ta çalışır).
    private static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("WScript.Shell COM bileşeni bulunamadı.");
        dynamic shell = Activator.CreateInstance(shellType)!;
        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = targetPath;
        shortcut.WorkingDirectory = workingDirectory;
        shortcut.Description = "ŞahinSoft Ön Muhasebe";
        shortcut.Save();
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // %TEMP% zaten periyodik temizlenir - kritik değil.
        }
    }
}
