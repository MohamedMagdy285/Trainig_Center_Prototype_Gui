namespace ECMS;

partial class IconButton
{
    private void InitializeComponent()
    {
        SuspendLayout();
        Width = 42;
        Height = 42;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ResumeLayout(false);
    }
}
