namespace ECMS;

partial class WindowButton
{
    private void InitializeComponent()
    {
        SuspendLayout();
        Width = 44;
        Height = 40;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ResumeLayout(false);
    }
}
