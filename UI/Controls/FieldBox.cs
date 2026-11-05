namespace ECMS;

/// <summary>Rounded input used by the CRUD dialogs. It can render a text field
/// or a dropdown while keeping the dashboard palette.</summary>
public partial class FieldBox : UserControl
{
    private Control? _input;
    private bool _focused;

    public FieldBox() : this(null, null) { }

    public FieldBox(string? placeholder, IEnumerable<string>? options, bool password = false)
    {
        InitializeComponent();
        BuildInput(placeholder, options, password);
    }

    public string Value
    {
        get => _input switch
        {
            TextBox text => text.Text,
            ComboBox combo => combo.Text,
            _ => ""
        };
        set
        {
            if (_input is TextBox text) text.Text = value;
            else if (_input is ComboBox combo) combo.Text = value;
        }
    }

    public void FocusInput() => _input?.Focus();

    private void BuildInput(string? placeholder, IEnumerable<string>? options, bool password)
    {
        if (options is not null)
        {
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Field,
                ForeColor = Theme.Ink,
                Font = Theme.Body,
                Location = new Point(10, 8),
                Width = Math.Max(20, Width - 20)
            };
            combo.Items.AddRange(options.Cast<object>().ToArray());
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
            combo.GotFocus += (_, _) => { _focused = true; Invalidate(); };
            combo.LostFocus += (_, _) => { _focused = false; Invalidate(); };
            _input = combo;
        }
        else
        {
            var text = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Theme.Field,
                ForeColor = Theme.Ink,
                Font = Theme.Body,
                Location = new Point(12, 10),
                Width = Math.Max(20, Width - 24),
                PlaceholderText = placeholder ?? "",
                UseSystemPasswordChar = password
            };
            text.GotFocus += (_, _) => { _focused = true; Invalidate(); };
            text.LostFocus += (_, _) => { _focused = false; Invalidate(); };
            _input = text;
        }

        Controls.Add(_input);
        Resize += (_, _) =>
        {
            if (_input is ComboBox combo)
            {
                combo.Top = (Height - combo.PreferredHeight) / 2;
                combo.Width = Math.Max(20, Width - 20);
            }
            else if (_input is TextBox text)
            {
                text.Top = (Height - text.PreferredHeight) / 2;
                text.Width = Math.Max(20, Width - 24);
            }
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Theme.SetupGraphics(e.Graphics);
        var r = new RectangleF(.5f, .5f, Width - 1, Height - 1);
        Theme.Fill(e.Graphics, Theme.Field, r, 10);
        Theme.DrawBorder(e.Graphics,
            _focused ? Theme.Mix(Theme.Primary, Color.White, .25f) : Theme.Border, r, 10);
    }
}
