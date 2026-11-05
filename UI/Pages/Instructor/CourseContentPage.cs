namespace ECMS;

/// <summary>Manage one course's sections and lessons as its instructor.</summary>
public partial class CourseContentPage : BasePage
{
    private readonly int _courseId;
    private readonly Course _course;
    private readonly List<Section> _sections;

    /// <summary>Parameterless constructor for the Visual Studio designer.</summary>
    public CourseContentPage() : base(_ => { })
    {
        InitializeComponent();
        _course = new Course();
        _sections = new List<Section>();
    }

    public CourseContentPage(int courseId) : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) { _course = new Course(); _sections = new List<Section>(); return; }
        _courseId = courseId;
        var course = CatalogService.Course(courseId);
        if (course is null || course.InstructorId != Session.Instructor!.Id)
        {
            AddBlock(Header("Course not found", "This course does not belong to you."), 60);
            _course = new Course();
            _sections = new List<Section>();
            return;
        }

        _course = course;
        _sections = CatalogService.Sections(courseId).ToList();
        var lessons = CatalogService.AllLessons(courseId).ToList();

        AddBlock(Header(course.Title,
            "Manage sections and lessons. Content is removed only after admin approval.",
            "Edit course", "\uE70F", EditCourse), 60);

        StatRow(
            Stat("Sections", _sections.Count.ToString("N0"), "In this course", "\uE8A9", Theme.Primary),
            Stat("Lessons", lessons.Count.ToString("N0"), "Published content", "\uE82D", Theme.Accent),
            Stat("Students", CatalogService.StudentCount(course.Id).ToString("N0"),
                "Enrolled learners", "\uE716", Theme.Secondary),
            Stat("Reviews", ReviewService.VisibleForCourse(course.Id).Count().ToString("N0"),
                "Student feedback", "\uE8BD", Theme.Amber));

        var sectionTable = MakeTable(
            new[] { "Section", "Type", "Lessons", "Reviews" },
            new[] { 3.4f, 1.1f, 0.9f, 1f },
            actions: true, pillColumns: new[] { 1 });
        sectionTable.SetRows(_sections.Select(s => new[]
        {
            s.Title,
            s.KindLabel,
            s.Lessons.Count(l => !l.IsDeleted).ToString(),
            ReviewService.VisibleForSection(s.Id).Count().ToString()
        }));
        sectionTable.ActionClicked += (_, e) => HandleSectionAction(e.RowIndex, e.Action);
        sectionTable.RowClicked += (_, index) => ShowSection(_sections[index]);
        AddTableCard("Sections", "Edit renames a section  •  Delete asks the admin to remove it",
            sectionTable, "Add section", AddSection,
            emptyText: "This course has no sections yet. Add the first one.");

        var lessonTable = MakeTable(
            new[] { "Lesson", "Section", "Minutes", "Notes" },
            new[] { 3f, 2.4f, 0.9f, 2.4f },
            actions: true);
        lessonTable.SetRows(lessons.Select(l => new[]
        {
            l.Title,
            CatalogService.SectionOfLesson(l.Id)?.Title ?? "Section",
            l.Minutes + " min",
            l.Notes.Length == 0 ? "—" : l.Notes
        }));
        lessonTable.ActionClicked += (_, e) => HandleLessonAction(e.RowIndex, e.Action, lessons);
        lessonTable.RowClicked += (_, index) => ShowLesson(lessons[index]);
        AddTableCard("Lessons", "Edit updates a lesson  •  Delete asks the admin to remove it",
            lessonTable, "Add lesson", AddLesson,
            emptyText: "This course has no lessons yet. Add a section first, then a lesson.");
    }

    private void EditCourse()
    {
        var fields = new[]
        {
            FormField.Text("title", "Course title", _course.Title),
            FormField.Multiline("description", "Description", _course.Description),
            FormField.Number("price", "Price (EGP)", _course.Price.ToString("0"), 0, 1000000)
        };
        using var dialog = new FormDialog("Edit course", "Update the details students see.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        _course.Title = dialog.Values["title"];
        _course.Description = dialog.Values["description"];
        _course.Price = decimal.Parse(dialog.Values["price"]);
        Navigator.Toast("Course updated.");
        Navigator.Go("content", _courseId);
    }

    private void AddSection()
    {
        var fields = new[]
        {
            FormField.Text("title", "Section title", "", "e.g. Advanced topics"),
            FormField.Choice("kind", "Section type", "Section", "Section", "Preview", "Bonus"),
            FormField.Number("order", "Order", (_sections.Count + 1).ToString(), 1, 99)
        };
        using var dialog = new FormDialog("Add section", "Create a new section in this course.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var section = new Section
        {
            Id = AppData.NextId(AppData.Courses.SelectMany(c => c.Sections), s => s.Id),
            CourseId = _courseId,
            Title = dialog.Values["title"],
            Order = int.Parse(dialog.Values["order"]),
            IsPreview = dialog.Values["kind"] == "Preview",
            IsBonus = dialog.Values["kind"] == "Bonus"
        };
        _course.Sections.Add(section);
        Navigator.Toast("Section added.");
        Navigator.Go("content", _courseId);
    }

    private void EditSection(Section section)
    {
        var fields = new[]
        {
            FormField.Text("title", "Section title", section.Title),
            FormField.Choice("kind", "Section type", section.KindLabel,
                "Section", "Preview", "Bonus"),
            FormField.Number("order", "Order", section.Order.ToString(), 1, 99)
        };
        using var dialog = new FormDialog("Edit section", "Rename the section or change its type.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        section.Title = dialog.Values["title"];
        section.Order = int.Parse(dialog.Values["order"]);
        section.IsPreview = dialog.Values["kind"] == "Preview";
        section.IsBonus = dialog.Values["kind"] == "Bonus";
        Navigator.Toast("Section updated.");
        Navigator.Go("content", _courseId);
    }

    private void AddLesson()
    {
        if (_sections.Count == 0)
        {
            Navigator.Toast("Add a section before adding a lesson.", false);
            return;
        }

        var fields = new[]
        {
            FormField.Choice("section", "Section", _sections[0].Title,
                _sections.Select(s => s.Title).ToArray()),
            FormField.Text("title", "Lesson title", "", "e.g. Working with APIs"),
            FormField.Number("minutes", "Minutes", "12", 1, 600),
            FormField.Multiline("notes", "Notes", "", "Optional notes for students")
        };
        using var dialog = new FormDialog("Add lesson", "Add a new lesson to a section.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var section = _sections.First(s => s.Title == dialog.Values["section"]);
        section.Lessons.Add(new Lesson
        {
            Id = AppData.NextId(AppData.Courses.SelectMany(c => c.Sections).SelectMany(s => s.Lessons), l => l.Id),
            SectionId = section.Id,
            Title = dialog.Values["title"],
            Order = section.Lessons.Count + 1,
            Minutes = int.Parse(dialog.Values["minutes"]),
            Notes = dialog.Values["notes"]
        });
        Navigator.Toast("Lesson added.");
        Navigator.Go("content", _courseId);
    }

    private void EditLesson(Lesson lesson)
    {
        var fields = new[]
        {
            FormField.Text("title", "Lesson title", lesson.Title),
            FormField.Number("minutes", "Minutes", lesson.Minutes.ToString(), 1, 600),
            FormField.Multiline("notes", "Notes", lesson.Notes)
        };
        using var dialog = new FormDialog("Edit lesson", "Update the lesson details.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        lesson.Title = dialog.Values["title"];
        lesson.Minutes = int.Parse(dialog.Values["minutes"]);
        lesson.Notes = dialog.Values["notes"];
        Navigator.Toast("Lesson updated.");
        Navigator.Go("content", _courseId);
    }

    private void HandleSectionAction(int index, TableRowAction action)
    {
        if (index < 0 || index >= _sections.Count) return;
        var section = _sections[index];
        if (action == TableRowAction.Edit)
        {
            EditSection(section);
            return;
        }

        RequestDelete("Section: " + section.Title, _courseId, section.Id, null);
    }

    private void HandleLessonAction(int index, TableRowAction action, List<Lesson> lessons)
    {
        if (index < 0 || index >= lessons.Count) return;
        var lesson = lessons[index];
        if (action == TableRowAction.Edit)
        {
            EditLesson(lesson);
            return;
        }

        RequestDelete("Lesson: " + lesson.Title, _courseId, null, lesson.Id);
    }

    private void RequestDelete(string targetName, int? courseId, int? sectionId, int? lessonId)
    {
        var instructor = Session.Instructor!;
        var alreadyPending = AppData.DeleteRequests.Any(r =>
            r.InstructorId == instructor.Id && r.Status == DeleteRequestStatus.Pending &&
            r.TargetName == targetName);
        if (alreadyPending)
        {
            Navigator.Toast("A removal request for this content is already waiting for the admin.", false);
            return;
        }

        using var dialog = new FormDialog("Request removal",
            "The admin must approve this request before the content is removed.", new[]
            {
                FormField.Multiline("reason", "Reason", "", "Why should this content be removed?")
            });
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        InstructorService.SubmitDeleteRequest(instructor.Id, courseId, sectionId, lessonId,
            targetName, dialog.Values["reason"]);
        Navigator.Toast("Removal request sent to the admin.");
        Navigator.Go("content", _courseId);
    }

    private void ShowSection(Section section)
    {
        DetailsDialog.Show(this, section.Title, "Course section",
            new[]
            {
                ("Course", _course.Title),
                ("Type", section.KindLabel),
                ("Order", section.Order.ToString()),
                ("Lessons", section.Lessons.Count(l => !l.IsDeleted).ToString()),
                ("Reviews", ReviewService.VisibleForSection(section.Id).Count().ToString())
            });
    }

    private void ShowLesson(Lesson lesson)
    {
        DetailsDialog.Show(this, lesson.Title, "Course lesson",
            new[]
            {
                ("Course", _course.Title),
                ("Section", CatalogService.SectionOfLesson(lesson.Id)?.Title ?? "Section"),
                ("Minutes", lesson.Minutes + " min"),
                ("Notes", lesson.Notes.Length == 0 ? "No notes" : lesson.Notes)
            });
    }
}
