using System.Drawing.Drawing2D;

namespace ECMS;

/// <summary>
/// The first window: choose a role, sign in, register as a student,
/// or apply to become an instructor.
/// </summary>
public partial class LoginForm : Form
{
    private readonly FieldBox _email = new("Email or username", null) { Size = new Size(376, 46) };
    private readonly FieldBox _password = new("Password", null, true) { Size = new Size(376, 46) };
    private readonly Label _error = new();
    private readonly Label _success = new();
    private readonly Dictionary<UserRole, Chip> _chips = new();
    private UserRole _role = UserRole.Student;

    public LoginForm()
    {
        InitializeComponent();
        WireEvents();
        SelectRole(UserRole.Student);
    }

    private void WireEvents()
    {
        // Brand panel paint (gradient + glow + decorative circles)
        _brandPanel.Paint += (_, e) =>
        {
            var g = e.Graphics;
            Theme.SetupGraphics(g);
            using (var brush = new LinearGradientBrush(_brandPanel.ClientRectangle,
                       Theme.Primary, Theme.Secondary, 115f))
                g.FillRectangle(brush, _brandPanel.ClientRectangle);
            using (var glow = new SolidBrush(Color.FromArgb(34, Theme.Accent)))
            {
                g.FillEllipse(glow, _brandPanel.Width - 190, -120, 360, 360);
                g.FillEllipse(glow, -160, _brandPanel.Height - 220, 330, 330);
            }
            using var pen = new Pen(Color.FromArgb(32, 255, 255, 255));
            for (var i = 0; i < 5; i++)
                g.DrawEllipse(pen, _brandPanel.Width - 120 - i * 34, 88 + i * 26, 190 + i * 38, 190 + i * 38);
        };
        _brandPanel.MouseDown += (_, _) => Theme.Drag(this);

        // Form panel drag & resize
        _formPanel.MouseDown += (_, _) => Theme.Drag(this);
        _formPanel.Resize += (_, _) => _closeButton.Left = _formPanel.Width - 54;

        // Close button
        _closeButton.Click += (_, _) => Close();

        // Role chips
        _chips[UserRole.Student] = _studentChip;
        _chips[UserRole.Instructor] = _instructorChip;
        _chips[UserRole.Admin] = _adminChip;

        _studentChip.Click += (_, _) => SelectRole(UserRole.Student);
        _instructorChip.Click += (_, _) => SelectRole(UserRole.Instructor);
        _adminChip.Click += (_, _) => SelectRole(UserRole.Admin);

        // Sign in button & Enter key
        _signInButton.Click += (_, _) => SignIn();
        KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) SignIn(); };

        // Register / Apply links
        _registerLink.Click += (_, _) => RegisterStudent();
        _applyLink.Click += (_, _) => ApplyInstructor();

        // Rounded region
        Resize += (_, _) =>
        {
            if (Width > 0 && Height > 0)
                Region = new Region(Theme.Rounded(ClientRectangle, 18));
        };
        Region = new Region(Theme.Rounded(ClientRectangle, 18));
    }

    private void SelectRole(UserRole role)
    {
        _role = role;
        foreach (var (chipRole, chip) in _chips)
            chip.Selected = chipRole == role;

        _error.Text = "";
        _success.Text = "";
        switch (role)
        {
            case UserRole.Admin:
                _email.Value = AppData.AdminEmail;
                _password.Value = AppData.AdminPassword;
                break;
            case UserRole.Instructor:
                _email.Value = "omar@edusphere.com";
                _password.Value = "123456";
                break;
            default:
                _email.Value = "nour";
                _password.Value = "123456";
                break;
        }
    }

    private void SignIn()
    {
        _error.Text = "";
        _success.Text = "";

        if (_email.Value.Trim().Length == 0 || _password.Value.Trim().Length == 0)
        {
            _error.Text = "Please enter your credentials.";
            return;
        }

        var (ok, message) = AuthService.Login(_role, _email.Value, _password.Value);
        if (!ok)
        {
            _error.Text = message;
            return;
        }

        Hide();
        using (var shell = new MainForm())
            shell.ShowDialog(this);

        Session.SignOut();
        _error.Text = "";
        _success.Text = "";
        Show();
        _email.FocusInput();
    }

    private void RegisterStudent()
    {
        var fields = new[]
        {
            FormField.Text("username", "Username", "", "e.g. nour"),
            FormField.Text("name", "Full name", "", "Your full name"),
            FormField.Text("email", "Email", "", "you@example.com"),
            FormField.Text("phone", "Phone", "", "+20 100 000 0000"),
            FormField.Password("password", "Password"),
            FormField.Password("confirm", "Confirm password")
        };
        using var dialog = new FormDialog("Create student account",
            "Register to buy courses and track your learning.", fields, values =>
            {
                if (values["password"] != values["confirm"])
                    return "The two passwords do not match.";
                return null;
            });

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var (student, error) = AuthService.RegisterStudent(
            dialog.Values["username"], dialog.Values["name"], dialog.Values["email"],
            dialog.Values["phone"], dialog.Values["password"]);

        if (student is null)
        {
            _error.Text = error;
            return;
        }

        SelectRole(UserRole.Student);
        _email.Value = student.Email;
        _password.Value = student.Password;
        _success.Text = "Account created. You can sign in now.";
    }

    private void ApplyInstructor()
    {
        var fields = new[]
        {
            FormField.Text("name", "Full name", "", "Your full name"),
            FormField.Text("email", "Email", "", "you@example.com"),
            FormField.Text("phone", "Phone", "", "+20 100 000 0000"),
            FormField.Password("password", "Password"),
            FormField.Text("specialization", "Specialization", "", "e.g. Data Science"),
            FormField.Text("cv", "CV file name", "", "my-cv.pdf"),
            FormField.Choice("preference", "Teaching preference", "Both", "Online", "Offline", "Both")
        };
        using var dialog = new FormDialog("Apply as an instructor",
            "Your application will be reviewed by the academy administrator.", fields);

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var preference = dialog.Values["preference"] switch
        {
            "Online" => TeachingPreference.Online,
            "Offline" => TeachingPreference.Offline,
            _ => TeachingPreference.Both
        };

        var (instructor, error) = AuthService.ApplyInstructor(
            dialog.Values["name"], dialog.Values["email"], dialog.Values["phone"],
            dialog.Values["password"], dialog.Values["specialization"], dialog.Values["cv"], preference);

        if (instructor is null)
        {
            _error.Text = error;
            return;
        }

        _error.Text = "";
        _success.Text = "Application submitted. The admin will review it soon.";
    }
}
