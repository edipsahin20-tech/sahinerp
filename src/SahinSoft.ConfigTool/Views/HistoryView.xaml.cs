using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SahinSoft.ConfigTool.Services;

namespace SahinSoft.ConfigTool.Views;

public sealed class HistoryRow
{
    public string When { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Brush StatusColor { get; set; } = Brushes.Black;
}

public partial class HistoryView : UserControl
{
    public HistoryView()
    {
        InitializeComponent();
    }

    public void RefreshData()
    {
        var entries = OperationLogStore.Load();
        var rows = entries.Select(x => new HistoryRow
        {
            When = x.AtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
            Operation = x.Operation,
            Database = x.Database,
            Detail = x.Detail,
            Status = x.Success ? "Başarılı" : "Başarısız",
            StatusColor = x.Success ? (Brush)FindResource("SuccessGreen") : (Brush)FindResource("DangerRed")
        }).ToList();

        LogItems.ItemsSource = rows;
        EmptyText.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
