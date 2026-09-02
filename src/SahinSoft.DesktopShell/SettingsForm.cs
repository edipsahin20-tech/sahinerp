namespace SahinSoft.DesktopShell;

public sealed class SettingsForm : Form
{
    private readonly TextBox _urlBox;
    private readonly TextBox _titleBox;

    public string ResultUrl { get; private set; } = string.Empty;
    public string ResultTitle { get; private set; } = string.Empty;

    public SettingsForm(ShellConfig current)
    {
        Text = "Bağlantı Ayarları";
        Width = 480;
        Height = 220;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(16),
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _urlBox = new TextBox { Text = current.Url, Dock = DockStyle.Fill };
        _titleBox = new TextBox { Text = current.Title, Dock = DockStyle.Fill };

        void AddRow(string label, Control input)
        {
            var row = layout.RowCount;
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.Controls.Add(new Label { Text = label, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.Left }, 0, row);
            layout.Controls.Add(input, 1, row);
        }

        AddRow("Adres (URL)", _urlBox);
        AddRow("Pencere Başlığı", _titleBox);

        var hint = new Label
        {
            Text = "Örnek: yerel kurulum için http://localhost:1666, buluttaki merkez için http://sunucu-adresi",
            AutoSize = false,
            Height = 40,
            ForeColor = Color.DimGray,
            Dock = DockStyle.Fill
        };
        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.Controls.Add(hint, 0, layout.RowCount - 1);
        layout.SetColumnSpan(hint, 2);

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
        var saveButton = new Button { Text = "Kaydet ve Yeniden Başlat", AutoSize = true, Padding = new Padding(8, 4, 8, 4), BackColor = Color.FromArgb(212, 164, 55) };
        saveButton.Click += SaveButton_Click;
        var cancelButton = new Button { Text = "Vazgeç", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
        cancelButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);

        layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.Controls.Add(buttonPanel, 0, layout.RowCount - 1);
        layout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(layout);
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        var url = _urlBox.Text.Trim();
        if (url.Length == 0 || !Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            MessageBox.Show(this, "Geçerli bir adres girin (örn. http://localhost:1666).", "Geçersiz Adres", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ResultUrl = url;
        ResultTitle = _titleBox.Text.Trim().Length > 0 ? _titleBox.Text.Trim() : "ŞahinSoft";
        DialogResult = DialogResult.OK;
        Close();
    }
}
