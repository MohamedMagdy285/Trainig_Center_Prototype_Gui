namespace ECMS;

/// <summary>Readable refund policy summary card for the payments page.</summary>
public partial class RefundPolicyCard : UserControl
{
    public RefundPolicyCard()
    {
        InitializeComponent();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width > 0 && Height > 0)
            Region = new Region(Theme.Rounded(ClientRectangle, 16));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        g.Clear(Color.White);
        Theme.DrawBorder(g, Theme.Border, new RectangleF(.5f, .5f, Width - 1, Height - 1), 16);

        Theme.Text(g, "Refund policy", Theme.H2, Theme.Ink, new Rectangle(20, 16, 220, 24));
        Theme.Text(g, "The rules agreed for each learning format", Theme.Small, Theme.Muted,
            new Rectangle(20, 40, Width - 40, 18));

        Section(g, "OFFLINE COURSES", 78, Theme.Violet);
        PolicyRow(g, 100, "Before the course starts", "100%", Theme.Green);
        PolicyRow(g, 130, "During the first week", "50%", Theme.Amber);
        PolicyRow(g, 160, "After the first week", "No refund", Theme.Red);

        using (var sep = new Pen(Theme.Mix(Theme.Border, Color.White, .25f)))
            g.DrawLine(sep, 20, 198, Width - 20, 198);

        Section(g, "ONLINE COURSES", 216, Theme.Teal);
        PolicyRow(g, 238, "First section preview", "Free", Theme.Blue);
        PolicyRow(g, 268, "After purchase", "No refund", Theme.Red);

        using (var sep = new Pen(Theme.Mix(Theme.Border, Color.White, .25f)))
            g.DrawLine(sep, 20, 306, Width - 20, 306);

        Section(g, "PAYMENT METHODS", 324, Theme.Primary);
        var x = 20;
        var y = 348;
        foreach (var method in new[] { "Visa", "Mastercard", "Vodafone Cash", "Fawry" })
        {
            var w = Theme.Measure(g, method, Theme.TinyBold) + 20;
            if (x > 20 && x + w > Width - 20)
            {
                x = 20;
                y += 31;
            }
            Theme.Pill(g, method, Theme.TinyBold, Theme.Muted, Theme.Field,
                new Rectangle(x, y, w, 24), 9);
            x += w + 7;
        }

        using var noteFont = new Font("Segoe UI", 7.5f, FontStyle.Italic);
        Theme.Text(g, "All payments are processed securely",
            noteFont, Theme.SoftText, new Rectangle(20, Height - 48, Width - 40, 18));
        Theme.Text(g, "through the payment gateway.",
            noteFont, Theme.SoftText, new Rectangle(20, Height - 32, Width - 40, 18));
    }

    private static void Section(Graphics g, string text, int y, Color color)
    {
        using var dot = new SolidBrush(color);
        g.FillEllipse(dot, 20, y + 4, 7, 7);
        Theme.Text(g, text, Theme.TinyBold, Theme.Muted, new Rectangle(33, y, 200, 16));
    }

    private void PolicyRow(Graphics g, int y, string label, string value, Color color)
    {
        var pillW = Theme.Measure(g, value, Theme.TinyBold) + 18;
        var pillX = Math.Max(20, Width - 20 - pillW);
        var labelWidth = Math.Max(60, pillX - 20 - 10);
        Theme.Text(g, label, Theme.Small, Theme.Ink, new Rectangle(20, y, labelWidth, 22));
        Theme.Pill(g, value, Theme.TinyBold, color, Theme.Mix(color, Color.White, .89f),
            new Rectangle(pillX, y, pillW, 22), 10);
    }
}
