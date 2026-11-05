namespace ECMS;

/// <summary>
/// Sandbox checkout page. It simulates the payment gateway, then confirms the
/// payment and creates the enrollment through the service layer.
/// </summary>
public partial class CheckoutPage : BasePage
{
    private readonly CheckoutRequest _request;
    private readonly Course? _course;
    private readonly Batch? _batch;
    private readonly ComboBox _method = new();
    private readonly FieldBox _cardNumber = new("Card number", null) { Size = new Size(330, 42) };
    private readonly FieldBox _cardName = new("Name on card", null) { Size = new Size(330, 42) };
    private readonly FieldBox _expiry = new("MM / YY", null) { Size = new Size(155, 42) };
    private readonly FieldBox _cvv = new("CVV", null, true) { Size = new Size(155, 42) };
    private readonly Label _error = new();

    /// <summary>Parameterless constructor for the Visual Studio designer.</summary>
    public CheckoutPage() : base(_ => { })
    {
        InitializeComponent();
    }

    public CheckoutPage(CheckoutRequest request) : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) return;
        _request = request;
        _course = CatalogService.Course(request.CourseId);
        _batch = request.BatchId is { } id ? CatalogService.Batch(id) : null;

        if (_course is null)
        {
            AddBlock(Header("Checkout", "The course could not be found."), 60);
            return;
        }

        AddBlock(Header("Checkout", "A safe sandbox payment — no real money is charged."), 60);
        AddBlock(Split(BuildPaymentCard(), BuildSummaryCard(), 470, 58f), 470);
        BuildCreditCard();
        AddBlock(BuildPolicyNote(), 96);
    }

    private Card BuildPaymentCard()
    {
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "Payment details", "Choose a method and enter the test details");

        card.Controls.Add(Caption("PAYMENT METHOD", 24, 74));
        _method.Items.AddRange(new object[] { "Credit card", "Debit card", "Vodafone Cash", "Fawry" });
        _method.SelectedIndex = 0;
        _method.DropDownStyle = ComboBoxStyle.DropDownList;
        _method.FlatStyle = FlatStyle.Flat;
        _method.Font = Theme.Body;
        _method.BackColor = Theme.Field;
        _method.ForeColor = Theme.Ink;
        _method.Location = new Point(24, 94);
        _method.Width = 330;
        card.Controls.Add(_method);

        card.Controls.Add(Caption("CARD NUMBER", 24, 142));
        _cardNumber.Location = new Point(24, 162);
        _cardNumber.Width = 330;
        card.Controls.Add(_cardNumber);

        card.Controls.Add(Caption("NAME ON CARD", 24, 214));
        _cardName.Location = new Point(24, 234);
        _cardName.Width = 330;
        card.Controls.Add(_cardName);

        card.Controls.Add(Caption("EXPIRY", 24, 286));
        _expiry.Location = new Point(24, 306);
        card.Controls.Add(_expiry);
        card.Controls.Add(Caption("CVV", 200, 286));
        _cvv.Location = new Point(200, 306);
        card.Controls.Add(_cvv);

        _error.AutoSize = false;
        _error.Location = new Point(24, 356);
        _error.Size = new Size(360, 20);
        _error.Font = Theme.Small;
        _error.ForeColor = Theme.Red;
        _error.BackColor = Color.White;
        card.Controls.Add(_error);

        var pay = new PillButton
        {
            Text = "Pay now", Filled = true, Accent = Theme.Primary,
            Size = new Size(160, 42), Location = new Point(card.Width - 184, 388)
        };
        pay.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        pay.Click += (_, _) => Pay();
        card.Controls.Add(pay);
        card.Resize += (_, _) => pay.Left = card.Width - 184;
        return card;
    }

    private Card BuildSummaryCard()
    {
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "Order summary", "Review your purchase before paying");

        var price = _batch?.Price ?? _course!.Price;
        var lines = new List<(string, string)>
        {
            ("Course", _course!.Title),
            ("Format", _course.Kind.ToString()),
            ("Instructor", CatalogService.InstructorName(_course.InstructorId))
        };
        if (_batch is not null)
        {
            lines.Add(("Batch", _batch.Title));
            lines.Add(("Schedule", _batch.Schedule));
            lines.Add(("Starts", Fmt.Date(_batch.StartDate)));
        }

        var y = 82;
        foreach (var (label, value) in lines)
        {
            card.Controls.Add(Caption(label.ToUpperInvariant(), 24, y));
            var text = new Label
            {
                Text = value, Font = Theme.Body, ForeColor = Theme.Ink, BackColor = Color.White,
                AutoSize = false, Location = new Point(24, y + 17), Size = new Size(340, 20)
            };
            card.Controls.Add(text);
            y += 48;
        }

        var separator = new Panel { BackColor = Theme.Border, Location = new Point(24, 330), Size = new Size(340, 1) };
        card.Controls.Add(separator);

        var totalLabel = new Label
        {
            Text = "TOTAL", Font = Theme.TinyBold, ForeColor = Theme.Muted, BackColor = Color.White,
            AutoSize = true, Location = new Point(24, 348)
        };
        var total = new Label
        {
            Text = Fmt.Money(price), Font = new Font("Segoe UI", 18f, FontStyle.Bold),
            ForeColor = Theme.Primary, BackColor = Color.White, AutoSize = true,
            Location = new Point(24, 366)
        };
        card.Controls.Add(totalLabel);
        card.Controls.Add(total);
        return card;
    }

    private void BuildCreditCard()
    {
        var student = Session.Student!;
        var balance = RefundService.CreditBalance(student.Id);
        if (balance <= 0) return;

        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "Platform credit", "Use credit from a previous refund or duplicate payment");

        var amount = new Label
        {
            Text = "Available: " + Fmt.Money(balance), Font = Theme.BodyBold, ForeColor = Theme.Ink,
            BackColor = Color.White, AutoSize = true, Location = new Point(24, 76)
        };
        var note = new Label
        {
            Text = "Credit is tied to your original payment and is used before any new charge.",
            Font = Theme.Small, ForeColor = Theme.Muted, BackColor = Color.White,
            AutoSize = true, Location = new Point(24, 98)
        };
        card.Controls.Add(amount);
        card.Controls.Add(note);

        var price = _batch?.Price ?? _course!.Price;
        var use = new PillButton
        {
            Text = "Use platform credit", Filled = false, Accent = Theme.Accent,
            Size = new Size(190, 38), Location = new Point(card.Width - 214, 78)
        };
        use.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        use.Click += (_, _) => PayWithCredit(price);
        card.Controls.Add(use);
        card.Resize += (_, _) => use.Left = card.Width - 214;
        AddBlock(card, 130);
    }

    private Card BuildPolicyNote()
    {
        var card = new Card { BackColor = Color.White };
        var label = new Label
        {
            Text = _course!.Kind == CourseKind.Online
                ? "Online course payments are non-refundable after purchase. You can preview the first section before buying."
                : "Offline refund policy: 100% before the batch starts, 50% during the first week, no refund after that. If the academy cancels the batch, every student is refunded in full.",
            Font = Theme.Small, ForeColor = Theme.Muted, BackColor = Color.White,
            AutoSize = false, Location = new Point(24, 20), Size = new Size(760, 50)
        };
        card.Controls.Add(label);
        card.Resize += (_, _) => label.Width = Math.Max(200, card.Width - 48);
        return card;
    }

    private void Pay()
    {
        _error.Text = "";
        var student = Session.Student!;
        var course = _course!;
        var price = _batch?.Price ?? course.Price;

        var canEnroll = _batch is null
            ? EnrollmentService.CanEnrollOnline(student, course)
            : EnrollmentService.CanEnrollBatch(student, _batch);
        if (canEnroll is not null)
        {
            _error.Text = canEnroll;
            return;
        }

        var method = _method.SelectedIndex switch
        {
            1 => PaymentMethod.DebitCard,
            2 => PaymentMethod.VodafoneCash,
            3 => PaymentMethod.Fawry,
            _ => PaymentMethod.CreditCard
        };

        if (method is PaymentMethod.CreditCard or PaymentMethod.DebitCard)
        {
            if (_cardNumber.Value.Replace(" ", "").Length < 12)
            {
                _error.Text = "Please enter a test card number.";
                return;
            }
            if (_cardName.Value.Trim().Length < 3)
            {
                _error.Text = "Please enter the name on the card.";
                return;
            }
            if (_expiry.Value.Trim().Length < 4 || _cvv.Value.Trim().Length < 3)
            {
                _error.Text = "Please enter the expiry date and CVV.";
                return;
            }
        }

        var payment = PaymentService.CreatePayment(student, course, _request.BatchId, price, method);

        var approved = ConfirmDialog.Ask(this, "Sandbox payment gateway",
            $"Approve the test payment of {Fmt.Money(price)} for {course.Title}?", "Approve payment");
        if (!approved)
        {
            PaymentService.MarkFailed(payment);
            _error.Text = "The test payment was declined. You can try again.";
            return;
        }

        var (ok, message) = PaymentService.ConfirmPayment(payment);
        Navigator.Toast(message, ok);
        if (ok)
        {
            Navigator.Go("learning");
            return;
        }

        _error.Text = message;
    }

    private void PayWithCredit(decimal price)
    {
        var student = Session.Student!;
        var canEnroll = _batch is null
            ? EnrollmentService.CanEnrollOnline(student, _course!)
            : EnrollmentService.CanEnrollBatch(student, _batch);
        if (canEnroll is not null)
        {
            _error.Text = canEnroll;
            return;
        }

        var (ok, message) = PaymentService.PayWithCredit(student, _course!, _request.BatchId, price);
        Navigator.Toast(message, ok);
        if (ok) Navigator.Go("learning");
        else _error.Text = message;
    }

    private static Label Caption(string text, int x, int y)
        => new()
        {
            Text = text, Font = Theme.TinyBold, ForeColor = Theme.Muted, BackColor = Color.White,
            AutoSize = true, Location = new Point(x, y)
        };
}
