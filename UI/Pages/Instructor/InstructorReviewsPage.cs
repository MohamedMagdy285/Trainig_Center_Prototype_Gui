namespace ECMS;

/// <summary>Student feedback for the courses an instructor teaches.</summary>
public partial class InstructorReviewsPage : BasePage
{
    private readonly TableControl _table;
    private readonly Card _card;
    private readonly SearchBox _search = new("Search reviews...");
    private List<Review> _shown = new();

    public InstructorReviewsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var reviews = ReviewService.ReviewsForInstructor(Session.Instructor!.Id).ToList();

        AddBlock(Header("Reviews", "Feedback from students in your courses."), 60);

        StatRow(
            Stat("Reviews", reviews.Count.ToString("N0"), "All student comments", "\uE8BD", Theme.Primary),
            Stat("Courses reviewed", reviews.Select(r => r.CourseId).Distinct().Count().ToString("N0"),
                "Different courses", "\uE7BE", Theme.Secondary),
            Stat("Students", reviews.Select(r => r.StudentId).Distinct().Count().ToString("N0"),
                "Left feedback", "\uE716", Theme.Accent),
            Stat("This month", reviews.Count(r =>
                    r.CreatedOn.Year == DateTime.Today.Year &&
                    r.CreatedOn.Month == DateTime.Today.Month).ToString("N0"),
                "Recent comments", "\uE823", Theme.Amber));

        var searchPanel = new Panel { BackColor = Theme.Bg };
        _search.Location = new Point(0, 0);
        _search.Size = new Size(360, 42);
        _search.ValueChanged += (_, _) => Reload();
        searchPanel.Controls.Add(_search);
        AddBlock(searchPanel, 44);

        _table = MakeTable(
            new[] { "Student", "Course", "Context", "Comment", "Date" },
            new[] { 1.7f, 2.2f, 1.9f, 4f, 1.1f },
            avatar: true);
        _table.RowClicked += (_, index) => ShowDetails(_shown[index]);
        _card = AddTableCard("Student reviews", "Click a review to read it in full", _table,
            emptyText: "No reviews match your search.");
        Reload();
    }

    private void Reload()
    {
        var query = _search.Value.Trim();
        _shown = ReviewService.ReviewsForInstructor(Session.Instructor!.Id)
            .Where(r => query.Length == 0 ||
                        CatalogService.StudentName(r.StudentId).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (CatalogService.Course(r.CourseId)?.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        r.Text.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _table.SetRows(_shown.Select(r => new[]
        {
            CatalogService.StudentName(r.StudentId),
            CatalogService.Course(r.CourseId)?.Title ?? "Course",
            Context(r),
            r.Text,
            Fmt.Date(r.CreatedOn)
        }));
        FitTableCard(_card, _table);
        RefreshLayout();
    }

    private static string Context(Review review)
    {
        if (review.SectionId is { } sectionId)
        {
            var section = CatalogService.Sections(review.CourseId).FirstOrDefault(s => s.Id == sectionId);
            return "Section: " + (section?.Title ?? "Section");
        }
        if (review.BatchId is { } batchId)
            return "Batch: " + (CatalogService.Batch(batchId)?.Title ?? "Batch");
        return "Course";
    }

    private void ShowDetails(Review review)
    {
        DetailsDialog.Show(this, "Student review", "Feedback for your course",
            new[]
            {
                ("Student", CatalogService.StudentName(review.StudentId)),
                ("Course", CatalogService.Course(review.CourseId)?.Title ?? "Course"),
                ("Context", Context(review)),
                ("Created", Fmt.Date(review.CreatedOn)),
                ("Edited", review.UpdatedOn is null ? "Never edited" : Fmt.Date(review.UpdatedOn.Value)),
                ("Comment", review.Text)
            });
    }
}
