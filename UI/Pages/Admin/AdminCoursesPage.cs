namespace ECMS;

/// <summary>Admin management of the course catalog.</summary>
public partial class AdminCoursesPage : BasePage
{
    private readonly TableControl _table;
    private readonly Card _card;
    private readonly SearchBox _search = new("Search courses or instructors...");
    private List<Course> _shown = new();

    public AdminCoursesPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        AddBlock(Header("Courses", "Online courses and offline course templates.",
            "Add course", "\uE710", AddCourse), 60);

        var courses = AppData.Courses.Where(c => !c.IsDeleted).ToList();
        StatRow(
            Stat("Total courses", courses.Count.ToString("N0"), "In the catalog", "\uE7BE", Theme.Primary),
            Stat("Online", courses.Count(c => c.Kind == CourseKind.Online).ToString("N0"),
                "Self-paced learning", "\uE774", Theme.Accent),
            Stat("Offline", courses.Count(c => c.Kind == CourseKind.Offline).ToString("N0"),
                "Scheduled batches", "\uE716", Theme.Secondary),
            Stat("Enrollments", AppData.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed).ToString("N0"),
                "Confirmed seats", "\uE73E", Theme.Amber));

        var searchPanel = new Panel { BackColor = Theme.Bg };
        _search.Location = new Point(0, 0);
        _search.Size = new Size(360, 42);
        _search.ValueChanged += (_, _) => Reload();
        searchPanel.Controls.Add(_search);
        AddBlock(searchPanel, 44);

        _table = MakeTable(
            new[] { "Course", "Instructor", "Format", "Price", "Students", "Status" },
            new[] { 3f, 2f, 1.2f, 1.2f, 1f, 1.4f },
            actions: true, pillColumns: new[] { 2, 5 });
        _table.ActionClicked += (_, e) => HandleAction(e.RowIndex, e.Action);
        _table.RowClicked += (_, index) => ShowDetails(_shown[index]);
        _card = AddTableCard("All courses", "Click a row to inspect the course", _table,
            emptyText: "No courses match your search.");
        Reload();
    }

    private void Reload()
    {
        var query = _search.Value.Trim();
        _shown = AppData.Courses
            .Where(c => !c.IsDeleted)
            .Where(c => query.Length == 0 ||
                        c.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        CatalogService.InstructorName(c.InstructorId).Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Title)
            .ToList();

        _table.SetRows(_shown.Select(c => new[]
        {
            c.Title,
            CatalogService.InstructorName(c.InstructorId),
            c.Kind.ToString(),
            Fmt.Money(c.Price),
            CatalogService.StudentCount(c.Id).ToString(),
            c.IsPublished ? (c.AcceptsNewEnrollments ? "Published" : "Closed") : "Draft"
        }));
        FitTableCard(_card, _table);
        RefreshLayout();
    }

    private void AddCourse()
    {
        var instructors = AppData.Instructors.Where(i => i.Status == AccountStatus.Active).ToList();
        if (instructors.Count == 0)
        {
            Navigator.Toast("Activate an instructor before creating a course.", false);
            return;
        }

        var fields = new[]
        {
            FormField.Text("title", "Course title", "", "e.g. Introduction to Cloud"),
            FormField.Multiline("description", "Description", "", "What will students learn?"),
            FormField.Choice("format", "Format", "Online", "Online", "Offline"),
            FormField.Choice("instructor", "Instructor", instructors[0].FullName,
                instructors.Select(i => i.FullName).ToArray()),
            FormField.Number("price", "Price (EGP)", "2000", 0, 1000000),
            FormField.Choice("published", "Status", "Published", "Published", "Draft")
        };
        using var dialog = new FormDialog("Add course", "Create a new course in the catalog.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var instructor = instructors.First(i => i.FullName == dialog.Values["instructor"]);
        AppData.Courses.Add(new Course
        {
            Id = AppData.NextId(AppData.Courses, c => c.Id),
            Title = dialog.Values["title"],
            Description = dialog.Values["description"],
            Kind = dialog.Values["format"] == "Online" ? CourseKind.Online : CourseKind.Offline,
            InstructorId = instructor.Id,
            Price = decimal.Parse(dialog.Values["price"]),
            IsPublished = dialog.Values["published"] == "Published",
            AcceptsNewEnrollments = true,
            CreatedOn = DateTime.Today
        });
        Navigator.Toast("Course created.");
        Reload();
    }

    private void EditCourse(Course course)
    {
        var instructors = AppData.Instructors.Where(i => i.Status == AccountStatus.Active).ToList();
        var names = instructors.Select(i => i.FullName).ToList();
        if (!names.Contains(CatalogService.InstructorName(course.InstructorId)))
            names.Insert(0, CatalogService.InstructorName(course.InstructorId));

        var fields = new[]
        {
            FormField.Text("title", "Course title", course.Title),
            FormField.Multiline("description", "Description", course.Description),
            FormField.Choice("format", "Format", course.Kind.ToString(), "Online", "Offline"),
            FormField.Choice("instructor", "Instructor", CatalogService.InstructorName(course.InstructorId),
                names.ToArray()),
            FormField.Number("price", "Price (EGP)", course.Price.ToString("0"), 0, 1000000),
            FormField.Choice("published", "Status", course.IsPublished ? "Published" : "Draft",
                "Published", "Draft"),
            FormField.Choice("accepts", "New enrollments", course.AcceptsNewEnrollments ? "Open" : "Closed",
                "Open", "Closed")
        };
        using var dialog = new FormDialog("Edit course", "Update the course details and availability.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        course.Title = dialog.Values["title"];
        course.Description = dialog.Values["description"];
        course.Kind = dialog.Values["format"] == "Online" ? CourseKind.Online : CourseKind.Offline;
        var instructor = instructors.FirstOrDefault(i => i.FullName == dialog.Values["instructor"]);
        if (instructor is not null) course.InstructorId = instructor.Id;
        course.Price = decimal.Parse(dialog.Values["price"]);
        course.IsPublished = dialog.Values["published"] == "Published";
        course.AcceptsNewEnrollments = dialog.Values["accepts"] == "Open";
        Navigator.Toast("Course updated.");
        Reload();
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

        var students = CatalogService.StudentCount(course.Id);
        var message = "Archive " + course.Title + "? " +
                      (students > 0
                          ? $"Enrolled students ({students}) keep access to the content, but the course leaves the catalog."
                          : "The course will leave the catalog.");
        if (!ConfirmDialog.Ask(this, "Archive course", message, "Archive")) return;

        course.IsDeleted = true;
        course.IsPublished = false;
        course.AcceptsNewEnrollments = false;
        Navigator.Toast("Course archived.");
        Reload();
    }

    private void ShowDetails(Course course)
    {
        var batches = CatalogService.BatchesForCourse(course.Id).ToList();
        DetailsDialog.Show(this, course.Title, "Course details",
            new[]
            {
                ("Instructor", CatalogService.InstructorName(course.InstructorId)),
                ("Format", course.Kind.ToString()),
                ("Price", Fmt.Money(course.Price)),
                ("Sections", course.TotalSections.ToString()),
                ("Lessons", course.TotalLessons.ToString()),
                ("Students", CatalogService.StudentCount(course.Id).ToString()),
                ("Batches", batches.Count.ToString()),
                ("Status", course.IsPublished ? "Published" : "Draft")
            },
            course.Description);
    }
}
