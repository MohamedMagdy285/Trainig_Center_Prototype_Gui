namespace ECMS;

partial class BasePage
{
    protected FlowLayoutPanel Body = null!;

    private void InitializeComponent()
    {
        SuspendLayout();
        Body = new FlowLayoutPanel();
        Body.Dock = DockStyle.Fill;
        Body.AutoScroll = true;
        Body.FlowDirection = FlowDirection.TopDown;
        Body.WrapContents = false;
        Body.BackColor = Theme.Bg;
        Body.Padding = new Padding(28, 22, 28, 26);
        DoubleBuffered = true;
        BackColor = Theme.Bg;
        Controls.Add(Body);
        Body.SizeChanged += Body_SizeChanged;
        ResumeLayout(false);
        PerformLayout();
    }
}