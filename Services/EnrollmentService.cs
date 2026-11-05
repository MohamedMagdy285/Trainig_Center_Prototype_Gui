namespace ECMS;

/// <summary>Rules for joining and cancelling courses and batches.</summary>
public static class EnrollmentService
{
    public static string? CanEnrollOnline(Student student, Course course)
    {
        if (course.Kind != CourseKind.Online) return "This is not an online course.";
        if (!course.IsPublished) return "This course is not published yet.";
        if (!course.AcceptsNewEnrollments) return "New enrollments are closed for this course.";
        if (CatalogService.IsEnrolled(student.Id, course.Id)) return "You already own this course.";
        return null;
    }

    public static string? CanEnrollBatch(Student student, Batch batch)
    {
        if (batch.Status == BatchStatus.Cancelled) return "This batch has been cancelled.";
        if (batch.IsFull) return "This batch is full. Please choose another upcoming batch.";
        if (CatalogService.EnrollmentFor(student.Id, batch.CourseId, batch.Id) is not null)
            return "You are already enrolled in this batch.";
        return null;
    }

    public static bool CanEditBatch(Batch batch) => !batch.HasPaymentTransaction;

    /// <summary>Cancels a confirmed enrollment and applies the refund policy.</summary>
    public static (bool Ok, string Message) CancelEnrollment(Enrollment enrollment, string reason)
    {
        if (enrollment.Status != EnrollmentStatus.Confirmed)
            return (false, "This enrollment is not active.");

        var payment = AppData.Payments.FirstOrDefault(p => p.Id == enrollment.PaymentId);
        if (payment is null)
        {
            enrollment.Status = EnrollmentStatus.Cancelled;
            return (true, "The enrollment was cancelled.");
        }

        var (amount, policy) = RefundService.CalculateRefund(payment);
        if (amount <= 0)
        {
            enrollment.Status = EnrollmentStatus.Cancelled;
            return (true, "The enrollment was cancelled. " + policy);
        }

        RefundService.Execute(payment, amount, RefundKind.Refund, reason.Length > 0 ? reason : policy);
        return (true, $"The enrollment was cancelled and {Fmt.Money(amount)} was refunded. {policy}");
    }

    /// <summary>Admin emergency cancellation. All affected students are refunded in full.</summary>
    public static int CancelBatch(Batch batch, string reason)
    {
        batch.Status = BatchStatus.Cancelled;
        var affected = AppData.Enrollments
            .Where(e => e.BatchId == batch.Id && e.Status == EnrollmentStatus.Confirmed)
            .ToList();

        foreach (var enrollment in affected)
        {
            var payment = AppData.Payments.FirstOrDefault(p => p.Id == enrollment.PaymentId);
            if (payment is not null && payment.Status == PaymentStatus.Successful)
                RefundService.Execute(payment, payment.Amount, RefundKind.Refund,
                    reason.Length > 0 ? reason : "Academy cancelled the batch");
            enrollment.Status = EnrollmentStatus.Refunded;
        }
        return affected.Count;
    }
}
