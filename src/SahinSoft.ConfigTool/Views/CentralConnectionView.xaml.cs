using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SahinSoft.ConfigTool.Models;
using SahinSoft.ConfigTool.Services;

namespace SahinSoft.ConfigTool.Views;

public partial class CentralConnectionView : UserControl
{
    public CentralConnectionView()
    {
        InitializeComponent();
    }

    public void RefreshData()
    {
        var settings = CentralConnectionStore.Load();
        EnabledToggle.IsChecked = settings.Enabled;
        ServerBox.Text = settings.Server;
        PortBox.Text = settings.Port.ToString();
        DatabaseBox.Text = settings.Database;
        SqlAuthRadio.IsChecked = !settings.WindowsAuth;
        WindowsAuthRadio.IsChecked = settings.WindowsAuth;
        UserBox.Text = settings.UserId;
        PasswordBoxCtrl.Password = settings.Password;
        BranchCodeBox.Text = settings.BranchCode;
        BranchNameBox.Text = settings.BranchName;
        UpdateToggleStatusText();
        UpdateSummary();
    }

    private void EnabledToggle_Changed(object sender, RoutedEventArgs e) => UpdateToggleStatusText();

    private void UpdateToggleStatusText()
    {
        if (ToggleStatusText is null)
        {
            return;
        }
        ToggleStatusText.Text = EnabledToggle.IsChecked == true ? "Ayar açık" : "Ayar kapalı";
    }

    private void AuthMode_Changed(object sender, RoutedEventArgs e)
    {
        if (SqlAuthFieldsPanel is null)
        {
            return;
        }
        SqlAuthFieldsPanel.Visibility = (SqlAuthRadio.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
    }

    private CentralConnectionSettings BuildSettings() => new()
    {
        Enabled = EnabledToggle.IsChecked == true,
        Server = ServerBox.Text.Trim(),
        Port = int.TryParse(PortBox.Text.Trim(), out var port) && port > 0 ? port : 1433,
        Database = DatabaseBox.Text.Trim(),
        WindowsAuth = WindowsAuthRadio.IsChecked == true,
        UserId = UserBox.Text.Trim(),
        Password = PasswordBoxCtrl.Password,
        BranchCode = BranchCodeBox.Text.Trim(),
        BranchName = BranchNameBox.Text.Trim()
    };

    private void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        var settings = BuildSettings();
        if (settings.Server.Length == 0)
        {
            TestResultText.Text = "Sunucu adresi boş olamaz.";
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var connStr = CentralConnectionStore.BuildConnectionString(settings);
            if (DbOperations.TestConnection(connStr, out var message))
            {
                TestResultText.Text = "✓ " + message;
                TestResultText.Foreground = (System.Windows.Media.Brush)FindResource("SuccessGreen");
            }
            else
            {
                TestResultText.Text = message;
                TestResultText.Foreground = (System.Windows.Media.Brush)FindResource("DangerRed");
            }
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => RefreshData();

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var settings = BuildSettings();
        if (settings.Enabled && settings.Server.Length == 0)
        {
            ShowStatus("Merkez bağlantı aktifken Sunucu adresi zorunludur.", isError: true);
            return;
        }

        try
        {
            CentralConnectionStore.Save(settings);
            UpdateSummary();
            OperationLogStore.Append("Merkez bağlantı ayarları kaydedildi", settings.Database, true, settings.Enabled ? "Aktif" : "Pasif");
            ShowStatus("Merkez bağlantı ayarları kaydedildi.", isError: false);
        }
        catch (Exception ex)
        {
            OperationLogStore.Append("Merkez bağlantı ayarları", settings.Database, false, ex.Message);
            ShowStatus($"Kaydedilemedi: {ex.Message}", isError: true);
        }
    }

    private void UpdateSummary()
    {
        ActiveConnectionContext.EnsureLoaded();
        LocalDbSummaryBox.Text = ActiveConnectionContext.Current?.Database ?? "(seçili değil)";
        CentralDbSummaryBox.Text = DatabaseBox.Text.Trim().Length > 0 ? DatabaseBox.Text.Trim() : "(tanımlı değil)";
    }

    private void ShowStatus(string message, bool isError)
    {
        StatusText.Text = message;
        StatusBox.Visibility = Visibility.Visible;
        StatusBox.Background = isError
            ? (System.Windows.Media.Brush)FindResource("DangerBoxBg")
            : (System.Windows.Media.Brush)FindResource("InfoBoxBg");
        StatusBox.BorderBrush = isError
            ? (System.Windows.Media.Brush)FindResource("DangerBoxBorder")
            : (System.Windows.Media.Brush)FindResource("InfoBoxBorder");
        StatusText.Foreground = isError
            ? (System.Windows.Media.Brush)FindResource("DangerBoxText")
            : (System.Windows.Media.Brush)FindResource("InfoBoxText");
    }
}
