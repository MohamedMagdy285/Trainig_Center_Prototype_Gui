namespace ECMS;

/// <summary>Admin list of every registered student.</summary>
public partial class AdminStudentsPage : BasePage
{
    private readonly TableControl _table;
    private readonly Card _card;
    private readonly SearchBox _search = new("Search by name, email or phone...");
    private List<Student> _shown = new();

    public AdminStudentsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        AddBlock(Header("Students", "Every learner registered on EduSphere.",
            "Add student", "\uE710", AddStudent), 60);

        var active = AppData.Students.Count(s => s.IsActive);
        var newThisMonth = AppData.Students.Count(s =>
            s.JoinedOn.Year == DateTime.Today.Year && s.JoinedOn.Month == DateTime.Today.Month);
        var enrolled = AppData.Students.Count(s => CatalogService.EnrollmentsForStudent(s.Id).Any());

        StatRow(
            Stat("Total students", AppData.Students.Count.ToString("N0"), "All accounts", "\uE716", Theme.Primary),
            Stat("Active", active.ToString("N0"), "Can sign in", "\uE73E", Theme.Accent),
            Stat("New this month", newThisMonth.ToString("N0"), "Recent registrations", "\uE710", Theme.Secondary),
            Stat("With a course", enrolled.ToString("N0"), "At least one enrollment", "\uE7BE", Theme.Amber));

        var searchPanel = new Panel { BackColor = Theme.Bg };
        _search.Location = new Point(0, 0);
        _search.Size = new Size(360, 42);
        _search.ValueChanged += (_, _) => Reload();
        searchPanel.Controls.Add(_search);
        AddBlock(searchPanel, 44);

        _table = MakeTable(
            new[] { "Student", "Email", "Phone", "Joined", "Courses", "Status" },
            new[] { 2.2f, 2.6f, 2f, 1.4f, 1f, 1.2f },
            avatar: true, actions: true, pillColumns: 5);
        _table.ActionClicked += (_, e) => HandleAction(e.RowIndex, e.Action);
        _table.RowClicked += (_, index) => ShowDetails(_shown[index]);
        _card = AddTableCard("All students", "Click a row to see the full profile", _table,
            emptyText: "No students match your search.");
        Reload();
    }

    private void Reload()
    {
        var query = _search.Value.Trim();
        _shown = AppData.Students
            .Where(s => query.Length == 0 ||
                        s.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        s.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        s.Phone.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.JoinedOn)
            .ToList();

        _table.SetRows(_shown.Select(s => new[]
        {
            s.DisplayName,
            s.Email,
            s.Phone,
            Fmt.Date(s.JoinedOn),
            CatalogService.EnrollmentsForStudent(s.Id).Count().ToString(),
            s.IsActive ? "Active" : "Inactive"
        }));
        FitTableCard(_card, _table);
        RefreshLayout();
    }

    private void AddStudent()
    {
        var fields = new[]
        {
            FormField.Text("username", "Username", "", "e.g. sara"),
            FormField.Text("name", "Full name", "", "Student full name"),
            FormField.Text("email", "Email", "", "student@example.com"),
            FormField.Text("phone", "Phone", "", "+20 100 000 0000"),
            FormField.Password("password", "Password", "123456")
        };
        using var dialog = new FormDialog("Add student", "Create a student account for the academy.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var (student, error) = AuthService.RegisterStudent(
            dialog.Values["username"], dialog.Values["name"], dialog.Values["email"],
            dialog.Values["phone"], dialog.Values["password"]);

        if (student is null)
        {
            Navigator.Toast(error, false);
            return;
        }

        Navigator.Toast("Student account created.");
        Reload();
    }

    private void EditStudent(Student student)
    {
        var fields = new[]
        {
            FormField.Text("name", "Full name", student.FullName),
            FormField.Text("email", "Email", student.Email),
            FormField.Text("phone", "Phone", student.Phone),
            FormField.Text("username", "Username", student.Username)
        };
        using var dialog = new FormDialog("Edit student", "Update the student profile.", fields);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var email = dialog.Values["email"];
        var phone = dialog.Values["phone"];
        if (AppData.Students.Any(s => s.Id != student.Id && s.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            Navigator.Toast("That email is already used by another student.", false);
            return;
        }
        if (AppData.Students.Any(s => s.Id != student.Id && s.Phone == phone))
        {
            Navigator.Toast("That phone number is already used by another student.", false);
            return;
        }

        student.FullName = dialog.Values["name"];
        student.Email = email;
        student.Phone = phone;
        student.Username = dialog.Values["username"];
        Navigator.Toast("Student profile updated.");
        Reload();
    }

    private void HandleAction(int index, TableRowAction action)
    {
        if (index < 0 || index >= _shown.Count) return;
        var student = _shown[index];
        if (action == TableRowAction.Edit)
        {
            EditStudent(student);
            return;
        }

        var activate = !student.IsActive;
        var message = activate
            ? "Reactivate " + student.DisplayName + "? They will be able to sign in again."
            : "Deactivate " + student.DisplayName + "? They will no longer be able to sign in.";
        if (!ConfirmDialog.Ask(this, activate ? "Reactivate student" : "Deactivate student",
                message, activate ? "Reactivate" : "Deactivate"))
            return;

        student.IsActive = activate;
        Navigator.Toast(activate ? "Student reactivated." : "Student deactivated.");
        Reload();
    }

    private void ShowDetails(Student student)
    {
        var enrollments = CatalogService.EnrollmentsForStudent(student.Id).ToList();
        DetailsDialog.Show(this, student.DisplayName, "Student profile and activity",
            new[]
            {
                ("Username", student.Username),
                ("Email", student.Email),
                ("Phone", student.Phone),
                ("Joined", Fmt.Date(student.JoinedOn)),
                ("Status", student.IsActive ? "Active" : "Inactive"),
                ("Enrollments", enrollments.Count.ToString()),
                ("Courses", enrollments.Count == 0
                    ? "No courses yet"
                    : string.Join(", ", enrollments.Select(e =>
                        CatalogService.Course(e.CourseId)?.Title ?? "Course")))
            });
    }
}
