namespace ECMS;

/// <summary>Student home page with progress, credit and recommendations.</summary>
public partial class StudentDashboardPage : BasePage
{
    public StudentDashboardPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var student = Session.Student!;
        var enrollments = CatalogService.EnrollmentsForStudent(student.Id).ToList();
        var courses = enrollments
            .Select(e => CatalogService.Course(e.CourseId))
            .Where(c => c is not null)
            .Select(c => c!)
            .DistinctBy(c => c.Id)
            .ToList();

        AddBlock(Header("Hello, " + FirstName(student.DisplayName),
            "Pick up where you left off, or find your next course.",
            "Browse courses", "\uE7BE", () => Navigate("browse")), 60);

        StatRow(
            Stat("My courses", courses.Count.ToString("N0"), "Active enrollments", "\uE7BE", Theme.Primary),
            Stat("Lessons completed", AppData.Completions.Count(c => c.StudentId == student.Id).ToString("N0"),
                "Across all courses", "\uE73E", Theme.Accent),
            Stat("Certificates", AppData.Certificates.Count(c => c.StudentId == student.Id).ToString("N0"),
                "Passed final exams", "\uE8D7", Theme.Secondary),
            Stat("Credit balance", Fmt.Money(RefundService.CreditBalance(student.Id)),
                "Available to spend", "\uE8C7", Theme.Amber));

        var table = MakeTable(
            new[] { "Course", "Format", "Progress", "Next lesson", "Enrolled" },
            new[] { 2.8f, 1f, 1.2f, 2.6f, 1.2f },
            pillColumns: new[] { 1 });
        table.SetRows(courses.Select(c =>
        {
            var enrollment = enrollments.First(e => e.CourseId == c.Id);
            var next = CatalogService.NextLesson(student.Id, c.Id);
            return new[]
            {
                c.Title,
                c.Kind.ToString(),
                Fmt.Percent(CatalogService.ProgressPercent(student.Id, c.Id)),
                next?.Title ?? "All lessons complete",
                Fmt.Date(enrollment.EnrolledOn)
            };
        }));
        table.RowClicked += (_, index) => Navigator.Go("player", courses[index].Id);
        AddTableCard("Continue learning", "Click a course to open the lesson player", table,
            emptyText: "You have not joined a course yet. Browse the catalog to begin.");

        var recommended = CatalogService.CatalogCourses
            .Where(c => !CatalogService.IsEnrolled(student.Id, c.Id) && c.AcceptsNewEnrollments)
            .OrderByDescending(c => CatalogService.StudentCount(c.Id))
            .Take(3)
            .ToList();

        if (recommended.Count > 0)
        {
            AddBlock(Header("Recommended for you", "Popular courses you do not own yet."), 54);
            var cards = recommended.Select(c =>
            {
                var card = new CourseCard(c);
                card.OpenRequested += (_, _) => Navigator.Go("course", c.Id);
                return card;
            }).ToArray();
            var grid = Grid(3, 1, cards, 232);
            AddBlock(grid, 232);
        }
    }

    private static string FirstName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? name : parts[0];
    }
}
