namespace ECMS;

/// <summary>Student exam requests, results and certificates.</summary>
public partial class StudentExamsPage : BasePage
{
    private readonly TableControl _examTable;
    private readonly Card _examCard;
    private List<ExamAttempt> _exams = new();
    private readonly TableControl _certificateTable;
    private readonly Card _certificateCard;
    private List<Certificate> _certificates = new();

    public StudentExamsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var student = Session.Student!;
        AddBlock(Header("Exams & certificates",
            "Request your final exam and download your certificate after passing.",
            "Request exam", "\uE710", RequestExam), 60);

        var enrolled = CatalogService.EnrollmentsForStudent(student.Id)
            .Select(e => e.CourseId).Distinct().Count();
        StatRow(
            Stat("My courses", enrolled.ToString("N0"), "Exam eligible", "\uE7BE", Theme.Primary),
            Stat("Scheduled", AppData.Exams.Count(e => e.StudentId == student.Id && e.Result == ExamResult.Pending).ToString("N0"),
                "Awaiting a result", "\uE823", Theme.Amber),
            Stat("Passed", AppData.Exams.Count(e => e.StudentId == student.Id && e.Result == ExamResult.Passed).ToString("N0"),
                "Score 90 or above", "\uE73E", Theme.Accent),
            Stat("Certificates", AppData.Certificates.Count(c => c.StudentId == student.Id).ToString("N0"),
                "Issued to you", "\uE8D7", Theme.Secondary));

        _examTable = MakeTable(
            new[] { "Course", "Type", "Scheduled", "Score", "Result", "Certificate" },
            new[] { 2.8f, 1f, 1.3f, 0.8f, 1.3f, 1.2f },
            actions: true, pillColumns: new[] { 4 });
        _examTable.ActionClicked += (_, e) => HandleExamAction(e.RowIndex, e.Action);
        _examTable.RowClicked += (_, index) => ShowExam(_exams[index]);
        _examCard = AddTableCard("My exam attempts", "Edit requests a retake  â€¢  Delete opens the certificate", _examTable,
            emptyText: "You have not requested an exam yet.");

        _certificateTable = MakeTable(
            new[] { "Course", "Type", "Certificate code", "Issued" },
            new[] { 2.6f, 1.4f, 1.8f, 1.2f });
        _certificateTable.RowClicked += (_, index) => ShowCertificate(_certificates[index]);
        _certificateCard = AddTableCard("My certificates", "Passed exams issue a certificate automatically", _certificateTable,
            emptyText: "Pass a final exam to earn your first certificate.");

        Reload();
    }

    private void Reload()
    {
        var student = Session.Student!;
        _exams = ExamService.ExamsForStudent(student.Id).ToList();
        _examTable.SetRows(_exams.Select(e => new[]
        {
            CatalogService.Course(e.CourseId)?.Title ?? "Course",
            e.TypeText,
            Fmt.Date(e.ScheduledOn),
            e.Score?.ToString() ?? "â€”",
            e.ResultText,
            e.CertificateId is null ? "â€”" : "Issued"
        }));
        FitTableCard(_examCard, _examTable);

        _certificates = AppData.Certificates
            .Where(c => c.StudentId == student.Id)
            .OrderByDescending(c => c.IssuedOn)
            .ToList();
        _certificateTable.SetRows(_certificates.Select(c => new[]
        {
            CatalogService.Course(c.CourseId)?.Title ?? "Course",
            c.TypeText,
            c.Code,
            Fmt.Date(c.IssuedOn)
        }));
        FitTableCard(_certificateCard, _certificateTable);
        RefreshLayout();
    }

    private void RequestExam()
    {
        var student = Session.Student!;
        var courses = CatalogService.EnrollmentsForStudent(student.Id)
            .Select(e => CatalogService.Course(e.CourseId))
            .Where(c => c is not null)
            .Select(c => c!)
            .DistinctBy(c => c.Id)
            .ToList();

        if (courses.Count == 0)
        {
            Navigator.Toast("You need to own a course before requesting an exam.", false);
            return;
        }

        var fields = new[]
        {
            FormField.Choice("course", "Course", courses[0].Title, courses.Select(c => c.Title).ToArray()),
            FormField.Choice("type", "Exam type", "On-site", "On-site", "Remote")
        };
        using var dialog = new FormDialog("Request final exam",
            $"The exam fee is {Fmt.Money(ExamService.DefaultFee)} and is scheduled seven days from today.", fields,
            values =>
            {
                var course = courses.First(c => c.Title == values["course"]);
                return ExamService.CanRequestExam(student.Id, course.Id);
            });
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var selected = courses.First(c => c.Title == dialog.Values["course"]);
        var (exam, error) = ExamService.RequestExam(student.Id, selected.Id,
            dialog.Values["type"] == "On-site" ? ExamType.OnSite : ExamType.Remote);

        Navigator.Toast(exam is null ? error : "Exam requested for " + Fmt.Date(exam.ScheduledOn) + ".", exam is not null);
        Reload();
    }

    private void HandleExamAction(int index, TableRowAction action)
    {
        if (index < 0 || index >= _exams.Count) return;
        var exam = _exams[index];

        if (action == TableRowAction.Edit)
        {
            if (exam.Result != ExamResult.Failed)
            {
                Navigator.Toast("A retake is only available after a failed attempt.", false);
                return;
            }
            if (!ConfirmDialog.Ask(this, "Request retake",
                    $"Request a retake of {CatalogService.Course(exam.CourseId)?.Title}? The fee is {Fmt.Money(ExamService.DefaultFee)}.",
                    "Request retake"))
                return;

            var (retake, error) = ExamService.RequestRetake(exam);
            Navigator.Toast(retake is null ? error : "Retake requested for " + Fmt.Date(retake.ScheduledOn) + ".",
                retake is not null);
            Reload();
            return;
        }

        if (exam.CertificateId is null)
        {
            Navigator.Toast("A certificate is only available after passing the exam.", false);
            return;
        }

        var certificate = AppData.Certificates.FirstOrDefault(c => c.Id == exam.CertificateId);
        if (certificate is not null) ShowCertificate(certificate);
    }

    private void ShowExam(ExamAttempt exam)
    {
        var certificate = exam.CertificateId is { } id
            ? AppData.Certificates.FirstOrDefault(c => c.Id == id)
            : null;
        DetailsDialog.Show(this, "Exam attempt", "Your final exam record",
            new[]
            {
                ("Course", CatalogService.Course(exam.CourseId)?.Title ?? "Course"),
                ("Type", exam.TypeText),
                ("Scheduled", Fmt.Date(exam.ScheduledOn)),
                ("Taken", exam.TakenOn is null ? "Not taken yet" : Fmt.Date(exam.TakenOn.Value)),
                ("Score", exam.Score?.ToString() ?? "Not scored"),
                ("Result", exam.ResultText),
                ("Certificate", certificate is null ? "Not issued" : certificate.Code)
            },
            exam.Result == ExamResult.Failed
                ? "You can request a retake from the table using the edit icon."
                : null);
    }

    private void ShowCertificate(Certificate certificate)
    {
        DetailsDialog.Show(this, certificate.Code, "EduSphere Academy certificate",
            new[]
            {
                ("Student", Session.Student!.DisplayName),
                ("Course", CatalogService.Course(certificate.CourseId)?.Title ?? "Course"),
                ("Certificate type", certificate.TypeText),
                ("Issued", Fmt.Date(certificate.IssuedOn)),
                ("Code", certificate.Code)
            },
            "On-site certificates are original. Remote certificates are clearly marked as remote.");
    }
}
