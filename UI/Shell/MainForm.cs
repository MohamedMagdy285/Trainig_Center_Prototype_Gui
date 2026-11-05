namespace ECMS;

/// <summary>
/// The main application window. It shows a role-aware sidebar, a top bar,
/// and hosts one page at a time inside the content area.
/// </summary>
public partial class MainForm : Form
{
    private readonly Panel _pageHost = new();
    private readonly Panel _topBar = new();
    private readonly Panel _content = new();
    private readonly Panel _sidebar = new();
    private readonly FlowLayoutPanel _nav = new();
    private readonly Label _workspaceTitle = new();
    private readonly Label _workspaceSubtitle = new();
    private readonly Label _toast = new();
    private readonly System.Windows.Forms.Timer _toastTimer = new();
    private readonly Dictionary<string, NavButton> _navButtons = new();
    private readonly Dictionary<string, string> _navParents = new();
    private readonly List<NavItem> _items;

    private string _current = "";

    private sealed record NavItem(string Key, string Label, string Glyph);

    public MainForm()
    {
        InitializeComponent();

        _items = ItemsForRole();
        _content.Dock = DockStyle.Fill;
        _content.BackColor = Theme.Bg;
        Controls.Add(_content);
        BuildSidebar();
        BuildPageHost();
        BuildTopBar();
        BuildToast();

        Navigator.Handler = ShowPage;
        Navigator.ToastHandler = ShowToast;
        FormClosed += (_, _) =>
        {
            Navigator.Handler = null;
            Navigator.ToastHandler = null;
        };

        Resize += (_, _) =>
        {
            if (WindowState == FormWindowState.Normal && Width > 0 && Height > 0)
                Region = new Region(Theme.Rounded(ClientRectangle, 14));
            else
                Region = null;
            PositionToast();
        };
        Region = new Region(Theme.Rounded(ClientRectangle, 14));

        ShowPage(_items[0].Key);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        MaximizedBounds = Screen.FromControl(this).WorkingArea;
        WindowState = FormWindowState.Maximized;
    }

    // ---------- shell layout ----------

    private List<NavItem> ItemsForRole()
    {
        if (Session.Role == UserRole.Admin)
        {
            return new List<NavItem>
            {
                new("dashboard", "Dashboard", "\uE80F"),
                new("students", "Students", "\uE716"),
                new("courses", "Courses", "\uE7BE"),
                new("batches", "Batches", "\uE787"),
                new("instructors", "Instructors", "\uE77B"),
                new("payments", "Payments & refunds", "\uE8C7"),
                new("exams", "Exams", "\uE8A5"),
                new("reviews", "Reviews", "\uE8BD"),
                new("reports", "Reports", "\uE9D2"),
                new("requests", "Delete requests", "\uE74D")
            };
        }

        if (Session.Role == UserRole.Student)
        {
            return new List<NavItem>
            {
                new("dashboard", "Dashboard", "\uE80F"),
                new("browse", "Browse courses", "\uE7BE"),
                new("learning", "My learning", "\uE82D"),
                new("exams", "Exams & certificates", "\uE8A5"),
                new("reviews", "My reviews", "\uE8BD"),
                new("profile", "My profile", "\uE77B")
            };
        }

        return new List<NavItem>
        {
            new("dashboard", "Dashboard", "\uE80F"),
            new("courses", "My courses", "\uE7BE"),
            new("requests", "Delete requests", "\uE74D"),
            new("reviews", "Reviews", "\uE8BD"),
            new("profile", "My profile", "\uE77B")
        };
    }

    private void BuildSidebar()
    {
        _sidebar.Dock = DockStyle.Left;
        _sidebar.Width = 258;
        _sidebar.BackColor = Theme.Primary;

        var sidebar = new SidebarPanel { Dock = DockStyle.Fill };
        _sidebar.Controls.Add(sidebar);

        var brand = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Color.Transparent };
        brand.MouseDown += (_, _) => Theme.Drag(this);
        var logo = new LogoMark { Size = new Size(44, 44), Location = new Point(22, 26) };
        var name = new Label
        {
            Text = "EduSphere", Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            ForeColor = Color.White, BackColor = Color.Transparent, AutoSize = true,
            Location = new Point(78, 28)
        };
        var academy = new Label
        {
            Text = "A C A D E M Y", Font = Theme.TinyBold, ForeColor = Theme.Accent,
            BackColor = Color.Transparent, AutoSize = true, Location = new Point(80, 52)
        };
        name.MouseDown += (_, _) => Theme.Drag(this);
        academy.MouseDown += (_, _) => Theme.Drag(this);
        brand.Controls.Add(logo);
        brand.Controls.Add(name);
        brand.Controls.Add(academy);

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 224, BackColor = Color.Transparent };

        if (Session.Role == UserRole.Admin)
        {
            var insight = new InsightCard { Location = new Point(20, 8), Size = new Size(218, 110) };
            bottom.Controls.Add(insight);
        }
        else
        {
            var card = new Panel { Location = new Point(20, 8), Size = new Size(218, 96), BackColor = Color.Transparent };
            card.Paint += (_, e) =>
            {
                var g = e.Graphics;
                Theme.SetupGraphics(g);
                var r = new RectangleF(.5f, .5f, card.Width - 1, card.Height - 1);
                Theme.Fill(g, Color.FromArgb(20, 255, 255, 255), r, 14);
                Theme.DrawBorder(g, Color.FromArgb(30, 255, 255, 255), r, 14);
                Theme.Text(g, "\uE946", Theme.Icon, Theme.Accent, new Rectangle(14, 14, 22, 22));
                Theme.Text(g, "Need a hand?", Theme.SmallBold, Color.White, new Rectangle(40, 13, 160, 20));
                Theme.Text(g, "Contact the academy team", Theme.Small, Color.FromArgb(190, 214, 220),
                    new Rectangle(14, 40, 190, 18));
                Theme.Text(g, "support@edusphere.com", Theme.Small, Color.FromArgb(190, 214, 220),
                    new Rectangle(14, 60, 190, 18));
            };
            bottom.Controls.Add(card);
        }

        var profile = new SidebarProfile { Location = new Point(0, 124), Size = new Size(258, 64) };
        var signOut = new NavButton
        {
            Glyph = "\uE7E8", LabelText = "Sign out",
            Size = new Size(226, 40), Location = new Point(14, 186)
        };
        signOut.Click += (_, _) => SignOut();
        bottom.Controls.Add(profile);
        bottom.Controls.Add(signOut);

        _nav.Dock = DockStyle.Fill;
        _nav.FlowDirection = FlowDirection.TopDown;
        _nav.WrapContents = false;
        _nav.AutoScroll = true;
        _nav.BackColor = Color.Transparent;
        _nav.Padding = new Padding(14, 10, 14, 10);
        _nav.Margin = Padding.Empty;

        foreach (var item in _items)
        {
            var button = new NavButton
            {
                Glyph = item.Glyph, LabelText = item.Label,
                Size = new Size(226, 44), Margin = new Padding(0, 0, 0, 4)
            };
            button.Click += (_, _) => ShowPage(item.Key);
            _navButtons[item.Key] = button;
            _nav.Controls.Add(button);
        }

        _navParents["course"] = "browse";
        _navParents["checkout"] = "browse";
        _navParents["player"] = "learning";
        _navParents["content"] = "courses";

        sidebar.Controls.Add(_nav);
        sidebar.Controls.Add(bottom);
        sidebar.Controls.Add(brand);
        Controls.Add(_sidebar);
    }

    private void BuildTopBar()
    {
        _topBar.Dock = DockStyle.Top;
        _topBar.Height = 76;
        _topBar.BackColor = Theme.Surface;
        _topBar.Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.Border);
            e.Graphics.DrawLine(pen, 0, _topBar.Height - 1, _topBar.Width, _topBar.Height - 1);
        };
        _topBar.MouseDown += (_, _) => Theme.Drag(this);

        _workspaceTitle.Text = Session.Role switch
        {
            UserRole.Admin => "Administration workspace",
            UserRole.Instructor => "Instructor workspace",
            _ => "Student workspace"
        };
        _workspaceTitle.Font = Theme.H2;
        _workspaceTitle.ForeColor = Theme.Ink;
        _workspaceTitle.BackColor = Theme.Surface;
        _workspaceTitle.AutoSize = true;
        _workspaceTitle.Location = new Point(28, 15);
        _workspaceTitle.MouseDown += (_, _) => Theme.Drag(this);

        _workspaceSubtitle.Text = "Signed in as " + Session.Name + "  •  " + Session.RoleText;
        _workspaceSubtitle.Font = Theme.Small;
        _workspaceSubtitle.ForeColor = Theme.Muted;
        _workspaceSubtitle.BackColor = Theme.Surface;
        _workspaceSubtitle.AutoSize = true;
        _workspaceSubtitle.Location = new Point(30, 41);
        _workspaceSubtitle.MouseDown += (_, _) => Theme.Drag(this);

        var date = new DateBadge { Size = new Size(158, 36) };
        var bell = new IconButton
        {
            Glyph = "\uEA8F", Size = new Size(38, 38),
            ShowBadge = Session.Role == UserRole.Admin
        };
        var chip = new AdminChip { Size = new Size(206, 44) };
        var minimize = new WindowButton { Kind = WindowButtonKind.Minimize, Size = new Size(30, 30) };
        var maximize = new WindowButton { Kind = WindowButtonKind.Maximize, Size = new Size(30, 30) };
        var close = new WindowButton { Kind = WindowButtonKind.Close, Size = new Size(30, 30) };

        _topBar.Controls.Add(_workspaceTitle);
        _topBar.Controls.Add(_workspaceSubtitle);
        _topBar.Controls.Add(date);
        _topBar.Controls.Add(bell);
        _topBar.Controls.Add(chip);
        _topBar.Controls.Add(minimize);
        _topBar.Controls.Add(maximize);
        _topBar.Controls.Add(close);

        void LayoutTopBar()
        {
            close.Location = new Point(_topBar.Width - 64, 23);
            maximize.Location = new Point(_topBar.Width - 96, 23);
            minimize.Location = new Point(_topBar.Width - 128, 23);
            chip.Location = new Point(_topBar.Width - 540, 16);
            bell.Location = new Point(_topBar.Width - 580, 19);
            date.Location = new Point(_topBar.Width - 760, 20);
        }
        _topBar.Resize += (_, _) => LayoutTopBar();
        LayoutTopBar();

        minimize.Click += (_, _) => WindowState = FormWindowState.Minimized;
        maximize.Click += (_, _) =>
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        close.Click += (_, _) => Close();

        _content.Controls.Add(_topBar);
    }

    private void BuildPageHost()
    {
        _pageHost.Dock = DockStyle.Fill;
        _pageHost.BackColor = Theme.Bg;
        _content.Controls.Add(_pageHost);
    }

    private void BuildToast()
    {
        _toast.Visible = false;
        _toast.AutoSize = false;
        _toast.Height = 44;
        _toast.TextAlign = ContentAlignment.MiddleCenter;
        _toast.Font = Theme.BodyBold;
        _toast.ForeColor = Color.White;
        _toast.BackColor = Theme.Primary;
        _toast.Padding = new Padding(18, 0, 18, 0);
        Controls.Add(_toast);

        _toastTimer.Interval = 3000;
        _toastTimer.Tick += (_, _) =>
        {
            _toastTimer.Stop();
            _toast.Visible = false;
        };
    }

    // ---------- navigation ----------

    private void ShowPage(string key, object? argument = null)
    {
        _current = key;
        var page = CreatePage(key, argument);
        while (_pageHost.Controls.Count > 0) _pageHost.Controls[0].Dispose();

        page.Dock = DockStyle.Fill;
        _pageHost.Controls.Add(page);

        var parent = _navParents.TryGetValue(key, out var p) ? p : key;
        foreach (var (navKey, button) in _navButtons)
        {
            button.Active = navKey == parent;
            button.Invalidate();
        }
    }

    private BasePage CreatePage(string key, object? argument)
    {
        if (Session.Role == UserRole.Admin)
        {
            return key switch
            {
                "students" => new AdminStudentsPage(),
                "courses" => new AdminCoursesPage(),
                "batches" => new AdminBatchesPage(),
                "instructors" => new AdminInstructorsPage(),
                "payments" => new AdminPaymentsPage(),
                "exams" => new AdminExamsPage(),
                "reviews" => new AdminReviewsPage(),
                "reports" => new AdminReportsPage(),
                "requests" => new AdminDeleteRequestsPage(),
                _ => new AdminDashboardPage()
            };
        }

        if (Session.Role == UserRole.Student)
        {
            return key switch
            {
                "browse" => new BrowseCoursesPage(),
                "course" when argument is int id => new CourseDetailsPage(id),
                "checkout" when argument is CheckoutRequest request => new CheckoutPage(request),
                "learning" => new MyLearningPage(),
                "player" when argument is int id => new LessonPlayerPage(id),
                "exams" => new StudentExamsPage(),
                "reviews" => new MyReviewsPage(),
                "profile" => new StudentProfilePage(),
                _ => new StudentDashboardPage()
            };
        }

        return key switch
        {
            "courses" => new InstructorCoursesPage(),
            "content" when argument is int id => new CourseContentPage(id),
            "requests" => new InstructorRequestsPage(),
            "reviews" => new InstructorReviewsPage(),
            "profile" => new InstructorProfilePage(),
            _ => new InstructorDashboardPage()
        };
    }

    private void SignOut()
    {
        if (!ConfirmDialog.Ask(this, "Sign out", "Do you want to sign out of EduSphere?", "Sign out"))
            return;
        Close();
    }

    // ---------- toast ----------

    private void ShowToast(string message, bool success)
    {
        var width = Math.Min(560, TextRenderer.MeasureText(message, Theme.BodyBold).Width + 52);
        _toast.Text = message;
        _toast.Width = Math.Max(240, width);
        _toast.BackColor = success ? Theme.Primary : Theme.Red;
        _toast.Visible = true;
        _toast.BringToFront();
        PositionToast();
        _toastTimer.Stop();
        _toastTimer.Start();
    }

    private void PositionToast()
    {
        if (!_toast.Visible) return;
        var left = _sidebar.Width + (_pageHost.Width - _toast.Width) / 2;
        _toast.Location = new Point(Math.Max(10, left), Height - 92);
    }
}
