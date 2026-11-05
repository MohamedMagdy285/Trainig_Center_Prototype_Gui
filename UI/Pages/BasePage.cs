namespace ECMS;

/// <summary>
/// Shared scrollable page scaffold plus small layout helpers.
/// Every page is a normal UserControl that adds blocks to <see cref="Body"/>.
/// </summary>
public partial class BasePage : UserControl
{
    protected readonly Action<string> Navigate;

    protected BasePage(Action<string> navigate)
    {
        Navigate = navigate;
        InitializeComponent();
    }

    /// <summary>Parameterless constructor so the base page itself can open in the designer.</summary>
    public BasePage() : this(_ => { }) { }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ResizeBlocks();
    }

    protected void AddBlock(Control control, int height)
    {
        control.Height = height;
        control.Margin = new Padding(0, 0, 0, 16);
        control.Width = AvailableWidth();
        Body.Controls.Add(control);
    }

    protected int AvailableWidth()
    {
        var scrollbar = Body.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
        return Math.Max(260, Body.ClientSize.Width - Body.Padding.Horizontal - scrollbar - 2);
    }

    protected void ResizeBlocks()
    {
        var width = AvailableWidth();
        foreach (Control control in Body.Controls)
            control.Width = width;
    }

    protected void RefreshLayout()
    {
        ResizeBlocks();
        Body.PerformLayout();
    }

    private void Body_SizeChanged(object? sender, EventArgs e) => ResizeBlocks();

    // ---------- header ----------

    protected Panel Header(string title, string subtitle, string? action = null,
        string glyph = "", Action? onClick = null)
        => Header(title, subtitle, action, glyph, onClick, out _);

    protected Panel Header(string title, string subtitle, string? action, string glyph,
        Action? onClick, out PillButton? actionButton)
    {
        var panel = new Panel { Height = 60, BackColor = Theme.Bg };
        var titleLabel = new Label
        {
            Text = title, Font = Theme.Title, ForeColor = Theme.Ink, BackColor = Theme.Bg,
            AutoSize = true, Location = new Point(0, -3)
        };
        var subtitleLabel = new Label
        {
            Text = subtitle, Font = Theme.Body, ForeColor = Theme.Muted, BackColor = Theme.Bg,
            AutoSize = true, Location = new Point(1, 34)
        };
        panel.Controls.Add(titleLabel);
        panel.Controls.Add(subtitleLabel);

        actionButton = null;
        if (!string.IsNullOrEmpty(action))
        {
            var button = new PillButton
            {
                Text = action, Glyph = glyph, Filled = true,
                Size = new Size(TextRenderer.MeasureText(action, Theme.BodyBold).Width + 54, 40),
                Location = new Point(panel.Width - 160, 8)
            };
            button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            if (onClick is not null) button.Click += (_, _) => onClick();
            panel.Controls.Add(button);
            panel.Resize += (_, _) => button.Left = panel.Width - button.Width;
            actionButton = button;
        }
        return panel;
    }

    // ---------- cards ----------

    protected Card NewCard(string title, string subtitle, int height = 300)
    {
        var card = new Card { BackColor = Color.White, Height = height };
        AddCardHeader(card, title, subtitle);
        AddBlock(card, height);
        return card;
    }

    protected static void AddCardHeader(Card card, string title, string subtitle,
        string? action = null, Action? onClick = null)
    {
        var titleLabel = new Label
        {
            Text = title, Font = Theme.H2, ForeColor = Theme.Ink, BackColor = Color.White,
            AutoSize = true, Location = new Point(20, 16)
        };
        var subtitleLabel = new Label
        {
            Text = subtitle, Font = Theme.Small, ForeColor = Theme.Muted, BackColor = Color.White,
            AutoSize = true, Location = new Point(20, 41)
        };
        card.Controls.Add(titleLabel);
        card.Controls.Add(subtitleLabel);

        if (string.IsNullOrEmpty(action)) return;
        var link = new Label
        {
            Text = action, Font = Theme.SmallBold, ForeColor = Theme.Accent, BackColor = Color.White,
            AutoSize = true, Cursor = Cursors.Hand, Location = new Point(card.Width - 85, 20)
        };
        link.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        if (onClick is not null) link.Click += (_, _) => onClick();
        card.Controls.Add(link);
        card.Resize += (_, _) => link.Left = card.Width - link.Width - 20;
    }

    protected static TableControl MakeTable(string[] headers, float[] weights,
        bool avatar = false, bool actions = false, params int[] pillColumns)
    {
        var table = new TableControl
        {
            Headers = headers, Weights = weights, ShowAvatar = avatar,
            ShowActions = actions, RowHeight = 50
        };
        foreach (var column in pillColumns) table.PillColumns.Add(column);
        return table;
    }

    protected Card AddTableCard(string title, string subtitle, TableControl table,
        string? action = null, Action? onClick = null, string emptyText = "Nothing to show yet.")
    {
        table.EmptyText = emptyText;
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, title, subtitle, action, onClick);
        table.Location = new Point(20, 72);
        table.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        card.Controls.Add(table);
        FitTableCard(card, table);
        card.Resize += (_, _) => table.Width = Math.Max(100, card.Width - 40);
        AddBlock(card, card.Height);
        return card;
    }

    protected static void FitTableCard(Card card, TableControl table)
    {
        table.Width = Math.Max(100, card.Width - 40);
        card.Height = 80 + table.PreferredHeight;
    }

    // ---------- stats ----------

    protected static StatCard Stat(string title, string value, string note, string glyph, Color accent)
        => new()
        {
            Title = title, Value = value, Note = note, Glyph = glyph, Accent = accent
        };

    protected TableLayoutPanel StatRow(params StatCard[] stats)
    {
        var grid = new TableLayoutPanel
        {
            ColumnCount = stats.Length * 2 - 1, RowCount = 1, Height = 118,
            BackColor = Theme.Bg, Margin = Padding.Empty, Padding = Padding.Empty
        };
        for (var i = 0; i < stats.Length; i++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / stats.Length));
            if (i < stats.Length - 1)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 14f));
            stats[i].Dock = DockStyle.Fill;
            stats[i].Margin = Padding.Empty;
            grid.Controls.Add(stats[i], i * 2, 0);
        }
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        AddBlock(grid, 118);
        return grid;
    }

    // ---------- buttons ----------

    protected static PillButton Button(string text, Action onClick, bool filled = true,
        string glyph = "", Color? accent = null)
    {
        var button = new PillButton
        {
            Text = text, Glyph = glyph, Filled = filled,
            Accent = accent ?? Theme.Primary,
            Size = new Size(TextRenderer.MeasureText(text, Theme.BodyBold).Width + 54, 40)
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    protected static Label TextLabel(string text, Font font, Color color)
        => new()
        {
            Text = text, Font = font, ForeColor = color, BackColor = Color.Transparent,
            AutoSize = true
        };

    protected static Label BodyText(string text)
        => new()
        {
            Text = text, Font = Theme.Body, ForeColor = Theme.Muted, BackColor = Color.Transparent,
            AutoSize = false
        };

    // ---------- profile helpers ----------

    /// <summary>A round avatar that draws itself, used by both profile pages.</summary>
    protected static Panel ProfileAvatar(string name, int size)
    {
        var panel = new Panel { Size = new Size(size, size), BackColor = Color.White };
        panel.Paint += (_, e) =>
        {
            Theme.SetupGraphics(e.Graphics);
            Theme.Avatar(e.Graphics, new Rectangle(0, 0, size, size), name, Theme.AccentFor(name));
        };
        return panel;
    }

    /// <summary>A small caption and value pair inside a card.</summary>
    protected static void AddInfo(Card card, string caption, string value, int x, int y, int width)
    {
        var label = new Label
        {
            Text = caption.ToUpperInvariant(), Font = Theme.TinyBold, ForeColor = Theme.SoftText,
            BackColor = Color.White, AutoSize = false, Location = new Point(x, y),
            Size = new Size(width, 16)
        };
        var text = new Label
        {
            Text = value, Font = Theme.BodyBold, ForeColor = Theme.Ink, BackColor = Color.White,
            AutoSize = false, Location = new Point(x, y + 17), Size = new Size(width, 22)
        };
        card.Controls.Add(label);
        card.Controls.Add(text);
    }

    // ---------- legacy grid helpers ----------

    protected static TableLayoutPanel Grid(int columns, int rows, Control[] items, int height, int gap = 14)
    {
        var grid = new TableLayoutPanel
        {
            ColumnCount = columns * 2 - 1, RowCount = rows * 2 - 1, Height = height,
            BackColor = Theme.Bg, Margin = Padding.Empty, Padding = Padding.Empty
        };
        for (var c = 0; c < columns; c++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            if (c < columns - 1)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, gap));
        }
        for (var r = 0; r < rows; r++)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
            if (r < rows - 1)
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, gap));
        }

        for (var i = 0; i < items.Length && i < columns * rows; i++)
        {
            var c = i % columns;
            var r = i / columns;
            items[i].Dock = DockStyle.Fill;
            items[i].Margin = Padding.Empty;
            grid.Controls.Add(items[i], c * 2, r * 2);
        }
        return grid;
    }

    protected static TableLayoutPanel Split(Control left, Control right, int height, float leftPercent = 64f)
    {
        var grid = new TableLayoutPanel
        {
            ColumnCount = 3, RowCount = 1, Height = height,
            BackColor = Theme.Bg, Margin = Padding.Empty, Padding = Padding.Empty
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, leftPercent));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f - leftPercent));
        left.Dock = DockStyle.Fill;
        left.Margin = Padding.Empty;
        right.Dock = DockStyle.Fill;
        right.Margin = Padding.Empty;
        grid.Controls.Add(left, 0, 0);
        grid.Controls.Add(right, 2, 0);
        return grid;
    }
}
