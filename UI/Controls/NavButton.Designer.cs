namespace ECMS;

partial class NavButton
{
    private void InitializeComponent()
    {
        SuspendLayout();
        Height = 46;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ResumeLayout(false);
    }
}
