namespace ECMS;

/// <summary>Small selectable filter chip.</summary>
public partial class Chip : UserControl
{
    private bool _hover;
    private bool _selected;

    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected == value) return;
            _selected = value;
            Invalidate();
        }
    }

    public Chip()
    {
        InitializeComponent();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        var r = new RectangleF(.5f, .5f, Width - 1, Height - 1);
        var bg = Selected ? Theme.Primary : _hover ? Theme.Mix(Theme.Field, Theme.Primary, .05f) : Color.White;
        Theme.Fill(g, bg, r, Height / 2f);
        if (!Selected) Theme.DrawBorder(g, Theme.Border, r, Height / 2f);
        Theme.CenteredText(g, Text, Theme.SmallBold, Selected ? Color.White : Theme.Muted, ClientRectangle);
    }
}
