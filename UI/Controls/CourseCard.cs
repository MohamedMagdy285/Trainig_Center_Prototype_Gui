using System.Drawing.Drawing2D;

namespace ECMS;

/// <summary>Course tile with a colourful cover and key course facts.</summary>
public partial class CourseCard : UserControl
{
    private bool _hover;
    private Course? _data;

    public event EventHandler? OpenRequested;
    public event EventHandler? EditRequested;

    public Course? Data
    {
        get => _data;
        set { _data = value; Invalidate(); }
    }

    public CourseCard()
    {
        InitializeComponent();
    }

    public CourseCard(Course course) : this()
    {
        Data = course;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width > 0 && Height > 0)
            Region = new Region(Theme.Rounded(ClientRectangle, 18));
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnClick(EventArgs e) { OpenRequested?.Invoke(this, e); base.OnClick(e); }
    protected override void OnDoubleClick(EventArgs e) { EditRequested?.Invoke(this, e); base.OnDoubleClick(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_data is null) return;
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        g.Clear(Color.White);

        var accent = Theme.AccentFor(_data.Title);
        var cover = new Rectangle(0, 0, Width, 82);
        using (var brush = new LinearGradientBrush(cover, accent, Theme.Mix(accent, Theme.Primary, .45f), 20f))
            g.FillRectangle(brush, cover);
        using (var glow = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
        {
            g.FillEllipse(glow, Width - 100, -50, 150, 150);
            g.FillEllipse(glow, Width - 30, 36, 64, 64);
        }

        var kindText = _data.Kind == CourseKind.Online ? "ONLINE" : "OFFLINE";
        var badgeW = Theme.Measure(g, kindText, Theme.TinyBold) + 20;
        Theme.Pill(g, kindText, Theme.TinyBold, accent, Color.FromArgb(235, 255, 255, 255),
            new Rectangle(16, 16, badgeW, 22), 11);

        var glyph = _data.Kind == CourseKind.Online ? "\uE774" : "\uE716";
        Theme.Text(g, glyph, Theme.IconLarge, Color.FromArgb(190, 255, 255, 255),
            new Rectangle(Width - 54, 12, 38, 38), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        if (!_data.AcceptsNewEnrollments)
        {
            var closed = "ENROLLMENT CLOSED";
            var w = Theme.Measure(g, closed, Theme.TinyBold) + 16;
            Theme.Pill(g, closed, Theme.TinyBold, Color.White, Color.FromArgb(90, 20, 43, 74),
                new Rectangle(Width - w - 16, 16, w, 22), 11);
        }

        Theme.Text(g, _data.Title, Theme.CardTitle, Theme.Ink, new Rectangle(18, 93, Width - 36, 34),
            TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
        Theme.Text(g, "with " + CatalogService.InstructorName(_data.InstructorId), Theme.Small, Theme.Muted,
            new Rectangle(18, 132, Width - 36, 18));

        var meta = _data.Kind == CourseKind.Online
            ? $"{_data.TotalSections} sections  •  {_data.TotalLessons} lessons"
            : NextBatchText(_data);
        Theme.Text(g, meta, Theme.Small, Theme.SoftText, new Rectangle(18, 154, Width - 36, 18));

        using (var sep = new Pen(Theme.Mix(Theme.Border, Color.White, .25f)))
            g.DrawLine(sep, 18, 180, Width - 18, 180);

        Theme.Text(g, Fmt.Money(_data.Price), Theme.Price, Theme.Ink,
            new Rectangle(18, 191, Width - 110, 26));
        Theme.Text(g, "View course  \uE76C", Theme.SmallBold, Theme.Accent,
            new Rectangle(Width - 120, 193, 102, 22),
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

        if (_hover)
            Theme.DrawBorder(g, Theme.Mix(accent, Color.White, .35f),
                new RectangleF(.5f, .5f, Width - 1, Height - 1), 18, 1.5f);
        else
            Theme.DrawBorder(g, Theme.Border, new RectangleF(.5f, .5f, Width - 1, Height - 1), 18);
    }

    private static string NextBatchText(Course course)
    {
        var batch = CatalogService.BatchesForCourse(course.Id)
            .FirstOrDefault(b => b.Status != BatchStatus.Cancelled && b.StartDate >= DateTime.Today);
        if (batch is null) return "No upcoming batch";
        return $"Starts {Fmt.ShortDate(batch.StartDate)}  •  {batch.SeatsLeft} seats left";
    }
}
