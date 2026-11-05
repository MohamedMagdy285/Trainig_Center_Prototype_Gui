namespace ECMS;

/// <summary>Admin view of every payment, refund and platform credit.</summary>
public partial class AdminPaymentsPage : BasePage
{
    private readonly TableControl _paymentsTable;
    private readonly Card _paymentsCard;
    private readonly TableControl _refundsTable;
    private readonly Card _refundsCard;
    private readonly SearchBox _search = new("Search student, course or status...");
    private List<Payment> _shown = new();
    private List<Refund> _refunds = new();

    public AdminPaymentsPage() : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        var stats = ReportService.Dashboard();
        AddBlock(Header("Payments & refunds", "Every transaction is kept permanently for the audit trail."), 60);

        StatRow(
            Stat("Gross revenue", Fmt.Money(stats.TotalRevenue), "Confirmed payments", "\uE8C7", Theme.Primary),
            Stat("Refunds", Fmt.Money(stats.TotalRefunds), "Money returned", "\uE8AB", Theme.Red),
            Stat("Net revenue", Fmt.Money(stats.NetRevenue), "After refunds", "\uE9D2", Theme.Accent),
            Stat("Pending", stats.PendingPayments.ToString("N0"), "Waiting for confirmation", "\uE823", Theme.Amber));

        var searchPanel = new Panel { BackColor = Theme.Bg };
        _search.Location = new Point(0, 0);
        _search.Size = new Size(360, 42);
        _search.ValueChanged += (_, _) => Reload();
        searchPanel.Controls.Add(_search);
        AddBlock(searchPanel, 44);

        _paymentsTable = MakeTable(
            new[] { "Student", "Course", "Amount", "Method", "Status", "Created" },
            new[] { 2f, 2.6f, 1.2f, 1.4f, 1.2f, 1.3f },
            actions: true, pillColumns: new[] { 4 });
        _paymentsTable.ActionClicked += (_, e) => HandlePayment(e.RowIndex, e.Action);
        _paymentsTable.RowClicked += (_, index) => ShowPayment(_shown[index]);
        _paymentsCard = AddTableCard("Payments", "Edit confirms a pending payment  â€¢  Delete issues a refund", _paymentsTable,
            emptyText: "No payments match your search.");

        _refundsTable = MakeTable(
            new[] { "Student", "Course", "Amount", "Kind", "Reason", "Date" },
            new[] { 2f, 2.2f, 1.2f, 0.9f, 3f, 1.2f },
            pillColumns: new[] { 3 });
        _refundsTable.RowClicked += (_, index) => ShowRefund(_refunds[index]);
        _refundsCard = AddTableCard("Refunds and credits", "Money returned to students", _refundsTable,
            emptyText: "No refunds have been issued.");

        AddBlock(new RefundPolicyCard(), 420);
        Reload();
    }

    private void Reload()
    {
        var query = _search.Value.Trim();
        _shown = AppData.Payments
            .Where(p => query.Length == 0 ||
                        CatalogService.StudentName(p.StudentId).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (CatalogService.Course(p.CourseId)?.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        p.StatusText.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.CreatedOn)
            .ToList();

        _paymentsTable.SetRows(_shown.Select(p => new[]
        {
            CatalogService.StudentName(p.StudentId),
            CatalogService.Course(p.CourseId)?.Title ?? "Course",
            Fmt.Money(p.Amount),
            p.MethodText,
            p.StatusText,
            Fmt.Date(p.CreatedOn)
        }));
        FitTableCard(_paymentsCard, _paymentsTable);

        _refunds = AppData.Refunds.OrderByDescending(r => r.ExecutedOn).ToList();
        _refundsTable.SetRows(_refunds.Select(r =>
        {
            var payment = AppData.Payments.FirstOrDefault(p => p.Id == r.PaymentId);
            return new[]
            {
                CatalogService.StudentName(r.StudentId),
                payment is null ? "Course" : CatalogService.Course(payment.CourseId)?.Title ?? "Course",
                Fmt.Money(r.Amount),
                r.Kind.ToString(),
                r.Reason,
                Fmt.Date(r.ExecutedOn)
            };
        }));
        FitTableCard(_refundsCard, _refundsTable);
        RefreshLayout();
    }

    private void HandlePayment(int index, TableRowAction action)
    {
        if (index < 0 || index >= _shown.Count) return;
        var payment = _shown[index];

        if (action == TableRowAction.Edit)
        {
            if (payment.Status != PaymentStatus.Pending)
            {
                Navigator.Toast("Only a pending payment can be confirmed.", false);
                return;
            }

            if (!ConfirmDialog.Ask(this, "Confirm payment",
                    $"Confirm {Fmt.Money(payment.Amount)} for {CatalogService.StudentName(payment.StudentId)}?",
                    "Confirm"))
                return;

            var (ok, message) = PaymentService.ConfirmPayment(payment);
            Navigator.Toast(message, ok);
            Reload();
            return;
        }

        if (payment.Status == PaymentStatus.Pending)
        {
            if (!ConfirmDialog.Ask(this, "Mark payment failed",
                    "Mark this pending payment as failed? The student can try again.", "Mark failed"))
                return;
            PaymentService.MarkFailed(payment);
            Navigator.Toast("Payment marked as failed.");
            Reload();
            return;
        }

        var (amount, policy) = RefundService.CalculateRefund(payment);
        if (amount <= 0)
        {
            Navigator.Toast(policy, false);
            return;
        }

        if (!ConfirmDialog.Ask(this, "Issue refund",
                $"{Fmt.Money(amount)} will be returned to {CatalogService.StudentName(payment.StudentId)}.\n{policy}",
                "Refund"))
            return;

        RefundService.Execute(payment, amount, RefundKind.Refund, policy);
        Navigator.Toast($"{Fmt.Money(amount)} refunded.");
        Reload();
    }

    private void ShowPayment(Payment payment)
    {
        var (amount, policy) = RefundService.CalculateRefund(payment);
        DetailsDialog.Show(this, "Payment " + payment.Reference, "Transaction details",
            new[]
            {
                ("Student", CatalogService.StudentName(payment.StudentId)),
                ("Course", CatalogService.Course(payment.CourseId)?.Title ?? "Course"),
                ("Batch", payment.BatchId is { } id
                    ? CatalogService.Batch(id)?.Title ?? "Batch"
                    : "Not a batch purchase"),
                ("Amount", Fmt.Money(payment.Amount)),
                ("Method", payment.MethodText),
                ("Status", payment.StatusText),
                ("Created", Fmt.Date(payment.CreatedOn)),
                ("Confirmed", payment.ConfirmedOn is null ? "Not confirmed" : Fmt.Date(payment.ConfirmedOn.Value)),
                ("Duplicate", payment.IsDuplicate ? "Yes" : "No")
            },
            payment.Status == PaymentStatus.Successful
                ? "Refund available now: " + Fmt.Money(amount) + ". " + policy
                : policy);
    }

    private void ShowRefund(Refund refund)
    {
        var payment = AppData.Payments.FirstOrDefault(p => p.Id == refund.PaymentId);
        DetailsDialog.Show(this, refund.Kind == RefundKind.Credit ? "Platform credit" : "Refund",
            "Money returned to the student",
            new[]
            {
                ("Student", CatalogService.StudentName(refund.StudentId)),
                ("Course", payment is null ? "Course" : CatalogService.Course(payment.CourseId)?.Title ?? "Course"),
                ("Amount", Fmt.Money(refund.Amount)),
                ("Kind", refund.Kind.ToString()),
                ("Executed", Fmt.Date(refund.ExecutedOn)),
                ("Reason", refund.Reason)
            });
    }
}
