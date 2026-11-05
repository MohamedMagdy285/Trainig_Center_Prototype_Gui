namespace ECMS;

/// <summary>Refund policy and refund / credit execution.</summary>
public static class RefundService
{
    public static (decimal Amount, string Policy) CalculateRefund(Payment payment)
    {
        if (payment.Status != PaymentStatus.Successful)
            return (0, "Only a successful payment can be refunded.");

        var course = CatalogService.Course(payment.CourseId);
        if (course is null) return (0, "The course no longer exists.");

        if (course.Kind == CourseKind.Online)
            return (0, "Online courses are non-refundable after purchase.");

        var batch = payment.BatchId is { } id ? CatalogService.Batch(id) : null;
        if (batch is null) return (0, "The batch could not be found.");

        if (batch.Status == BatchStatus.Cancelled)
            return (payment.Amount, "The academy cancelled this batch. Full refund applies.");

        if (DateTime.Today < batch.StartDate)
            return (payment.Amount, "Cancellation before the course starts. 100% refund applies.");

        if (DateTime.Today <= batch.StartDate.AddDays(7))
            return (payment.Amount * 0.5m, "Cancellation during the first week. 50% refund applies.");

        return (0, "No refund is available after the first week.");
    }

    public static Refund? Execute(Payment payment, decimal amount, RefundKind kind, string reason)
    {
        if (amount <= 0) return null;

        var refund = new Refund
        {
            Id = AppData.NextId(AppData.Refunds, r => r.Id),
            PaymentId = payment.Id, StudentId = payment.StudentId,
            Amount = amount, ExecutedOn = DateTime.Today, Kind = kind, Reason = reason
        };
        AppData.Refunds.Add(refund);
        payment.Status = PaymentStatus.Refunded;

        foreach (var enrollment in AppData.Enrollments.Where(e => e.PaymentId == payment.Id))
            enrollment.Status = EnrollmentStatus.Refunded;

        if (kind == RefundKind.Credit)
        {
            AppData.Credits.Add(new Credit
            {
                Id = AppData.NextId(AppData.Credits, c => c.Id),
                StudentId = payment.StudentId, SourcePaymentId = payment.Id,
                Amount = amount, Remaining = amount, CreatedOn = DateTime.Today
            });
        }

        return refund;
    }

    /// <summary>Refunds every affected student when the academy cancels a batch.</summary>
    public static int RefundBatch(Batch batch, string reason)
    {
        var payments = AppData.Payments
            .Where(p => p.BatchId == batch.Id && p.Status == PaymentStatus.Successful)
            .ToList();

        foreach (var payment in payments)
            Execute(payment, payment.Amount, RefundKind.Refund,
                reason.Length > 0 ? reason : "Academy cancelled the batch");

        return payments.Count;
    }

    public static decimal CreditBalance(int studentId) =>
        AppData.Credits.Where(c => c.StudentId == studentId).Sum(c => c.Remaining);

    public static bool UseCredit(int studentId, decimal amount)
    {
        if (CreditBalance(studentId) < amount) return false;

        var remaining = amount;
        foreach (var credit in AppData.Credits.Where(c => c.StudentId == studentId && c.Remaining > 0)
                     .OrderBy(c => c.CreatedOn))
        {
            var take = Math.Min(credit.Remaining, remaining);
            credit.Remaining -= take;
            remaining -= take;
            if (remaining <= 0) break;
        }
        return remaining <= 0;
    }
}
