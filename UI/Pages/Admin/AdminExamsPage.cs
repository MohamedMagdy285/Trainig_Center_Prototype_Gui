namespace ECMS;

/// <summary>Admin scheduling, scoring and certificate oversight.</summary>
public partial class AdminExamsPage : BasePage
{
    private readonly TableControl _table;
    private readonly Card _card;
    private List<ExamAttempt> _shown = new();

    public AdminExamsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        AddBlock(Header("Exams", "Final exam attempts, scores and issued certificates.",
            "Schedule exam", "\uE710", ScheduleExam), 60);

        StatRow(
            Stat("Exam attempts", AppData.Exams.Count.ToString("N0"), "All attempts", "\uE8A5", Theme.Primary),
            Stat("Awaiting result", AppData.Exams.Count(e => e.Result == ExamResult.Pending).ToString("N0"),
                "Scheduled or not scored", "\uE823", Theme.Amber),
            Stat("Passed", AppData.Exams.Count(e => e.Result == ExamResult.Passed).ToString("N0"),
                "Score 90 or above", "\uE73E", Theme.Accent),
            Stat("Certificates", AppData.Certificates.Count.ToString("N0"), "Issued to students", "\uE8D7", Theme.Secondary));

        _table = MakeTable(
            new[] { "Student", "Course", "Type", "Scheduled", "Score", "Result", "Certificate" },
            new[] { 2f, 2.6f, 1f, 1.3f, 0.8f, 1.3f, 1.2f },
            actions: true, pillColumns: new[] { 5 });
        _table.ActionClicked += (_, e) => HandleAction(e.RowIndex, e.Action);
        _table.RowClicked += (_, index) => ShowDetails(_shown[index]);
        _card = AddTableCard("All exam attempts", "Edit records a score  â€¢  Delete cancels a pending exam", _table,
            emptyText: "No exam attempts yet.");
        Reload();
    }

    private void Reload()
    {
        _shown = AppData.Exams.OrderByDescending(e => e.ScheduledOn).ToList();
        _table.SetRows(_shown.Select(e => new[]
        {
            CatalogService.StudentName(e.StudentId),
            CatalogService.Course(e.CourseId)?.Title ?? "Course",
            e.TypeText,
            Fmt.Date(e.ScheduledOn),
            e.Score?.ToString() ?? "â€”",
            e.ResultText,
            e.CertificateId is null ? "â€”" : "Issued"
        }));
        FitTableCard(_card, _table);
        RefreshLayout();
    }

    private void ScheduleExam()
    {
        var students = AppData.Students.Where(s => s.IsActive).ToList();
        var courses = CatalogService.CatalogCourses.ToList();
        if (students.Count == 0 || courses.Count == 0)
        {
            Navigator.Toast("There are no students or courses to schedule.", false);
            return;
        }

        var fields = new[]
        {
            FormField.Choice("student", "Student", students[0].DisplayName,
                students.Select(s => s.DisplayName).ToArray()),
            FormField.Choice("course", "Course", courses[0].Title,
                courses.Select(c => c.Title).ToArray()),
            FormField.Choice("type", "Exam type", "On-site", "On-site", "Remote")
        };
        using var dialog = new FormDialog("Schedule exam", "Create an exam request for a student.", fields,
            values =>
            {
                var student = students.First(s => s.DisplayName == values["student"]);
                var course = courses.First(c => c.Title == values["course"]);
                return CatalogService.IsEnrolled(student.Id, course.Id)
                    ? null
                    : "That student is not enrolled in the selected course.";
            });
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var selectedStudent = students.First(s => s.DisplayName == dialog.Values["student"]);
        var selectedCourse = courses.First(c => c.Title == dialog.Values["course"]);
        var (exam, error) = ExamService.RequestExam(selectedStudent.Id, selectedCourse.Id,
            dialog.Values["type"] == "On-site" ? ExamType.OnSite : ExamType.Remote);

        Navigator.Toast(exam is null ? error : "Exam scheduled for " + Fmt.Date(exam.ScheduledOn) + ".", exam is not null);
        Reload();
    }

    private void HandleAction(int index, TableRowAction action)
    {
        if (index < 0 || index >= _shown.Count) return;
        var exam = _shown[index];

        if (action == TableRowAction.Edit)
        {
            if (exam.Result != ExamResult.Pending)
            {
                Navigator.Toast("This exam has already been scored.", false);
                return;
            }

            using var dialog = new FormDialog("Record exam score",
                "Enter the score for " + CatalogService.StudentName(exam.StudentId) + ".", new[]
                {
                    FormField.Number("score", "Score (0-100)", "90", 0, 100)
                });
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            var error = ExamService.RecordScore(exam, int.Parse(dialog.Values["score"]));
            if (error is not null)
            {
                Navigator.Toast(error, false);
                return;
            }

            Navigator.Toast(exam.Result == ExamResult.Passed
                ? "Exam passed. A certificate was issued."
                : "Exam recorded. The student can request a retake.");
            Reload();
            return;
        }

        if (exam.Result != ExamResult.Pending)
        {
            Navigator.Toast("A completed exam cannot be deleted.", false);
            return;
        }

        if (!ConfirmDialog.Ask(this, "Cancel exam",
                "Cancel this pending exam request for " + CatalogService.StudentName(exam.StudentId) + "?",
                "Cancel exam"))
            return;

        AppData.Exams.Remove(exam);
        Navigator.Toast("Exam request cancelled.");
        Reload();
    }

    private void ShowDetails(ExamAttempt exam)
    {
        var certificate = exam.CertificateId is { } id
            ? AppData.Certificates.FirstOrDefault(c => c.Id == id)
            : null;
        DetailsDialog.Show(this, "Exam attempt", "Scheduled exam and result",
            new[]
            {
                ("Student", CatalogService.StudentName(exam.StudentId)),
                ("Course", CatalogService.Course(exam.CourseId)?.Title ?? "Course"),
                ("Type", exam.TypeText),
                ("Scheduled", Fmt.Date(exam.ScheduledOn)),
                ("Taken", exam.TakenOn is null ? "Not taken yet" : Fmt.Date(exam.TakenOn.Value)),
                ("Score", exam.Score?.ToString() ?? "Not scored"),
                ("Result", exam.ResultText),
                ("Retake", exam.IsRetake ? "Yes" : "No"),
                ("Certificate", certificate is null ? "Not issued" : certificate.Code + " (" + certificate.TypeText + ")")
            });
    }
}
