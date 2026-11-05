using System.Drawing.Drawing2D;

namespace ECMS;

/// <summary>Revenue / refunds line chart with a hover tooltip.</summary>
public partial class RevenueChartCard : UserControl
{
    private int _hoverIndex = -1;
    private int[] _revenue = Array.Empty<int>();
    private int[] _refunds = Array.Empty<int>();
    private string[] _months = Array.Empty<string>();

    public RevenueChartCard()
    {
        InitializeComponent();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width > 0 && Height > 0)
            Region = new Region(Theme.Rounded(ClientRectangle, 16));
    }

    private void RefreshData()
    {
        var series = ReportService.MonthlySeries(9);
        _revenue = series.Revenue;
        _refunds = series.Refunds;
        _months = series.Months;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_revenue.Length == 0) return;
        var chart = ChartRect();
        if (!chart.Contains(e.Location))
        {
            if (_hoverIndex != -1) { _hoverIndex = -1; Invalidate(); }
            return;
        }
        var step = chart.Width / (float)Math.Max(1, _revenue.Length - 1);
        var index = (int)Math.Round((e.X - chart.Left) / step);
        index = Math.Clamp(index, 0, _revenue.Length - 1);
        if (index != _hoverIndex) { _hoverIndex = index; Invalidate(); }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hoverIndex = -1;
        Invalidate();
        base.OnMouseLeave(e);
    }

    private Rectangle ChartRect() => new(54, 80, Math.Max(10, Width - 78), Math.Max(10, Height - 132));

    protected override void OnPaint(PaintEventArgs e)
    {
        RefreshData();
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        var outer = new RectangleF(.5f, .5f, Width - 1, Height - 1);
        Theme.Fill(g, Color.White, outer, 16);
        Theme.DrawBorder(g, Theme.Border, outer, 16);

        Theme.Text(g, "Revenue overview", Theme.H2, Theme.Ink, new Rectangle(20, 15, 240, 24));
        Theme.Text(g, "Successful payments and refunds  •  Last 9 months", Theme.Small, Theme.Muted,
            new Rectangle(20, 39, 310, 18));

        var legendX = Width - 214;
        if (legendX > 330)
        {
            using var primary = new SolidBrush(Theme.Primary);
            using var amber = new SolidBrush(Theme.Amber);
            g.FillEllipse(primary, legendX, 23, 8, 8);
            Theme.Text(g, "Revenue", Theme.Small, Theme.Muted, new Rectangle(legendX + 14, 17, 64, 20));
            g.FillEllipse(amber, legendX + 92, 23, 8, 8);
            Theme.Text(g, "Refunds", Theme.Small, Theme.Muted, new Rectangle(legendX + 106, 17, 64, 20));
        }

        if (_revenue.Length == 0) return;

        var chart = ChartRect();
        var maxValue = Math.Max(1, Math.Max(_revenue.Max(), _refunds.Max()));
        var max = (int)(Math.Ceiling(maxValue / 50000.0) * 50000);

        using var grid = new Pen(Theme.Mix(Theme.Border, Color.White, .25f)) { DashStyle = DashStyle.Dash };
        for (var i = 0; i <= 4; i++)
        {
            var y = chart.Bottom - i * (chart.Height / 4f);
            g.DrawLine(grid, chart.Left, y, chart.Right, y);
            var label = i == 0 ? "0" : $"{i * max / 4 / 1000}K";
            Theme.Text(g, label, Theme.Small, Theme.SoftText,
                new Rectangle(2, (int)y - 9, 45, 18),
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        var points = new PointF[_revenue.Length];
        var refundPoints = new PointF[_revenue.Length];
        var step = chart.Width / (float)Math.Max(1, _revenue.Length - 1);
        for (var i = 0; i < points.Length; i++)
        {
            var x = chart.Left + i * step;
            points[i] = new PointF(x, chart.Bottom - _revenue[i] / (float)max * chart.Height);
            refundPoints[i] = new PointF(x, chart.Bottom - _refunds[i] / (float)max * chart.Height);
        }

        var area = new List<PointF> { new(points[0].X, chart.Bottom) };
        area.AddRange(points);
        area.Add(new PointF(points[^1].X, chart.Bottom));
        using (var areaPath = new GraphicsPath())
        {
            areaPath.AddPolygon(area.ToArray());
            using var fill = new LinearGradientBrush(
                new PointF(0, chart.Top), new PointF(0, chart.Bottom),
                Color.FromArgb(72, Theme.Primary), Color.FromArgb(2, Theme.Primary));
            g.FillPath(fill, areaPath);
        }

        using (var line = new Pen(Theme.Primary, 2.5f) { LineJoin = LineJoin.Round })
            g.DrawLines(line, points);
        using (var refundPen = new Pen(Theme.Amber, 1.7f) { DashStyle = DashStyle.Dash })
            g.DrawLines(refundPen, refundPoints);

        for (var i = 0; i < _months.Length; i++)
        {
            var x = chart.Left + i * step;
            Theme.Text(g, _months[i], Theme.Small, Theme.SoftText,
                new Rectangle((int)(x - 22), chart.Bottom + 8, 44, 18),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        if (_hoverIndex >= 0)
        {
            var p = points[_hoverIndex];
            using (var guide = new Pen(Theme.Mix(Theme.Primary, Color.White, .65f)) { DashStyle = DashStyle.Dash })
                g.DrawLine(guide, p.X, chart.Top, p.X, chart.Bottom);
            using (var halo = new SolidBrush(Color.FromArgb(40, Theme.Primary)))
                g.FillEllipse(halo, p.X - 8, p.Y - 8, 16, 16);
            using (var dot = new SolidBrush(Theme.Primary))
                g.FillEllipse(dot, p.X - 4, p.Y - 4, 8, 8);
            using (var white = new SolidBrush(Color.White))
                g.FillEllipse(white, p.X - 1.5f, p.Y - 1.5f, 3, 3);

            var amount = Fmt.Money(_revenue[_hoverIndex]);
            var tipW = 146;
            var tipX = (int)Math.Clamp(p.X - tipW / 2f, chart.Left, chart.Right - tipW);
            var tipY = Math.Max(58, (int)p.Y - 62);
            Theme.Fill(g, Theme.Ink, new RectangleF(tipX, tipY, tipW, 48), 10);
            Theme.Text(g, _months[_hoverIndex].ToUpperInvariant() + " " + DateTime.Today.Year, Theme.TinyBold,
                Color.FromArgb(190, 214, 220), new Rectangle(tipX + 10, tipY + 5, tipW - 20, 15));
            Theme.Text(g, amount, Theme.SmallBold, Color.White, new Rectangle(tipX + 10, tipY + 21, tipW - 20, 18));
        }
    }
}
