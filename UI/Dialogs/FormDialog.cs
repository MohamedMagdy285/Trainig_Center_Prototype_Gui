namespace ECMS;

public enum FormFieldKind { Text, Password, Multiline, Number, Date, Choice }

/// <summary>One field description used by <see cref="FormDialog"/>.</summary>
public class FormField
{
    public required string Key { get; init; }
    public required string Label { get; init; }
    public string Value { get; init; } = "";
    public string Placeholder { get; init; } = "";
    public FormFieldKind Kind { get; init; } = FormFieldKind.Text;
    public string[] Options { get; init; } = Array.Empty<string>();
    public bool Required { get; init; } = true;
    public decimal Minimum { get; init; }
    public decimal Maximum { get; init; } = 1000000;

    public static FormField Text(string key, string label, string value = "", string placeholder = "")
        => new() { Key = key, Label = label, Value = value, Placeholder = placeholder };

    public static FormField Password(string key, string label, string value = "")
        => new() { Key = key, Label = label, Value = value, Kind = FormFieldKind.Password };

    public static FormField Multiline(string key, string label, string value = "", string placeholder = "")
        => new() { Key = key, Label = label, Value = value, Placeholder = placeholder, Kind = FormFieldKind.Multiline };

    public static FormField Number(string key, string label, string value = "0",
        decimal minimum = 0, decimal maximum = 1000000)
        => new()
        {
            Key = key, Label = label, Value = value, Kind = FormFieldKind.Number,
            Minimum = minimum, Maximum = maximum
        };

    public static FormField Date(string key, string label, DateTime value)
        => new() { Key = key, Label = label, Value = value.ToString("yyyy-MM-dd"), Kind = FormFieldKind.Date };

    public static FormField Choice(string key, string label, string value, params string[] options)
        => new() { Key = key, Label = label, Value = value, Options = options, Kind = FormFieldKind.Choice };
}

/// <summary>
/// A simple, reusable themed form. Pages describe the fields and a validation rule;
/// the dialog returns the values as a dictionary.
/// </summary>
public class FormDialog : Form
{
    private readonly Dictionary<string, Control> _inputs = new();
    private readonly Dictionary<string, FormField> _fields = new();
    private readonly Panel _fieldsPanel = new();
    private readonly Label _errorLabel = new();
    private readonly Func<IReadOnlyDictionary<string, string>, string?> _validate;

    public Dictionary<string, string> Values { get; } = new();

    public FormDialog(string title, string subtitle, IEnumerable<FormField> fields,
        Func<IReadOnlyDictionary<string, string>, string?>? validate = null)
    {
        _validate = validate ?? (_ => null);
        Text = title;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        BackColor = Theme.Surface;
        DoubleBuffered = true;
        KeyPreview = true;
        Width = 540;

        var titleLabel = new Label
        {
            Text = title, Font = Theme.H3, ForeColor = Theme.Ink, BackColor = Theme.Surface,
            AutoSize = true, Location = new Point(28, 24)
        };
        var subtitleLabel = new Label
        {
            Text = subtitle, Font = Theme.Small, ForeColor = Theme.Muted, BackColor = Theme.Surface,
            AutoSize = false, Location = new Point(28, 48), Size = new Size(Width - 90, 32)
        };
        var close = new IconButton
        {
            Glyph = "\uE711", Size = new Size(36, 36), Location = new Point(Width - 50, 16)
        };
        close.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        _fieldsPanel.Location = new Point(28, 88);
        _fieldsPanel.Width = Width - 56;
        _fieldsPanel.BackColor = Theme.Surface;
        _fieldsPanel.AutoScroll = true;

        _errorLabel.Location = new Point(28, 0);
        _errorLabel.Width = Width - 56;
        _errorLabel.Height = 24;
        _errorLabel.Font = Theme.Small;
        _errorLabel.ForeColor = Theme.Red;
        _errorLabel.BackColor = Theme.Surface;

        var cancel = new PillButton
        {
            Text = "Cancel", Filled = false, Size = new Size(100, 40)
        };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        var save = new PillButton
        {
            Text = "Save", Filled = true, Accent = Theme.Primary, Size = new Size(120, 40)
        };
        save.Click += (_, _) => Save();

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(close);
        Controls.Add(_fieldsPanel);
        Controls.Add(_errorLabel);
        Controls.Add(cancel);
        Controls.Add(save);

        MouseDown += (_, _) => Theme.Drag(this);
        titleLabel.MouseDown += (_, _) => Theme.Drag(this);
        subtitleLabel.MouseDown += (_, _) => Theme.Drag(this);

        var y = 4;
        foreach (var field in fields)
        {
            _fields[field.Key] = field;
            var label = new Label
            {
                Text = field.Label.ToUpperInvariant(), Font = Theme.TinyBold, ForeColor = Theme.Muted,
                BackColor = Theme.Surface, AutoSize = true, Location = new Point(0, y)
            };
            var input = MakeInput(field);
            input.Location = new Point(0, y + 20);
            input.Width = _fieldsPanel.Width - 24;
            input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _fieldsPanel.Controls.Add(label);
            _fieldsPanel.Controls.Add(input);
            _inputs[field.Key] = input;
            y += field.Kind == FormFieldKind.Multiline ? 112 : 70;
        }

        var fieldsHeight = Math.Min(420, Math.Max(90, y + 6));
        _fieldsPanel.Height = fieldsHeight;
        var footerTop = _fieldsPanel.Bottom + 18;
        _errorLabel.Top = footerTop;
        var buttonTop = footerTop + 30;
        cancel.Location = new Point(Width - 56 - save.Width - cancel.Width - 10, buttonTop);
        save.Location = new Point(Width - 56 - save.Width, buttonTop);
        Height = buttonTop + 40 + 34;

        Resize += (_, _) =>
        {
            if (Width <= 0 || Height <= 0) return;
            Region = new Region(Theme.Rounded(ClientRectangle, 16));
            close.Left = Width - 50;
            subtitleLabel.Width = Width - 90;
            _fieldsPanel.Width = Width - 56;
            _errorLabel.Width = Width - 56;
            save.Left = Width - 56 - save.Width;
            cancel.Left = save.Left - cancel.Width - 10;
            foreach (Control control in _fieldsPanel.Controls)
                if (control is not Label) control.Width = _fieldsPanel.Width - 24;
        };
        Region = new Region(Theme.Rounded(ClientRectangle, 16));
        ActiveControl = _inputs.Values.FirstOrDefault();
    }

    private Control MakeInput(FormField field)
    {
        switch (field.Kind)
        {
            case FormFieldKind.Choice:
            {
                var combo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.Field, ForeColor = Theme.Ink, Font = Theme.Body
                };
                combo.Items.AddRange(field.Options);
                combo.SelectedItem = field.Options.Contains(field.Value) ? field.Value : field.Options.FirstOrDefault();
                return combo;
            }
            case FormFieldKind.Date:
            {
                var picker = new DateTimePicker
                {
                    Format = DateTimePickerFormat.Custom, CustomFormat = "dd MMM yyyy",
                    Font = Theme.Body, Value = DateTime.TryParse(field.Value, out var date) ? date : DateTime.Today
                };
                return picker;
            }
            case FormFieldKind.Number:
            {
                var number = new NumericUpDown
                {
                    Minimum = field.Minimum, Maximum = field.Maximum, Font = Theme.Body,
                    BackColor = Theme.Field, ForeColor = Theme.Ink,
                    Value = decimal.TryParse(field.Value, out var value) ? value : field.Minimum
                };
                return number;
            }
            case FormFieldKind.Multiline:
            {
                return new TextBox
                {
                    Multiline = true, Height = 84, ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.FixedSingle, BackColor = Theme.Field,
                    ForeColor = Theme.Ink, Font = Theme.Body, Text = field.Value,
                    PlaceholderText = field.Placeholder
                };
            }
            case FormFieldKind.Password:
            {
                return new TextBox
                {
                    UseSystemPasswordChar = true, BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Theme.Field, ForeColor = Theme.Ink, Font = Theme.Body,
                    Text = field.Value, PlaceholderText = field.Placeholder
                };
            }
            default:
            {
                return new TextBox
                {
                    BorderStyle = BorderStyle.FixedSingle, BackColor = Theme.Field,
                    ForeColor = Theme.Ink, Font = Theme.Body, Text = field.Value,
                    PlaceholderText = field.Placeholder
                };
            }
        }
    }

    private void Save()
    {
        Values.Clear();
        foreach (var (key, input) in _inputs)
        {
            var field = _fields[key];
            var value = input switch
            {
                TextBox text => text.Text.Trim(),
                ComboBox combo => combo.Text,
                NumericUpDown number => number.Value.ToString("0.##"),
                DateTimePicker date => date.Value.ToString("yyyy-MM-dd"),
                _ => ""
            };

            if (field.Required && value.Length == 0)
            {
                _errorLabel.Text = field.Label + " is required.";
                input.Focus();
                return;
            }
            Values[key] = value;
        }

        var error = _validate(Values);
        if (!string.IsNullOrEmpty(error))
        {
            _errorLabel.Text = error;
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
