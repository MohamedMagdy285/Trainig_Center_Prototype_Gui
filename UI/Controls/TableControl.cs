namespace ECMS;

public enum TableRowAction { Edit, Delete }

public sealed class TableRowActionEventArgs : EventArgs
{
    public TableRowActionEventArgs(int rowIndex, TableRowAction action)
    {
        RowIndex = rowIndex;
        Action = action;
    }

    public int RowIndex { get; }
    public TableRowAction Action { get; }
}

/// <summary>A lightweight, hand-drawn data table with avatars, pills and hover rows.</summary>
public partial class TableControl : UserControl
{
    private int _hoverRow = -1;
    private int _hoverActionRow = -1;
    private int _hoverActionIndex = -1;

    public string[] Headers { get; set; } = Array.Empty<string>();
    public float[] Weights { get; set; } = Array.Empty<float>();
    public List<string[]> Rows { get; set; } = new();
    public HashSet<int> PillColumns { get; set; } = new();
    public HashSet<int> RightAligned { get; set; } = new();
    public bool ShowAvatar { get; set; }
    public int RowHeight { get; set; } = 51;
    public int HeaderHeight { get; set; } = 38;
    public string EmptyText { get; set; } = "Nothing to show yet.";
    public bool ShowActions { get; set; }
    public int ActionWidth { get; set; } = 84;

    public event EventHandler<TableRowActionEventArgs>? ActionClicked;

    /// <summary>Raised when the user clicks a normal table row (not the action icons).</summary>
    public event EventHandler<int>? RowClicked;

    public string? RowText(int index) =>
        index >= 0 && index < Rows.Count ? Rows[index].FirstOrDefault() : null;

    public int PreferredHeight =>
        HeaderHeight + (Rows.Count == 0 ? 74 : Rows.Count * RowHeight) + 6;

    public TableControl()
    {
        InitializeComponent();
    }

    public void SetRows(IEnumerable<string[]> rows)
    {
        Rows = rows.ToList();
        Height = PreferredHeight;
        _hoverRow = -1;
        _hoverActionRow = -1;
        _hoverActionIndex = -1;
        Invalidate();
    }

    private int TableWidth => Math.Max(0, ShowActions ? Width - ActionWidth : Width);

    private (int row, int action) ActionAt(Point location)
    {
        if (!ShowActions) return (-1, -1);
        if (location.Y < HeaderHeight) return (-1, -1);
        var row = (location.Y - HeaderHeight) / RowHeight;
        if (row < 0 || row >= Rows.Count) return (-1, -1);
        var x = location.X - (Width - ActionWidth);
        if (x >= 12 && x < 38) return (row, 0);
        if (x >= 46 && x < 72) return (row, 1);
        return (-1, -1);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var index = e.Y < HeaderHeight ? -1 : (e.Y - HeaderHeight) / RowHeight;
        if (index >= Rows.Count) index = -1;

        var (actionRow, actionIndex) = ActionAt(e.Location);
        if (index != _hoverRow || actionRow != _hoverActionRow || actionIndex != _hoverActionIndex)
        {
            _hoverRow = index;
            _hoverActionRow = actionRow;
            _hoverActionIndex = actionIndex;
            Cursor = actionRow >= 0 ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hoverRow = -1;
        _hoverActionRow = -1;
        _hoverActionIndex = -1;
        Cursor = Cursors.Default;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        var (actionRow, action) = ActionAt(e.Location);
        if (actionRow >= 0)
        {
            ActionClicked?.Invoke(this, new TableRowActionEventArgs(actionRow,
                action == 1 ? TableRowAction.Delete : TableRowAction.Edit));
            return;
        }

        if (e.Y < HeaderHeight) return;
        var row = (e.Y - HeaderHeight) / RowHeight;
        if (row >= 0 && row < Rows.Count)
            RowClicked?.Invoke(this, row);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        g.Clear(Color.White);

        if (Headers.Length == 0) return;
        var columns = Headers.Length;
        var totalWeight = Weights.Length == columns ? Weights.Sum() : columns;
        var tableWidth = TableWidth;
        var edges = new float[columns + 1];
        for (var i = 0; i < columns; i++)
        {
            var weight = Weights.Length == columns ? Weights[i] : 1f;
            edges[i + 1] = edges[i] + tableWidth * weight / totalWeight;
        }

        Theme.Fill(g, Theme.Field, new RectangleF(0, 0, Width, HeaderHeight), 9);
        for (var i = 0; i < columns; i++)
            Theme.Text(g, Headers[i], Theme.SmallBold, Theme.Muted,
                new Rectangle((int)edges[i] + 12, 0, (int)(edges[i + 1] - edges[i]) - 18, HeaderHeight));

        if (ShowActions)
            Theme.Text(g, "Actions", Theme.SmallBold, Theme.Muted,
                new Rectangle(Width - ActionWidth + 8, 0, ActionWidth - 14, HeaderHeight),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        if (Rows.Count == 0)
        {
            Theme.CenteredText(g, EmptyText, Theme.Body, Theme.SoftText,
                new Rectangle(0, HeaderHeight, tableWidth, 70));
            return;
        }

        for (var r = 0; r < Rows.Count; r++)
        {
            var y = HeaderHeight + r * RowHeight;
            if (r == _hoverRow)
                Theme.Fill(g, Color.FromArgb(248, 250, 255), new RectangleF(0, y, Width, RowHeight), 8);

            var row = Rows[r];
            for (var c = 0; c < columns && c < row.Length; c++)
            {
                var left = (int)edges[c] + 12;
                var cellWidth = (int)(edges[c + 1] - edges[c]) - 20;
                var bounds = new Rectangle(left, y, Math.Max(8, cellWidth), RowHeight);

                if (c == 0 && ShowAvatar)
                {
                    var avatarRect = new Rectangle(left, y + (RowHeight - 34) / 2, 34, 34);
                    Theme.Avatar(g, avatarRect, row[c], Theme.AccentFor(row[c]));
                    Theme.Text(g, row[c], Theme.BodyBold, Theme.Ink,
                        new Rectangle(left + 44, y, cellWidth - 44, RowHeight));
                    continue;
                }

                if (PillColumns.Contains(c))
                {
                    var (fg, bg) = Theme.StatusColors(row[c]);
                    var pillW = Math.Min(cellWidth, Theme.Measure(g, row[c], Theme.TinyBold) + 20);
                    var x = RightAligned.Contains(c) ? left + cellWidth - pillW : left;
                    Theme.Pill(g, row[c], Theme.TinyBold, fg, bg,
                        new Rectangle(x, y + (RowHeight - 23) / 2, pillW, 23), 11);
                    continue;
                }

                var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                if (RightAligned.Contains(c))
                {
                    flags |= TextFormatFlags.Right;
                    bounds = new Rectangle(left, y, cellWidth + 8, RowHeight);
                }
                Theme.Text(g, row[c], Theme.Body, Theme.Muted, bounds, flags);
            }

            if (ShowActions)
                DrawActions(g, r, y);

            using var sep = new Pen(Theme.Mix(Theme.Border, Color.White, .25f));
            g.DrawLine(sep, 12, y + RowHeight - 1, Width - 12, y + RowHeight - 1);
        }
    }

    private void DrawActions(Graphics g, int row, int y)
    {
        var start = Width - ActionWidth;
        var editRect = new Rectangle(start + 12, y + (RowHeight - 26) / 2, 26, 26);
        var deleteRect = new Rectangle(start + 46, y + (RowHeight - 26) / 2, 26, 26);

        if (_hoverActionRow == row && _hoverActionIndex == 0)
            Theme.Fill(g, Theme.Mix(Theme.Primary, Color.White, .90f), editRect, 8);
        if (_hoverActionRow == row && _hoverActionIndex == 1)
            Theme.Fill(g, Theme.Mix(Theme.Red, Color.White, .90f), deleteRect, 8);

        Theme.CenteredText(g, "\uE70F", Theme.IconSmall, Theme.Primary, editRect);
        Theme.CenteredText(g, "\uE74D", Theme.IconSmall, Theme.Red, deleteRect);
    }
}
