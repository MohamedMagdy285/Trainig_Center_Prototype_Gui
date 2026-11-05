namespace ECMS;

/// <summary>
/// A simple read-only details window. Pages use it when a table row is clicked.
/// </summary>
public class DetailsDialog : Form
{
    public DetailsDialog(string title, string subtitle, IEnumerable<(string Label, string Value)> rows,
        string? note = null)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        BackColor = Theme.Surface;
        DoubleBuffered = true;
        Width = 520;

        var titleLabel = new Label
        {
            Text = title, Font = Theme.H3, ForeColor = Theme.Ink, BackColor = Theme.Surface,
            AutoSize = true, Location = new Point(28, 24)
        };
        var subtitleLabel = new Label
        {
            Text = subtitle, Font = Theme.Small, ForeColor = Theme.Muted, BackColor = Theme.Surface,
            AutoSize = false, Location = new Point(28, 48), Size = new Size(Width - 90, 30)
        };
        var close = new IconButton
        {
            Glyph = "\uE711", Size = new Size(36, 36), Location = new Point(Width - 50, 16)
        };
        close.Click += (_, _) => Close();

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(close);

        var y = 92;
        foreach (var (label, value) in rows)
        {
            var caption = new Label
            {
                Text = label.ToUpperInvariant(), Font = Theme.TinyBold, ForeColor = Theme.SoftText,
                BackColor = Theme.Surface, AutoSize = true, Location = new Point(28, y)
            };
            var text = new Label
            {
                Text = value, Font = Theme.Body, ForeColor = Theme.Ink, BackColor = Theme.Surface,
                AutoSize = false, Location = new Point(28, y + 17),
                Size = new Size(Width - 56, TextHeight(value))
            };
            Controls.Add(caption);
            Controls.Add(text);
            y += 22 + text.Height + 12;
        }

        if (!string.IsNullOrWhiteSpace(note))
        {
            var noteBox = new Label
            {
                Text = note, Font = Theme.Small, ForeColor = Theme.Secondary, BackColor = Theme.Field,
                AutoSize = false, Location = new Point(28, y), Size = new Size(Width - 56, 52),
                Padding = new Padding(12, 10, 12, 10)
            };
            Controls.Add(noteBox);
            y += 62;
        }

        var done = new PillButton
        {
            Text = "Close", Filled = true, Accent = Theme.Primary, Size = new Size(110, 40),
            Location = new Point(Width - 28 - 110, y + 8)
        };
        done.Click += (_, _) => Close();
        Controls.Add(done);
        Height = y + 70;

        MouseDown += (_, _) => Theme.Drag(this);
        titleLabel.MouseDown += (_, _) => Theme.Drag(this);
        subtitleLabel.MouseDown += (_, _) => Theme.Drag(this);

        Region = new Region(Theme.Rounded(ClientRectangle, 16));
    }

    private static int TextHeight(string text)
    {
        var height = TextRenderer.MeasureText(text, Theme.Body,
            new Size(440, int.MaxValue), TextFormatFlags.WordBreak).Height;
        return Math.Max(20, Math.Min(90, height));
    }

    public static void Show(IWin32Window owner, string title, string subtitle,
        IEnumerable<(string Label, string Value)> rows, string? note = null)
    {
        using var dialog = new DetailsDialog(title, subtitle, rows, note);
        dialog.ShowDialog(owner);
    }
}
