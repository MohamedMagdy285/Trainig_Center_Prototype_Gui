namespace ECMS;

/// <summary>Student comments and admin moderation.</summary>
public static class ReviewService
{
    public static bool CanReviewSection(int studentId, int courseId, int sectionId)
        => CatalogService.IsEnrolled(studentId, courseId) &&
           CatalogService.IsSectionCompleted(studentId, sectionId);

    public static bool CanReviewOffline(int studentId, int courseId, int batchId)
    {
        var batch = CatalogService.Batch(batchId);
        var enrolled = CatalogService.EnrollmentFor(studentId, courseId, batchId) is not null;
        return enrolled && batch is not null &&
               (batch.Status == BatchStatus.Completed || batch.EndDate < DateTime.Today);
    }

    public static string? AddOnlineReview(int studentId, int courseId, int sectionId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Please write a comment.";
        if (!CanReviewSection(studentId, courseId, sectionId))
            return "Complete the section before adding a comment.";

        AppData.Reviews.Add(new Review
        {
            Id = AppData.NextId(AppData.Reviews, r => r.Id),
            StudentId = studentId, CourseId = courseId, SectionId = sectionId,
            Text = text.Trim(), CreatedOn = DateTime.Today
        });
        return null;
    }

    public static string? AddOfflineReview(int studentId, int courseId, int batchId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Please write a comment.";
        if (!CanReviewOffline(studentId, courseId, batchId))
            return "Feedback is available after the offline course is completed.";

        AppData.Reviews.Add(new Review
        {
            Id = AppData.NextId(AppData.Reviews, r => r.Id),
            StudentId = studentId, CourseId = courseId, BatchId = batchId,
            Text = text.Trim(), CreatedOn = DateTime.Today
        });
        return null;
    }

    public static void Edit(Review review, string text)
    {
        review.Text = text.Trim();
        review.UpdatedOn = DateTime.Today;
    }

    public static void DeleteByStudent(Review review) => review.IsDeleted = true;
    public static void DeleteByAdmin(Review review) => review.IsDeleted = true;

    public static IEnumerable<Review> VisibleForCourse(int courseId) =>
        AppData.Reviews.Where(r => r.CourseId == courseId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedOn);

    public static IEnumerable<Review> VisibleForSection(int sectionId) =>
        AppData.Reviews.Where(r => r.SectionId == sectionId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedOn);

    public static IEnumerable<Review> ReviewsForInstructor(int instructorId)
    {
        var courseIds = CatalogService.CoursesForInstructor(instructorId).Select(c => c.Id).ToHashSet();
        return AppData.Reviews.Where(r => courseIds.Contains(r.CourseId) && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedOn);
    }
}
