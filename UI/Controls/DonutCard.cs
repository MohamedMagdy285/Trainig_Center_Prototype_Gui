using System.Drawing.Drawing2D;

namespace ECMS;

/// <summary>Donut chart for online / offline learners.</summary>
public partial class DonutCard : UserControl
{
    public DonutCard()
    {
        InitializeComponent();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width > 0 && Height > 0)
            Region = new Region(Theme.Rounded(ClientRectangle, 16));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        var outer = new RectangleF(.5f, .5f, Width - 1, Height - 1);
        Theme.Fill(g, Color.White, outer, 16);
        Theme.DrawBorder(g, Theme.Border, outer, 16);

        Theme.Text(g, "Student mix", Theme.H2, Theme.Ink, new Rectangle(20, 15, 200, 24));
        Theme.Text(g, "Distribution by learning format", Theme.Small, Theme.Muted,
            new Rectangle(20, 39, 240, 18));

        var (online, offline) = ReportService.StudentMix();
        var total = Math.Max(1, online + offline);

        var size = 148;
        var cx = 20 + size / 2;
        var cy = 86 + size / 2;
        var ring = new Rectangle(cx - size / 2, cy - size / 2, size, size);
        using (var track = new Pen(Theme.Field, 18f))
            g.DrawArc(track, ring, 0, 360);

        var offlineSweep = 360f * offline / total;
        var onlineSweep = 360f * online / total;
        using (var offlinePen = new Pen(Theme.Primary, 18f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            g.DrawArc(offlinePen, ring, -90, Math.Max(1, offlineSweep - 3));
        using (var onlinePen = new Pen(Theme.Accent, 18f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            g.DrawArc(onlinePen, ring, -90 + offlineSweep + 3, Math.Max(1, onlineSweep - 6));

        using var totalFont = new Font("Segoe UI", 18f, FontStyle.Bold);
        Theme.Text(g, total.ToString("N0"), totalFont, Theme.Ink,
            new Rectangle(cx - 60, cy - 20, 120, 30),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        Theme.Text(g, "learners", Theme.Small, Theme.Muted,
            new Rectangle(cx - 50, cy + 10, 100, 18),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var legendX = 190;
        if (legendX + 80 < Width)
        {
            DrawLegend(g, legendX, 108, Theme.Primary, "Offline learners", offline.ToString("N0"),
                Fmt.Percent(offline * 100.0 / total));
            DrawLegend(g, legendX, 162, Theme.Accent, "Online learners", online.ToString("N0"),
                Fmt.Percent(online * 100.0 / total));
        }

        using var note = new Font("Segoe UI", 7.5f, FontStyle.Italic);
        Theme.Text(g, "A learner may appear in both formats.", note, Theme.SoftText,
            new Rectangle(20, Height - 30, Width - 40, 18));
    }

    private static void DrawLegend(Graphics g, int x, int y, Color color, string label, string value, string percent)
    {
        using var dot = new SolidBrush(color);
        g.FillEllipse(dot, x, y + 4, 9, 9);
        Theme.Text(g, label, Theme.Small, Theme.Muted, new Rectangle(x + 17, y, 130, 18));
        Theme.Text(g, value, Theme.BodyBold, Theme.Ink, new Rectangle(x + 17, y + 17, 75, 20));
        Theme.Text(g, percent, Theme.TinyBold, color, new Rectangle(x + 96, y + 18, 54, 18));
    }
}
