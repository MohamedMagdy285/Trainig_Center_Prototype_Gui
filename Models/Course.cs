namespace ECMS;

/// <summary>An online course or the template of an offline course.</summary>
public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public CourseKind Kind { get; set; }
    public int InstructorId { get; set; }
    public decimal Price { get; set; }
    public bool IsPublished { get; set; } = true;
    public bool AcceptsNewEnrollments { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public List<Section> Sections { get; set; } = new();

    public int TotalLessons => Sections.Sum(s => s.Lessons.Count);
    public int TotalSections => Sections.Count;

    public Section? PreviewSection =>
        Sections.Where(s => s.IsPreview).OrderBy(s => s.Order).FirstOrDefault()
        ?? Sections.OrderBy(s => s.Order).FirstOrDefault();
}

public class Section
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public bool IsBonus { get; set; }
    public bool IsPreview { get; set; }
    public bool IsDeleted { get; set; }
    public List<Lesson> Lessons { get; set; } = new();

    public string KindLabel => IsBonus ? "Bonus" : IsPreview ? "Preview" : "Section";
}

public class Lesson
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public int Minutes { get; set; } = 10;
    public string Notes { get; set; } = "";
    public bool IsDeleted { get; set; }
}

public class LessonCompletion
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int LessonId { get; set; }
    public DateTime CompletedOn { get; set; } = DateTime.Today;
}
