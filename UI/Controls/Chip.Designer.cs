namespace ECMS;

partial class Chip
{
    private void InitializeComponent()
    {
        SuspendLayout();
        Height = 32;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ResumeLayout(false);
    }
}
