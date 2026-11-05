namespace ECMS;

/// <summary>Admin moderation of student comments.</summary>
public partial class AdminReviewsPage : BasePage
{
    private readonly TableControl _table;
    private readonly Card _card;
    private readonly SearchBox _search = new("Search comments, students or courses...");
    private List<Review> _shown = new();

    public AdminReviewsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        AddBlock(Header("Reviews", "Comments left by students after completing content."), 60);

        var visible = AppData.Reviews.Where(r => !r.IsDeleted).ToList();
        StatRow(
            Stat("Visible reviews", visible.Count.ToString("N0"), "Published comments", "\uE8BD", Theme.Primary),
            Stat("This month", visible.Count(r =>
                    r.CreatedOn.Year == DateTime.Today.Year && r.CreatedOn.Month == DateTime.Today.Month).ToString("N0"),
                "New feedback", "\uE823", Theme.Secondary),
            Stat("Section reviews", visible.Count(r => r.SectionId is not null).ToString("N0"),
                "Online courses", "\uE774", Theme.Accent),
            Stat("Offline reviews", visible.Count(r => r.BatchId is not null).ToString("N0"),
                "Batch feedback", "\uE716", Theme.Amber));

        var searchPanel = new Panel { BackColor = Theme.Bg };
        _search.Location = new Point(0, 0);
        _search.Size = new Size(360, 42);
        _search.ValueChanged += (_, _) => Reload();
        searchPanel.Controls.Add(_search);
        AddBlock(searchPanel, 44);

        _table = MakeTable(
            new[] { "Student", "Course", "Context", "Comment", "Date" },
            new[] { 1.7f, 2f, 1.8f, 4f, 1.2f },
            actions: true);
        _table.ActionClicked += (_, e) => HandleAction(e.RowIndex, e.Action);
        _table.RowClicked += (_, index) => ShowDetails(_shown[index]);
        _card = AddTableCard("All reviews", "Edit changes the comment  â€¢  Delete hides it from the course", _table,
            emptyText: "No reviews match your search.");
        Reload();
    }

    private void Reload()
    {
        var query = _search.Value.Trim();
        _shown = AppData.Reviews
            .Where(r => !r.IsDeleted)
            .Where(r => query.Length == 0 ||
                        r.Text.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        CatalogService.StudentName(r.StudentId).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (CatalogService.Course(r.CourseId)?.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(r => r.CreatedOn)
            .ToList();

        _table.SetRows(_shown.Select(r => new[]
        {
            CatalogService.StudentName(r.StudentId),
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

    private void HandleAction(int index, TableRowAction action)
    {
        if (index < 0 || index >= _shown.Count) return;
        var review = _shown[index];

        if (action == TableRowAction.Edit)
        {
            using var dialog = new FormDialog("Edit review", "Update the student comment.", new[]
            {
                FormField.Multiline("text", "Comment", review.Text)
            });
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            ReviewService.Edit(review, dialog.Values["text"]);
            Navigator.Toast("Review updated.");
            Reload();
            return;
        }

        if (!ConfirmDialog.Ask(this, "Hide review",
                "Hide this review? It will no longer appear on the course page.", "Hide"))
            return;

        ReviewService.DeleteByAdmin(review);
        Navigator.Toast("Review hidden.");
        Reload();
    }

    private void ShowDetails(Review review)
    {
        DetailsDialog.Show(this, "Review", "Comment moderation details",
            new[]
            {
                ("Student", CatalogService.StudentName(review.StudentId)),
                ("Course", CatalogService.Course(review.CourseId)?.Title ?? "Course"),
                ("Context", Context(review)),
                ("Created", Fmt.Date(review.CreatedOn)),
                ("Edited", review.UpdatedOn is null ? "Never edited" : Fmt.Date(review.UpdatedOn.Value)),
                ("Comment", review.Text)
            });
    }
}
