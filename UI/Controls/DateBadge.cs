namespace ECMS;

/// <summary>Small date chip in the top bar.</summary>
public partial class DateBadge : UserControl
{
    public DateBadge()
    {
        InitializeComponent();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        var r = new RectangleF(.5f, .5f, Width - 1, Height - 1);
        Theme.Fill(g, Color.White, r, 11);
        Theme.DrawBorder(g, Theme.Border, r, 11);
        Theme.Text(g, "\uE787", Theme.IconSmall, Theme.Primary, new Rectangle(12, 0, 20, Height),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        Theme.Text(g, DateTime.Today.ToString("ddd, dd MMM"), Theme.SmallBold, Theme.Ink,
            new Rectangle(34, 0, Width - 44, Height));
    }
}
