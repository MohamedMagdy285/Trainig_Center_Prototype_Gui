namespace ECMS;

partial class ConfirmDialog
{
    private Label iconLabel = null!;
    private Label titleLabel = null!;
    private Label messageLabel = null!;
    private Label errorLabel = null!;
    private IconButton closeButton = null!;
    private PillButton cancelButton = null!;
    private PillButton deleteButton = null!;

    private void InitializeComponent()
    {
        iconLabel = new Label();
        titleLabel = new Label();
        messageLabel = new Label();
        errorLabel = new Label();
        closeButton = new IconButton();
        cancelButton = new PillButton();
        deleteButton = new PillButton();
        SuspendLayout();
        // 
        // iconLabel
        // 
        iconLabel.BackColor = Color.Transparent;
        iconLabel.Font = new Font("Segoe MDL2 Assets", 17F);
        iconLabel.ForeColor = Color.FromArgb(239, 68, 68);
        iconLabel.Location = new Point(30, 30);
        iconLabel.Name = "iconLabel";
        iconLabel.Size = new Size(50, 50);
        iconLabel.TabIndex = 0;
        iconLabel.Text = "";
        iconLabel.TextAlign = ContentAlignment.MiddleCenter;
        iconLabel.MouseDown += DragWindow_MouseDown;
        // 
        // titleLabel
        // 
        titleLabel.AutoSize = true;
        titleLabel.BackColor = Color.Transparent;
        titleLabel.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        titleLabel.ForeColor = Color.FromArgb(15, 23, 42);
        titleLabel.Location = new Point(94, 30);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(101, 19);
        titleLabel.TabIndex = 1;
        titleLabel.Text = "Delete record";
        titleLabel.MouseDown += DragWindow_MouseDown;
        // 
        // messageLabel
        // 
        messageLabel.BackColor = Color.Transparent;
        messageLabel.Font = new Font("Segoe UI", 9.5F);
        messageLabel.ForeColor = Color.FromArgb(100, 116, 139);
        messageLabel.Location = new Point(48, 68);
        messageLabel.Name = "messageLabel";
        messageLabel.Size = new Size(330, 74);
        messageLabel.TabIndex = 2;
        // 
        // errorLabel
        // 
        errorLabel.AutoSize = true;
        errorLabel.BackColor = Color.Transparent;
        errorLabel.Font = new Font("Segoe UI", 8.5F);
        errorLabel.ForeColor = Color.FromArgb(239, 68, 68);
        errorLabel.Location = new Point(30, 142);
        errorLabel.Name = "errorLabel";
        errorLabel.Size = new Size(171, 15);
        errorLabel.TabIndex = 3;
        errorLabel.Text = "This action can not be undone.";
        // 
        // closeButton
        // 
        closeButton.BackColor = Color.Transparent;
        closeButton.Glyph = "";
        closeButton.Location = new Point(404, 16);
        closeButton.Name = "closeButton";
        closeButton.ShowBadge = false;
        closeButton.Size = new Size(38, 38);
        closeButton.TabIndex = 4;
        closeButton.Click += CloseButton_Click;
        // 
        // cancelButton
        // 
        cancelButton.Accent = Color.FromArgb(99, 91, 255);
        cancelButton.BackColor = Color.Transparent;
        cancelButton.Filled = false;
        cancelButton.Glyph = "";
        cancelButton.Location = new Point(238, 182);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(96, 40);
        cancelButton.TabIndex = 5;
        cancelButton.Load += cancelButton_Load;
        cancelButton.Click += CancelButton_Click;
        // 
        // deleteButton
        // 
        deleteButton.Accent = Color.FromArgb(239, 68, 68);
        deleteButton.BackColor = Color.Transparent;
        deleteButton.Filled = true;
        deleteButton.Glyph = "";
        deleteButton.Location = new Point(344, 182);
        deleteButton.Name = "deleteButton";
        deleteButton.Size = new Size(110, 40);
        deleteButton.TabIndex = 6;
        deleteButton.Click += DeleteButton_Click;
        // 
        // ConfirmDialog
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(244, 246, 251);
        ClientSize = new Size(460, 250);
        Controls.Add(iconLabel);
        Controls.Add(titleLabel);
        Controls.Add(messageLabel);
        Controls.Add(errorLabel);
        Controls.Add(closeButton);
        Controls.Add(cancelButton);
        Controls.Add(deleteButton);
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        KeyPreview = true;
        Name = "ConfirmDialog";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Confirm";
        MouseDown += DragWindow_MouseDown;
        ResumeLayout(false);
        PerformLayout();
    }
}