namespace ECMS;

/// <summary>One attempt at the final exam.</summary>
public class ExamAttempt
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public ExamType Type { get; set; } = ExamType.OnSite;
    public DateTime ScheduledOn { get; set; } = DateTime.Today.AddDays(7);
    public decimal Fee { get; set; }
    public int? Score { get; set; }
    public ExamResult Result { get; set; } = ExamResult.Pending;
    public DateTime? TakenOn { get; set; }
    public int? CertificateId { get; set; }
    public bool IsRetake { get; set; }

    public string TypeText => Type == ExamType.OnSite ? "On-site" : "Remote";
    public string ResultText => Result == ExamResult.Pending ? "Awaiting result" : Result.ToString();
}

/// <summary>A certificate issued after a passing exam.</summary>
public class Certificate
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public int ExamAttemptId { get; set; }
    public ExamType Type { get; set; }
    public string Code { get; set; } = "";
    public DateTime IssuedOn { get; set; } = DateTime.Today;

    public string TypeText => Type == ExamType.OnSite ? "Original on-site" : "Remote";
}
