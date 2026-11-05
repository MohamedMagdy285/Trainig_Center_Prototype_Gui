namespace ECMS;

partial class FieldBox
{
    private void InitializeComponent()
    {
        SuspendLayout();
        Height = 38;
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.Transparent;
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint, true);
        ResumeLayout(false);
    }
}
