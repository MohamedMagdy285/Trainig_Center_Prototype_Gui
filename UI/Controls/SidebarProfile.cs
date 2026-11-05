namespace ECMS;

/// <summary>Compact admin identity block at the bottom of the sidebar.</summary>
public partial class SidebarProfile : UserControl
{
    public SidebarProfile()
    {
        InitializeComponent();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        Theme.Avatar(g, new Rectangle(8, 14, 38, 38), Session.Name, Theme.Accent);
        Theme.Text(g, Session.Name, Theme.SmallBold, Color.White, new Rectangle(56, 14, Width - 80, 18));
        Theme.Text(g, Session.RoleText, Theme.Small, Color.FromArgb(190, 214, 220),
            new Rectangle(56, 32, Width - 80, 17));
        Theme.Text(g, "\uE712", Theme.IconSmall, Color.FromArgb(148, 163, 184),
            new Rectangle(Width - 30, 0, 20, Height), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }
}
