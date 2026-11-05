namespace ECMS;

/// <summary>Admin landing page with the main numbers, charts and recent activity.</summary>
public partial class AdminDashboardPage : BasePage
{
    public AdminDashboardPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        Build();
    }

    private void Build()
    {
        var stats = ReportService.Dashboard();

        AddBlock(Header("Dashboard",
            "Welcome back, " + Session.Name + ". Here is what is happening at the academy.",
            "View reports", "\uE9D2", () => Navigate("reports")), 60);

        StatRow(
            Stat("Total students", stats.TotalStudents.ToString("N0"), "Registered learners", "\uE716", Theme.Primary),
            Stat("Net revenue", Fmt.Money(stats.NetRevenue), "After refunds this year", "\uE8C7", Theme.Accent),
            Stat("Pending payments", stats.PendingPayments.ToString("N0"), "Waiting for confirmation", "\uE8C8", Theme.Amber),
            Stat("Needs attention", (stats.PendingInstructorApplications + stats.PendingDeleteRequests).ToString("N0"),
                "Applications and requests", "\uE7BA", Theme.Red));

        AddBlock(Split(new RevenueChartCard(), new DonutCard(), 300, 64f), 300);

        var recent = AppData.Payments.OrderByDescending(p => p.CreatedOn).Take(6).ToList();
        var table = MakeTable(
            new[] { "Student", "Course", "Amount", "Method", "Status", "Date" },
            new[] { 2f, 2.6f, 1.3f, 1.4f, 1.2f, 1.2f },
            pillColumns: 4);
        table.SetRows(recent.Select(p => new[]
        {
            CatalogService.StudentName(p.StudentId),
            CatalogService.Course(p.CourseId)?.Title ?? "Course",
            Fmt.Money(p.Amount),
            p.MethodText,
            p.StatusText,
            Fmt.Date(p.CreatedOn)
        }));
        table.RowClicked += (_, index) =>
        {
            var payment = recent[index];
            DetailsDialog.Show(this, "Payment " + payment.Reference,
                "Payment record kept permanently, even after a refund.",
                new[]
                {
                    ("Student", CatalogService.StudentName(payment.StudentId)),
                    ("Course", CatalogService.Course(payment.CourseId)?.Title ?? "Course"),
                    ("Amount", Fmt.Money(payment.Amount)),
                    ("Method", payment.MethodText),
                    ("Status", payment.StatusText),
                    ("Created", Fmt.Date(payment.CreatedOn))
                });
        };
        AddTableCard("Recent payments", "The latest transactions across the academy", table);

        AddBlock(Split(new UpcomingBatchesCard(), BuildAttentionCard(stats), 260, 52f), 260);
    }

    private static Card BuildAttentionCard(DashboardStats stats)
    {
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "Needs your attention", "Open items waiting for an admin decision");

        var y = 78;
        AddItem(card, "\uE77B", "Instructor applications",
            stats.PendingInstructorApplications + " waiting for review", y, Theme.Secondary);
        AddItem(card, "\uE74D", "Delete requests",
            stats.PendingDeleteRequests + " waiting for approval", y + 62, Theme.Red);
        AddItem(card, "\uE8C8", "Pending payments",
            stats.PendingPayments + " payments to confirm", y + 124, Theme.Amber);

        var button = new PillButton
        {
            Text = "Open requests", Filled = false, Size = new Size(150, 36),
            Location = new Point(20, 216)
        };
        button.Click += (_, _) => Navigator.Go("requests");
        card.Controls.Add(button);
        return card;
    }

    private static void AddItem(Card card, string glyph, string title, string note, int y, Color color)
    {
        var icon = new Label
        {
            Text = glyph, Font = Theme.IconLarge, ForeColor = color, BackColor = Color.White,
            AutoSize = false, Location = new Point(20, y), Size = new Size(38, 38),
            TextAlign = ContentAlignment.MiddleCenter
        };
        var titleLabel = new Label
        {
            Text = title, Font = Theme.BodyBold, ForeColor = Theme.Ink, BackColor = Color.White,
            AutoSize = true, Location = new Point(68, y + 1)
        };
        var noteLabel = new Label
        {
            Text = note, Font = Theme.Small, ForeColor = Theme.Muted, BackColor = Color.White,
            AutoSize = true, Location = new Point(68, y + 21)
        };
        card.Controls.Add(icon);
        card.Controls.Add(titleLabel);
        card.Controls.Add(noteLabel);
    }
}
