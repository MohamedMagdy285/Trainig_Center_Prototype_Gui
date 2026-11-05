using System.Drawing.Drawing2D;

namespace ECMS;

/// <summary>Small "this year" financial insight card in the sidebar.</summary>
public partial class InsightCard : UserControl
{
    public InsightCard()
    {
        InitializeComponent();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        var r = new RectangleF(.5f, .5f, Width - 1, Height - 1);
        Theme.Fill(g, Color.FromArgb(20, 255, 255, 255), r, 14);
        Theme.DrawBorder(g, Color.FromArgb(28, 255, 255, 255), r, 14);

        var stats = ReportService.Dashboard();
        Theme.Text(g, "NET REVENUE", Theme.TinyBold, Color.FromArgb(190, 214, 220),
            new Rectangle(16, 13, Width - 30, 16));
        using var big = new Font("Segoe UI", 16f, FontStyle.Bold);
        Theme.Text(g, Fmt.Money(stats.NetRevenue), big, Color.White,
            new Rectangle(14, 30, Width - 28, 32), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        Theme.Text(g, "Revenue minus refunds", Theme.Small, Color.FromArgb(190, 214, 220),
            new Rectangle(16, 62, Width - 40, 17));

        var (revenue, _, _) = ReportService.MonthlySeries(9);
        var max = Math.Max(1, revenue.Max());
        var plot = new Rectangle(16, 82, Width - 32, 20);
        var pts = new PointF[revenue.Length];
        for (var i = 0; i < revenue.Length; i++)
            pts[i] = new PointF(plot.Left + i * (plot.Width / (float)Math.Max(1, revenue.Length - 1)),
                plot.Bottom - revenue[i] / (float)max * plot.Height);
        using var pen = new Pen(Theme.Accent, 1.8f) { LineJoin = LineJoin.Round };
        g.DrawLines(pen, pts);
        using var dot = new SolidBrush(Theme.Accent);
        g.FillEllipse(dot, pts[^1].X - 3, pts[^1].Y - 3, 6, 6);
    }
}
