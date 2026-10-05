using System.Windows;
using System.Windows.Controls;
using SahinSoft.ConfigTool.Views;

namespace SahinSoft.ConfigTool;

public partial class MainWindow : Window
{
    private ConnectionSettingsView? _connectionView;
    private CentralConnectionView? _centralView;
    private MaintenanceView? _maintenanceView;
    private ClearTransactionsView? _clearTransactionsView;
    private CleanDatabaseView? _cleanDatabaseView;
    private HistoryView? _historyView;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => ShowConnectionView();
    }

    private void NavItem_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        if (ReferenceEquals(sender, NavConnection)) { ShowConnectionView(); }
        else if (ReferenceEquals(sender, NavCentral)) { ShowCentralView(); }
        else if (ReferenceEquals(sender, NavMaintenance)) { ShowMaintenanceView(); }
        else if (ReferenceEquals(sender, NavClearTransactions)) { ShowClearTransactionsView(); }
        else if (ReferenceEquals(sender, NavCleanDatabase)) { ShowCleanDatabaseView(); }
        else if (ReferenceEquals(sender, NavHistory)) { ShowHistoryView(); }
    }

    private void ShowConnectionView()
    {
        _connectionView ??= new ConnectionSettingsView();
        _connectionView.RefreshData();
        PageHost.Content = _connectionView;
    }

    private void ShowCentralView()
    {
        _centralView ??= new CentralConnectionView();
        _centralView.RefreshData();
        PageHost.Content = _centralView;
    }

    private void ShowMaintenanceView()
    {
        _maintenanceView ??= new MaintenanceView();
        _maintenanceView.RefreshData();
        PageHost.Content = _maintenanceView;
    }

    private void ShowClearTransactionsView()
    {
        if (_clearTransactionsView is null)
        {
            _clearTransactionsView = new ClearTransactionsView();
            _clearTransactionsView.EditConnectionRequested += () => NavConnection.IsChecked = true;
        }
        _clearTransactionsView.RefreshData();
        PageHost.Content = _clearTransactionsView;
    }

    private void ShowCleanDatabaseView()
    {
        if (_cleanDatabaseView is null)
        {
            _cleanDatabaseView = new CleanDatabaseView();
            _cleanDatabaseView.EditConnectionRequested += () => NavConnection.IsChecked = true;
        }
        _cleanDatabaseView.RefreshData();
        PageHost.Content = _cleanDatabaseView;
    }

    private void ShowHistoryView()
    {
        _historyView ??= new HistoryView();
        _historyView.RefreshData();
        PageHost.Content = _historyView;
    }
}
