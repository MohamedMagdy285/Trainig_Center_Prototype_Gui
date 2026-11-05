namespace ECMS;

/// <summary>Round icon button (bell, notifications...).</summary>
public partial class IconButton : UserControl
{
    private bool _hover;

    public string Glyph { get; set; } = "";
    public bool ShowBadge { get; set; }

    public IconButton()
    {
        InitializeComponent();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        if (_hover) Theme.Fill(g, Theme.Field, new RectangleF(2, 2, Width - 4, Height - 4), 12);
        Theme.CenteredText(g, Glyph, Theme.Icon, Theme.Muted, ClientRectangle);
        if (ShowBadge)
        {
            using var b = new SolidBrush(Theme.Red);
            g.FillEllipse(b, Width - 14, 9, 8, 8);
            using var w = new SolidBrush(Color.White);
            g.FillEllipse(w, Width - 12, 11, 4, 4);
        }
    }
}
