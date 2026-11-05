namespace ECMS;

/// <summary>Instructor approval, offboarding and delete requests.</summary>
public static class InstructorService
{
    public static void Approve(Instructor instructor)
    {
        instructor.Status = AccountStatus.Active;
        instructor.AdminNote = "Agreement reached. Account activated.";
    }

    public static void Reject(Instructor instructor, string note)
    {
        instructor.Status = AccountStatus.Rejected;
        instructor.AdminNote = note.Length > 0 ? note : "No agreement was reached.";
    }

    public static void Deactivate(Instructor instructor)
    {
        instructor.Status = AccountStatus.Deactivated;
        instructor.AdminNote = "Account deactivated. Historical records retained.";

        // Online courses keep their content and existing students, but new enrollments stop.
        foreach (var course in CatalogService.CoursesForInstructor(instructor.Id)
                     .Where(c => c.Kind == CourseKind.Online))
        {
            course.AcceptsNewEnrollments = false;
        }
    }

    public static void Reactivate(Instructor instructor)
    {
        instructor.Status = AccountStatus.Active;
        instructor.AdminNote = "Account reactivated after a new agreement.";
        foreach (var course in CatalogService.CoursesForInstructor(instructor.Id)
                     .Where(c => c.Kind == CourseKind.Online && c.IsPublished))
        {
            course.AcceptsNewEnrollments = true;
        }
    }

    public static DeleteRequest SubmitDeleteRequest(int instructorId, int? courseId, int? sectionId,
        int? lessonId, string targetName, string reason)
    {
        var request = new DeleteRequest
        {
            Id = AppData.NextId(AppData.DeleteRequests, r => r.Id),
            InstructorId = instructorId, CourseId = courseId, SectionId = sectionId,
            LessonId = lessonId, TargetName = targetName, Reason = reason.Trim(),
            CreatedOn = DateTime.Today, Status = DeleteRequestStatus.Pending
        };
        AppData.DeleteRequests.Add(request);
        return request;
    }

    public static void ReviewDeleteRequest(DeleteRequest request, bool approve)
    {
        request.Status = approve ? DeleteRequestStatus.Approved : DeleteRequestStatus.Rejected;
        request.ReviewedOn = DateTime.Today;
        if (!approve) return;

        if (request.CourseId is { } courseId)
        {
            var course = CatalogService.Course(courseId);
            if (course is not null)
            {
                course.IsDeleted = true;
                course.IsPublished = false;
                course.AcceptsNewEnrollments = false;
            }
        }

        if (request.SectionId is { } sectionId)
        {
            var section = AppData.Courses.SelectMany(c => c.Sections).FirstOrDefault(s => s.Id == sectionId);
            if (section is not null) section.IsDeleted = true;
        }

        if (request.LessonId is { } lessonId)
        {
            var lesson = AppData.Courses.SelectMany(c => c.Sections).SelectMany(s => s.Lessons)
                .FirstOrDefault(l => l.Id == lessonId);
            if (lesson is not null) lesson.IsDeleted = true;
        }
    }

    public static IEnumerable<DeleteRequest> RequestsForInstructor(int instructorId) =>
        AppData.DeleteRequests.Where(r => r.InstructorId == instructorId)
            .OrderByDescending(r => r.CreatedOn);

    public static IEnumerable<DeleteRequest> PendingRequests =>
        AppData.DeleteRequests.Where(r => r.Status == DeleteRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedOn);
}
