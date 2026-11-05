namespace ECMS;

/// <summary>An instructor request to delete a course, section or lesson.</summary>
public class DeleteRequest
{
    public int Id { get; set; }
    public int InstructorId { get; set; }
    public int? CourseId { get; set; }
    public int? SectionId { get; set; }
    public int? LessonId { get; set; }
    public string TargetName { get; set; } = "";
    public string Reason { get; set; } = "";
    public DeleteRequestStatus Status { get; set; } = DeleteRequestStatus.Pending;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public DateTime? ReviewedOn { get; set; }
}
