namespace ECMS;

/// <summary>Small themed confirmation dialog used before deleting a record.</summary>
public partial class ConfirmDialog : Form
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public static bool Ask(IWin32Window owner, string title, string message, string confirmText = "Delete")
    {
        using var dialog = new ConfirmDialog();
        dialog.Text = title;
        dialog.titleLabel.Text = title;
        dialog.messageLabel.Text = message;
        dialog.deleteButton.Text = confirmText;
        dialog.LayoutDialog();
        return dialog.ShowDialog(owner) == DialogResult.OK;
    }

    private void LayoutDialog()
    {
        SuspendLayout();
        deleteButton.Width = TextRenderer.MeasureText(deleteButton.Text, Theme.BodyBold).Width + 54;
        cancelButton.Width = TextRenderer.MeasureText(cancelButton.Text, Theme.BodyBold).Width + 54;

        closeButton.Left = Width - 54;
        errorLabel.Left = 30;

        var footerTop = Height - 68;
        deleteButton.Location = new Point(Width - 30 - deleteButton.Width, footerTop);
        cancelButton.Location = new Point(deleteButton.Left - cancelButton.Width - 10, footerTop);
        ResumeLayout();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width <= 0 || Height <= 0) return;
        Region = new Region(Theme.Rounded(ClientRectangle, 16));
        LayoutDialog();
    }

    private void DeleteButton_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void CloseButton_Click(object? sender, EventArgs e) => CancelButton_Click(sender, e);

    private void DragWindow_MouseDown(object? sender, MouseEventArgs e) => Theme.Drag(this);

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void cancelButton_Load(object sender, EventArgs e)
    {

    }
}
