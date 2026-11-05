namespace ECMS;

/// <summary>Everything the signed-in student owns and their progress.</summary>
public partial class MyLearningPage : BasePage
{
    public MyLearningPage() : base(key => Navigator.Go(key))
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

        var completedIds = AppData.Completions
            .Where(c => c.StudentId == student.Id)
            .Select(c => c.LessonId)
            .ToHashSet();
        var minutes = AppData.Courses
            .SelectMany(c => c.Sections)
            .SelectMany(s => s.Lessons)
            .Where(l => completedIds.Contains(l.Id))
            .Sum(l => l.Minutes);

        AddBlock(Header("My learning", "Your courses, progress and offline batches.",
            "Browse more", "\uE7BE", () => Navigate("browse")), 60);

        StatRow(
            Stat("My courses", courses.Count.ToString("N0"), "Active enrollments", "\uE7BE", Theme.Primary),
            Stat("Lessons completed", completedIds.Count.ToString("N0"), "Across all courses", "\uE73E", Theme.Accent),
            Stat("Learning time", (minutes / 60) + "h " + (minutes % 60) + "m", "Estimated video time", "\uE823", Theme.Secondary),
            Stat("Certificates", AppData.Certificates.Count(c => c.StudentId == student.Id).ToString("N0"),
                "Passed final exams", "\uE8D7", Theme.Amber));

        var table = MakeTable(
            new[] { "Course", "Format", "Progress", "Next lesson", "Enrolled", "Schedule" },
            new[] { 2.6f, 0.9f, 1.1f, 2.4f, 1.1f, 2f },
            pillColumns: new[] { 1 });
        table.SetRows(courses.Select(c =>
        {
            var enrollment = enrollments.First(e => e.CourseId == c.Id);
            var next = CatalogService.NextLesson(student.Id, c.Id);
            var batch = enrollment.BatchId is { } id ? CatalogService.Batch(id) : null;
            return new[]
            {
                c.Title,
                c.Kind.ToString(),
                Fmt.Percent(CatalogService.ProgressPercent(student.Id, c.Id)),
                next?.Title ?? "All lessons complete",
                Fmt.Date(enrollment.EnrolledOn),
                batch?.Schedule ?? "Self-paced"
            };
        }));
        table.RowClicked += (_, index) => Navigator.Go("player", courses[index].Id);
        AddTableCard("My courses", "Click a course to continue learning", table,
            emptyText: "You do not own any courses yet. Browse the catalog to get started.");

        var batchEnrollments = enrollments
            .Where(e => e.BatchId is not null)
            .Select(e => CatalogService.Batch(e.BatchId!.Value))
            .Where(b => b is not null)
            .Select(b => b!)
            .DistinctBy(b => b.Id)
            .ToList();

        if (batchEnrollments.Count > 0)
        {
            var batches = MakeTable(
                new[] { "Batch", "Course", "Starts", "Ends", "Schedule", "Status" },
                new[] { 2.2f, 2.2f, 1.1f, 1.1f, 2.2f, 1.2f },
                pillColumns: new[] { 5 });
            batches.SetRows(batchEnrollments.Select(b => new[]
            {
                b.Title,
                CatalogService.Course(b.CourseId)?.Title ?? "Course",
                Fmt.Date(b.StartDate),
                Fmt.Date(b.EndDate),
                b.Schedule,
                b.StatusText
            }));
            AddTableCard("My offline batches", "Your scheduled group sessions", batches);
        }
    }
}
