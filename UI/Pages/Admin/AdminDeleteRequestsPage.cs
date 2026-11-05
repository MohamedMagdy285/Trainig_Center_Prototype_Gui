namespace ECMS;

/// <summary>Admin approval of instructor content deletion requests.</summary>
public partial class AdminDeleteRequestsPage : BasePage
{
    private readonly TableControl _table;
    private readonly Card _card;
    private readonly SearchBox _search = new("Search requests...");
    private List<DeleteRequest> _shown = new();

    public AdminDeleteRequestsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        AddBlock(Header("Delete requests",
            "Instructors never delete content directly. Every request is reviewed here."), 60);

        StatRow(
            Stat("Pending", AppData.DeleteRequests.Count(r => r.Status == DeleteRequestStatus.Pending).ToString("N0"),
                "Waiting for a decision", "\uE823", Theme.Amber),
            Stat("Approved", AppData.DeleteRequests.Count(r => r.Status == DeleteRequestStatus.Approved).ToString("N0"),
                "Content removed", "\uE73E", Theme.Accent),
            Stat("Rejected", AppData.DeleteRequests.Count(r => r.Status == DeleteRequestStatus.Rejected).ToString("N0"),
                "Content kept", "\uE711", Theme.Red),
            Stat("Total requests", AppData.DeleteRequests.Count.ToString("N0"),
                "All time", "\uE74D", Theme.Primary));

        var searchPanel = new Panel { BackColor = Theme.Bg };
        _search.Location = new Point(0, 0);
        _search.Size = new Size(360, 42);
        _search.ValueChanged += (_, _) => Reload();
        searchPanel.Controls.Add(_search);
        AddBlock(searchPanel, 44);

        _table = MakeTable(
            new[] { "Instructor", "Target", "Reason", "Requested", "Status" },
            new[] { 1.8f, 2.4f, 3.6f, 1.3f, 1.3f },
            actions: true, pillColumns: new[] { 4 });
        _table.ActionClicked += (_, e) => HandleAction(e.RowIndex, e.Action);
        _table.RowClicked += (_, index) => ShowDetails(_shown[index]);
        _card = AddTableCard("All requests", "Edit approves the request  â€¢  Delete rejects it", _table,
            emptyText: "No delete requests match your search.");
        Reload();
    }

    private void Reload()
    {
        var query = _search.Value.Trim();
        _shown = AppData.DeleteRequests
            .Where(r => query.Length == 0 ||
                        r.TargetName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        r.Reason.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        CatalogService.InstructorName(r.InstructorId).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        r.Status.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.CreatedOn)
            .ToList();

        _table.SetRows(_shown.Select(r => new[]
        {
            CatalogService.InstructorName(r.InstructorId),
            r.TargetName,
            r.Reason,
            Fmt.Date(r.CreatedOn),
            r.Status.ToString()
        }));
        FitTableCard(_card, _table);
        RefreshLayout();
    }

    private void HandleAction(int index, TableRowAction action)
    {
        if (index < 0 || index >= _shown.Count) return;
        var request = _shown[index];
        if (request.Status != DeleteRequestStatus.Pending)
        {
            Navigator.Toast("This request has already been reviewed.", false);
            return;
        }

        var approve = action == TableRowAction.Edit;
        var message = approve
            ? "Approve this request? The content will be removed from the course."
            : "Reject this request? The content stays available to students.";
        if (!ConfirmDialog.Ask(this, approve ? "Approve request" : "Reject request", message,
                approve ? "Approve" : "Reject"))
            return;

        InstructorService.ReviewDeleteRequest(request, approve);
        Navigator.Toast(approve ? "Request approved. Content removed." : "Request rejected. Content kept.");
        Reload();
    }

    private void ShowDetails(DeleteRequest request)
    {
        DetailsDialog.Show(this, request.TargetName, "Delete request",
            new[]
            {
                ("Instructor", CatalogService.InstructorName(request.InstructorId)),
                ("Target", request.TargetName),
                ("Requested", Fmt.Date(request.CreatedOn)),
                ("Reviewed", request.ReviewedOn is null ? "Not reviewed yet" : Fmt.Date(request.ReviewedOn.Value)),
                ("Status", request.Status.ToString()),
                ("Reason", request.Reason)
            },
            request.Status == DeleteRequestStatus.Pending
                ? "Approving removes the content. Rejecting keeps it unchanged."
                : null);
    }
}
