using System.Drawing.Drawing2D;

namespace ECMS;

/// <summary>A small graduation-cap mark for the sidebar brand.</summary>
public partial class LogoMark : UserControl
{
    public LogoMark()
    {
        InitializeComponent();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        var r = new RectangleF(0, 0, Width, Height);
        using (var path = Theme.Rounded(r, 13))
        using (var brush = new LinearGradientBrush(r, Theme.Primary, Theme.Accent, 45f))
            g.FillPath(brush, path);

        using (var glow = new SolidBrush(Color.FromArgb(35, 255, 255, 255)))
            g.FillEllipse(glow, -8, -14, 46, 46);

        var cx = Width / 2f;
        var capY = Height * .36f;
        var cap = new[]
        {
            new PointF(cx, capY - 6), new PointF(Width - 10, capY),
            new PointF(cx, capY + 6), new PointF(10, capY)
        };
        using var white = new SolidBrush(Color.White);
        g.FillPolygon(white, cap);
        g.FillRectangle(white, cx - 9, capY + 4, 18, 6);
        using var line = new Pen(Color.FromArgb(210, 255, 255, 255), 1.6f);
        g.DrawLine(line, Width - 13, capY, Width - 13, capY + 10);
        g.FillEllipse(white, Width - 15, capY + 9, 4, 4);
    }
}
