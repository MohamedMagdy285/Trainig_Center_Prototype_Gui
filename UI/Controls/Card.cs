namespace ECMS;

/// <summary>A white panel with softly rounded corners.</summary>
public partial class Card : UserControl
{
    public int Radius { get; set; } = 16;
    public Color BorderColor { get; set; } = Theme.Border;

    public Card()
    {
        InitializeComponent();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width > 0 && Height > 0)
            Region = new Region(Theme.Rounded(ClientRectangle, Radius));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Theme.SetupGraphics(e.Graphics);
        var r = new RectangleF(.5f, .5f, Width - 1f, Height - 1f);
        Theme.DrawBorder(e.Graphics, BorderColor, r, Radius);
    }
}
