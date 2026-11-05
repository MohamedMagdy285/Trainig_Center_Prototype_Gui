namespace ECMS;

/// <summary>A payment transaction. Records are never deleted, even after a refund.</summary>
public class Payment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public int? BatchId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ConfirmedOn { get; set; }
    public string Reference { get; set; } = "";
    public bool IsDuplicate { get; set; }

    public string StatusText => Status.ToString();
    public string MethodText => Method switch
    {
        PaymentMethod.CreditCard => "Credit Card",
        PaymentMethod.DebitCard => "Debit Card",
        PaymentMethod.VodafoneCash => "Vodafone Cash",
        PaymentMethod.PlatformCredit => "Platform credit",
        _ => "Fawry"
    };
}

/// <summary>A refund executed against a payment, or credit linked to it.</summary>
public class Refund
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public int StudentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExecutedOn { get; set; } = DateTime.Today;
    public RefundKind Kind { get; set; } = RefundKind.Refund;
    public string Reason { get; set; } = "";
}

/// <summary>Store credit tied to the original payment transaction.</summary>
public class Credit
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SourcePaymentId { get; set; }
    public decimal Amount { get; set; }
    public decimal Remaining { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Today;
}
