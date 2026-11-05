namespace ECMS;

/// <summary>Admin management of offline batches and their seats.</summary>
public partial class AdminBatchesPage : BasePage
{
    private readonly TableControl _table;
    private readonly Card _card;
    private readonly SearchBox _search = new("Search batches or courses...");
    private List<Batch> _shown = new();

    public AdminBatchesPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        AddBlock(Header("Batches", "Scheduled offline groups, capacity and status.",
            "Add batch", "\uE710", AddBatch), 60);

        StatRow(
            Stat("Total batches", AppData.Batches.Count.ToString("N0"), "All groups", "\uE787", Theme.Primary),
            Stat("Scheduled", AppData.Batches.Count(b => b.StatusText == "Scheduled").ToString("N0"),
                "Starting soon", "\uE823", Theme.Secondary),
            Stat("Ongoing", AppData.Batches.Count(b => b.StatusText == "Ongoing").ToString("N0"),
                "Running now", "\uE916", Theme.Accent),
            Stat("Seats left", AppData.Batches.Where(b => b.Status != BatchStatus.Cancelled).Sum(b => b.SeatsLeft).ToString("N0"),
                "Across open batches", "\uE716", Theme.Amber));

        var searchPanel = new Panel { BackColor = Theme.Bg };
        _search.Location = new Point(0, 0);
        _search.Size = new Size(360, 42);
        _search.ValueChanged += (_, _) => Reload();
        searchPanel.Controls.Add(_search);
        AddBlock(searchPanel, 44);

        _table = MakeTable(
            new[] { "Batch", "Course", "Instructor", "Starts", "Ends", "Seats", "Status" },
            new[] { 2.4f, 2f, 1.8f, 1.2f, 1.2f, 0.9f, 1.2f },
            actions: true, pillColumns: new[] { 6 });
        _table.ActionClicked += (_, e) => HandleAction(e.RowIndex, e.Action);
        _table.RowClicked += (_, index) => ShowDetails(_shown[index]);
        _card = AddTableCard("All batches", "Click a row to inspect the group", _table,
            emptyText: "No batches match your search.");
        Reload();
    }

    private void Reload()
    {
        var query = _search.Value.Trim();
        _shown = AppData.Batches
            .Where(b => query.Length == 0 ||
                        b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (CatalogService.Course(b.CourseId)?.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(b => b.StartDate)
            .ToList();

        _table.SetRows(_shown.Select(b => new[]
        {
            b.Title,
            CatalogService.Course(b.CourseId)?.Title ?? "Course",
            CatalogService.InstructorName(b.InstructorId),
            Fmt.Date(b.StartDate),
            Fmt.Date(b.EndDate),
            b.SeatsLeft + "/" + b.Capacity,
            b.StatusText
        }));
        FitTableCard(_card, _table);
        RefreshLayout();
    }

    private void AddBatch()
    {
        var courses = CatalogService.OfflineCourses.ToList();
        if (courses.Count == 0)
        {
            Navigator.Toast("Create an offline course first.", false);
            return;
        }

        var instructors = AppData.Instructors.Where(i => i.Status == AccountStatus.Active).ToList();
        var fields = new[]
        {
            FormField.Choice("course", "Offline course", courses[0].Title,
                courses.Select(c => c.Title).ToArray()),
            FormField.Text("title", "Batch title", "", "e.g. Business English - March"),
            FormField.Choice("instructor", "Instructor",
                instructors.Count > 0 ? instructors[0].FullName : "Instructor",
                instructors.Select(i => i.FullName).ToArray()),
            FormField.Date("start", "Start date", DateTime.Today.AddDays(14)),
            FormField.Date("end", "End date", DateTime.Today.AddDays(60)),
            FormField.Number("capacity", "Capacity", "20", 1, 500),
            FormField.Number("price", "Price (EGP)", courses[0].Price.ToString("0"), 0, 1000000),
            FormField.Text("schedule", "Schedule", "", "e.g. Sat & Mon, 6:00 - 8:00 PM")
        };
        using var dialog = new FormDialog("Add batch", "Schedule a new offline group.", fields,
            values => DateTime.Parse(values["end"]) <= DateTime.Parse(values["start"])
                ? "The end date must be after the start date."
                : null);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var course = courses.First(c => c.Title == dialog.Values["course"]);
        var instructor = instructors.FirstOrDefault(i => i.FullName == dialog.Values["instructor"]);
        AppData.Batches.Add(new Batch
        {
            Id = AppData.NextId(AppData.Batches, b => b.Id),
            CourseId = course.Id,
            InstructorId = instructor?.Id ?? course.InstructorId,
            Title = dialog.Values["title"],
            StartDate = DateTime.Parse(dialog.Values["start"]),
            EndDate = DateTime.Parse(dialog.Values["end"]),
            Capacity = int.Parse(dialog.Values["capacity"]),
            Price = decimal.Parse(dialog.Values["price"]),
            Schedule = dialog.Values["schedule"],
            Status = BatchStatus.Scheduled
        });
        Navigator.Toast("Batch scheduled.");
        Reload();
    }

    private void EditBatch(Batch batch)
    {
        if (!EnrollmentService.CanEditBatch(batch))
        {
            Navigator.Toast("This batch already has payment transactions and cannot be edited.", false);
            return;
        }

        var fields = new[]
        {
            FormField.Text("title", "Batch title", batch.Title),
            FormField.Date("start", "Start date", batch.StartDate),
            FormField.Date("end", "End date", batch.EndDate),
            FormField.Number("capacity", "Capacity", batch.Capacity.ToString(), 1, 500),
            FormField.Number("price", "Price (EGP)", batch.Price.ToString("0"), 0, 1000000),
            FormField.Text("schedule", "Schedule", batch.Schedule)
        };
        using var dialog = new FormDialog("Edit batch", "Update the schedule and capacity.", fields,
            values => DateTime.Parse(values["end"]) <= DateTime.Parse(values["start"])
                ? "The end date must be after the start date."
                : null);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        batch.Title = dialog.Values["title"];
        batch.StartDate = DateTime.Parse(dialog.Values["start"]);
        batch.EndDate = DateTime.Parse(dialog.Values["end"]);
        batch.Capacity = int.Parse(dialog.Values["capacity"]);
        batch.Price = decimal.Parse(dialog.Values["price"]);
        batch.Schedule = dialog.Values["schedule"];
        Navigator.Toast("Batch updated.");
        Reload();
    }

    private void HandleAction(int index, TableRowAction action)
    {
        if (index < 0 || index >= _shown.Count) return;
        var batch = _shown[index];
        if (action == TableRowAction.Edit)
        {
            EditBatch(batch);
            return;
        }

        if (batch.Status == BatchStatus.Cancelled)
        {
            Navigator.Toast("This batch is already cancelled.", false);
            return;
        }

        if (!ConfirmDialog.Ask(this, "Cancel batch",
                "Cancel " + batch.Title + "? Every enrolled student will be refunded in full.",
                "Cancel batch"))
            return;

        var affected = EnrollmentService.CancelBatch(batch, "Academy cancelled the batch");
        Navigator.Toast(affected == 0
            ? "Batch cancelled. There were no enrolled students."
            : $"Batch cancelled. {affected} student(s) refunded in full.");
        Reload();
    }

    private void ShowDetails(Batch batch)
    {
        var enrolled = AppData.Enrollments
            .Where(e => e.BatchId == batch.Id && e.Status == EnrollmentStatus.Confirmed)
            .ToList();
        DetailsDialog.Show(this, batch.Title, "Offline batch details",
            new[]
            {
                ("Course", CatalogService.Course(batch.CourseId)?.Title ?? "Course"),
                ("Instructor", CatalogService.InstructorName(batch.InstructorId)),
                ("Starts", Fmt.Date(batch.StartDate)),
                ("Ends", Fmt.Date(batch.EndDate)),
                ("Schedule", batch.Schedule),
                ("Seats", batch.SeatsLeft + " left of " + batch.Capacity),
                ("Enrolled", enrolled.Count.ToString()),
                ("Status", batch.StatusText)
            },
            batch.IsFull ? "This batch is full. New payments are refunded automatically." : null);
    }
}
