using System.Drawing.Drawing2D;

namespace SahinSoft.Setup;

/// <summary>
/// Windows'un varsayılan (yeşil, tema bağımlı) ProgressBar'ı marka renkleriyle hiç uyuşmuyordu -
/// kendi çizdiğimiz bu kontrol yuvarlak köşeli, altın degrade dolgulu, marka rengine tam uyan
/// bir ilerleme çubuğu veriyor.
/// </summary>
public sealed class GoldProgressBar : Panel
{
    private int _value;
    private int _maximum = 100;

    // Bu kontrol yalnızca kod içinden kullanılıyor (Designer'da sürükle-bırak yok) - WFO1000
    // tasarımcı serileştirme uyarısını burada bilerek bastırıyoruz.
#pragma warning disable WFO1000
    public int Maximum
    {
        get => _maximum;
        set { _maximum = Math.Max(1, value); Invalidate(); }
    }

    public int Value
    {
        get => _value;
        set { _value = Math.Clamp(value, 0, _maximum); Invalidate(); }
    }
#pragma warning restore WFO1000

    public GoldProgressBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = 14;
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var trackRect = new Rectangle(0, 0, Width - 1, Height - 1);
        var radius = Height / 2;

        using (var trackPath = RoundedRect(trackRect, radius))
        using (var trackBrush = new SolidBrush(Color.FromArgb(230, 230, 230)))
        using (var trackPen = new Pen(Color.FromArgb(210, 210, 210)))
        {
            g.FillPath(trackBrush, trackPath);
            g.DrawPath(trackPen, trackPath);
        }

        var fillRatio = Math.Clamp((double)_value / _maximum, 0, 1);
        var fillWidth = (int)(trackRect.Width * fillRatio);
        if (fillWidth > Height)
        {
            var fillRect = new Rectangle(0, 0, fillWidth, Height - 1);
            using var fillPath = RoundedRect(fillRect, radius);
            using var fillBrush = new LinearGradientBrush(
                fillRect,
                Color.FromArgb(232, 189, 90),
                Color.FromArgb(196, 148, 43),
                LinearGradientMode.Vertical);
            g.FillPath(fillBrush, fillPath);
        }
        else if (fillWidth > 0)
        {
            // Çok küçük değerlerde de en azından bir nokta/iz görünsün.
            using var fillBrush = new SolidBrush(Color.FromArgb(212, 164, 55));
            g.FillEllipse(fillBrush, 0, 0, Height - 1, Height - 1);
        }
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        if (diameter <= 0 || bounds.Width <= diameter || bounds.Height <= diameter)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
