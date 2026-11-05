namespace ECMS;

/// <summary>Profile chip at the far right of the top bar.</summary>
public partial class AdminChip : UserControl
{
    private bool _hover;

    public AdminChip()
    {
        InitializeComponent();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        if (_hover) Theme.Fill(g, Theme.Field, new RectangleF(0, 2, Width, Height - 4), 12);
        Theme.Avatar(g, new Rectangle(4, 5, 34, 34), Session.Name, Theme.Primary);
        Theme.Text(g, Session.Name, Theme.SmallBold, Theme.Ink, new Rectangle(46, 5, Width - 70, 17));
        Theme.Text(g, Session.RoleText, Theme.Small, Theme.Muted, new Rectangle(46, 22, Width - 70, 15));
        Theme.Text(g, "\uE70D", Theme.IconSmall, Theme.SoftText, new Rectangle(Width - 25, 0, 18, Height),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }
}
