namespace ECMS;

/// <summary>Compact list of upcoming offline batches with seat progress.</summary>
public partial class UpcomingBatchesCard : UserControl
{
    public UpcomingBatchesCard()
    {
        InitializeComponent();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width > 0 && Height > 0)
            Region = new Region(Theme.Rounded(ClientRectangle, 16));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        var outer = new RectangleF(.5f, .5f, Width - 1, Height - 1);
        Theme.Fill(g, Color.White, outer, 16);
        Theme.DrawBorder(g, Theme.Border, outer, 16);

        Theme.Text(g, "Upcoming batches", Theme.H2, Theme.Ink, new Rectangle(20, 15, 220, 24));
        Theme.Text(g, "Next scheduled offline groups", Theme.Small, Theme.Muted,
            new Rectangle(20, 39, 260, 18));

        var batches = CatalogService.UpcomingBatches(3).ToList();
        if (batches.Count == 0)
        {
            Theme.Text(g, "No upcoming batches yet.", Theme.Body, Theme.SoftText,
                new Rectangle(20, 80, Width - 40, 24));
            return;
        }

        for (var i = 0; i < batches.Count; i++)
        {
            var batch = batches[i];
            var course = CatalogService.Course(batch.CourseId);
            if (course is null) continue;
            var accent = Theme.AccentFor(course.Title);
            var y = 76 + i * 70;
            Theme.Fill(g, Theme.Mix(accent, Color.White, .90f), new RectangleF(18, y, 50, 50), 12);
            Theme.Text(g, batch.StartDate.ToString("MMM").ToUpperInvariant(), Theme.TinyBold, accent,
                new Rectangle(18, y + 5, 50, 14), TextFormatFlags.HorizontalCenter);
            using var dayFont = new Font("Segoe UI", 14f, FontStyle.Bold);
            Theme.Text(g, batch.StartDate.Day.ToString(), dayFont, accent,
                new Rectangle(18, y + 17, 50, 24), TextFormatFlags.HorizontalCenter);

            var textX = 80;
            var textW = Width - textX - 24;
            Theme.Text(g, course.Title, Theme.SmallBold, Theme.Ink,
                new Rectangle(textX, y + 1, textW, 19));
            Theme.Text(g, $"{batch.SeatsLeft} of {batch.Capacity} seats left  •  {batch.Schedule}", Theme.Small, Theme.Muted,
                new Rectangle(textX, y + 20, textW, 17));

            var bar = new Rectangle(textX, y + 43, textW, 5);
            Theme.Fill(g, Theme.Field, bar, 2.5f);
            var filled = (int)(bar.Width * (batch.ConfirmedSeats / (float)Math.Max(1, batch.Capacity)));
            if (filled > 0)
                Theme.Fill(g, accent, new Rectangle(bar.X, bar.Y, filled, bar.Height), 2.5f);
        }
    }
}
