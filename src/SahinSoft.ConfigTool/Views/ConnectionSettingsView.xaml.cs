using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using SahinSoft.ConfigTool.Models;
using SahinSoft.ConfigTool.Services;

namespace SahinSoft.ConfigTool.Views;

public partial class ConnectionSettingsView : UserControl
{
    private readonly ObservableCollection<ConnectionProfile> _profiles = [];
    private ConnectionProfile? _selected;

    public ConnectionSettingsView()
    {
        InitializeComponent();
        ProfileList.ItemsSource = _profiles;
        ConfigPathBox.Text = ProfileStore.LocalOverridePath;
    }

    public void RefreshData()
    {
        _profiles.Clear();
        foreach (var p in ProfileStore.LoadProfiles())
        {
            _profiles.Add(p);
        }

        if (_selected is null && _profiles.Count > 0)
        {
            LoadIntoForm(_profiles.FirstOrDefault(x => x.IsDefault) ?? _profiles[0]);
        }
    }

    private void ProfileCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { Tag: ConnectionProfile profile })
        {
            LoadIntoForm(profile);
        }
    }

    private void LoadIntoForm(ConnectionProfile profile)
    {
        _selected = profile;
        NameBox.Text = profile.Name;
        ServerBox.Text = profile.Server;
        DatabaseBox.Text = profile.Database;
        SqlAuthRadio.IsChecked = !profile.WindowsAuth;
        WindowsAuthRadio.IsChecked = profile.WindowsAuth;
        UserBox.Text = profile.UserId;
        PasswordBoxCtrl.Password = profile.Password;
        SetDefaultCheck.IsChecked = profile.IsDefault;
        DatabaseListBox.Items.Clear();
        HideStatus();
    }

    private void NewConnection_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        NameBox.Text = string.Empty;
        ServerBox.Text = string.Empty;
        DatabaseBox.Text = "SahinSoftDb";
        SqlAuthRadio.IsChecked = true;
        UserBox.Text = string.Empty;
        PasswordBoxCtrl.Password = string.Empty;
        SetDefaultCheck.IsChecked = _profiles.Count == 0;
        DatabaseListBox.Items.Clear();
        HideStatus();
    }

    private void AuthMode_Changed(object sender, RoutedEventArgs e)
    {
        if (SqlAuthFieldsPanel is null)
        {
            return;
        }
        SqlAuthFieldsPanel.Visibility = (SqlAuthRadio.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
    }

    private bool TryBuildProfile(out ConnectionProfile profile, out string error)
    {
        profile = new ConnectionProfile();
        error = string.Empty;

        var name = NameBox.Text.Trim();
        var server = ServerBox.Text.Trim();
        var database = DatabaseBox.Text.Trim();

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
        var windowsAuth = WindowsAuthRadio.IsChecked == true;
        if (!windowsAuth && UserBox.Text.Trim().Length == 0)
        {
            error = "SQL kullanıcı adı boş olamaz.";
            return false;
        }

        profile = new ConnectionProfile
        {
            Name = name.Length == 0 ? database : name,
            Server = server,
            Database = database,
            WindowsAuth = windowsAuth,
            UserId = UserBox.Text.Trim(),
            Password = PasswordBoxCtrl.Password,
            IsDefault = SetDefaultCheck.IsChecked == true
        };
        return true;
    }

    private void ListDatabases_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildProfile(out var profile, out var error))
        {
            ShowStatus(error, isError: true);
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var master = ProfileStore.BuildMasterConnectionString(profile);
            var databases = DbOperations.ListDatabases(master);
            DatabaseListBox.Items.Clear();
            var existing = ProfileStore.LoadProfiles();
            foreach (var db in databases)
            {
                DatabaseListBox.Items.Add(db);
                if (existing.Any(x => x.Server.Equals(profile.Server, StringComparison.OrdinalIgnoreCase) && x.Database.Equals(db, StringComparison.OrdinalIgnoreCase)))
                {
                    DatabaseListBox.SelectedItems.Add(db);
                }
            }
            ShowStatus(databases.Count == 0
                ? "Sunucuda kullanıcı veritabanı bulunamadı."
                : $"{databases.Count} veritabanı bulundu.", isError: false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Listelenemedi: {ex.Message}", isError: true);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    // Tiklenen veritabanları giriş ekranında firma olarak görünür; tiklenmeyen ama daha önce eklenmiş
    // (aynı sunucudaki) olanlar listeden çıkarılır. Her firma adı = veritabanı adı.
    private void SyncLoginDatabases_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildProfile(out var server, out var error))
        {
            ShowStatus(error, isError: true);
            return;
        }

        var selected = DatabaseListBox.SelectedItems.Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var all = ProfileStore.LoadProfiles();
        var sameServer = all.Where(x => x.Server.Equals(server.Server, StringComparison.OrdinalIgnoreCase)).ToList();

        var removed = all.RemoveAll(x => sameServer.Contains(x) && !selected.Contains(x.Database));
        var added = 0;
        var skipped = new List<string>();
        foreach (var db in selected)
        {
            if (all.Any(x => x.Server.Equals(server.Server, StringComparison.OrdinalIgnoreCase) && x.Database.Equals(db, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }
            if (all.Any(x => x.Name.Equals(db, StringComparison.CurrentCultureIgnoreCase)))
            {
                skipped.Add(db);
                continue;
            }
            all.Add(new ConnectionProfile
            {
                Name = db,
                Server = server.Server,
                Database = db,
                WindowsAuth = server.WindowsAuth,
                UserId = server.UserId,
                Password = server.Password
            });
            added++;
        }

        if (all.Count > 0 && !all.Any(x => x.IsDefault))
        {
            all[0].IsDefault = true;
        }
        ProfileStore.SaveProfiles(all);
        _profiles.Clear();
        foreach (var p in all)
        {
            _profiles.Add(p);
        }

        var message = $"Giriş ekranı güncellendi: {added} eklendi, {removed} kaldırıldı.";
        if (skipped.Count > 0)
        {
            message += $" Şu adlar zaten başka bir sunucuda kayıtlı, atlandı: {string.Join(", ", skipped)}";
        }
        ShowStatus(message, isError: false);
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON dosyaları (*.json)|*.json|Tüm dosyalar (*.*)|*.*",
            FileName = "appsettings.Local.json",
            CheckFileExists = false
        };
        if (dialog.ShowDialog() == true)
        {
            ConfigPathBox.Text = dialog.FileName;
        }
    }

    private void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildProfile(out var profile, out var error))
        {
            ShowStatus(error, isError: true);
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var master = ProfileStore.BuildMasterConnectionString(profile);
            if (!DbOperations.TestConnection(master, out var message))
            {
                ShowStatus($"Sunucuya bağlantı başarısız: {message}", isError: true);
                return;
            }

            var exists = DbOperations.DatabaseExists(master, profile.Database);
            ShowStatus(exists
                ? $"Sunucuya bağlantı başarılı. \"{profile.Database}\" veritabanı mevcut, kullanıma hazır."
                : $"Sunucuya bağlantı başarılı. \"{profile.Database}\" adında bir veritabanı henüz yok - \"Oluştur ve Bakım\" sayfasından oluşturabilirsiniz.",
                isError: false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Bağlantı başarısız: {ex.Message}", isError: true);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildProfile(out var profile, out var error))
        {
            ShowStatus(error, isError: true);
            return;
        }

        var all = _profiles.ToList();
        var existing = _selected is not null ? all.FirstOrDefault(x => x.Name == _selected.Name) : null;
        if (existing is null)
        {
            existing = all.FirstOrDefault(x => x.Name.Equals(profile.Name, StringComparison.CurrentCultureIgnoreCase));
        }
        if (existing is not null)
        {
            all.Remove(existing);
        }

        if (profile.IsDefault)
        {
            foreach (var p in all)
            {
                p.IsDefault = false;
            }
        }
        else if (all.Count == 0)
        {
            profile.IsDefault = true;
        }

        all.Add(profile);
        ProfileStore.SaveProfiles(all);

        _profiles.Clear();
        foreach (var p in all)
        {
            _profiles.Add(p);
        }
        _selected = profile;
        OperationLogStore.Append("Bağlantı profili kaydedildi", profile.Database, true, profile.Name);
        ShowStatus($"\"{profile.Name}\" profili kaydedildi.", isError: false);
    }

    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            return;
        }

        var result = MessageBox.Show($"\"{_selected.Name}\" profili silinsin mi?", "Profili Sil", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var all = _profiles.Where(x => x.Name != _selected.Name).ToList();
        if (all.Count > 0 && !all.Any(x => x.IsDefault))
        {
            all[0].IsDefault = true;
        }
        ProfileStore.SaveProfiles(all);

        var deletedName = _selected.Name;
        _profiles.Clear();
        foreach (var p in all)
        {
            _profiles.Add(p);
        }
        _selected = null;
        NewConnection_Click(sender, e);
        OperationLogStore.Append("Bağlantı profili silindi", string.Empty, true, deletedName);
        ShowStatus($"\"{deletedName}\" profili silindi.", isError: false);
    }

    private void Activate_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildProfile(out var profile, out var error))
        {
            ShowStatus(error, isError: true);
            return;
        }

        try
        {
            var path = ConfigPathBox.Text.Trim();
            if (path.Length == 0)
            {
                path = ProfileStore.LocalOverridePath;
            }

            if (path == ProfileStore.LocalOverridePath)
            {
                ProfileStore.ActivateProfile(profile);
            }
            else
            {
                // Gelişmiş: kullanıcı farklı bir dosya yolu seçtiyse, SADECE o dosyaya yaz
                // (varsayılan appsettings.Local.json'a dokunma).
                var root = ProfileStore.ReadRoot(path);
                if (root["ConnectionStrings"] is not System.Text.Json.Nodes.JsonObject cs)
                {
                    cs = new System.Text.Json.Nodes.JsonObject();
                    root["ConnectionStrings"] = cs;
                }
                cs["DefaultConnection"] = ProfileStore.BuildConnectionString(profile);
                root["TerminalConnectionMode"] = "Cloud";
                ProfileStore.WriteRoot(path, root);
            }

            ActiveConnectionContext.Set(profile);
            OperationLogStore.Append("Bağlantı etkinleştirildi", profile.Database, true, $"{profile.Server} -> {path}");
            ShowStatus("Bağlantı etkinleştirildi. Değişikliğin etkili olması için IIS'i (iisreset) veya uygulamayı yeniden başlatın.", isError: false);
        }
        catch (Exception ex)
        {
            OperationLogStore.Append("Bağlantı etkinleştirme", profile.Database, false, ex.Message);
            ShowStatus($"Etkinleştirilemedi: {ex.Message}", isError: true);
        }
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

    private void HideStatus() => StatusBox.Visibility = Visibility.Collapsed;
}
