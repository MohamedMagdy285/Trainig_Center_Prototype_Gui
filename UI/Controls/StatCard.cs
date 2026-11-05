namespace ECMS;

/// <summary>KPI card with an accent icon, value and a small comparison line.</summary>
public partial class StatCard : UserControl
{
    private bool _hover;

    public string Title { get; set; } = "";
    public string Value { get; set; } = "";
    public string Note { get; set; } = "";
    public string Delta { get; set; } = "";
    public bool Positive { get; set; } = true;
    public string Glyph { get; set; } = "";
    public Color Accent { get; set; } = Theme.Primary;

    public StatCard()
    {
        InitializeComponent();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

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
        Theme.DrawBorder(g, _hover ? Theme.Mix(Accent, Color.White, .45f) : Theme.Border, outer, 16);

        var iconRect = new Rectangle(Width - 62, 17, 44, 44);
        Theme.Fill(g, Theme.Mix(Accent, Color.White, .88f), iconRect, 14);
        Theme.CenteredText(g, Glyph, Theme.IconLarge, Accent, iconRect);

        Theme.Text(g, Title, Theme.SmallBold, Theme.Muted, new Rectangle(18, 14, Width - 80, 18));
        Theme.Text(g, Value, Theme.Value, Theme.Ink, new Rectangle(17, 32, Width - 34, 38),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        var x = 18;
        var bottom = 84;
        if (!string.IsNullOrEmpty(Delta))
        {
            var text = (Positive ? "\u25B2  " : "\u25BC  ") + Delta;
            var width = Theme.Measure(g, text, Theme.TinyBold) + 14;
            var fg = Positive ? Color.FromArgb(5, 150, 105) : Color.FromArgb(185, 28, 28);
            var bg = Positive ? Color.FromArgb(236, 253, 245) : Color.FromArgb(254, 242, 242);
            Theme.Pill(g, text, Theme.TinyBold, fg, bg, new Rectangle(x, bottom, width, 20), 10);
            x += width + 8;
            Theme.Text(g, "vs last month", Theme.Small, Theme.SoftText,
                new Rectangle(x, bottom, Math.Max(0, Width - x - 16), 20));
        }
        else
        {
            Theme.Text(g, Note, Theme.Small, Theme.Muted,
                new Rectangle(x, bottom, Width - 34, 20));
        }

        if (!string.IsNullOrEmpty(Delta) && !string.IsNullOrEmpty(Note))
        {
            Theme.Text(g, Note, Theme.Small, Theme.Muted,
                new Rectangle(18, 105, Width - 34, 15));
        }
    }
}
