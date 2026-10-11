namespace MusicConverterPro
{
    partial class AboutDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            tableLayoutPanel = new TableLayoutPanel();
            logoPictureBox = new PictureBox();
            labelProductName = new Label();
            labelVersion = new Label();
            labelCopyright = new Label();
            labelCompanyName = new Label();
            textBoxDescription = new TextBox();
            panelButtons = new Panel();
            linkLicense = new LinkLabel();
            linkNotices = new LinkLabel();
            okButton = new Button();
            tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
            panelButtons.SuspendLayout();
            SuspendLayout();
            //
            // tableLayoutPanel
            //
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(logoPictureBox, 0, 0);
            tableLayoutPanel.Controls.Add(labelProductName, 1, 0);
            tableLayoutPanel.Controls.Add(labelVersion, 1, 1);
            tableLayoutPanel.Controls.Add(labelCopyright, 1, 2);
            tableLayoutPanel.Controls.Add(labelCompanyName, 1, 3);
            tableLayoutPanel.Controls.Add(textBoxDescription, 1, 4);
            tableLayoutPanel.Controls.Add(panelButtons, 1, 5);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(12, 12);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 6;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            tableLayoutPanel.Size = new Size(576, 416);
            tableLayoutPanel.TabIndex = 0;
            //
            // logoPictureBox
            //
            logoPictureBox.Dock = DockStyle.Fill;
            logoPictureBox.Location = new Point(12, 12);
            logoPictureBox.Margin = new Padding(12, 12, 24, 12);
            logoPictureBox.Name = "logoPictureBox";
            tableLayoutPanel.SetRowSpan(logoPictureBox, 6);
            logoPictureBox.Size = new Size(114, 392);
            logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoPictureBox.TabIndex = 12;
            logoPictureBox.TabStop = false;
            //
            // labelProductName
            //
            labelProductName.Dock = DockStyle.Fill;
            labelProductName.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            labelProductName.Location = new Point(150, 0);
            labelProductName.Margin = new Padding(0);
            labelProductName.Name = "labelProductName";
            labelProductName.Size = new Size(426, 30);
            labelProductName.TabIndex = 19;
            labelProductName.Text = "Music Converter Pro";
            labelProductName.TextAlign = ContentAlignment.MiddleLeft;
            //
            // labelVersion
            //
            labelVersion.Dock = DockStyle.Fill;
            labelVersion.Location = new Point(150, 30);
            labelVersion.Margin = new Padding(0);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(426, 22);
            labelVersion.TabIndex = 0;
            labelVersion.Text = "Versione";
            labelVersion.TextAlign = ContentAlignment.MiddleLeft;
            //
            // labelCopyright
            //
            labelCopyright.Dock = DockStyle.Fill;
            labelCopyright.Location = new Point(150, 52);
            labelCopyright.Margin = new Padding(0);
            labelCopyright.Name = "labelCopyright";
            labelCopyright.Size = new Size(426, 22);
            labelCopyright.TabIndex = 21;
            labelCopyright.Text = "Copyright";
            labelCopyright.TextAlign = ContentAlignment.MiddleLeft;
            //
            // labelCompanyName
            //
            labelCompanyName.Dock = DockStyle.Fill;
            labelCompanyName.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelCompanyName.Location = new Point(150, 74);
            labelCompanyName.Margin = new Padding(0);
            labelCompanyName.Name = "labelCompanyName";
            labelCompanyName.Size = new Size(426, 22);
            labelCompanyName.TabIndex = 22;
            labelCompanyName.Text = "Licenza";
            labelCompanyName.TextAlign = ContentAlignment.MiddleLeft;
            //
            // textBoxDescription
            //
            textBoxDescription.BorderStyle = BorderStyle.FixedSingle;
            textBoxDescription.Dock = DockStyle.Fill;
            textBoxDescription.Location = new Point(150, 104);
            textBoxDescription.Margin = new Padding(0, 8, 0, 8);
            textBoxDescription.Multiline = true;
            textBoxDescription.Name = "textBoxDescription";
            textBoxDescription.ReadOnly = true;
            textBoxDescription.ScrollBars = ScrollBars.Vertical;
            textBoxDescription.Size = new Size(426, 260);
            textBoxDescription.TabIndex = 23;
            textBoxDescription.TabStop = false;
            //
            // panelButtons
            //
            panelButtons.Controls.Add(linkLicense);
            panelButtons.Controls.Add(linkNotices);
            panelButtons.Controls.Add(okButton);
            panelButtons.Dock = DockStyle.Fill;
            panelButtons.Location = new Point(150, 372);
            panelButtons.Margin = new Padding(0);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(426, 44);
            panelButtons.TabIndex = 24;
            //
            // linkLicense
            //
            linkLicense.AutoSize = true;
            linkLicense.Location = new Point(0, 14);
            linkLicense.Name = "linkLicense";
            linkLicense.TabIndex = 0;
            linkLicense.TabStop = true;
            linkLicense.Text = "Testo della licenza";
            linkLicense.LinkClicked += linkLicense_LinkClicked;
            //
            // linkNotices
            //
            linkNotices.AutoSize = true;
            linkNotices.Location = new Point(130, 14);
            linkNotices.Name = "linkNotices";
            linkNotices.TabIndex = 1;
            linkNotices.TabStop = true;
            linkNotices.Text = "Componenti di terze parti";
            linkNotices.LinkClicked += linkNotices_LinkClicked;
            //
            // okButton
            //
            okButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            okButton.DialogResult = DialogResult.Cancel;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            okButton.Location = new Point(330, 8);
            okButton.Name = "okButton";
            okButton.Size = new Size(96, 32);
            okButton.TabIndex = 2;
            okButton.Text = "Chiudi";
            okButton.Click += okButton_Click;
            //
            // AboutDialog
            //
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = okButton;
            ClientSize = new Size(600, 440);
            Controls.Add(tableLayoutPanel);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutDialog";
            Padding = new Padding(12);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Informazioni";
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
            panelButtons.ResumeLayout(false);
            panelButtons.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel;
        private PictureBox logoPictureBox;
        private Label labelProductName;
        private Label labelVersion;
        private Label labelCopyright;
        private Label labelCompanyName;
        private TextBox textBoxDescription;
        private Panel panelButtons;
        private LinkLabel linkLicense;
        private LinkLabel linkNotices;
        private Button okButton;
    }
}
