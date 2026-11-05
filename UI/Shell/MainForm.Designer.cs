namespace ECMS;

partial class MainForm
{
    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "EduSphere Academy";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Bg;
        Size = new Size(1440, 900);
        MinimumSize = new Size(1180, 720);
        DoubleBuffered = true;
        KeyPreview = true;
        ResumeLayout(false);
    }
}

