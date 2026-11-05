namespace ECMS;

/// <summary>Student account details, password and store credit.</summary>
public partial class StudentProfilePage : BasePage
{
    public StudentProfilePage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var student = Session.Student!;
        var courses = CatalogService.EnrollmentsForStudent(student.Id)
            .Select(e => e.CourseId).Distinct().Count();
        var certificates = AppData.Certificates.Count(c => c.StudentId == student.Id);

        AddBlock(Header("My profile", "Your account details, password and store credit.",
            "Edit profile", "\uE70F", EditProfile), 60);

        StatRow(
            Stat("Courses owned", courses.ToString("N0"), "Confirmed enrollments", "\uE7BE", Theme.Primary),
            Stat("Certificates", certificates.ToString("N0"), "Passed final exams", "\uE8D7", Theme.Accent),
            Stat("Credit balance", Fmt.Money(RefundService.CreditBalance(student.Id)),
                "Available to spend", "\uE8C7", Theme.Amber),
            Stat("Member since", student.JoinedOn.ToString("MMM yyyy"), "Joined EduSphere", "\uE823", Theme.Secondary));

        AddBlock(Split(BuildProfileCard(student), BuildAccountCard(student), 350, 58f), 350);
        BuildCreditTable(student);
    }

    private static Card BuildProfileCard(Student student)
    {
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "Profile", "How you appear across EduSphere");

        var avatar = ProfileAvatar(student.DisplayName, 96);
        avatar.Location = new Point(26, 76);
        card.Controls.Add(avatar);

        var name = new Label
        {
            Text = student.DisplayName, Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Theme.Ink, BackColor = Color.White, AutoSize = false,
            Location = new Point(142, 80), Size = new Size(300, 30)
        };
        var username = new Label
        {
            Text = "@" + student.Username, Font = Theme.Body, ForeColor = Theme.Muted,
            BackColor = Color.White, AutoSize = false, Location = new Point(142, 114),
            Size = new Size(300, 22)
        };
        var status = new Label
        {
            Text = student.IsActive ? "Active account" : "Inactive account",
            Font = Theme.SmallBold, ForeColor = student.IsActive ? Theme.Green : Theme.Red,
            BackColor = Color.White, AutoSize = true, Location = new Point(142, 140)
        };
        card.Controls.Add(name);
        card.Controls.Add(username);
        card.Controls.Add(status);

        AddInfo(card, "Email", student.Email, 26, 192, 260);
        AddInfo(card, "Phone", student.Phone, 26, 242, 260);
        AddInfo(card, "Joined", Fmt.Date(student.JoinedOn), 26, 292, 260);
        return card;
    }

    private Card BuildAccountCard(Student student)
    {
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "Account & security", "Sign-in details and credit");

        AddInfo(card, "Username", student.Username, 24, 78, 240);
        AddInfo(card, "Password", "â€¢â€¢â€¢â€¢â€¢â€¢â€¢â€¢", 24, 128, 240);
        AddInfo(card, "Account status", student.IsActive ? "Active" : "Inactive", 24, 178, 240);
        AddInfo(card, "Credit balance", Fmt.Money(RefundService.CreditBalance(student.Id)), 24, 228, 240);

        var change = Button("Change password", ChangePassword, false, "\uE72E");
        change.Location = new Point(24, 284);
        change.Size = new Size(190, 40);
        card.Controls.Add(change);
        return card;
    }

    private void BuildCreditTable(Student student)
    {
        var credits = AppData.Credits
            .Where(c => c.StudentId == student.Id)
            .OrderByDescending(c => c.CreatedOn)
            .ToList();

        var table = MakeTable(
            new[] { "Source", "Amount", "Remaining", "Created" },
            new[] { 2f, 1.4f, 1.4f, 1.4f });
        table.SetRows(credits.Select(c => new[]
        {
            "Payment #" + c.SourcePaymentId,
            Fmt.Money(c.Amount),
            Fmt.Money(c.Remaining),
            Fmt.Date(c.CreatedOn)
        }));
        table.RowClicked += (_, index) =>
        {
            var credit = credits[index];
            DetailsDialog.Show(this, "Store credit", "Credit from a payment",
                new[]
                {
                    ("Source payment", "#" + credit.SourcePaymentId),
                    ("Original amount", Fmt.Money(credit.Amount)),
                    ("Remaining", Fmt.Money(credit.Remaining)),
                    ("Created", Fmt.Date(credit.CreatedOn))
                },
                "Credit can be used at checkout instead of a card.");
        };
        AddTableCard("Store credit", "Credit comes from duplicate payments or refunds chosen as credit", table,
            emptyText: "You do not have any store credit yet.");
    }

    private void EditProfile()
    {
        var student = Session.Student!;
        var fields = new[]
        {
            FormField.Text("name", "Full name", student.FullName),
            FormField.Text("username", "Username", student.Username),
            FormField.Text("email", "Email", student.Email),
            FormField.Text("phone", "Phone", student.Phone)
        };
        using var dialog = new FormDialog("Edit profile", "Update your personal details.", fields,
            values =>
            {
                var email = values["email"];
                var phone = values["phone"];
                if (values["name"].Length < 3) return "Please enter your full name.";
                if (values["username"].Length < 3) return "The username must be at least 3 characters.";
                if (!email.Contains('@') || !email.Contains('.')) return "Please enter a valid email address.";
                if (phone.Length < 8) return "Please enter a valid phone number.";
                if (AppData.Students.Any(s => s.Id != student.Id &&
                                              s.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                    return "That email is already used by another student.";
                if (AppData.Students.Any(s => s.Id != student.Id &&
                                              s.Username.Equals(values["username"], StringComparison.OrdinalIgnoreCase)))
                    return "That username is already taken.";
                if (AppData.Students.Any(s => s.Id != student.Id && s.Phone == phone))
                    return "That phone number is already used by another student.";
                return null;
            });
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        student.FullName = dialog.Values["name"];
        student.Username = dialog.Values["username"];
        student.Email = dialog.Values["email"];
        student.Phone = dialog.Values["phone"];
        Navigator.Toast("Profile updated.");
        Navigator.Go("profile");
    }

    private void ChangePassword()
    {
        var student = Session.Student!;
        var fields = new[]
        {
            FormField.Password("current", "Current password"),
            FormField.Password("next", "New password"),
            FormField.Password("confirm", "Confirm new password")
        };
        using var dialog = new FormDialog("Change password", "Choose a new password for your account.", fields,
            values =>
            {
                if (values["current"] != student.Password) return "Your current password is not correct.";
                if (values["next"].Length < 6) return "The new password must be at least 6 characters.";
                if (values["next"] != values["confirm"]) return "The two new passwords do not match.";
                return null;
            });
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        student.Password = dialog.Values["next"];
        Navigator.Toast("Password updated.");
    }
}
