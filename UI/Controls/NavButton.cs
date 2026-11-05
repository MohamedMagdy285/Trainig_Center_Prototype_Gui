namespace ECMS;

/// <summary>Pill-shaped navigation button used in the sidebar.</summary>
public partial class NavButton : UserControl
{
    private bool _hover;

    public string Glyph { get; set; } = "";
    public string LabelText { get; set; } = "";
    public bool Active { get; set; }

    public NavButton()
    {
        InitializeComponent();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        var r = new RectangleF(0, 0, Width, Height);
        if (Active)
        {
            Theme.Fill(g, Color.FromArgb(32, 255, 255, 255), r, 12);
            Theme.Fill(g, Theme.Accent, new RectangleF(0, 13, 3, Height - 26), 1.5f);
        }
        else if (_hover)
            Theme.Fill(g, Color.FromArgb(14, 255, 255, 255), r, 12);

        var iconColor = Active ? Color.White : Color.FromArgb(148, 163, 184);
        Theme.Text(g, Glyph, Theme.Icon, iconColor, new Rectangle(15, 0, 26, Height),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        Theme.Text(g, LabelText, Theme.Nav, Active ? Color.White : Color.FromArgb(170, 181, 200),
            new Rectangle(46, 0, Width - 54, Height));
    }
}
