namespace ECMS;

partial class SearchBox
{
    private Label _icon = null!;
    private TextBox _box = null!;

    private void InitializeComponent()
    {
        SuspendLayout();
        _icon = new Label();
        _icon.Text = "\uE721";
        _icon.Font = Theme.IconSmall;
        _icon.ForeColor = Theme.SoftText;
        _icon.BackColor = Theme.Field;
        _icon.AutoSize = false;
        _icon.TextAlign = ContentAlignment.MiddleCenter;
        _icon.Bounds = new Rectangle(8, 0, 26, 40);
        _icon.Cursor = Cursors.IBeam;

        _box = new TextBox();
        _box.BorderStyle = BorderStyle.None;
        _box.BackColor = Theme.Field;
        _box.ForeColor = Theme.Ink;
        _box.Font = Theme.Body;
        _box.Location = new Point(36, 10);
        _box.Width = 192;

        Height = 40;
        Width = 240;
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Theme.Field;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        Controls.Add(_icon);
        Controls.Add(_box);
        ResumeLayout(false);
        PerformLayout();
    }
}