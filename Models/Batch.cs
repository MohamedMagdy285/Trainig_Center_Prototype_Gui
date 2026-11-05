namespace ECMS;

/// <summary>A scheduled offline group for an offline course.</summary>
public class Batch
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int InstructorId { get; set; }
    public string Title { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public string Schedule { get; set; } = "";
    public BatchStatus Status { get; set; } = BatchStatus.Scheduled;

    public int ConfirmedSeats =>
        AppData.Enrollments.Count(e => e.BatchId == Id && e.Status == EnrollmentStatus.Confirmed);

    public int SeatsLeft => Math.Max(0, Capacity - ConfirmedSeats);
    public bool IsFull => SeatsLeft <= 0;

    public bool HasPaymentTransaction =>
        AppData.Payments.Any(p => p.BatchId == Id && p.Status != PaymentStatus.Failed);

    public string StatusText
    {
        get
        {
            if (Status == BatchStatus.Cancelled) return "Cancelled";
            if (IsFull) return "Full";
            if (StartDate <= DateTime.Today && EndDate >= DateTime.Today) return "Ongoing";
            if (EndDate < DateTime.Today) return "Completed";
            return "Scheduled";
        }
    }
}
