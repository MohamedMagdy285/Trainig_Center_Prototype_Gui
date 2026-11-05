namespace ECMS;

/// <summary>Admin review of instructor applications and accounts.</summary>
public partial class AdminInstructorsPage : BasePage
{
    private readonly TableControl _pendingTable;
    private readonly Card _pendingCard;
    private List<Instructor> _pending = new();

    public AdminInstructorsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        AddBlock(Header("Instructors", "Approve applications and manage teaching accounts."), 60);

        StatRow(
            Stat("Instructors", AppData.Instructors.Count.ToString("N0"), "All applications", "\uE77B", Theme.Primary),
            Stat("Active", AppData.Instructors.Count(i => i.Status == AccountStatus.Active).ToString("N0"),
                "Teaching now", "\uE73E", Theme.Accent),
            Stat("Pending", AppData.Instructors.Count(i => i.Status == AccountStatus.Pending).ToString("N0"),
                "Waiting for review", "\uE823", Theme.Amber),
            Stat("Deactivated", AppData.Instructors.Count(i => i.Status == AccountStatus.Deactivated).ToString("N0"),
                "Historical records kept", "\uE7E8", Theme.Red));

        _pending = AppData.Instructors
            .Where(i => i.Status == AccountStatus.Pending)
            .OrderBy(i => i.AppliedOn)
            .ToList();
        _pendingTable = MakeTable(
            new[] { "Applicant", "Specialization", "Preference", "Applied", "Status" },
            new[] { 2.2f, 2.2f, 1.4f, 1.3f, 1.2f },
            avatar: true, actions: true, pillColumns: 4);
        _pendingTable.ActionClicked += (_, e) => HandlePending(e.RowIndex, e.Action);
        _pendingTable.RowClicked += (_, index) => ShowDetails(_pending[index]);
        _pendingCard = AddTableCard("Pending applications",
            "Approve with the edit icon, reject with the delete icon", _pendingTable,
            emptyText: "No applications are waiting for review.");
        ReloadPending();

        AddBlock(Header("Instructor accounts", "Click a card to edit or change the account status."), 54);

        var cards = AppData.Instructors
            .OrderBy(i => i.Status)
            .ThenBy(i => i.FullName)
            .Select(i =>
            {
                var card = new InstructorCard(i);
                card.OpenRequested += (_, _) => Manage(i);
                card.EditRequested += (_, _) => Manage(i);
                return card;
            })
            .ToArray();

        var rows = Math.Max(1, (int)Math.Ceiling(cards.Length / 3.0));
        var height = rows * 192 + (rows - 1) * 14;
        var grid = Grid(3, rows, cards, height);
        AddBlock(grid, height);
    }

    private void ReloadPending()
    {
        _pending = AppData.Instructors
            .Where(i => i.Status == AccountStatus.Pending)
            .OrderBy(i => i.AppliedOn)
            .ToList();
        _pendingTable.SetRows(_pending.Select(i => new[]
        {
            i.FullName,
            i.Specialization,
            i.Preference.ToString(),
            Fmt.Date(i.AppliedOn),
            i.StatusText
        }));
        FitTableCard(_pendingCard, _pendingTable);
        RefreshLayout();
    }

    private void HandlePending(int index, TableRowAction action)
    {
        if (index < 0 || index >= _pending.Count) return;
        var instructor = _pending[index];

        if (action == TableRowAction.Edit)
        {
            if (!ConfirmDialog.Ask(this, "Approve instructor",
                    "Approve " + instructor.FullName + "? They will be able to sign in and teach.",
                    "Approve"))
                return;
            InstructorService.Approve(instructor);
            Navigator.Toast("Instructor approved.");
            Navigator.Go("instructors");
            return;
        }

        using var dialog = new FormDialog("Reject application",
            "Explain why " + instructor.FullName + " was not approved.", new[]
            {
                FormField.Multiline("note", "Admin note", "", "Reason for the decision")
            });
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        InstructorService.Reject(instructor, dialog.Values["note"]);
        Navigator.Toast("Application rejected.");
        Navigator.Go("instructors");
    }

    private void Manage(Instructor instructor)
    {
        var fields = new[]
        {
            FormField.Text("name", "Full name", instructor.FullName),
            FormField.Text("email", "Email", instructor.Email),
            FormField.Text("phone", "Phone", instructor.Phone),
            FormField.Text("specialization", "Specialization", instructor.Specialization),
            FormField.Choice("preference", "Teaching preference", instructor.Preference.ToString(),
                "Online", "Offline", "Both"),
            FormField.Choice("status", "Account status", instructor.StatusText,
                "Pending", "Active", "Rejected", "Deactivated"),
            FormField.Multiline("note", "Admin note", instructor.AdminNote)
        };
        using var dialog = new FormDialog("Instructor account",
            "Update the profile or change the account status.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        instructor.FullName = dialog.Values["name"];
        instructor.Email = dialog.Values["email"];
        instructor.Phone = dialog.Values["phone"];
        instructor.Specialization = dialog.Values["specialization"];
        instructor.Preference = dialog.Values["preference"] switch
        {
            "Online" => TeachingPreference.Online,
            "Offline" => TeachingPreference.Offline,
            _ => TeachingPreference.Both
        };

        var note = dialog.Values["note"];
        switch (dialog.Values["status"])
        {
            case "Active":
                if (instructor.Status == AccountStatus.Deactivated)
                    InstructorService.Reactivate(instructor);
                else
                    InstructorService.Approve(instructor);
                if (note.Length > 0) instructor.AdminNote = note;
                break;
            case "Rejected":
                InstructorService.Reject(instructor, note);
                break;
            case "Deactivated":
                InstructorService.Deactivate(instructor);
                if (note.Length > 0) instructor.AdminNote = note;
                break;
            default:
                instructor.Status = AccountStatus.Pending;
                instructor.AdminNote = note;
                break;
        }

        Navigator.Toast("Instructor account updated.");
        Navigator.Go("instructors");
    }

    private void ShowDetails(Instructor instructor)
    {
        DetailsDialog.Show(this, instructor.FullName, "Instructor application",
            new[]
            {
                ("Email", instructor.Email),
                ("Phone", instructor.Phone),
                ("Specialization", instructor.Specialization),
                ("Preference", instructor.Preference.ToString()),
                ("CV", instructor.CvFileName),
                ("Applied", Fmt.Date(instructor.AppliedOn)),
                ("Status", instructor.StatusText),
                ("Admin note", instructor.AdminNote.Length == 0 ? "No note yet" : instructor.AdminNote)
            });
    }
}
