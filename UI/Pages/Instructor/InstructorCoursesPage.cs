namespace ECMS;

/// <summary>The courses an instructor teaches and their removal requests.</summary>
public partial class InstructorCoursesPage : BasePage
{
    private readonly TableControl _table;
    private readonly Card _card;
    private readonly SearchBox _search = new("Search your courses...");
    private List<Course> _shown = new();

    public InstructorCoursesPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var instructor = Session.Instructor!;
        var courses = CatalogService.CoursesForInstructor(instructor.Id).ToList();

        AddBlock(Header("My courses", "Maintain the courses you teach and request removals.",
            "My profile", "\uE77B", () => Navigate("profile")), 60);

        StatRow(
            Stat("My courses", courses.Count.ToString("N0"), "Assigned to you", "\uE7BE", Theme.Primary),
            Stat("Students", CatalogService.StudentCountForInstructor(instructor.Id).ToString("N0"),
                "Across all courses", "\uE716", Theme.Accent),
            Stat("Lessons", courses.Sum(c => c.TotalLessons).ToString("N0"),
                "Published content", "\uE82D", Theme.Secondary),
            Stat("Reviews", ReviewService.ReviewsForInstructor(instructor.Id).Count().ToString("N0"),
                "Student feedback", "\uE8BD", Theme.Amber));

        var searchPanel = new Panel { BackColor = Theme.Bg };
        _search.Location = new Point(0, 0);
        _search.Size = new Size(360, 42);
        _search.ValueChanged += (_, _) => Reload();
        searchPanel.Controls.Add(_search);
        AddBlock(searchPanel, 44);

        _table = MakeTable(
            new[] { "Course", "Format", "Price", "Students", "Lessons", "Status" },
            new[] { 3f, 1.1f, 1.2f, 1f, 0.9f, 1.3f },
            actions: true, pillColumns: new[] { 1, 5 });
        _table.ActionClicked += (_, e) => HandleAction(e.RowIndex, e.Action);
        _table.RowClicked += (_, index) => Navigator.Go("content", _shown[index].Id);
        _card = AddTableCard("My courses", "Edit updates the course  â€¢  Delete asks the admin to remove it", _table,
            emptyText: "No courses match your search.");
        Reload();
    }

    private void Reload()
    {
        var instructor = Session.Instructor!;
        var query = _search.Value.Trim();
        _shown = CatalogService.CoursesForInstructor(instructor.Id)
            .Where(c => query.Length == 0 ||
                        c.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        c.Kind.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Title)
            .ToList();

        _table.SetRows(_shown.Select(c => new[]
        {
            c.Title,
            c.Kind.ToString(),
            Fmt.Money(c.Price),
            CatalogService.StudentCount(c.Id).ToString(),
            c.TotalLessons.ToString(),
            Status(c)
        }));
        FitTableCard(_card, _table);
        RefreshLayout();
    }

    private static string Status(Course course)
    {
        if (!course.IsPublished) return "Draft";
        return course.AcceptsNewEnrollments ? "Published" : "Closed";
    }

    private void HandleAction(int index, TableRowAction action)
    {
        if (index < 0 || index >= _shown.Count) return;
        var course = _shown[index];

        if (action == TableRowAction.Edit)
        {
            EditCourse(course);
            return;
        }

        RequestDelete(course);
    }

    private void EditCourse(Course course)
    {
        var fields = new[]
        {
            FormField.Text("title", "Course title", course.Title),
            FormField.Multiline("description", "Description", course.Description),
            FormField.Number("price", "Price (EGP)", course.Price.ToString("0"), 0, 1000000)
        };
        using var dialog = new FormDialog("Edit course", "Update the details students see.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        course.Title = dialog.Values["title"];
        course.Description = dialog.Values["description"];
        course.Price = decimal.Parse(dialog.Values["price"]);
        Navigator.Toast("Course updated.");
        Navigator.Go("courses");
    }

    private void RequestDelete(Course course)
    {
        var instructor = Session.Instructor!;
        var alreadyPending = AppData.DeleteRequests.Any(r =>
            r.InstructorId == instructor.Id && r.CourseId == course.Id &&
            r.SectionId is null && r.LessonId is null &&
            r.Status == DeleteRequestStatus.Pending);
        if (alreadyPending)
        {
            Navigator.Toast("A removal request for this course is already waiting for the admin.", false);
            return;
        }

        using var dialog = new FormDialog("Request course removal",
            "The admin must approve this request before the course is removed.", new[]
            {
                FormField.Multiline("reason", "Reason", "", "Why should this course be removed?")
            });
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        InstructorService.SubmitDeleteRequest(instructor.Id, course.Id, null, null,
            "Course: " + course.Title, dialog.Values["reason"]);
        Navigator.Toast("Removal request sent to the admin.");
        Navigator.Go("courses");
    }
}
