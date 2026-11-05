namespace ECMS;

/// <summary>A student comment on an online section or an offline course.</summary>
public class Review
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public int? SectionId { get; set; }
    public int? BatchId { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public DateTime? UpdatedOn { get; set; }
    public bool IsDeleted { get; set; }
}
