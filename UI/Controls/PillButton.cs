using System.Drawing.Drawing2D;

namespace ECMS;

/// <summary>Rounded button used for page actions and filters.</summary>
public partial class PillButton : UserControl
{
    private bool _hover;
    private bool _pressed;

    public string Glyph { get; set; } = "";
    public bool Filled { get; set; } = true;
    public Color Accent { get; set; } = Theme.Primary;

    public PillButton()
    {
        InitializeComponent();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        var r = new RectangleF(.5f, .5f, Width - 1, Height - 1);
        var radius = Height / 2f;

        Color fg;
        if (Filled)
        {
            var fill = _pressed ? Theme.Mix(Accent, Color.Black, .12f)
                : _hover ? Theme.Mix(Accent, Color.White, .10f) : Accent;
            using var path = Theme.Rounded(r, radius);
            using var brush = new LinearGradientBrush(r, fill, Theme.Mix(fill, Theme.Secondary, .38f), 20f);
            g.FillPath(brush, path);
            fg = Color.White;
        }
        else
        {
            var bg = _hover ? Theme.Mix(Theme.Field, Theme.Primary, .04f) : Color.White;
            Theme.Fill(g, bg, r, radius);
            Theme.DrawBorder(g, _hover ? Theme.Mix(Theme.Border, Theme.Primary, .30f) : Theme.Border, r, radius);
            fg = Theme.Ink;
        }

        var textW = Theme.Measure(g, Text, Theme.BodyBold);
        var iconW = string.IsNullOrEmpty(Glyph) ? 0 : 22;
        var total = textW + iconW;
        var x = (Width - total) / 2;
        if (!string.IsNullOrEmpty(Glyph))
        {
            Theme.Text(g, Glyph, Theme.IconSmall, fg, new Rectangle(x, 0, 18, Height),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            x += iconW;
        }
        Theme.Text(g, Text, Theme.BodyBold, fg, new Rectangle(x, 0, textW + 4, Height),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }
}
