namespace ECMS;

public record DashboardStats(
    int TotalStudents, int OnlineStudents, int OfflineStudents,
    int TotalInstructors, decimal TotalRevenue, decimal TotalRefunds,
    int PendingPayments, int PendingInstructorApplications, int PendingDeleteRequests)
{
    public decimal NetRevenue => TotalRevenue - TotalRefunds;
}

/// <summary>Dashboard numbers and financial reports.</summary>
public static class ReportService
{
    public static DashboardStats Dashboard(DateTime? from = null, DateTime? to = null)
    {
        var start = from ?? new DateTime(DateTime.Today.Year, 1, 1);
        var end = to ?? DateTime.Today;

        var onlineStudents = AppData.Enrollments
            .Where(e => e.Status == EnrollmentStatus.Confirmed)
            .Join(AppData.Courses.Where(c => c.Kind == CourseKind.Online),
                e => e.CourseId, c => c.Id, (e, c) => e.StudentId)
            .Distinct().Count();

        var offlineStudents = AppData.Enrollments
            .Where(e => e.Status == EnrollmentStatus.Confirmed)
            .Join(AppData.Courses.Where(c => c.Kind == CourseKind.Offline),
                e => e.CourseId, c => c.Id, (e, c) => e.StudentId)
            .Distinct().Count();

        var revenue = AppData.Payments
            .Where(p => p.ConfirmedOn is not null &&
                        (p.Status == PaymentStatus.Successful || p.Status == PaymentStatus.Refunded) &&
                        p.ConfirmedOn.Value.Date >= start.Date && p.ConfirmedOn.Value.Date <= end.Date)
            .Sum(p => p.Amount);

        var refunds = AppData.Refunds
            .Where(r => r.ExecutedOn.Date >= start.Date && r.ExecutedOn.Date <= end.Date)
            .Sum(r => r.Amount);

        return new DashboardStats(
            AppData.Students.Count, onlineStudents, offlineStudents,
            AppData.Instructors.Count, revenue, refunds,
            AppData.Payments.Count(p => p.Status == PaymentStatus.Pending),
            CatalogService.PendingInstructorApplications,
            CatalogService.PendingDeleteRequests);
    }

    public static (int[] Revenue, int[] Refunds, string[] Months) MonthlySeries(int months = 9)
    {
        var revenue = new int[months];
        var refunds = new int[months];
        var labels = new string[months];
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-(months - 1));

        for (var i = 0; i < months; i++)
        {
            var month = start.AddMonths(i);
            labels[i] = month.ToString("MMM");
            revenue[i] = (int)AppData.Payments
                .Where(p => p.ConfirmedOn is not null &&
                            (p.Status == PaymentStatus.Successful || p.Status == PaymentStatus.Refunded) &&
                            p.ConfirmedOn.Value.Year == month.Year && p.ConfirmedOn.Value.Month == month.Month)
                .Sum(p => p.Amount);
            refunds[i] = (int)AppData.Refunds
                .Where(r => r.ExecutedOn.Year == month.Year && r.ExecutedOn.Month == month.Month)
                .Sum(r => r.Amount);
        }
        return (revenue, refunds, labels);
    }

    public static (int Online, int Offline) StudentMix()
    {
        var online = AppData.Enrollments
            .Where(e => e.Status == EnrollmentStatus.Confirmed)
            .Join(AppData.Courses.Where(c => c.Kind == CourseKind.Online),
                e => e.CourseId, c => c.Id, (e, c) => e.StudentId)
            .Distinct().Count();
        var offline = AppData.Enrollments
            .Where(e => e.Status == EnrollmentStatus.Confirmed)
            .Join(AppData.Courses.Where(c => c.Kind == CourseKind.Offline),
                e => e.CourseId, c => c.Id, (e, c) => e.StudentId)
            .Distinct().Count();
        return (online, offline);
    }
}
