namespace ECMS;

public enum WindowButtonKind { Minimize, Maximize, Close }

/// <summary>Minimal, hand-drawn window controls for the frameless shell.</summary>
public partial class WindowButton : UserControl
{
    private bool _hover;

    public WindowButtonKind Kind { get; set; }

    public WindowButton()
    {
        InitializeComponent();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        if (_hover)
        {
            var bg = Kind == WindowButtonKind.Close ? Color.FromArgb(254, 226, 226) : Theme.Field;
            Theme.Fill(g, bg, new RectangleF(3, 3, Width - 6, Height - 6), 10);
        }
        var fg = _hover && Kind == WindowButtonKind.Close ? Theme.Red : Theme.Muted;
        using var pen = new Pen(fg, 1.5f);
        var cx = Width / 2f;
        var cy = Height / 2f;
        if (Kind == WindowButtonKind.Minimize)
            g.DrawLine(pen, cx - 5, cy, cx + 5, cy);
        else if (Kind == WindowButtonKind.Maximize)
            g.DrawRectangle(pen, cx - 5, cy - 5, 10, 10);
        else
        {
            g.DrawLine(pen, cx - 4.5f, cy - 4.5f, cx + 4.5f, cy + 4.5f);
            g.DrawLine(pen, cx + 4.5f, cy - 4.5f, cx - 4.5f, cy + 4.5f);
        }
    }
}
