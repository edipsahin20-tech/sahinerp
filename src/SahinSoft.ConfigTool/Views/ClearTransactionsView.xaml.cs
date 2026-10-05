using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SahinSoft.ConfigTool.Services;

namespace SahinSoft.ConfigTool.Views;

public partial class ClearTransactionsView : UserControl
{
    public List<string> DeletedItems { get; } =
    [
        "Faturalar ve stok hareketleri",
        "Teklifler, siparişler ve irsaliyeler",
        "Tahsilat ve tediye fişleri",
        "Masraflar, çek ve senetler",
        "Cari hareketleri ve fiyat hareketleri",
        "Döviz kuru geçmişi ve denetim kayıtları"
    ];

    public event Action? EditConnectionRequested;

    public ClearTransactionsView()
    {
        InitializeComponent();
    }

    public void RefreshData()
    {
        ActiveConnectionContext.EnsureLoaded();
        var profile = ActiveConnectionContext.Current;
        ServerText.Text = profile?.Server ?? "(seçili değil)";
        DatabaseText.Text = profile?.Database ?? "(seçili değil)";
        DatabaseSummaryBox.Text = profile?.Database ?? string.Empty;
        ConfirmBox.Text = string.Empty;
        LogText.Text = "Henüz işlem yapılmadı.";
    }

    private void EditConnection_Click(object sender, RoutedEventArgs e) => EditConnectionRequested?.Invoke();

    private void BrowseBackupFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Yedek Klasörü Seçin" };
        if (dialog.ShowDialog() == true)
        {
            BackupFolderBox.Text = dialog.FolderName;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => RefreshData();

    private void Execute_Click(object sender, RoutedEventArgs e)
    {
        var profile = ActiveConnectionContext.Current;
        if (profile is null)
        {
            MessageBox.Show("Önce Bağlantı Ayarları'ndan bir bağlantı seçin/etkinleştirin.", "Bağlantı Yok", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!string.Equals(ConfirmBox.Text.Trim(), "SİL", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ConfirmBox.Text.Trim(), "SIL", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Onaylamak için \"SİL\" yazmanız gerekiyor.", "Onay Gerekli", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"\"{profile.Database}\" veritabanındaki TÜM hareketler (faturalar, siparişler, tahsilatlar, cari/stok hareketleri) kalıcı olarak silinecek. Cari ve stok KARTLARI korunacak. Önce yedek alınacak. Devam edilsin mi?",
            "Son Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var master = ProfileStore.BuildMasterConnectionString(profile);
            var backupFolder = BackupFolderBox.Text.Trim();
            if (backupFolder.Length == 0)
            {
                backupFolder = DbOperations.TryDetectBackupDirectory(master) ?? @"C:\SahinSoft\Yedekler";
            }
            var backupPath = Path.Combine(backupFolder, $"{profile.Database}-hareket-sil-{DateTime.Now:yyyyMMdd-HHmmss}.bak");

            DbOperations.BackupDatabase(master, profile.Database, backupPath, _ => { });
            DbOperations.ClearTransactionalData(ProfileStore.BuildConnectionString(profile), _ => { });

            LogText.Text = $"[{DateTime.Now:HH:mm:ss}] Hareketler silindi. Yedek: {backupPath}";
            OperationLogStore.Append("Hareketleri Sil", profile.Database, true, backupPath);
            MessageBox.Show("Hareketler silindi, cari/stok kartları korundu.", "Tamamlandı", MessageBoxButton.OK, MessageBoxImage.Information);
            ConfirmBox.Text = string.Empty;
        }
        catch (Exception ex)
        {
            LogText.Text = $"[{DateTime.Now:HH:mm:ss}] HATA: {ex.Message}";
            OperationLogStore.Append("Hareketleri Sil", profile.Database, false, ex.Message);
            MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }
}
