namespace ECMS;

/// <summary>Soft search field with a real text box and placeholder support.</summary>
public partial class SearchBox : UserControl
{
    private bool _focused;

    public event EventHandler? ValueChanged;

    public SearchBox() : this("Search...") { }

    public SearchBox(string placeholder)
    {
        InitializeComponent();
        _box.PlaceholderText = placeholder;
        _box.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        _box.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        _box.TextChanged += (_, _) => ValueChanged?.Invoke(this, EventArgs.Empty);
        _icon.MouseDown += (_, _) => _box.Focus();
        Resize += (_, _) =>
        {
            _box.Top = (Height - _box.PreferredHeight) / 2;
            _box.Width = Math.Max(20, Width - 48);
            Region = new Region(Theme.Rounded(ClientRectangle, 11));
        };
    }

    public string Value => _box.Text;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Theme.SetupGraphics(e.Graphics);
        var r = new RectangleF(.5f, .5f, Width - 1, Height - 1);
        Theme.DrawBorder(e.Graphics, _focused ? Theme.Mix(Theme.Primary, Color.White, .25f) : Theme.Border, r, 11);
    }
}
