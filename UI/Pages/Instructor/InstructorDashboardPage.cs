namespace ECMS;

/// <summary>Instructor home page with courses, requests and student feedback.</summary>
public partial class InstructorDashboardPage : BasePage
{
    public InstructorDashboardPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var instructor = Session.Instructor!;
        var courses = CatalogService.CoursesForInstructor(instructor.Id).ToList();
        var students = CatalogService.StudentCountForInstructor(instructor.Id);
        var requests = InstructorService.RequestsForInstructor(instructor.Id).ToList();
        var reviews = ReviewService.ReviewsForInstructor(instructor.Id).ToList();

        AddBlock(Header("Hello, " + FirstName(instructor.FullName),
            "Here is what is happening across your courses.",
            "My courses", "\uE7BE", () => Navigate("courses")), 60);

        StatRow(
            Stat("My courses", courses.Count.ToString("N0"), "Courses you teach", "\uE7BE", Theme.Primary),
            Stat("Students", students.ToString("N0"), "Across all courses", "\uE716", Theme.Accent),
            Stat("Pending requests", requests.Count(r => r.Status == DeleteRequestStatus.Pending).ToString("N0"),
                "Waiting for the admin", "\uE74D", Theme.Amber),
            Stat("Reviews", reviews.Count.ToString("N0"), "Student comments", "\uE8BD", Theme.Secondary));

        var courseTable = MakeTable(
            new[] { "Course", "Format", "Students", "Lessons", "Status" },
            new[] { 3f, 1.1f, 1f, 0.9f, 1.3f },
            pillColumns: new[] { 1, 4 });
        courseTable.SetRows(courses.Select(c => new[]
        {
            c.Title,
            c.Kind.ToString(),
            CatalogService.StudentCount(c.Id).ToString(),
            c.TotalLessons.ToString(),
            CourseStatus(c)
        }));
        courseTable.RowClicked += (_, index) => Navigator.Go("content", courses[index].Id);
        AddTableCard("My courses", "Click a course to manage its sections and lessons", courseTable,
            emptyText: "No courses are assigned to you yet.");

        var pending = requests
            .Where(r => r.Status == DeleteRequestStatus.Pending)
            .Take(6)
            .ToList();
        var requestTable = MakeTable(
            new[] { "Target", "Requested", "Status" },
            new[] { 4f, 1.3f, 1.2f },
            pillColumns: new[] { 2 });
        requestTable.SetRows(pending.Select(r => new[]
        {
            r.TargetName,
            Fmt.Date(r.CreatedOn),
            r.Status.ToString()
        }));
        requestTable.RowClicked += (_, index) => ShowRequest(pending[index]);
        AddTableCard("Pending delete requests", "The admin reviews every request before content is removed",
            requestTable, emptyText: "You have no pending delete requests.");

        var latest = reviews.Take(6).ToList();
        var reviewTable = MakeTable(
            new[] { "Student", "Course", "Comment", "Date" },
            new[] { 1.8f, 2.4f, 4f, 1.2f },
            avatar: true);
        reviewTable.SetRows(latest.Select(r => new[]
        {
            CatalogService.StudentName(r.StudentId),
            CatalogService.Course(r.CourseId)?.Title ?? "Course",
            r.Text,
            Fmt.Date(r.CreatedOn)
        }));
        reviewTable.RowClicked += (_, index) => ShowReview(latest[index]);
        AddTableCard("Latest reviews", "Feedback from your students", reviewTable,
            emptyText: "No reviews have been written for your courses yet.");
    }

    private static string CourseStatus(Course course)
    {
        if (!course.IsPublished) return "Draft";
        return course.AcceptsNewEnrollments ? "Published" : "Closed";
    }

    private void ShowRequest(DeleteRequest request)
    {
        DetailsDialog.Show(this, request.TargetName, "Delete request",
            new[]
            {
                ("Target", request.TargetName),
                ("Requested", Fmt.Date(request.CreatedOn)),
                ("Reviewed", request.ReviewedOn is null ? "Not reviewed yet" : Fmt.Date(request.ReviewedOn.Value)),
                ("Status", request.Status.ToString()),
                ("Reason", request.Reason)
            });
    }

    private void ShowReview(Review review)
    {
        DetailsDialog.Show(this, "Student review", "Feedback for your course",
            new[]
            {
                ("Student", CatalogService.StudentName(review.StudentId)),
                ("Course", CatalogService.Course(review.CourseId)?.Title ?? "Course"),
                ("Date", Fmt.Date(review.CreatedOn)),
                ("Comment", review.Text)
            });
    }

    private static string FirstName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? name : parts[0];
    }
}
