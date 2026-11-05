using System.Drawing.Drawing2D;

namespace ECMS;

/// <summary>Dark sidebar with a subtle gradient and two soft glows.</summary>
public partial class SidebarPanel : UserControl
{
    public SidebarPanel()
    {
        InitializeComponent();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.SetupGraphics(g);
        using (var brush = new LinearGradientBrush(ClientRectangle, Theme.SidebarTop, Theme.SidebarBottom, 90f))
            g.FillRectangle(brush, ClientRectangle);
        using (var glow = new SolidBrush(Color.FromArgb(30, Theme.Accent)))
        {
            g.FillEllipse(glow, Width - 170, -100, 300, 300);
            g.FillEllipse(glow, -140, Height - 230, 280, 280);
        }
    }
}
