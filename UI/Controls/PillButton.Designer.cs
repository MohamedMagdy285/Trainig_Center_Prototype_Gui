namespace ECMS;

partial class PillButton
{
    private void InitializeComponent()
    {
        SuspendLayout();
        Height = 40;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ResumeLayout(false);
    }
}
