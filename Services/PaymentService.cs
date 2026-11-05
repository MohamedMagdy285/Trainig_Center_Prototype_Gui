namespace ECMS;

/// <summary>Payment gateway simulation and enrollment confirmation.</summary>
public static class PaymentService
{
    public static Payment CreatePayment(Student student, Course course, int? batchId,
        decimal amount, PaymentMethod method)
    {
        var payment = new Payment
        {
            Id = AppData.NextId(AppData.Payments, p => p.Id),
            StudentId = student.Id, CourseId = course.Id, BatchId = batchId,
            Amount = amount, Method = method, Status = PaymentStatus.Pending,
            CreatedOn = DateTime.Now, Reference = "GW-" + (1000 + AppData.Payments.Count + 1)
        };
        AppData.Payments.Add(payment);
        return payment;
    }

    /// <summary>Called when the gateway reports a successful payment.</summary>
    public static (bool Ok, string Message) ConfirmPayment(Payment payment)
    {
        if (payment.Status != PaymentStatus.Pending)
            return (false, "This payment is not pending.");

        var course = CatalogService.Course(payment.CourseId);
        if (course is null) return (false, "The course no longer exists.");

        // A duplicate payment is kept, but the extra amount is refunded separately.
        if (CatalogService.EnrollmentFor(payment.StudentId, payment.CourseId, payment.BatchId) is not null)
        {
            payment.Status = PaymentStatus.Successful;
            payment.ConfirmedOn = DateTime.Now;
            payment.IsDuplicate = true;
            RefundService.Execute(payment, payment.Amount, RefundKind.Refund,
                "Duplicate successful payment");
            return (false, "This payment was a duplicate. The amount was refunded.");
        }

        // There is no temporary seat reservation, so the seat is checked again now.
        if (payment.BatchId is { } batchId)
        {
            var batch = CatalogService.Batch(batchId);
            if (batch is null || batch.Status == BatchStatus.Cancelled || batch.IsFull)
            {
                payment.Status = PaymentStatus.Refunded;
                payment.ConfirmedOn = DateTime.Now;
                RefundService.Execute(payment, payment.Amount, RefundKind.Refund,
                    "The batch became full before confirmation");
                return (false, "The batch is no longer available. Your payment was refunded.");
            }
        }

        payment.Status = PaymentStatus.Successful;
        payment.ConfirmedOn = DateTime.Now;

        AppData.Enrollments.Add(new Enrollment
        {
            Id = AppData.NextId(AppData.Enrollments, e => e.Id),
            StudentId = payment.StudentId, CourseId = payment.CourseId,
            BatchId = payment.BatchId, PaymentId = payment.Id,
            EnrolledOn = DateTime.Today, Status = EnrollmentStatus.Confirmed
        });

        return (true, "Payment confirmed. Your enrollment is now active.");
    }

    public static void MarkFailed(Payment payment)
    {
        if (payment.Status == PaymentStatus.Pending)
            payment.Status = PaymentStatus.Failed;
    }

    /// <summary>Uses platform credit linked to an earlier payment.</summary>
    public static (bool Ok, string Message) PayWithCredit(Student student, Course course, int? batchId, decimal price)
    {
        if (RefundService.CreditBalance(student.Id) < price)
            return (false, "The available platform credit does not cover this course.");

        if (!RefundService.UseCredit(student.Id, price))
            return (false, "The credit could not be used.");

        var payment = new Payment
        {
            Id = AppData.NextId(AppData.Payments, p => p.Id),
            StudentId = student.Id, CourseId = course.Id, BatchId = batchId,
            Amount = price, Method = PaymentMethod.PlatformCredit,
            Status = PaymentStatus.Successful, CreatedOn = DateTime.Now,
            ConfirmedOn = DateTime.Now, Reference = "CREDIT-" + (AppData.Payments.Count + 1)
        };
        AppData.Payments.Add(payment);

        AppData.Enrollments.Add(new Enrollment
        {
            Id = AppData.NextId(AppData.Enrollments, e => e.Id),
            StudentId = student.Id, CourseId = course.Id, BatchId = batchId,
            PaymentId = payment.Id, EnrolledOn = DateTime.Today, Status = EnrollmentStatus.Confirmed
        });

        return (true, "Credit applied. Your enrollment is now active.");
    }
}
