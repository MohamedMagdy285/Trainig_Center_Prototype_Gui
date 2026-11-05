namespace ECMS;

/// <summary>Financial and learning reports for the academy.</summary>
public partial class AdminReportsPage : BasePage
{
    public AdminReportsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var stats = ReportService.Dashboard();
        AddBlock(Header("Reports", "Revenue, refunds and course performance at a glance."), 60);

        StatRow(
            Stat("Revenue", Fmt.Money(stats.TotalRevenue), "Confirmed payments", "\uE8C7", Theme.Primary),
            Stat("Refunds", Fmt.Money(stats.TotalRefunds), "Returned to students", "\uE8AB", Theme.Red),
            Stat("Net revenue", Fmt.Money(stats.NetRevenue), "Revenue minus refunds", "\uE9D2", Theme.Accent),
            Stat("Learners", (stats.OnlineStudents + stats.OfflineStudents).ToString("N0"),
                "Online + offline", "\uE716", Theme.Secondary));

        AddBlock(Split(new RevenueChartCard(), new DonutCard(), 300, 64f), 300);

        var series = ReportService.MonthlySeries(9);
        var monthly = MakeTable(
            new[] { "Month", "Revenue", "Refunds", "Net" },
            new[] { 2f, 1.5f, 1.5f, 1.5f });
        monthly.RightAligned.Add(1);
        monthly.RightAligned.Add(2);
        monthly.RightAligned.Add(3);
        monthly.SetRows(Enumerable.Range(0, series.Months.Length).Select(i => new[]
        {
            series.Months[i],
            Fmt.Money(series.Revenue[i]),
            Fmt.Money(series.Refunds[i]),
            Fmt.Money(series.Revenue[i] - series.Refunds[i])
        }));
        AddTableCard("Monthly breakdown", "Last nine months of payments and refunds", monthly);

        var courses = AppData.Courses.Where(c => !c.IsDeleted).OrderBy(c => c.Title).ToList();
        var performance = MakeTable(
            new[] { "Course", "Format", "Students", "Lessons", "Revenue" },
            new[] { 3f, 1.1f, 1f, 1f, 1.4f },
            pillColumns: new[] { 1 });
        performance.RightAligned.Add(2);
        performance.RightAligned.Add(3);
        performance.RightAligned.Add(4);
        performance.SetRows(courses.Select(c => new[]
        {
            c.Title,
            c.Kind.ToString(),
            CatalogService.StudentCount(c.Id).ToString(),
            c.TotalLessons.ToString(),
            Fmt.Money(AppData.Payments
                .Where(p => p.CourseId == c.Id &&
                            (p.Status == PaymentStatus.Successful || p.Status == PaymentStatus.Refunded))
                .Sum(p => p.Amount))
        }));
        AddTableCard("Course performance", "Enrollment and revenue by course", performance);
    }
}
