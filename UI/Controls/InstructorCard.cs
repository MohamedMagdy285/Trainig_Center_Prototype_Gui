namespace ECMS;

/// <summary>Instructor profile tile used on the instructors page.</summary>
public partial class InstructorCard : UserControl
{
    private bool _hover;
    private Instructor? _data;

    public event EventHandler? OpenRequested;
    public event EventHandler? EditRequested;

    public Instructor? Data
    {
        get => _data;
        set { _data = value; Invalidate(); }
    }

    public InstructorCard()
    {
        InitializeComponent();
    }

    public InstructorCard(Instructor data) : this()
    {
        Data = data;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width > 0 && Height > 0)
            Region = new Region(Theme.Rounded(ClientRectangle, 16));
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
        Theme.DrawBorder(g, _hover ? Theme.Mix(Theme.Accent, Color.White, .45f) : Theme.Border,
            new RectangleF(.5f, .5f, Width - 1, Height - 1), 16, _hover ? 1.5f : 1f);

        var accent = Theme.AccentFor(_data.FullName);
        Theme.Avatar(g, new Rectangle(18, 18, 48, 48), _data.FullName, accent);
        Theme.Text(g, _data.FullName, Theme.H3, Theme.Ink, new Rectangle(78, 20, Width - 160, 20));
        Theme.Text(g, _data.Specialization, Theme.Small, Theme.Muted, new Rectangle(78, 42, Width - 120, 18));

        var (fg, bg) = Theme.StatusColors(_data.StatusText);
        var statusW = Theme.Measure(g, _data.StatusText, Theme.TinyBold) + 20;
        Theme.Pill(g, _data.StatusText, Theme.TinyBold, fg, bg,
            new Rectangle(Width - statusW - 18, 20, statusW, 22), 11);

        using (var sep = new Pen(Theme.Mix(Theme.Border, Color.White, .25f)))
            g.DrawLine(sep, 18, 80, Width - 18, 80);

        var courses = CatalogService.CoursesForInstructor(_data.Id).Count();
        var students = CatalogService.StudentCountForInstructor(_data.Id);
        var col = (Width - 36) / 3;
        DrawStat(g, 18, 94, "COURSES", courses.ToString());
        DrawStat(g, 18 + col, 94, "STUDENTS", students.ToString());
        DrawStat(g, 18 + col * 2, 94, "FORMAT", _data.Preference.ToString());

        using (var sep = new Pen(Theme.Mix(Theme.Border, Color.White, .35f)))
        {
            g.DrawLine(sep, 18 + col - 8, 96, 18 + col - 8, 132);
            g.DrawLine(sep, 18 + col * 2 - 8, 96, 18 + col * 2 - 8, 132);
        }

        var prefW = Theme.Measure(g, _data.CvFileName, Theme.TinyBold) + 20;
        Theme.Pill(g, "CV attached", Theme.TinyBold, accent, Theme.Mix(accent, Color.White, .89f),
            new Rectangle(18, 148, prefW, 23), 11);
        Theme.Text(g, "View profile  \uE76C", Theme.SmallBold, Theme.Accent,
            new Rectangle(Width - 126, 148, 108, 23),
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }

    private static void DrawStat(Graphics g, int x, int y, string label, string value)
    {
        Theme.Text(g, value, Theme.H3, Theme.Ink, new Rectangle(x, y, 80, 22));
        Theme.Text(g, label, Theme.TinyBold, Theme.SoftText, new Rectangle(x, y + 20, 80, 15));
    }
}
