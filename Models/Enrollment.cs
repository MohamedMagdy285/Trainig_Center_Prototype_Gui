namespace ECMS;

/// <summary>Links a student to a course or an offline batch after payment.</summary>
public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public int? BatchId { get; set; }
    public int PaymentId { get; set; }
    public DateTime EnrolledOn { get; set; } = DateTime.Today;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Confirmed;
}
