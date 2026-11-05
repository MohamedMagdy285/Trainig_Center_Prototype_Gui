namespace ECMS;

/// <summary>Read-only questions about courses, content and progress.</summary>
public static class CatalogService
{
    public static Course? Course(int id) => AppData.Courses.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
    public static Batch? Batch(int id) => AppData.Batches.FirstOrDefault(b => b.Id == id);
    public static Student? Student(int id) => AppData.Students.FirstOrDefault(s => s.Id == id);
    public static Instructor? Instructor(int id) => AppData.Instructors.FirstOrDefault(i => i.Id == id);

    public static string InstructorName(int id) => Instructor(id)?.FullName ?? "Instructor";
    public static string StudentName(int id) => Student(id)?.DisplayName ?? "Student";

    public static IEnumerable<Course> CatalogCourses =>
        AppData.Courses.Where(c => !c.IsDeleted && c.IsPublished);

    public static IEnumerable<Course> OnlineCourses =>
        CatalogCourses.Where(c => c.Kind == CourseKind.Online);

    public static IEnumerable<Course> OfflineCourses =>
        CatalogCourses.Where(c => c.Kind == CourseKind.Offline);

    public static IEnumerable<Course> CoursesForInstructor(int instructorId) =>
        AppData.Courses.Where(c => c.InstructorId == instructorId && !c.IsDeleted);

    public static IEnumerable<Batch> BatchesForCourse(int courseId) =>
        AppData.Batches.Where(b => b.CourseId == courseId).OrderBy(b => b.StartDate);

    public static IEnumerable<Batch> UpcomingBatches(int count = 4) =>
        AppData.Batches
            .Where(b => b.Status != BatchStatus.Cancelled && b.StartDate >= DateTime.Today)
            .OrderBy(b => b.StartDate)
            .Take(count);

    public static IEnumerable<Section> Sections(int courseId) =>
        AppData.Courses.Where(c => c.Id == courseId && !c.IsDeleted)
            .SelectMany(c => c.Sections)
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Order);

    public static IEnumerable<Lesson> Lessons(int sectionId) =>
        AppData.Courses.SelectMany(c => c.Sections)
            .Where(s => s.Id == sectionId && !s.IsDeleted)
            .SelectMany(s => s.Lessons)
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.Order);

    public static IEnumerable<Lesson> AllLessons(int courseId) =>
        Sections(courseId).SelectMany(s => s.Lessons.Where(l => !l.IsDeleted).OrderBy(l => l.Order));

    public static Section? SectionOfLesson(int lessonId) =>
        AppData.Courses.SelectMany(c => c.Sections)
            .FirstOrDefault(s => !s.IsDeleted && s.Lessons.Any(l => l.Id == lessonId && !l.IsDeleted));

    public static Course? CourseOfLesson(int lessonId) =>
        SectionOfLesson(lessonId) is { } section ? Course(section.CourseId) : null;

    public static bool IsLessonCompleted(int studentId, int lessonId) =>
        AppData.Completions.Any(c => c.StudentId == studentId && c.LessonId == lessonId);

    public static (int Completed, int Total) Progress(int studentId, int courseId)
    {
        var lessons = AllLessons(courseId).ToList();
        var completed = lessons.Count(l => IsLessonCompleted(studentId, l.Id));
        return (completed, lessons.Count);
    }

    public static double ProgressPercent(int studentId, int courseId)
    {
        var (completed, total) = Progress(studentId, courseId);
        return total == 0 ? 0 : completed * 100.0 / total;
    }

    public static bool IsSectionCompleted(int studentId, int sectionId)
    {
        var lessons = Lessons(sectionId).ToList();
        return lessons.Count > 0 && lessons.All(l => IsLessonCompleted(studentId, l.Id));
    }

    public static bool CanOpenLesson(int studentId, int courseId, int lessonId)
    {
        var lessons = AllLessons(courseId).ToList();
        var index = lessons.FindIndex(l => l.Id == lessonId);
        if (index <= 0) return index == 0;
        return IsLessonCompleted(studentId, lessons[index - 1].Id);
    }

    public static Lesson? NextLesson(int studentId, int courseId)
    {
        var lessons = AllLessons(courseId).ToList();
        return lessons.FirstOrDefault(l => !IsLessonCompleted(studentId, l.Id) && CanOpenLesson(studentId, courseId, l.Id))
               ?? lessons.FirstOrDefault(l => !IsLessonCompleted(studentId, l.Id));
    }

    public static Enrollment? EnrollmentFor(int studentId, int courseId, int? batchId = null) =>
        AppData.Enrollments.FirstOrDefault(e =>
            e.StudentId == studentId && e.CourseId == courseId &&
            (batchId is null || e.BatchId == batchId) &&
            e.Status == EnrollmentStatus.Confirmed);

    public static bool IsEnrolled(int studentId, int courseId) =>
        EnrollmentFor(studentId, courseId) is not null;

    public static IEnumerable<Enrollment> EnrollmentsForStudent(int studentId) =>
        AppData.Enrollments.Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Confirmed);

    public static IEnumerable<Enrollment> EnrollmentsForCourse(int courseId) =>
        AppData.Enrollments.Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Confirmed);

    public static int StudentCount(int courseId) => EnrollmentsForCourse(courseId).Select(e => e.StudentId).Distinct().Count();

    public static int StudentCountForInstructor(int instructorId) =>
        CoursesForInstructor(instructorId)
            .SelectMany(c => EnrollmentsForCourse(c.Id))
            .Select(e => e.StudentId)
            .Distinct()
            .Count();

    public static int PendingInstructorApplications =>
        AppData.Instructors.Count(i => i.Status == AccountStatus.Pending);

    public static int PendingDeleteRequests =>
        AppData.DeleteRequests.Count(r => r.Status == DeleteRequestStatus.Pending);
}
