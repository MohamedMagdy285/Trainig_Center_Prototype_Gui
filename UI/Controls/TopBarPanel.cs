namespace ECMS;

/// <summary>Top bar that paints its own hairline bottom border.</summary>
public partial class TopBarPanel : UserControl
{
    public TopBarPanel()
    {
        InitializeComponent();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(Theme.Border);
        e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
    }
}
