namespace ECMS;

/// <summary>The public course catalog for students.</summary>
public partial class BrowseCoursesPage : BasePage
{
    private readonly Panel _gridHost = new() { BackColor = Theme.Bg };
    private readonly SearchBox _search = new("Search courses or instructors...");
    private readonly Chip _all = new() { Text = "All", Size = new Size(74, 34), Selected = true };
    private readonly Chip _online = new() { Text = "Online", Size = new Size(90, 34) };
    private readonly Chip _offline = new() { Text = "Offline", Size = new Size(90, 34) };
    private string _kind = "All";

    public BrowseCoursesPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        AddBlock(Header("Browse courses", "Find the right course for your next step."), 60);

        StatRow(
            Stat("Courses", CatalogService.CatalogCourses.Count().ToString("N0"), "Available now", "\uE7BE", Theme.Primary),
            Stat("Online", CatalogService.OnlineCourses.Count().ToString("N0"), "Learn at your pace", "\uE774", Theme.Accent),
            Stat("Offline", CatalogService.OfflineCourses.Count().ToString("N0"), "Join a live batch", "\uE716", Theme.Secondary),
            Stat("Starting from", Fmt.Money(CatalogService.CatalogCourses.Any()
                    ? CatalogService.CatalogCourses.Min(c => c.Price) : 0),
                "Lowest course price", "\uE8C7", Theme.Amber));

        var filter = new Panel { BackColor = Theme.Bg };
        _search.Location = new Point(0, 0);
        _search.Size = new Size(340, 42);
        _search.ValueChanged += (_, _) => RebuildGrid();
        filter.Controls.Add(_search);

        _all.Location = new Point(356, 4);
        _online.Location = new Point(438, 4);
        _offline.Location = new Point(536, 4);
        _all.Click += (_, _) => SetKind("All");
        _online.Click += (_, _) => SetKind("Online");
        _offline.Click += (_, _) => SetKind("Offline");
        filter.Controls.Add(_all);
        filter.Controls.Add(_online);
        filter.Controls.Add(_offline);
        AddBlock(filter, 44);

        AddBlock(_gridHost, 240);
        RebuildGrid();
    }

    private void SetKind(string kind)
    {
        _kind = kind;
        _all.Selected = kind == "All";
        _online.Selected = kind == "Online";
        _offline.Selected = kind == "Offline";
        _all.Invalidate();
        _online.Invalidate();
        _offline.Invalidate();
        RebuildGrid();
    }

    private void RebuildGrid()
    {
        var query = _search.Value.Trim();
        var courses = CatalogService.CatalogCourses
            .Where(c => _kind == "All" || c.Kind.ToString() == _kind)
            .Where(c => query.Length == 0 ||
                        c.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        c.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        CatalogService.InstructorName(c.InstructorId).Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Title)
            .ToList();

        _gridHost.SuspendLayout();
        while (_gridHost.Controls.Count > 0) _gridHost.Controls[0].Dispose();

        if (courses.Count == 0)
        {
            var empty = new Label
            {
                Text = "No courses match your search.", Font = Theme.Body, ForeColor = Theme.Muted,
                BackColor = Theme.Bg, AutoSize = true, Location = new Point(4, 12)
            };
            _gridHost.Controls.Add(empty);
            _gridHost.Height = 60;
            _gridHost.ResumeLayout();
            RefreshLayout();
            return;
        }

        var cards = courses.Select(c =>
        {
            var card = new CourseCard(c);
            card.OpenRequested += (_, _) => Navigator.Go("course", c.Id);
            return card;
        }).ToArray();

        var rows = (int)Math.Ceiling(cards.Length / 3.0);
        var height = rows * 232 + (rows - 1) * 14;
        var grid = Grid(3, rows, cards, height);
        grid.Dock = DockStyle.Top;
        _gridHost.Controls.Add(grid);
        _gridHost.Height = height;
        _gridHost.ResumeLayout();
        RefreshLayout();
    }
}
