using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.Http.Json;
using System.Windows.Media;
using Microsoft.Win32;
using SahinSoft.ConfigTool.Models;
using SahinSoft.ConfigTool.Services;

namespace SahinSoft.ConfigTool.Views;

public sealed class MaintenanceLogRow
{
    public string Time { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Brush StatusColor { get; set; } = Brushes.Black;
}

public partial class MaintenanceView : UserControl
{
    private readonly ObservableCollection<MaintenanceLogRow> _log = [];

    public MaintenanceView()
    {
        InitializeComponent();
        LogItems.ItemsSource = _log;
    }

    private void TransferList_Click(object sender, RoutedEventArgs e)
    {
        var profiles = ProfileStore.LoadProfiles();
        TransferSourceCombo.ItemsSource = profiles.Select(x => x.Name).ToList();
        TransferTargetCombo.ItemsSource = profiles.Select(x => x.Name).ToList();
        var source = TransferSourceCombo.SelectedItem as string;
        if (source is null)
        {
            MessageBox.Show("Önce kaynak firmayı seçin.", "Veri Aktarımı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var profile = profiles.First(x => x.Name == source);
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(ProfileStore.BuildConnectionString(profile));
            connection.Open();
            CategoriesList.Items.Clear();
            ProductsList.Items.Clear();
            CustomersList.Items.Clear();
            Fill(connection, "SELECT Code, Name FROM ProductCategories ORDER BY Name", CategoriesList);
            Fill(connection, "SELECT StockCode, Name FROM Products ORDER BY StockCode", ProductsList);
            Fill(connection, "SELECT Code, Name FROM Customers ORDER BY Name", CustomersList);
            CategoriesAll.IsChecked = false;
            ProductsAll.IsChecked = false;
            CustomersAll.IsChecked = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Listelenemedi: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private static void Fill(Microsoft.Data.SqlClient.SqlConnection connection, string sql, ListBox target)
    {
        using var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            target.Items.Add($"{reader.GetString(0)} | {reader.GetString(1)}");
        }
    }

    private void SelectAllCategories(object sender, RoutedEventArgs e) => CategoriesList.SelectAll();
    private void UnselectAllCategories(object sender, RoutedEventArgs e) => CategoriesList.UnselectAll();
    private void SelectAllProducts(object sender, RoutedEventArgs e) => ProductsList.SelectAll();
    private void UnselectAllProducts(object sender, RoutedEventArgs e) => ProductsList.UnselectAll();
    private void SelectAllCustomers(object sender, RoutedEventArgs e) => CustomersList.SelectAll();
    private void UnselectAllCustomers(object sender, RoutedEventArgs e) => CustomersList.UnselectAll();

    private async void TransferRun_Click(object sender, RoutedEventArgs e)
    {
        var source = TransferSourceCombo.SelectedItem as string;
        var target = TransferTargetCombo.SelectedItem as string;
        if (source is null || target is null || source == target)
        {
            MessageBox.Show("Farklı bir kaynak ve hedef firma seçin.", "Veri Aktarımı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        static List<string> Codes(ListBox list) => list.SelectedItems.Cast<string>().Select(x => x.Split(" | ")[0]).ToList();
        var categories = Codes(CategoriesList);
        var products = Codes(ProductsList);
        var customers = Codes(CustomersList);
        if (categories.Count + products.Count + customers.Count == 0)
        {
            MessageBox.Show("Aktarılacak kayıt seçilmedi.", "Veri Aktarımı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"\"{source}\" firmasından \"{target}\" firmasına {categories.Count} kategori, {products.Count} stok kartı ve {customers.Count} cari kartı aktarılacak. Aynı kodlu kayıtlar atlanır. Devam edilsin mi?",
            "Veri Aktarımı", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromMinutes(20) };
            var response = await http.PostAsJsonAsync("http://localhost:1666/Setup/Transfer",
                new { Source = source, Target = target, Categories = categories, Products = products, Customers = customers });
            var body = await response.Content.ReadAsStringAsync();
            OperationLogStore.Append("Veri aktarımı", target, response.IsSuccessStatusCode, $"{source} -> {target}");
            MessageBox.Show(response.IsSuccessStatusCode ? body : $"Aktarılamadı: {body}", "Veri Aktarımı",
                MessageBoxButton.OK, response.IsSuccessStatusCode ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Web uygulamasına ulaşılamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    public void RefreshData()
    {
        ActiveConnectionContext.EnsureLoaded();
        var profile = ActiveConnectionContext.Current;
        ServerText.Text = profile?.Server ?? "(seçili değil)";
        DatabaseText.Text = profile?.Database ?? "(seçili değil)";
        NewDbNameBox.Text = string.Empty;

        if (BackupFolderBox.Text.Length == 0 && profile is not null)
        {
            try
            {
                var detected = DbOperations.TryDetectBackupDirectory(ProfileStore.BuildMasterConnectionString(profile));
                if (!string.IsNullOrWhiteSpace(detected))
                {
                    BackupFolderBox.Text = detected;
                }
            }
            catch
            {
                // Otomatik tespit edilemezse boş kalır, kullanıcı elle girer.
            }
        }
    }

    private void AddLog(string operation, bool success)
    {
        _log.Insert(0, new MaintenanceLogRow
        {
            Time = DateTime.Now.ToString("HH:mm:ss"),
            Operation = operation,
            Status = success ? "Başarılı" : "Başarısız",
            StatusColor = success ? (Brush)FindResource("SuccessGreen") : (Brush)FindResource("DangerRed")
        });
        EmptyLogText.Visibility = _log.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        var profile = ActiveConnectionContext.Current;
        if (profile is null)
        {
            MessageBox.Show("Önce Bağlantı Ayarları'ndan bir bağlantı seçin/etkinleştirin.", "Bağlantı Yok", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var ok = DbOperations.TestConnection(ProfileStore.BuildConnectionString(profile), out var message);
            AddLog("Bağlantı testi", ok);
            MessageBox.Show(message, "Bağlantı Testi", MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private async void CreateDatabase_Click(object sender, RoutedEventArgs e)
    {
        var profile = ActiveConnectionContext.Current;
        if (profile is null)
        {
            MessageBox.Show("Önce Bağlantı Ayarları'ndan bir sunucu bağlantısı seçin/etkinleştirin.", "Bağlantı Yok", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var name = NewDbNameBox.Text.Trim();
        if (name.Length == 0)
        {
            MessageBox.Show("Veritabanı adı boş olamaz.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var master = ProfileStore.BuildMasterConnectionString(profile);
            if (DbOperations.DatabaseExists(master, name))
            {
                AddLog($"'{name}' oluşturma", false);
                MessageBox.Show($"\"{name}\" zaten var - üzerine yazılmadı.", "Veritabanı Oluştur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DbOperations.CreateDatabase(master, name);
            AddLog($"'{name}' veritabanı oluşturuldu", true);
            OperationLogStore.Append("Veritabanı oluşturuldu", name, true, profile.Server);

            var profiles = ProfileStore.LoadProfiles();
            if (!profiles.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
            {
                profiles.Add(new ConnectionProfile
                {
                    Name = name,
                    Server = profile.Server,
                    Database = name,
                    WindowsAuth = profile.WindowsAuth,
                    UserId = profile.UserId,
                    Password = profile.Password
                });
                ProfileStore.SaveProfiles(profiles);
            }

            Mouse.OverrideCursor = Cursors.Wait;
            var prepared = await PrepareOnWebAsync(name);
            AddLog($"'{name}' kurulumu (tablolar, giriş hesabı, varsayılanlar)", prepared.ok);
            OperationLogStore.Append("Veritabanı hazırlandı", name, prepared.ok, prepared.message);
            MessageBox.Show(prepared.ok
                ? $"\"{name}\" oluşturuldu ve hazırlandı. Giriş: edip@sahinbilisim.com.tr - ilk firma olarak giriş ekranında görünür."
                : $"\"{name}\" oluşturuldu ama hazırlanamadı: {prepared.message}\nWeb uygulamasının çalıştığından emin olup tekrar deneyin.",
                "Veritabanı Oluştur", MessageBoxButton.OK, prepared.ok ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            AddLog($"'{name}' oluşturma", false);
            OperationLogStore.Append("Veritabanı oluşturma", name, false, ex.Message);
            MessageBox.Show($"Oluşturulamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private static async Task<(bool ok, string message)> PrepareOnWebAsync(string profileName)
    {
        try
        {
            using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            var response = await http.PostAsJsonAsync("http://localhost:1666/Setup/Prepare", new { Profile = profileName });
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? (true, body) : (false, body);
        }
        catch (Exception ex)
        {
            return (false, $"Web uygulamasına ulaşılamadı ({ex.Message})");
        }
    }

    private static string PromptForText(string prompt, string title)
    {
        var box = new TextBox { Margin = new Thickness(0, 10, 0, 0), Padding = new Thickness(8, 6, 8, 6) };
        var ok = new Button { Content = "Onayla", Width = 100, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
        var cancel = new Button { Content = "Vazgeç", Width = 100, IsCancel = true };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        var content = new StackPanel { Margin = new Thickness(18) };
        content.Children.Add(new TextBlock { Text = prompt, TextWrapping = TextWrapping.Wrap });
        content.Children.Add(box);
        content.Children.Add(buttons);
        var window = new Window { Title = title, Content = content, SizeToContent = SizeToContent.WidthAndHeight, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = Window.GetWindow(Application.Current.MainWindow), ResizeMode = ResizeMode.NoResize, MaxWidth = 520 };
        ok.Click += (_, _) => { window.DialogResult = true; };
        window.Loaded += (_, _) => box.Focus();
        return window.ShowDialog() == true ? box.Text : string.Empty;
    }

    private void DeleteDatabase_Click(object sender, RoutedEventArgs e)
    {
        var profile = ActiveConnectionContext.Current;
        if (profile is null)
        {
            MessageBox.Show("Önce Bağlantı Ayarları'ndan bir sunucu bağlantısı seçin/etkinleştirin.", "Bağlantı Yok", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var name = NewDbNameBox.Text.Trim();
        if (name.Length == 0)
        {
            MessageBox.Show("Silinecek veritabanının adını \"Veritabanı Adı\" kutusuna yazın.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var all = ProfileStore.LoadProfiles();
        var defaultProfile = all.FirstOrDefault(x => x.IsDefault);
        if (name.Equals(profile.Database, StringComparison.OrdinalIgnoreCase)
            || (defaultProfile is not null && name.Equals(defaultProfile.Database, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show($"\"{name}\" şu an aktif ya da varsayılan veritabanı, silinemez. Önce başka bir firmayı varsayılan yapın.", "Silinemez", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var typed = PromptForText(
            $"Bu işlem \"{name}\" veritabanını KALICI olarak siler (önce yedek alınmaz). Onaylamak için adı aynen yazın:",
            "Veritabanını Sil");
        if (!string.Equals(typed.Trim(), name, StringComparison.Ordinal))
        {
            MessageBox.Show("Ad eşleşmedi, işlem iptal edildi.", "İptal", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            DbOperations.DropDatabase(ProfileStore.BuildMasterConnectionString(profile), name);

            var remaining = all.Where(x => !(x.Database.Equals(name, StringComparison.OrdinalIgnoreCase) && x.Server.Equals(profile.Server, StringComparison.OrdinalIgnoreCase))).ToList();
            if (remaining.Count > 0 && !remaining.Any(x => x.IsDefault))
            {
                remaining[0].IsDefault = true;
            }
            ProfileStore.SaveProfiles(remaining);

            AddLog($"'{name}' veritabanı silindi", true);
            OperationLogStore.Append("Veritabanı silindi", name, true, profile.Server);
            MessageBox.Show($"\"{name}\" silindi.", "Veritabanı Sil", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            AddLog($"'{name}' silme", false);
            OperationLogStore.Append("Veritabanı silme", name, false, ex.Message);
            MessageBox.Show($"Silinemedi: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void BrowseBackupFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Yedek Klasörü Seçin" };
        if (dialog.ShowDialog() == true)
        {
            BackupFolderBox.Text = dialog.FolderName;
        }
    }

    private void RunMaintenance_Click(object sender, RoutedEventArgs e)
    {
        var profile = ActiveConnectionContext.Current;
        if (profile is null)
        {
            MessageBox.Show("Önce Bağlantı Ayarları'ndan bir bağlantı seçin/etkinleştirin.", "Bağlantı Yok", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CheckIntegrityCheck.IsChecked != true && CleanupLogCheck.IsChecked != true && IndexMaintenanceCheck.IsChecked != true && ShrinkCheck.IsChecked != true)
        {
            MessageBox.Show("En az bir bakım işlemi seçin.", "Seçim Yok", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var connStr = ProfileStore.BuildConnectionString(profile);

            if (CheckIntegrityCheck.IsChecked == true)
            {
                RunStep("Veri bütünlüğü denetimi", () => DbOperations.CheckIntegrity(connStr, profile.Database, _ => { }));
            }
            if (CleanupLogCheck.IsChecked == true)
            {
                RunStep("Log temizleme", () => DbOperations.CleanupTransactionLog(connStr, profile.Database, _ => { }));
            }
            if (IndexMaintenanceCheck.IsChecked == true)
            {
                RunStep("İndeks bakımı", () => DbOperations.RunIndexMaintenance(connStr, _ => { }));
            }
            if (ShrinkCheck.IsChecked == true)
            {
                RunStep("Dosya boyutunu küçült", () => DbOperations.ShrinkDatabase(connStr, profile.Database, _ => { }));
            }

            MessageBox.Show("Seçili bakım işlemleri tamamlandı.", "Bakım", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void RunStep(string name, Action action)
    {
        try
        {
            action();
            AddLog(name, true);
            OperationLogStore.Append(name, ActiveConnectionContext.Current?.Database ?? string.Empty, true, string.Empty);
        }
        catch (Exception ex)
        {
            AddLog(name, false);
            OperationLogStore.Append(name, ActiveConnectionContext.Current?.Database ?? string.Empty, false, ex.Message);
            MessageBox.Show($"{name} başarısız: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
