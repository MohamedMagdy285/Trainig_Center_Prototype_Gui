using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace ECMS;

/// <summary>
/// One place for the EduSphere colours, fonts and drawing helpers.
/// The whole application uses this palette:
/// Primary #142B4A, Secondary #245B78, Accent #2A9D8F,
/// Background #EAF4F4 and Surface #FFFFFF.
/// </summary>
internal static class Theme
{
    // Brand palette
    public static readonly Color Primary = Hex("#142B4A");
    public static readonly Color PrimaryDark = Hex("#0E1F36");
    public static readonly Color Secondary = Hex("#245B78");
    public static readonly Color Accent = Hex("#2A9D8F");

    public static readonly Color Bg = Hex("#EAF4F4");
    public static readonly Color Surface = Hex("#FFFFFF");

    // Supporting colours
    public static readonly Color Ink = Hex("#142B4A");
    public static readonly Color Muted = Hex("#5E7385");
    public static readonly Color SoftText = Hex("#8AA0AE");
    public static readonly Color Border = Hex("#D5E6E6");
    public static readonly Color Field = Hex("#F3F9F9");

    public static readonly Color Green = Hex("#2A9D8F");
    public static readonly Color Amber = Hex("#E9A23B");
    public static readonly Color Red = Hex("#D1495B");
    public static readonly Color Blue = Hex("#245B78");
    public static readonly Color Teal = Hex("#2A9D8F");
    public static readonly Color Violet = Hex("#5C7C99");

    public static readonly Color SidebarTop = Primary;
    public static readonly Color SidebarBottom = Secondary;

    // Fonts
    public static readonly Font Title = new("Segoe UI", 20f, FontStyle.Bold);
    public static readonly Font H2 = new("Segoe UI", 13f, FontStyle.Bold);
    public static readonly Font H3 = new("Segoe UI", 10.5f, FontStyle.Bold);
    public static readonly Font CardTitle = new("Segoe UI", 11.5f, FontStyle.Bold);
    public static readonly Font Price = new("Segoe UI", 12.5f, FontStyle.Bold);
    public static readonly Font Body = new("Segoe UI", 9.5f);
    public static readonly Font BodyBold = new("Segoe UI", 9.5f, FontStyle.Bold);
    public static readonly Font Small = new("Segoe UI", 8.5f);
    public static readonly Font SmallBold = new("Segoe UI", 8.5f, FontStyle.Bold);
    public static readonly Font TinyBold = new("Segoe UI", 7.5f, FontStyle.Bold);
    public static readonly Font Value = new("Segoe UI", 23f, FontStyle.Bold);
    public static readonly Font Nav = new("Segoe UI", 9.75f, FontStyle.Bold);
    public static readonly Font IconSmall = new("Segoe MDL2 Assets", 10f);
    public static readonly Font Icon = new("Segoe MDL2 Assets", 12f);
    public static readonly Font IconLarge = new("Segoe MDL2 Assets", 17f);

    public static readonly Color[] AvatarPalette =
    {
        Primary, Secondary, Accent, Amber, Blue, Violet
    };

    public static Color Hex(string value) => ColorTranslator.FromHtml(value);

    public static Color AccentFor(string value)
    {
        var hash = 0;
        foreach (var c in value) hash = (hash * 31 + c) & 0x7fffffff;
        return AvatarPalette[hash % AvatarPalette.Length];
    }

    public static Color Mix(Color a, Color b, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        return Color.FromArgb(
            (int)(a.A + (b.A - a.A) * amount),
            (int)(a.R + (b.R - a.R) * amount),
            (int)(a.G + (b.G - a.G) * amount),
            (int)(a.B + (b.B - a.B) * amount));
    }

    public static GraphicsPath Rounded(RectangleF r, float radius)
    {
        var path = new GraphicsPath();
        radius = Math.Min(radius, Math.Min(r.Width, r.Height) / 2f);
        if (radius <= 0) { path.AddRectangle(r); return path; }
        var d = radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void Fill(Graphics g, Color color, RectangleF r, float radius)
    {
        using var path = Rounded(r, radius);
        using var brush = new SolidBrush(color);
        g.FillPath(brush, path);
    }

    public static void DrawBorder(Graphics g, Color color, RectangleF r, float radius, float width = 1f)
    {
        using var path = Rounded(r, radius);
        using var pen = new Pen(color, width);
        g.DrawPath(pen, path);
    }

    public static void Text(Graphics g, string text, Font font, Color color, Rectangle bounds,
        TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis)
    {
        TextRenderer.DrawText(g, text, font, bounds, color, flags | TextFormatFlags.NoPrefix);
    }

    public static void CenteredText(Graphics g, string text, Font font, Color color, Rectangle bounds)
    {
        TextRenderer.DrawText(g, text, font, bounds, color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    public static int Measure(Graphics g, string text, Font font)
        => TextRenderer.MeasureText(g, text, font, Size.Empty, TextFormatFlags.NoPrefix).Width;

    public static void Avatar(Graphics g, Rectangle r, string name, Color color)
    {
        using (var brush = new LinearGradientBrush(r, Mix(color, Color.White, .18f),
                   Mix(color, Color.Black, .12f), 45f))
            g.FillEllipse(brush, r);
        var initials = Initials(name);
        using var f = new Font("Segoe UI", Math.Max(7f, r.Height * .31f), FontStyle.Bold);
        CenteredText(g, initials, f, Color.White, r);
    }

    public static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        return (parts.Length == 1 ? parts[0][..1] : parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
    }

    public static void Pill(Graphics g, string text, Font font, Color fg, Color bg, Rectangle r, float radius = 9f)
    {
        Fill(g, bg, r, radius);
        CenteredText(g, text, font, fg, r);
    }

    public static (Color fg, Color bg) StatusColors(string status) => status switch
    {
        "Successful" or "Active" or "Paid" or "Confirmed" or "Passed" or "Approved"
            => (Hex("#1B7F73"), Hex("#E6F5F2")),
        "Online" or "Remote" => (Secondary, Hex("#E9F2F7")),
        "Offline" => (Violet, Hex("#EEF3F7")),
        "Both" => (Accent, Hex("#E6F5F2")),
        "Pending" or "Scheduled" or "Ongoing"
            => (Hex("#A56A0A"), Hex("#FFF5E2")),
        "Refunded" or "Deactivated" or "Inactive" or "Completed" or "Cancelled"
            => (Muted, Hex("#EEF3F5")),
        "Failed" or "Rejected" or "Full"
            => (Hex("#B03A4A"), Hex("#FBECEE")),
        "Bonus" or "Preview" => (Accent, Hex("#E6F5F2")),
        _ => (Muted, Field)
    };

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    public static void Drag(Form? form)
    {
        if (form is null) return;
        ReleaseCapture();
        SendMessage(form.Handle, 0xA1, 0x2, 0);
    }

    public static void SetupGraphics(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
    }

    /// <summary>Applies the EduSphere palette to a context menu.</summary>
    public static void StyleMenu(ContextMenuStrip menu)
    {
        menu.BackColor = Surface;
        menu.ForeColor = Ink;
        menu.Font = Body;
        menu.ShowImageMargin = false;
        menu.Padding = new Padding(4);
        menu.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
        foreach (ToolStripItem item in menu.Items)
        {
            item.BackColor = Surface;
            item.ForeColor = Ink;
            item.Padding = new Padding(8, 4, 12, 4);
        }
    }

    private sealed class MenuColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Field;
        public override Color MenuItemSelectedGradientBegin => Field;
        public override Color MenuItemSelectedGradientEnd => Field;
        public override Color MenuItemBorder => Border;
        public override Color MenuBorder => Border;
        public override Color ToolStripDropDownBackground => Surface;
        public override Color ImageMarginGradientBegin => Surface;
        public override Color ImageMarginGradientMiddle => Surface;
        public override Color ImageMarginGradientEnd => Surface;
    }
}
