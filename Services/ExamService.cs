namespace ECMS;

/// <summary>Exam scheduling, scoring, retakes and certificates.</summary>
public static class ExamService
{
    public const int PassingScore = 90;
    public const decimal DefaultFee = 300;

    public static string? CanRequestExam(int studentId, int courseId)
    {
        if (!CatalogService.IsEnrolled(studentId, courseId))
            return "You must own the course before requesting the final exam.";
        if (AppData.Exams.Any(e => e.StudentId == studentId && e.CourseId == courseId && e.Result == ExamResult.Pending))
            return "You already have an exam request awaiting a result.";
        return null;
    }

    public static (ExamAttempt? Exam, string Error) RequestExam(int studentId, int courseId,
        ExamType type, decimal fee = DefaultFee)
    {
        var error = CanRequestExam(studentId, courseId);
        if (error is not null) return (null, error);

        var exam = new ExamAttempt
        {
            Id = AppData.NextId(AppData.Exams, e => e.Id),
            StudentId = studentId, CourseId = courseId, Type = type,
            ScheduledOn = DateTime.Today.AddDays(7), Fee = fee, Result = ExamResult.Pending
        };
        AppData.Exams.Add(exam);
        return (exam, "");
    }

    public static (ExamAttempt? Exam, string Error) RequestRetake(ExamAttempt previous,
        decimal fee = DefaultFee)
    {
        if (previous.Result != ExamResult.Failed)
            return (null, "A retake can only be requested after a failed attempt.");

        var exam = new ExamAttempt
        {
            Id = AppData.NextId(AppData.Exams, e => e.Id),
            StudentId = previous.StudentId, CourseId = previous.CourseId,
            Type = previous.Type, ScheduledOn = DateTime.Today.AddDays(7),
            Fee = fee, Result = ExamResult.Pending, IsRetake = true
        };
        AppData.Exams.Add(exam);
        return (exam, "");
    }

    public static string? RecordScore(ExamAttempt exam, int score)
    {
        if (score < 0 || score > 100) return "The score must be between 0 and 100.";
        if (exam.Result != ExamResult.Pending) return "This exam has already been scored.";

        exam.Score = score;
        exam.TakenOn = DateTime.Today;

        if (score < PassingScore)
        {
            exam.Result = ExamResult.Failed;
            return null;
        }

        exam.Result = ExamResult.Passed;
        var certificate = new Certificate
        {
            Id = AppData.NextId(AppData.Certificates, c => c.Id),
            StudentId = exam.StudentId, CourseId = exam.CourseId, ExamAttemptId = exam.Id,
            Type = exam.Type, Code = $"EDS-{DateTime.Today.Year}-{AppData.Certificates.Count + 1:0000}",
            IssuedOn = DateTime.Today
        };
        AppData.Certificates.Add(certificate);
        exam.CertificateId = certificate.Id;
        return null;
    }

    public static Certificate? CertificateFor(int studentId, int courseId) =>
        AppData.Certificates.Where(c => c.StudentId == studentId && c.CourseId == courseId)
            .OrderByDescending(c => c.IssuedOn).FirstOrDefault();

    public static IEnumerable<ExamAttempt> ExamsForStudent(int studentId) =>
        AppData.Exams.Where(e => e.StudentId == studentId).OrderByDescending(e => e.ScheduledOn);

    public static IEnumerable<ExamAttempt> ExamsForCourse(int courseId) =>
        AppData.Exams.Where(e => e.CourseId == courseId).OrderByDescending(e => e.ScheduledOn);
}
