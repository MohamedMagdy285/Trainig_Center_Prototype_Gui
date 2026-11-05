namespace ECMS;

/// <summary>Student comments and the sections or batches they can review.</summary>
public partial class MyReviewsPage : BasePage
{
    private readonly TableControl _table;
    private readonly Card _card;
    private List<Review> _shown = new();

    private sealed record ReviewTarget(string Label, int CourseId, int? SectionId, int? BatchId);

    public MyReviewsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var student = Session.Student!;
        AddBlock(Header("My reviews", "Share feedback after completing a section or an offline course.",
            "Write review", "\uE70F", WriteReview), 60);

        var mine = AppData.Reviews.Where(r => r.StudentId == student.Id && !r.IsDeleted).ToList();
        StatRow(
            Stat("My reviews", mine.Count.ToString("N0"), "Published comments", "\uE8BD", Theme.Primary),
            Stat("Courses reviewed", mine.Select(r => r.CourseId).Distinct().Count().ToString("N0"),
                "Different courses", "\uE7BE", Theme.Secondary),
            Stat("This month", mine.Count(r =>
                    r.CreatedOn.Year == DateTime.Today.Year && r.CreatedOn.Month == DateTime.Today.Month).ToString("N0"),
                "Recent feedback", "\uE823", Theme.Accent),
            Stat("Edited", mine.Count(r => r.UpdatedOn is not null).ToString("N0"),
                "Updated after posting", "\uE70F", Theme.Amber));

        _table = MakeTable(
            new[] { "Course", "Context", "Comment", "Date" },
            new[] { 2.4f, 2f, 4f, 1.2f },
            actions: true);
        _table.ActionClicked += (_, e) => HandleAction(e.RowIndex, e.Action);
        _table.RowClicked += (_, index) => ShowDetails(_shown[index]);
        _card = AddTableCard("My reviews", "Edit changes your comment  â€¢  Delete removes it", _table,
            emptyText: "You have not written a review yet.");
        Reload();
    }

    private void Reload()
    {
        var student = Session.Student!;
        _shown = AppData.Reviews
            .Where(r => r.StudentId == student.Id && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedOn)
            .ToList();
        _table.SetRows(_shown.Select(r => new[]
        {
            CatalogService.Course(r.CourseId)?.Title ?? "Course",
            Context(r),
            r.Text,
            Fmt.Date(r.CreatedOn)
        }));
        FitTableCard(_card, _table);
        RefreshLayout();
    }

    private static string Context(Review review)
    {
        if (review.SectionId is { } sectionId)
        {
            var section = CatalogService.Sections(review.CourseId).FirstOrDefault(s => s.Id == sectionId);
            return "Section: " + (section?.Title ?? "Section");
        }
        if (review.BatchId is { } batchId)
            return "Batch: " + (CatalogService.Batch(batchId)?.Title ?? "Batch");
        return "Course";
    }

    private void WriteReview()
    {
        var student = Session.Student!;
        var targets = BuildTargets(student);

        if (targets.Count == 0)
        {
            Navigator.Toast("Complete a section or an offline course before writing a review.", false);
            return;
        }

        var fields = new[]
        {
            FormField.Choice("context", "What are you reviewing", targets[0].Label,
                targets.Select(t => t.Label).ToArray()),
            FormField.Multiline("text", "Your comment", "", "What did you think?")
        };
        using var dialog = new FormDialog("Write review", "Your feedback helps other students.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var target = targets.First(t => t.Label == dialog.Values["context"]);
        var error = target.SectionId is { } sectionId
            ? ReviewService.AddOnlineReview(student.Id, target.CourseId, sectionId, dialog.Values["text"])
            : ReviewService.AddOfflineReview(student.Id, target.CourseId, target.BatchId!.Value, dialog.Values["text"]);

        if (error is not null)
        {
            Navigator.Toast(error, false);
            return;
        }

        Navigator.Toast("Review published.");
        Navigator.Go("reviews");
    }

    private static List<ReviewTarget> BuildTargets(Student student)
    {
        var targets = new List<ReviewTarget>();
        foreach (var enrollment in CatalogService.EnrollmentsForStudent(student.Id))
        {
            var course = CatalogService.Course(enrollment.CourseId);
            if (course is null) continue;

            foreach (var section in CatalogService.Sections(course.Id))
            {
                if (!CatalogService.IsSectionCompleted(student.Id, section.Id)) continue;
                var already = AppData.Reviews.Any(r =>
                    r.StudentId == student.Id && r.SectionId == section.Id && !r.IsDeleted);
                if (already) continue;
                targets.Add(new ReviewTarget(course.Title + "  â€¢  Section: " + section.Title,
                    course.Id, section.Id, null));
            }

            if (enrollment.BatchId is { } batchId && ReviewService.CanReviewOffline(student.Id, course.Id, batchId))
            {
                var already = AppData.Reviews.Any(r =>
                    r.StudentId == student.Id && r.BatchId == batchId && !r.IsDeleted);
                if (already) continue;
                var batch = CatalogService.Batch(batchId);
                targets.Add(new ReviewTarget(course.Title + "  â€¢  Batch: " + (batch?.Title ?? "Batch"),
                    course.Id, null, batchId));
            }
        }
        return targets;
    }

    private void HandleAction(int index, TableRowAction action)
    {
        if (index < 0 || index >= _shown.Count) return;
        var review = _shown[index];

        if (action == TableRowAction.Edit)
        {
            using var dialog = new FormDialog("Edit review", "Update your comment.", new[]
            {
                FormField.Multiline("text", "Your comment", review.Text)
            });
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            ReviewService.Edit(review, dialog.Values["text"]);
            Navigator.Toast("Review updated.");
            Reload();
            return;
        }

        if (!ConfirmDialog.Ask(this, "Delete review",
                "Delete this review? It will no longer be visible on the course.", "Delete"))
            return;

        ReviewService.DeleteByStudent(review);
        Navigator.Toast("Review deleted.");
        Navigator.Go("reviews");
    }

    private void ShowDetails(Review review)
    {
        DetailsDialog.Show(this, "My review", "Your published feedback",
            new[]
            {
                ("Course", CatalogService.Course(review.CourseId)?.Title ?? "Course"),
                ("Context", Context(review)),
                ("Created", Fmt.Date(review.CreatedOn)),
                ("Edited", review.UpdatedOn is null ? "Never edited" : Fmt.Date(review.UpdatedOn.Value)),
                ("Comment", review.Text)
            });
    }
}
