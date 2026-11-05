namespace ECMS;

/// <summary>Instructor profile, teaching details and password.</summary>
public partial class InstructorProfilePage : BasePage
{
    public InstructorProfilePage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var instructor = Session.Instructor!;
        var courses = CatalogService.CoursesForInstructor(instructor.Id).ToList();
        var reviews = ReviewService.ReviewsForInstructor(instructor.Id).ToList();

        AddBlock(Header("My profile", "Your teaching profile, account status and password.",
            "Edit profile", "\uE70F", EditProfile), 60);

        StatRow(
            Stat("My courses", courses.Count.ToString("N0"), "Assigned to you", "\uE7BE", Theme.Primary),
            Stat("Students", CatalogService.StudentCountForInstructor(instructor.Id).ToString("N0"),
                "Across all courses", "\uE716", Theme.Accent),
            Stat("Reviews", reviews.Count.ToString("N0"), "Student comments", "\uE8BD", Theme.Secondary),
            Stat("Account", instructor.StatusText, "Current status", "\uE73E", Theme.Amber));

        AddBlock(Split(BuildProfileCard(instructor), BuildAccountCard(instructor), 420, 58f), 420);

        var table = MakeTable(
            new[] { "Course", "Format", "Students", "Lessons", "Status" },
            new[] { 3f, 1.1f, 1f, 0.9f, 1.3f },
            pillColumns: new[] { 1, 4 });
        table.SetRows(courses.Select(c => new[]
        {
            c.Title,
            c.Kind.ToString(),
            CatalogService.StudentCount(c.Id).ToString(),
            c.TotalLessons.ToString(),
            Status(c)
        }));
        table.RowClicked += (_, index) => Navigator.Go("content", courses[index].Id);
        AddTableCard("My courses", "Click a course to manage its content", table,
            emptyText: "No courses are assigned to you yet.");
    }

    private static Card BuildProfileCard(Instructor instructor)
    {
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "Profile", "How students see you");

        var avatar = ProfileAvatar(instructor.FullName, 96);
        avatar.Location = new Point(26, 76);
        card.Controls.Add(avatar);

        var name = new Label
        {
            Text = instructor.FullName, Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Theme.Ink, BackColor = Color.White, AutoSize = false,
            Location = new Point(142, 80), Size = new Size(300, 30)
        };
        var specialization = new Label
        {
            Text = instructor.Specialization, Font = Theme.Body, ForeColor = Theme.Muted,
            BackColor = Color.White, AutoSize = false, Location = new Point(142, 114),
            Size = new Size(300, 22)
        };
        var status = new Label
        {
            Text = instructor.StatusText + " instructor", Font = Theme.SmallBold,
            ForeColor = instructor.Status == AccountStatus.Active ? Theme.Green : Theme.Amber,
            BackColor = Color.White, AutoSize = true, Location = new Point(142, 140)
        };
        card.Controls.Add(name);
        card.Controls.Add(specialization);
        card.Controls.Add(status);

        AddInfo(card, "Email", instructor.Email, 26, 192, 260);
        AddInfo(card, "Phone", instructor.Phone, 26, 242, 260);
        AddInfo(card, "Teaching preference", instructor.Preference.ToString(), 26, 292, 260);

        AddInfo(card, "CV file", instructor.CvFileName, 320, 192, 230);
        AddInfo(card, "Applied", Fmt.Date(instructor.AppliedOn), 320, 242, 230);

        var note = new Label
        {
            Text = instructor.AdminNote.Length == 0 ? "No admin note." : instructor.AdminNote,
            Font = Theme.Small, ForeColor = Theme.Secondary, BackColor = Theme.Field,
            AutoSize = false, Location = new Point(26, 342), Size = new Size(500, 52),
            Padding = new Padding(12, 10, 12, 10)
        };
        card.Controls.Add(note);
        card.Resize += (_, _) => note.Width = Math.Max(180, card.Width - 52);
        return card;
    }

    private Card BuildAccountCard(Instructor instructor)
    {
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "Account & security", "Sign-in details and teaching status");

        AddInfo(card, "Account status", instructor.StatusText, 24, 78, 240);
        AddInfo(card, "Courses", CatalogService.CoursesForInstructor(instructor.Id).Count().ToString(), 24, 128, 240);
        AddInfo(card, "Students", CatalogService.StudentCountForInstructor(instructor.Id).ToString(), 24, 178, 240);
        AddInfo(card, "Reviews", ReviewService.ReviewsForInstructor(instructor.Id).Count().ToString(), 24, 228, 240);

        var change = Button("Change password", ChangePassword, false, "\uE72E");
        change.Location = new Point(24, 284);
        change.Size = new Size(190, 40);
        card.Controls.Add(change);

        var hint = new Label
        {
            Text = "Contact the admin to change your teaching status or availability.",
            Font = Theme.Small, ForeColor = Theme.SoftText, BackColor = Color.White,
            AutoSize = false, Location = new Point(24, 338), Size = new Size(250, 48)
        };
        card.Controls.Add(hint);
        return card;
    }

    private static string Status(Course course)
    {
        if (!course.IsPublished) return "Draft";
        return course.AcceptsNewEnrollments ? "Published" : "Closed";
    }

    private void EditProfile()
    {
        var instructor = Session.Instructor!;
        var fields = new[]
        {
            FormField.Text("name", "Full name", instructor.FullName),
            FormField.Text("email", "Email", instructor.Email),
            FormField.Text("phone", "Phone", instructor.Phone),
            FormField.Text("specialization", "Specialization", instructor.Specialization),
            FormField.Choice("preference", "Teaching preference", instructor.Preference.ToString(),
                "Online", "Offline", "Both"),
            FormField.Text("cv", "CV file name", instructor.CvFileName)
        };
        using var dialog = new FormDialog("Edit profile", "Update your teaching profile.", fields,
            values =>
            {
                if (values["name"].Length < 3) return "Please enter your full name.";
                if (!values["email"].Contains('@') || !values["email"].Contains('.'))
                    return "Please enter a valid email address.";
                if (values["phone"].Length < 8) return "Please enter a valid phone number.";
                if (values["specialization"].Length < 2) return "Please enter your specialization.";
                if (values["cv"].Length < 2) return "Please enter the CV file name.";
                if (AppData.Instructors.Any(i => i.Id != instructor.Id &&
                                                 i.Email.Equals(values["email"], StringComparison.OrdinalIgnoreCase)))
                    return "That email is already used by another instructor.";
                return null;
            });
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        instructor.FullName = dialog.Values["name"];
        instructor.Email = dialog.Values["email"];
        instructor.Phone = dialog.Values["phone"];
        instructor.Specialization = dialog.Values["specialization"];
        instructor.CvFileName = dialog.Values["cv"];
        instructor.Preference = dialog.Values["preference"] switch
        {
            "Online" => TeachingPreference.Online,
            "Offline" => TeachingPreference.Offline,
            _ => TeachingPreference.Both
        };
        Navigator.Toast("Profile updated.");
        Navigator.Go("profile");
    }

    private void ChangePassword()
    {
        var instructor = Session.Instructor!;
        var fields = new[]
        {
            FormField.Password("current", "Current password"),
            FormField.Password("next", "New password"),
            FormField.Password("confirm", "Confirm new password")
        };
        using var dialog = new FormDialog("Change password", "Choose a new password for your account.", fields,
            values =>
            {
                if (values["current"] != instructor.Password) return "Your current password is not correct.";
                if (values["next"].Length < 6) return "The new password must be at least 6 characters.";
                if (values["next"] != values["confirm"]) return "The two new passwords do not match.";
                return null;
            });
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        instructor.Password = dialog.Values["next"];
        Navigator.Toast("Password updated.");
    }
}
