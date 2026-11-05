namespace ECMS;

/// <summary>Instructor requests for the admin to remove course content.</summary>
public partial class InstructorRequestsPage : BasePage
{
    private readonly TableControl _table;
    private readonly Card _card;
    private List<DeleteRequest> _shown = new();

    private sealed record RequestTarget(string Label, int? CourseId, int? SectionId, int? LessonId);

    public InstructorRequestsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var requests = InstructorService.RequestsForInstructor(Session.Instructor!.Id).ToList();

        AddBlock(Header("Delete requests",
            "Ask the admin to remove a course, section or lesson.",
            "New request", "\uE710", NewRequest), 60);

        StatRow(
            Stat("Pending", requests.Count(r => r.Status == DeleteRequestStatus.Pending).ToString("N0"),
                "Waiting for the admin", "\uE823", Theme.Amber),
            Stat("Approved", requests.Count(r => r.Status == DeleteRequestStatus.Approved).ToString("N0"),
                "Content removed", "\uE73E", Theme.Accent),
            Stat("Rejected", requests.Count(r => r.Status == DeleteRequestStatus.Rejected).ToString("N0"),
                "Content kept", "\uE711", Theme.Red),
            Stat("Total requests", requests.Count.ToString("N0"), "All time", "\uE74D", Theme.Primary));

        _table = MakeTable(
            new[] { "Target", "Reason", "Requested", "Reviewed", "Status" },
            new[] { 2.6f, 3.6f, 1.3f, 1.3f, 1.2f },
            pillColumns: new[] { 4 });
        _table.RowClicked += (_, index) => ShowDetails(_shown[index]);
        _card = AddTableCard("My requests", "Click a request to read the full reason", _table,
            emptyText: "You have not sent any delete requests yet.");
        Reload();
    }

    private void Reload()
    {
        _shown = InstructorService.RequestsForInstructor(Session.Instructor!.Id).ToList();
        _table.SetRows(_shown.Select(r => new[]
        {
            r.TargetName,
            r.Reason,
            Fmt.Date(r.CreatedOn),
            r.ReviewedOn is null ? "â€”" : Fmt.Date(r.ReviewedOn.Value),
            r.Status.ToString()
        }));
        FitTableCard(_card, _table);
        RefreshLayout();
    }

    private void NewRequest()
    {
        var instructor = Session.Instructor!;
        var targets = BuildTargets(instructor.Id);
        if (targets.Count == 0)
        {
            Navigator.Toast("You have no course content to request removal for.", false);
            return;
        }

        var fields = new[]
        {
            FormField.Choice("target", "What should be removed", targets[0].Label,
                targets.Select(t => t.Label).ToArray()),
            FormField.Multiline("reason", "Reason", "", "Explain why the content should be removed")
        };
        using var dialog = new FormDialog("New delete request",
            "The admin reviews every request. Nothing is removed immediately.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var target = targets.First(t => t.Label == dialog.Values["target"]);
        InstructorService.SubmitDeleteRequest(instructor.Id, target.CourseId, target.SectionId,
            target.LessonId, target.Label, dialog.Values["reason"]);
        Navigator.Toast("Delete request sent to the admin.");
        Navigator.Go("requests");
    }

    private static List<RequestTarget> BuildTargets(int instructorId)
    {
        var targets = new List<RequestTarget>();
        foreach (var course in CatalogService.CoursesForInstructor(instructorId).OrderBy(c => c.Title))
        {
            targets.Add(new RequestTarget("Course: " + course.Title, course.Id, null, null));

            foreach (var section in CatalogService.Sections(course.Id))
            {
                targets.Add(new RequestTarget(
                    "Section: " + course.Title + " / " + section.Title,
                    course.Id, section.Id, null));

                foreach (var lesson in CatalogService.Lessons(section.Id))
                {
                    targets.Add(new RequestTarget(
                        "Lesson: " + course.Title + " / " + lesson.Title,
                        course.Id, null, lesson.Id));
                }
            }
        }
        return targets;
    }

    private void ShowDetails(DeleteRequest request)
    {
        DetailsDialog.Show(this, request.TargetName, "Delete request",
            new[]
            {
                ("Target", request.TargetName),
                ("Requested", Fmt.Date(request.CreatedOn)),
                ("Reviewed", request.ReviewedOn is null ? "Not reviewed yet" : Fmt.Date(request.ReviewedOn.Value)),
                ("Status", request.Status.ToString()),
                ("Reason", request.Reason)
            },
            request.Status == DeleteRequestStatus.Pending
                ? "The admin will approve or reject this request."
                : null);
    }
}
