using System.Drawing.Drawing2D;

namespace ECMS;

partial class LoginForm
{
    // ── Brand panel controls ────────────────────────────────────────────────
    private Panel _brandPanel = null!;
    private LogoMark _logo = null!;
    private Label _brandName = null!;
    private Label _brandAcademy = null!;
    private Label _tagline = null!;
    private Label _blurb = null!;
    private Label _bulletIcon1 = null!;
    private Label _bulletText1 = null!;
    private Label _bulletIcon2 = null!;
    private Label _bulletText2 = null!;
    private Label _bulletIcon3 = null!;
    private Label _bulletText3 = null!;
    private Label _footer = null!;

    // ── Form panel controls ─────────────────────────────────────────────────
    private Panel _formPanel = null!;
    private IconButton _closeButton = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Label _roleCaption = null!;
    private Chip _studentChip = null!;
    private Chip _instructorChip = null!;
    private Chip _adminChip = null!;
    private Label _emailCaption = null!;
    private Label _passwordCaption = null!;
    private PillButton _signInButton = null!;
    private Label _demoLabel = null!;
    private Label _demoText = null!;
    private Label _registerLink = null!;
    private Label _applyLink = null!;

    private void InitializeComponent()
    {
        SuspendLayout();

        // ── Form settings ──────────────────────────────────────────────────
        Text = "EduSphere Academy";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Surface;
        Size = new Size(980, 620);
        MinimumSize = Size;
        DoubleBuffered = true;
        KeyPreview = true;

        // ── Brand panel (left side) ────────────────────────────────────────
        _brandPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 392,
            BackColor = Theme.Primary
        };

        _logo = new LogoMark { Size = new Size(52, 52), Location = new Point(36, 42) };

        _brandName = new Label
        {
            Text = "EduSphere",
            Font = Theme.Title,
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(100, 46)
        };

        _brandAcademy = new Label
        {
            Text = "A C A D E M Y",
            Font = Theme.TinyBold,
            ForeColor = Theme.Accent,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(102, 76)
        };

        _tagline = new Label
        {
            Text = "Learn a skill.\r\nBuild a future.",
            Font = new Font("Segoe UI", 25f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = false,
            Size = new Size(320, 96),
            Location = new Point(36, 158)
        };

        _blurb = new Label
        {
            Text = "A calm, focused place for online courses,\r\noffline batches and recognised certificates.",
            Font = Theme.Body,
            ForeColor = Color.FromArgb(205, 226, 232),
            BackColor = Color.Transparent,
            AutoSize = false,
            Size = new Size(320, 52),
            Location = new Point(38, 268)
        };

        // Bullet points
        _bulletIcon1 = new Label
        {
            Text = "\uE7BE",
            Font = Theme.Icon,
            ForeColor = Theme.Accent,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(40, 348)
        };
        _bulletText1 = new Label
        {
            Text = "Online courses with lifetime access",
            Font = Theme.Small,
            ForeColor = Color.FromArgb(220, 236, 240),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(70, 351)
        };

        _bulletIcon2 = new Label
        {
            Text = "\uE716",
            Font = Theme.Icon,
            ForeColor = Theme.Accent,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(40, 390)
        };
        _bulletText2 = new Label
        {
            Text = "Small offline batches with real teachers",
            Font = Theme.Small,
            ForeColor = Color.FromArgb(220, 236, 240),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(70, 393)
        };

        _bulletIcon3 = new Label
        {
            Text = "\uE8A5",
            Font = Theme.Icon,
            ForeColor = Theme.Accent,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(40, 432)
        };
        _bulletText3 = new Label
        {
            Text = "Certificates after a passing final exam",
            Font = Theme.Small,
            ForeColor = Color.FromArgb(220, 236, 240),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(70, 435)
        };

        _footer = new Label
        {
            Text = "EduSphere Academy  •  Cairo, Egypt",
            Font = Theme.Small,
            ForeColor = Color.FromArgb(150, 184, 198),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(38, 548)
        };

        _brandPanel.Controls.AddRange(new Control[]
        {
            _logo, _brandName, _brandAcademy, _tagline, _blurb,
            _bulletIcon1, _bulletText1,
            _bulletIcon2, _bulletText2,
            _bulletIcon3, _bulletText3,
            _footer
        });

        // ── Form panel (right side) ────────────────────────────────────────
        _formPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Surface
        };

        _closeButton = new IconButton
        {
            Glyph = "\uE711",
            Size = new Size(38, 38),
            Location = new Point(534, 14),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        _titleLabel = new Label
        {
            Text = "Welcome back",
            Font = new Font("Segoe UI", 23f, FontStyle.Bold),
            ForeColor = Theme.Ink,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(56, 52)
        };

        _subtitleLabel = new Label
        {
            Text = "Sign in to continue to your EduSphere workspace.",
            Font = Theme.Body,
            ForeColor = Theme.Muted,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(58, 96)
        };

        _roleCaption = new Label
        {
            Text = "I AM SIGNING IN AS",
            Font = Theme.TinyBold,
            ForeColor = Theme.Muted,
            BackColor = Theme.Surface,
            AutoSize = true,
            Location = new Point(58, 140)
        };

        _studentChip = new Chip { Text = "Student", Size = new Size(116, 36), Location = new Point(58, 162) };
        _instructorChip = new Chip { Text = "Instructor", Size = new Size(116, 36), Location = new Point(182, 162) };
        _adminChip = new Chip { Text = "Admin", Size = new Size(116, 36), Location = new Point(306, 162) };

        _emailCaption = new Label
        {
            Text = "EMAIL OR USERNAME",
            Font = Theme.TinyBold,
            ForeColor = Theme.Muted,
            BackColor = Theme.Surface,
            AutoSize = true,
            Location = new Point(58, 224)
        };

        // _email FieldBox is defined in code-behind, positioned here
        _email.Location = new Point(58, 244);

        _passwordCaption = new Label
        {
            Text = "PASSWORD",
            Font = Theme.TinyBold,
            ForeColor = Theme.Muted,
            BackColor = Theme.Surface,
            AutoSize = true,
            Location = new Point(58, 304)
        };

        // _password FieldBox is defined in code-behind, positioned here
        _password.Location = new Point(58, 324);

        // _error and _success labels configured in code-behind
        _error.AutoSize = false;
        _error.Location = new Point(58, 378);
        _error.Size = new Size(376, 20);
        _error.Font = Theme.Small;
        _error.ForeColor = Theme.Red;
        _error.BackColor = Theme.Surface;

        _success.AutoSize = false;
        _success.Location = new Point(58, 378);
        _success.Size = new Size(376, 20);
        _success.Font = Theme.Small;
        _success.ForeColor = Theme.Green;
        _success.BackColor = Theme.Surface;

        _signInButton = new PillButton
        {
            Text = "Sign in",
            Filled = true,
            Accent = Theme.Primary,
            Size = new Size(376, 46),
            Location = new Point(58, 404)
        };

        _demoLabel = new Label
        {
            Text = "Demo accounts",
            Font = Theme.SmallBold,
            ForeColor = Theme.Muted,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(58, 468)
        };

        _demoText = new Label
        {
            Text = "Student: nour / 123456     Instructor: omar@edusphere.com / 123456     Admin: admin@edusphere.com / admin123",
            Font = Theme.Small,
            ForeColor = Theme.SoftText,
            BackColor = Color.Transparent,
            AutoSize = false,
            Size = new Size(460, 18),
            Location = new Point(58, 488)
        };

        _registerLink = new Label
        {
            Text = "Create a student account",
            Font = Theme.SmallBold,
            ForeColor = Theme.Accent,
            BackColor = Theme.Surface,
            AutoSize = true,
            Location = new Point(58, 536),
            Cursor = Cursors.Hand
        };

        _applyLink = new Label
        {
            Text = "Apply as an instructor",
            Font = Theme.SmallBold,
            ForeColor = Theme.Accent,
            BackColor = Theme.Surface,
            AutoSize = true,
            Location = new Point(248, 536),
            Cursor = Cursors.Hand
        };

        _formPanel.Controls.AddRange(new Control[]
        {
            _closeButton, _titleLabel, _subtitleLabel, _roleCaption,
            _studentChip, _instructorChip, _adminChip,
            _emailCaption, _email,
            _passwordCaption, _password,
            _error, _success,
            _signInButton,
            _demoLabel, _demoText,
            _registerLink, _applyLink
        });

        // ── Add panels to form ─────────────────────────────────────────────
        Controls.Add(_formPanel);
        Controls.Add(_brandPanel);

        ResumeLayout(false);
        PerformLayout();
    }
}

