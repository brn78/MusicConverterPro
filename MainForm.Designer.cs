using MusicConverterPro.UI;

namespace MusicConverterPro
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();

            notifyIcon = new NotifyIcon(components);
            clipboardTimer = new System.Windows.Forms.Timer(components);

            // Barra laterale
            panelSidebar = new Panel();
            labelBrand = new Label();
            labelBrandSub = new Label();
            buttonNavDownloader = new Button();
            buttonNavOptimizer = new Button();
            cardEngine = new CardPanel();
            labelEngineTitle = new Label();
            labelEngineState = new Label();
            dotYtdlp = new Panel();
            labelEngineYtdlp = new Label();
            dotFfmpeg = new Panel();
            labelEngineFfmpeg = new Label();
            dotJs = new Panel();
            labelEngineJs = new Label();
            comboBoxTheme = new ComboBox();
            buttonAbout = new Button();

            // Contenuto
            panelContent = new Panel();

            // Scaricatore
            panelDownloaderContainer = new Panel();
            panelDlHeader = new Panel();
            labelDlTitle = new Label();
            labelDlSubtitle = new Label();
            toggleAutoPaste = new ToggleSwitch();
            cardInput = new CardPanel();
            textBoxUrl = new TextBox();
            buttonPaste = new Button();
            buttonAddQueue = new Button();
            buttonDownloadDirect = new Button();
            labelFormat = new Label();
            comboBoxFormat = new ComboBox();
            labelBitrate = new Label();
            comboBoxBitrate = new ComboBox();
            toggleThumbnail = new ToggleSwitch();
            toggleMetadata = new ToggleSwitch();
            toggleCarAudio = new ToggleSwitch();
            toggleCleanNames = new ToggleSwitch();
            labelFolderHeader = new Label();
            textBoxFolder = new TextBox();
            buttonBrowseFolder = new Button();
            buttonOpenFolder = new Button();
            panelDlSpacer1 = new Panel();
            cardQueue = new CardPanel();
            labelQueueHeader = new Label();
            labelQueueCount = new Label();
            buttonRemoveSelected = new Button();
            btnClearQueue = new Button();
            listViewQueue = new ListView();
            columnHeaderTitle = new ColumnHeader();
            columnHeaderFormat = new ColumnHeader();
            columnHeaderQuality = new ColumnHeader();
            columnHeaderProgress = new ColumnHeader();
            columnHeaderStatus = new ColumnHeader();
            panelDlSpacer2 = new Panel();
            cardDlBottom = new CardPanel();
            labelStatusText = new Label();
            labelMetricsText = new Label();
            progressBar = new FlatProgressBar();
            labelPercentText = new Label();
            buttonStartQueue = new Button();
            buttonCancel = new Button();

            // Ottimizzatore
            panelOptimizerContainer = new Panel();
            panelOptHeader = new Panel();
            labelOptTitle = new Label();
            labelOptSubtitle = new Label();
            buttonOptScan = new Button();
            cardOptDrive = new CardPanel();
            labelOptDriveHeader = new Label();
            comboBoxOptDrive = new ComboBox();
            textBoxOptFolder = new TextBox();
            buttonOptBrowse = new Button();
            buttonOptOpenFolder = new Button();
            panelOptSpacer1 = new Panel();
            cardOptOptions = new CardPanel();
            labelOptSettingsHeader = new Label();
            toggleOptNormalize = new ToggleSwitch();
            toggleOptFixStandard = new ToggleSwitch();
            toggleOptConvertAll = new ToggleSwitch();
            toggleOptSkipCompliant = new ToggleSwitch();
            toggleOptInPlace = new ToggleSwitch();
            toggleOptRemoveDuplicates = new ToggleSwitch();
            toggleOptCleanNames = new ToggleSwitch();
            panelOptSpacer2 = new Panel();
            cardOptQueue = new CardPanel();
            labelOptQueueHeader = new Label();
            labelOptSummary = new Label();
            buttonOptClear = new Button();
            buttonOptRefresh = new Button();
            listViewOpt = new ListView();
            columnHeaderOptFile = new ColumnHeader();
            columnHeaderOptFolder = new ColumnHeader();
            columnHeaderOptFormat = new ColumnHeader();
            columnHeaderOptSize = new ColumnHeader();
            columnHeaderOptStatus = new ColumnHeader();
            panelOptSpacer3 = new Panel();
            cardOptBottom = new CardPanel();
            labelOptStatus = new Label();
            labelOptMetrics = new Label();
            progressBarOpt = new FlatProgressBar();
            labelOptPercent = new Label();
            buttonOptStart = new Button();
            buttonOptCancel = new Button();

            panelSidebar.SuspendLayout();
            cardEngine.SuspendLayout();
            panelContent.SuspendLayout();
            panelDownloaderContainer.SuspendLayout();
            panelDlHeader.SuspendLayout();
            cardInput.SuspendLayout();
            cardQueue.SuspendLayout();
            cardDlBottom.SuspendLayout();
            panelOptimizerContainer.SuspendLayout();
            panelOptHeader.SuspendLayout();
            cardOptDrive.SuspendLayout();
            cardOptOptions.SuspendLayout();
            cardOptQueue.SuspendLayout();
            cardOptBottom.SuspendLayout();
            SuspendLayout();

            //
            // notifyIcon / timer
            //
            notifyIcon.BalloonTipTitle = "Music Converter Pro";
            notifyIcon.Text = "Music Converter Pro";
            notifyIcon.BalloonTipClicked += notifyIcon_BalloonTipClicked;
            notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            clipboardTimer.Interval = 750;
            clipboardTimer.Tick += clipboardTimer_Tick;

            // =====================================================================
            // BARRA LATERALE
            // =====================================================================
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Size = new Size(224, 780);
            panelSidebar.Tag = "sidebar";
            panelSidebar.Controls.Add(labelBrand);
            panelSidebar.Controls.Add(labelBrandSub);
            panelSidebar.Controls.Add(buttonNavDownloader);
            panelSidebar.Controls.Add(buttonNavOptimizer);
            panelSidebar.Controls.Add(cardEngine);
            panelSidebar.Controls.Add(comboBoxTheme);
            panelSidebar.Controls.Add(buttonAbout);

            labelBrand.AutoSize = true;
            labelBrand.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            labelBrand.Location = new Point(16, 22);
            labelBrand.Name = "labelBrand";
            labelBrand.Text = "Music Converter Pro";

            labelBrandSub.AutoSize = true;
            labelBrandSub.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            labelBrandSub.Location = new Point(17, 46);
            labelBrandSub.Name = "labelBrandSub";
            labelBrandSub.Tag = "muted";
            labelBrandSub.Text = "v2.6 · scarica, converti, ottimizza";

            buttonNavDownloader.FlatAppearance.BorderSize = 0;
            buttonNavDownloader.FlatStyle = FlatStyle.Flat;
            buttonNavDownloader.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            buttonNavDownloader.Location = new Point(14, 88);
            buttonNavDownloader.Name = "buttonNavDownloader";
            buttonNavDownloader.Padding = new Padding(10, 0, 0, 0);
            buttonNavDownloader.Size = new Size(196, 40);
            buttonNavDownloader.Tag = "nav";
            buttonNavDownloader.Text = "Scaricatore";
            buttonNavDownloader.TextAlign = ContentAlignment.MiddleLeft;
            buttonNavDownloader.UseVisualStyleBackColor = false;
            buttonNavDownloader.Click += buttonTabDownloader_Click;

            buttonNavOptimizer.FlatAppearance.BorderSize = 0;
            buttonNavOptimizer.FlatStyle = FlatStyle.Flat;
            buttonNavOptimizer.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            buttonNavOptimizer.Location = new Point(14, 134);
            buttonNavOptimizer.Name = "buttonNavOptimizer";
            buttonNavOptimizer.Padding = new Padding(10, 0, 0, 0);
            buttonNavOptimizer.Size = new Size(196, 40);
            buttonNavOptimizer.Tag = "nav";
            buttonNavOptimizer.Text = "Ottimizza USB";
            buttonNavOptimizer.TextAlign = ContentAlignment.MiddleLeft;
            buttonNavOptimizer.UseVisualStyleBackColor = false;
            buttonNavOptimizer.Click += buttonTabOptimizer_Click;

            //
            // cardEngine (stato motore)
            //
            cardEngine.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cardEngine.Location = new Point(14, 622);
            cardEngine.Name = "cardEngine";
            cardEngine.Size = new Size(196, 100);
            cardEngine.Tag = "sidebar-card";
            cardEngine.Controls.Add(labelEngineTitle);
            cardEngine.Controls.Add(labelEngineState);
            cardEngine.Controls.Add(dotYtdlp);
            cardEngine.Controls.Add(labelEngineYtdlp);
            cardEngine.Controls.Add(dotFfmpeg);
            cardEngine.Controls.Add(labelEngineFfmpeg);
            cardEngine.Controls.Add(dotJs);
            cardEngine.Controls.Add(labelEngineJs);

            labelEngineTitle.AutoSize = true;
            labelEngineTitle.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point);
            labelEngineTitle.Location = new Point(12, 10);
            labelEngineTitle.Name = "labelEngineTitle";
            labelEngineTitle.Tag = "caption";
            labelEngineTitle.Text = "MOTORE";

            labelEngineState.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelEngineState.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
            labelEngineState.Location = new Point(96, 8);
            labelEngineState.Name = "labelEngineState";
            labelEngineState.Size = new Size(88, 18);
            labelEngineState.Tag = "success";
            labelEngineState.Text = "Verifica...";
            labelEngineState.TextAlign = ContentAlignment.MiddleRight;

            dotYtdlp.Location = new Point(12, 39);
            dotYtdlp.Name = "dotYtdlp";
            dotYtdlp.Size = new Size(8, 8);
            dotYtdlp.Tag = "dot";

            labelEngineYtdlp.AutoSize = true;
            labelEngineYtdlp.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelEngineYtdlp.Location = new Point(26, 34);
            labelEngineYtdlp.Name = "labelEngineYtdlp";
            labelEngineYtdlp.Text = "yt-dlp";

            dotFfmpeg.Location = new Point(12, 59);
            dotFfmpeg.Name = "dotFfmpeg";
            dotFfmpeg.Size = new Size(8, 8);
            dotFfmpeg.Tag = "dot";

            labelEngineFfmpeg.AutoSize = true;
            labelEngineFfmpeg.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelEngineFfmpeg.Location = new Point(26, 54);
            labelEngineFfmpeg.Name = "labelEngineFfmpeg";
            labelEngineFfmpeg.Text = "FFmpeg";

            dotJs.Location = new Point(12, 79);
            dotJs.Name = "dotJs";
            dotJs.Size = new Size(8, 8);
            dotJs.Tag = "dot";

            labelEngineJs.AutoSize = true;
            labelEngineJs.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelEngineJs.Location = new Point(26, 74);
            labelEngineJs.Name = "labelEngineJs";
            labelEngineJs.Text = "Runtime JS";

            comboBoxTheme.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            comboBoxTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxTheme.FlatStyle = FlatStyle.Flat;
            comboBoxTheme.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            comboBoxTheme.FormattingEnabled = true;
            comboBoxTheme.Items.AddRange(new object[] { "Tema: automatico", "Tema: scuro", "Tema: chiaro" });
            comboBoxTheme.Location = new Point(14, 737);
            comboBoxTheme.Name = "comboBoxTheme";
            comboBoxTheme.Size = new Size(146, 23);
            comboBoxTheme.SelectedIndexChanged += comboBoxTheme_SelectedIndexChanged;

            buttonAbout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonAbout.FlatAppearance.BorderSize = 0;
            buttonAbout.FlatStyle = FlatStyle.Flat;
            buttonAbout.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            buttonAbout.Location = new Point(166, 737);
            buttonAbout.Name = "buttonAbout";
            buttonAbout.Size = new Size(44, 23);
            buttonAbout.Tag = "ghost";
            buttonAbout.Text = "Info";
            buttonAbout.UseVisualStyleBackColor = false;
            buttonAbout.Click += buttonAbout_Click;

            // =====================================================================
            // CONTENUTO
            // =====================================================================
            panelContent.Dock = DockStyle.Fill;
            panelContent.Name = "panelContent";
            panelContent.Padding = new Padding(24, 16, 24, 16);
            panelContent.Size = new Size(976, 780);
            panelContent.Controls.Add(panelDownloaderContainer);
            panelContent.Controls.Add(panelOptimizerContainer);

            // ---------------------------------------------------------------------
            // SCARICATORE
            // ---------------------------------------------------------------------
            panelDownloaderContainer.Dock = DockStyle.Fill;
            panelDownloaderContainer.Name = "panelDownloaderContainer";
            panelDownloaderContainer.Size = new Size(928, 748);
            // Ordine: il controllo Fill per primo, poi quelli ancorati ai bordi (dal più interno al più esterno)
            panelDownloaderContainer.Controls.Add(cardQueue);
            panelDownloaderContainer.Controls.Add(panelDlSpacer2);
            panelDownloaderContainer.Controls.Add(cardDlBottom);
            panelDownloaderContainer.Controls.Add(panelDlSpacer1);
            panelDownloaderContainer.Controls.Add(cardInput);
            panelDownloaderContainer.Controls.Add(panelDlHeader);

            //
            // panelDlHeader
            //
            panelDlHeader.Dock = DockStyle.Top;
            panelDlHeader.Size = new Size(928, 60);
            panelDlHeader.Name = "panelDlHeader";
            panelDlHeader.Controls.Add(labelDlTitle);
            panelDlHeader.Controls.Add(labelDlSubtitle);
            panelDlHeader.Controls.Add(toggleAutoPaste);

            labelDlTitle.AutoSize = true;
            labelDlTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
            labelDlTitle.Location = new Point(0, 0);
            labelDlTitle.Name = "labelDlTitle";
            labelDlTitle.Text = "Scaricatore";

            labelDlSubtitle.AutoSize = true;
            labelDlSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelDlSubtitle.Location = new Point(1, 32);
            labelDlSubtitle.Name = "labelDlSubtitle";
            labelDlSubtitle.Tag = "muted";
            labelDlSubtitle.Text = "Estrai l'audio da video e playlist dei siti supportati";

            toggleAutoPaste.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            toggleAutoPaste.Checked = true;
            toggleAutoPaste.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleAutoPaste.Location = new Point(638, 14);
            toggleAutoPaste.Name = "toggleAutoPaste";
            toggleAutoPaste.Size = new Size(290, 24);
            toggleAutoPaste.Text = "Auto-Paste: aggiunge i link copiati";
            toggleAutoPaste.CheckedChanged += checkBoxAutoPaste_CheckedChanged;

            //
            // cardInput (link + opzioni + cartella)
            //
            cardInput.Dock = DockStyle.Top;
            cardInput.Size = new Size(928, 182);
            cardInput.Name = "cardInput";
            cardInput.Controls.Add(toggleCleanNames);
            cardInput.Controls.Add(textBoxUrl);
            cardInput.Controls.Add(buttonPaste);
            cardInput.Controls.Add(buttonAddQueue);
            cardInput.Controls.Add(buttonDownloadDirect);
            cardInput.Controls.Add(labelFormat);
            cardInput.Controls.Add(comboBoxFormat);
            cardInput.Controls.Add(labelBitrate);
            cardInput.Controls.Add(comboBoxBitrate);
            cardInput.Controls.Add(toggleThumbnail);
            cardInput.Controls.Add(toggleMetadata);
            cardInput.Controls.Add(toggleCarAudio);
            cardInput.Controls.Add(labelFolderHeader);
            cardInput.Controls.Add(textBoxFolder);
            cardInput.Controls.Add(buttonBrowseFolder);
            cardInput.Controls.Add(buttonOpenFolder);

            textBoxUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxUrl.BorderStyle = BorderStyle.FixedSingle;
            textBoxUrl.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxUrl.Location = new Point(16, 18);
            textBoxUrl.Name = "textBoxUrl";
            textBoxUrl.PlaceholderText = "Incolla qui il link di un video o di una playlist";
            textBoxUrl.Size = new Size(496, 27);
            textBoxUrl.KeyDown += textBoxUrl_KeyDown;

            buttonPaste.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonPaste.FlatStyle = FlatStyle.Flat;
            buttonPaste.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            buttonPaste.Location = new Point(520, 18);
            buttonPaste.Name = "buttonPaste";
            buttonPaste.Size = new Size(96, 27);
            buttonPaste.Tag = "ghost";
            buttonPaste.Text = "Incolla";
            buttonPaste.UseVisualStyleBackColor = false;
            buttonPaste.Click += buttonPaste_Click;

            buttonAddQueue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonAddQueue.FlatStyle = FlatStyle.Flat;
            buttonAddQueue.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            buttonAddQueue.Location = new Point(624, 18);
            buttonAddQueue.Name = "buttonAddQueue";
            buttonAddQueue.Size = new Size(150, 27);
            buttonAddQueue.Tag = "secondary";
            buttonAddQueue.Text = "Aggiungi in coda";
            buttonAddQueue.UseVisualStyleBackColor = false;
            buttonAddQueue.Click += buttonAddQueue_Click;

            buttonDownloadDirect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonDownloadDirect.FlatStyle = FlatStyle.Flat;
            buttonDownloadDirect.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            buttonDownloadDirect.Location = new Point(782, 18);
            buttonDownloadDirect.Name = "buttonDownloadDirect";
            buttonDownloadDirect.Size = new Size(130, 27);
            buttonDownloadDirect.Tag = "primary";
            buttonDownloadDirect.Text = "Scarica ora";
            buttonDownloadDirect.UseVisualStyleBackColor = false;
            buttonDownloadDirect.Click += buttonDownloadDirect_Click;

            labelFormat.AutoSize = true;
            labelFormat.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelFormat.Location = new Point(16, 66);
            labelFormat.Name = "labelFormat";
            labelFormat.Tag = "muted";
            labelFormat.Text = "Formato";

            comboBoxFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxFormat.FlatStyle = FlatStyle.Flat;
            comboBoxFormat.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            comboBoxFormat.FormattingEnabled = true;
            comboBoxFormat.Items.AddRange(new object[] { "MP3", "M4A", "FLAC", "WAV", "OPUS" });
            comboBoxFormat.Location = new Point(76, 63);
            comboBoxFormat.Name = "comboBoxFormat";
            comboBoxFormat.Size = new Size(90, 23);
            comboBoxFormat.SelectedIndexChanged += comboBoxFormat_SelectedIndexChanged;

            labelBitrate.AutoSize = true;
            labelBitrate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelBitrate.Location = new Point(180, 66);
            labelBitrate.Name = "labelBitrate";
            labelBitrate.Tag = "muted";
            labelBitrate.Text = "Qualità";

            comboBoxBitrate.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxBitrate.FlatStyle = FlatStyle.Flat;
            comboBoxBitrate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            comboBoxBitrate.FormattingEnabled = true;
            comboBoxBitrate.Items.AddRange(new object[] { "320 kbps · massima", "256 kbps · alta", "192 kbps · standard", "128 kbps · compatta" });
            comboBoxBitrate.Location = new Point(232, 63);
            comboBoxBitrate.Name = "comboBoxBitrate";
            comboBoxBitrate.Size = new Size(150, 23);
            comboBoxBitrate.SelectedIndexChanged += comboBoxBitrate_SelectedIndexChanged;

            toggleThumbnail.Checked = true;
            toggleThumbnail.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleThumbnail.Location = new Point(400, 62);
            toggleThumbnail.Name = "toggleThumbnail";
            toggleThumbnail.Size = new Size(140, 24);
            toggleThumbnail.Text = "Copertina HD";
            toggleThumbnail.CheckedChanged += checkBoxThumbnail_CheckedChanged;

            toggleMetadata.Checked = true;
            toggleMetadata.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleMetadata.Location = new Point(548, 62);
            toggleMetadata.Name = "toggleMetadata";
            toggleMetadata.Size = new Size(100, 24);
            toggleMetadata.Text = "Tag ID3";
            toggleMetadata.CheckedChanged += checkBoxMetadata_CheckedChanged;

            toggleCarAudio.Checked = true;
            toggleCarAudio.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleCarAudio.Location = new Point(656, 62);
            toggleCarAudio.Name = "toggleCarAudio";
            toggleCarAudio.Size = new Size(176, 24);
            toggleCarAudio.Text = "Autoradio · 44,1 kHz";
            toggleCarAudio.CheckedChanged += checkBoxCarAudio_CheckedChanged;

            toggleCleanNames.Checked = true;
            toggleCleanNames.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleCleanNames.Location = new Point(16, 142);
            toggleCleanNames.Name = "toggleCleanNames";
            toggleCleanNames.Size = new Size(640, 24);
            toggleCleanNames.Text = "Nomi file puliti: via (Official Video), 360p, 256k, simboli · tag titolo e artista coerenti";
            toggleCleanNames.CheckedChanged += checkBoxCleanNames_CheckedChanged;

            labelFolderHeader.AutoSize = true;
            labelFolderHeader.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelFolderHeader.Location = new Point(16, 108);
            labelFolderHeader.Name = "labelFolderHeader";
            labelFolderHeader.Tag = "muted";
            labelFolderHeader.Text = "Salva in";

            textBoxFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxFolder.BorderStyle = BorderStyle.FixedSingle;
            textBoxFolder.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxFolder.Location = new Point(76, 105);
            textBoxFolder.Name = "textBoxFolder";
            textBoxFolder.ReadOnly = true;
            textBoxFolder.Size = new Size(610, 24);

            buttonBrowseFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonBrowseFolder.FlatStyle = FlatStyle.Flat;
            buttonBrowseFolder.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            buttonBrowseFolder.Location = new Point(694, 105);
            buttonBrowseFolder.Name = "buttonBrowseFolder";
            buttonBrowseFolder.Size = new Size(100, 24);
            buttonBrowseFolder.Tag = "ghost";
            buttonBrowseFolder.Text = "Sfoglia...";
            buttonBrowseFolder.UseVisualStyleBackColor = false;
            buttonBrowseFolder.Click += buttonBrowseFolder_Click;

            buttonOpenFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOpenFolder.FlatStyle = FlatStyle.Flat;
            buttonOpenFolder.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            buttonOpenFolder.Location = new Point(802, 105);
            buttonOpenFolder.Name = "buttonOpenFolder";
            buttonOpenFolder.Size = new Size(110, 24);
            buttonOpenFolder.Tag = "ghost";
            buttonOpenFolder.Text = "Apri cartella";
            buttonOpenFolder.UseVisualStyleBackColor = false;
            buttonOpenFolder.Click += buttonOpenFolder_Click;

            panelDlSpacer1.Dock = DockStyle.Top;
            panelDlSpacer1.Height = 12;
            panelDlSpacer1.Name = "panelDlSpacer1";

            //
            // cardQueue
            //
            cardQueue.Dock = DockStyle.Fill;
            cardQueue.Name = "cardQueue";
            cardQueue.Size = new Size(928, 420);
            cardQueue.Controls.Add(labelQueueHeader);
            cardQueue.Controls.Add(labelQueueCount);
            cardQueue.Controls.Add(buttonRemoveSelected);
            cardQueue.Controls.Add(btnClearQueue);
            cardQueue.Controls.Add(listViewQueue);

            labelQueueHeader.AutoSize = true;
            labelQueueHeader.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            labelQueueHeader.Location = new Point(16, 16);
            labelQueueHeader.Name = "labelQueueHeader";
            labelQueueHeader.Text = "Coda di download";

            labelQueueCount.AutoSize = true;
            labelQueueCount.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelQueueCount.Location = new Point(150, 18);
            labelQueueCount.Name = "labelQueueCount";
            labelQueueCount.Tag = "muted";
            labelQueueCount.Text = "0 elementi";

            buttonRemoveSelected.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRemoveSelected.FlatStyle = FlatStyle.Flat;
            buttonRemoveSelected.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            buttonRemoveSelected.Location = new Point(644, 13);
            buttonRemoveSelected.Name = "buttonRemoveSelected";
            buttonRemoveSelected.Size = new Size(150, 27);
            buttonRemoveSelected.Tag = "ghost";
            buttonRemoveSelected.Text = "Rimuovi selezionati";
            buttonRemoveSelected.UseVisualStyleBackColor = false;
            buttonRemoveSelected.Click += buttonRemoveSelected_Click;

            btnClearQueue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearQueue.FlatStyle = FlatStyle.Flat;
            btnClearQueue.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnClearQueue.Location = new Point(802, 13);
            btnClearQueue.Name = "btnClearQueue";
            btnClearQueue.Size = new Size(110, 27);
            btnClearQueue.Tag = "ghost";
            btnClearQueue.Text = "Svuota coda";
            btnClearQueue.UseVisualStyleBackColor = false;
            btnClearQueue.Click += btnClearQueue_Click;

            listViewQueue.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewQueue.BorderStyle = BorderStyle.None;
            listViewQueue.Columns.AddRange(new ColumnHeader[] { columnHeaderTitle, columnHeaderFormat, columnHeaderQuality, columnHeaderProgress, columnHeaderStatus });
            listViewQueue.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            listViewQueue.FullRowSelect = true;
            listViewQueue.HeaderStyle = ColumnHeaderStyle.Clickable;
            listViewQueue.Location = new Point(16, 52);
            listViewQueue.Name = "listViewQueue";
            listViewQueue.Size = new Size(896, 352);
            listViewQueue.UseCompatibleStateImageBehavior = false;
            listViewQueue.View = View.Details;
            listViewQueue.DoubleClick += listViewQueue_DoubleClick;
            listViewQueue.KeyDown += listViewQueue_KeyDown;

            columnHeaderTitle.Text = "Titolo / link";
            columnHeaderTitle.Width = 420;
            columnHeaderFormat.Text = "Formato";
            columnHeaderFormat.Width = 80;
            columnHeaderQuality.Text = "Qualità";
            columnHeaderQuality.Width = 90;
            columnHeaderProgress.Text = "Avanzamento";
            columnHeaderProgress.Width = 150;
            columnHeaderStatus.Text = "Stato";
            columnHeaderStatus.Width = 150;

            panelDlSpacer2.Dock = DockStyle.Bottom;
            panelDlSpacer2.Height = 12;
            panelDlSpacer2.Name = "panelDlSpacer2";

            //
            // cardDlBottom (avanzamento globale + azioni)
            //
            cardDlBottom.Dock = DockStyle.Bottom;
            cardDlBottom.Size = new Size(928, 100);
            cardDlBottom.Name = "cardDlBottom";
            cardDlBottom.Tag = "bar";
            cardDlBottom.Controls.Add(labelStatusText);
            cardDlBottom.Controls.Add(labelMetricsText);
            cardDlBottom.Controls.Add(progressBar);
            cardDlBottom.Controls.Add(labelPercentText);
            cardDlBottom.Controls.Add(buttonStartQueue);
            cardDlBottom.Controls.Add(buttonCancel);

            labelStatusText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelStatusText.AutoEllipsis = true;
            labelStatusText.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelStatusText.Location = new Point(16, 14);
            labelStatusText.Name = "labelStatusText";
            labelStatusText.Size = new Size(570, 18);
            labelStatusText.Text = "Pronto. Incolla un link per iniziare.";

            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(16, 44);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(516, 8);

            labelPercentText.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelPercentText.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            labelPercentText.Location = new Point(536, 36);
            labelPercentText.Name = "labelPercentText";
            labelPercentText.Size = new Size(50, 24);
            labelPercentText.Tag = "accent";
            labelPercentText.Text = "0%";
            labelPercentText.TextAlign = ContentAlignment.MiddleRight;

            labelMetricsText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelMetricsText.AutoEllipsis = true;
            labelMetricsText.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            labelMetricsText.Location = new Point(16, 64);
            labelMetricsText.Name = "labelMetricsText";
            labelMetricsText.Size = new Size(570, 18);
            labelMetricsText.Tag = "muted";
            labelMetricsText.Text = "Velocità -  ·  rimanente -";

            buttonStartQueue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonStartQueue.FlatStyle = FlatStyle.Flat;
            buttonStartQueue.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            buttonStartQueue.Location = new Point(604, 36);
            buttonStartQueue.Name = "buttonStartQueue";
            buttonStartQueue.Size = new Size(190, 27);
            buttonStartQueue.Tag = "primary";
            buttonStartQueue.Text = "Avvia coda";
            buttonStartQueue.UseVisualStyleBackColor = false;
            buttonStartQueue.Click += buttonStartQueue_Click;

            buttonCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonCancel.Enabled = false;
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            buttonCancel.Location = new Point(802, 36);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(110, 27);
            buttonCancel.Tag = "secondary";
            buttonCancel.Text = "Annulla";
            buttonCancel.UseVisualStyleBackColor = false;
            buttonCancel.Click += buttonCancel_Click;

            // ---------------------------------------------------------------------
            // OTTIMIZZA USB
            // ---------------------------------------------------------------------
            panelOptimizerContainer.Dock = DockStyle.Fill;
            panelOptimizerContainer.Name = "panelOptimizerContainer";
            panelOptimizerContainer.Size = new Size(928, 748);
            panelOptimizerContainer.Visible = false;
            panelOptimizerContainer.Controls.Add(cardOptQueue);
            panelOptimizerContainer.Controls.Add(panelOptSpacer3);
            panelOptimizerContainer.Controls.Add(cardOptBottom);
            panelOptimizerContainer.Controls.Add(panelOptSpacer2);
            panelOptimizerContainer.Controls.Add(cardOptOptions);
            panelOptimizerContainer.Controls.Add(panelOptSpacer1);
            panelOptimizerContainer.Controls.Add(cardOptDrive);
            panelOptimizerContainer.Controls.Add(panelOptHeader);

            panelOptHeader.Dock = DockStyle.Top;
            panelOptHeader.Size = new Size(928, 60);
            panelOptHeader.Name = "panelOptHeader";
            panelOptHeader.Controls.Add(labelOptTitle);
            panelOptHeader.Controls.Add(labelOptSubtitle);
            panelOptHeader.Controls.Add(buttonOptScan);

            labelOptTitle.AutoSize = true;
            labelOptTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
            labelOptTitle.Location = new Point(0, 0);
            labelOptTitle.Name = "labelOptTitle";
            labelOptTitle.Text = "Ottimizza USB";

            labelOptSubtitle.AutoSize = true;
            labelOptSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelOptSubtitle.Location = new Point(1, 32);
            labelOptSubtitle.Name = "labelOptSubtitle";
            labelOptSubtitle.Tag = "muted";
            labelOptSubtitle.Text = "Rende i file leggibili da qualsiasi autoradio: MP3 320 kbps, 44,1 kHz stereo, tag ID3v2.3";

            buttonOptScan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOptScan.FlatStyle = FlatStyle.Flat;
            buttonOptScan.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            buttonOptScan.Location = new Point(798, 12);
            buttonOptScan.Name = "buttonOptScan";
            buttonOptScan.Size = new Size(130, 27);
            buttonOptScan.Tag = "ghost";
            buttonOptScan.Text = "Analizza";
            buttonOptScan.UseVisualStyleBackColor = false;
            buttonOptScan.Click += buttonOptScan_Click;

            //
            // cardOptDrive
            //
            cardOptDrive.Dock = DockStyle.Top;
            cardOptDrive.Size = new Size(928, 74);
            cardOptDrive.Name = "cardOptDrive";
            cardOptDrive.Controls.Add(labelOptDriveHeader);
            cardOptDrive.Controls.Add(comboBoxOptDrive);
            cardOptDrive.Controls.Add(textBoxOptFolder);
            cardOptDrive.Controls.Add(buttonOptBrowse);
            cardOptDrive.Controls.Add(buttonOptOpenFolder);

            labelOptDriveHeader.AutoSize = true;
            labelOptDriveHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point);
            labelOptDriveHeader.Location = new Point(16, 12);
            labelOptDriveHeader.Name = "labelOptDriveHeader";
            labelOptDriveHeader.Tag = "caption";
            labelOptDriveHeader.Text = "UNITÀ USB O CARTELLA";

            comboBoxOptDrive.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOptDrive.FlatStyle = FlatStyle.Flat;
            comboBoxOptDrive.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            comboBoxOptDrive.FormattingEnabled = true;
            comboBoxOptDrive.Location = new Point(16, 36);
            comboBoxOptDrive.Name = "comboBoxOptDrive";
            comboBoxOptDrive.Size = new Size(262, 23);
            comboBoxOptDrive.DropDown += comboBoxOptDrive_DropDown;
            comboBoxOptDrive.SelectedIndexChanged += comboBoxOptDrive_SelectedIndexChanged;

            textBoxOptFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxOptFolder.BorderStyle = BorderStyle.FixedSingle;
            textBoxOptFolder.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxOptFolder.Location = new Point(288, 36);
            textBoxOptFolder.Name = "textBoxOptFolder";
            textBoxOptFolder.PlaceholderText = "Percorso della pendrive o della cartella (Invio per analizzare)";
            textBoxOptFolder.Size = new Size(398, 24);
            textBoxOptFolder.KeyDown += textBoxOptFolder_KeyDown;

            buttonOptBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOptBrowse.FlatStyle = FlatStyle.Flat;
            buttonOptBrowse.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            buttonOptBrowse.Location = new Point(694, 36);
            buttonOptBrowse.Name = "buttonOptBrowse";
            buttonOptBrowse.Size = new Size(100, 24);
            buttonOptBrowse.Tag = "ghost";
            buttonOptBrowse.Text = "Sfoglia...";
            buttonOptBrowse.UseVisualStyleBackColor = false;
            buttonOptBrowse.Click += buttonOptBrowse_Click;

            buttonOptOpenFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOptOpenFolder.FlatStyle = FlatStyle.Flat;
            buttonOptOpenFolder.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            buttonOptOpenFolder.Location = new Point(802, 36);
            buttonOptOpenFolder.Name = "buttonOptOpenFolder";
            buttonOptOpenFolder.Size = new Size(110, 24);
            buttonOptOpenFolder.Tag = "ghost";
            buttonOptOpenFolder.Text = "Apri cartella";
            buttonOptOpenFolder.UseVisualStyleBackColor = false;
            buttonOptOpenFolder.Click += buttonOptOpenFolder_Click;

            panelOptSpacer1.Dock = DockStyle.Top;
            panelOptSpacer1.Height = 12;
            panelOptSpacer1.Name = "panelOptSpacer1";

            //
            // cardOptOptions
            //
            cardOptOptions.Dock = DockStyle.Top;
            cardOptOptions.Size = new Size(928, 146);
            cardOptOptions.Name = "cardOptOptions";
            cardOptOptions.Controls.Add(toggleOptCleanNames);
            cardOptOptions.Controls.Add(labelOptSettingsHeader);
            cardOptOptions.Controls.Add(toggleOptNormalize);
            cardOptOptions.Controls.Add(toggleOptFixStandard);
            cardOptOptions.Controls.Add(toggleOptConvertAll);
            cardOptOptions.Controls.Add(toggleOptSkipCompliant);
            cardOptOptions.Controls.Add(toggleOptInPlace);
            cardOptOptions.Controls.Add(toggleOptRemoveDuplicates);

            labelOptSettingsHeader.AutoSize = true;
            labelOptSettingsHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point);
            labelOptSettingsHeader.Location = new Point(16, 12);
            labelOptSettingsHeader.Name = "labelOptSettingsHeader";
            labelOptSettingsHeader.Tag = "caption";
            labelOptSettingsHeader.Text = "OPZIONI";

            toggleOptNormalize.Checked = true;
            toggleOptNormalize.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleOptNormalize.Location = new Point(16, 38);
            toggleOptNormalize.Name = "toggleOptNormalize";
            toggleOptNormalize.Size = new Size(262, 24);
            toggleOptNormalize.Text = "Normalizza il volume (EBU R128)";
            toggleOptNormalize.CheckedChanged += optToggle_CheckedChanged;

            toggleOptFixStandard.Checked = true;
            toggleOptFixStandard.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleOptFixStandard.Location = new Point(290, 38);
            toggleOptFixStandard.Name = "toggleOptFixStandard";
            toggleOptFixStandard.Size = new Size(262, 24);
            toggleOptFixStandard.Text = "44,1 kHz · stereo · tag ID3v2.3";
            toggleOptFixStandard.CheckedChanged += optToggle_CheckedChanged;

            toggleOptConvertAll.Checked = true;
            toggleOptConvertAll.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleOptConvertAll.Location = new Point(564, 38);
            toggleOptConvertAll.Name = "toggleOptConvertAll";
            toggleOptConvertAll.Size = new Size(262, 24);
            toggleOptConvertAll.Text = "Converti in MP3 320 kbps";
            toggleOptConvertAll.CheckedChanged += optToggle_CheckedChanged;

            toggleOptSkipCompliant.Checked = true;
            toggleOptSkipCompliant.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleOptSkipCompliant.Location = new Point(16, 72);
            toggleOptSkipCompliant.Name = "toggleOptSkipCompliant";
            toggleOptSkipCompliant.Size = new Size(262, 24);
            toggleOptSkipCompliant.Text = "Salta i file già conformi";
            toggleOptSkipCompliant.CheckedChanged += optToggle_CheckedChanged;

            toggleOptInPlace.Checked = true;
            toggleOptInPlace.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleOptInPlace.Location = new Point(290, 72);
            toggleOptInPlace.Name = "toggleOptInPlace";
            toggleOptInPlace.Size = new Size(262, 24);
            toggleOptInPlace.Text = "Sostituisci gli originali";
            toggleOptInPlace.CheckedChanged += optToggle_CheckedChanged;

            toggleOptRemoveDuplicates.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            toggleOptRemoveDuplicates.Location = new Point(564, 72);
            toggleOptRemoveDuplicates.Name = "toggleOptRemoveDuplicates";
            toggleOptRemoveDuplicates.Size = new Size(262, 24);
            toggleOptRemoveDuplicates.Tag = "warning";
            toggleOptRemoveDuplicates.Text = "Elimina i doppioni";
            toggleOptRemoveDuplicates.CheckedChanged += checkBoxOptRemoveDuplicates_CheckedChanged;

            toggleOptCleanNames.Checked = true;
            toggleOptCleanNames.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            toggleOptCleanNames.Location = new Point(16, 106);
            toggleOptCleanNames.Name = "toggleOptCleanNames";
            toggleOptCleanNames.Size = new Size(640, 24);
            toggleOptCleanNames.Text = "Rinomina con nomi puliti: via (Official Video), 360p, 256k, simboli · tag titolo e artista";
            toggleOptCleanNames.CheckedChanged += optToggle_CheckedChanged;

            panelOptSpacer2.Dock = DockStyle.Top;
            panelOptSpacer2.Height = 12;
            panelOptSpacer2.Name = "panelOptSpacer2";

            //
            // cardOptQueue
            //
            cardOptQueue.Dock = DockStyle.Fill;
            cardOptQueue.Name = "cardOptQueue";
            cardOptQueue.Size = new Size(928, 380);
            cardOptQueue.Controls.Add(labelOptQueueHeader);
            cardOptQueue.Controls.Add(labelOptSummary);
            cardOptQueue.Controls.Add(buttonOptRefresh);
            cardOptQueue.Controls.Add(buttonOptClear);
            cardOptQueue.Controls.Add(listViewOpt);

            labelOptQueueHeader.AutoSize = true;
            labelOptQueueHeader.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            labelOptQueueHeader.Location = new Point(16, 16);
            labelOptQueueHeader.Name = "labelOptQueueHeader";
            labelOptQueueHeader.Text = "File rilevati";

            labelOptSummary.AutoSize = true;
            labelOptSummary.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelOptSummary.Location = new Point(110, 18);
            labelOptSummary.Name = "labelOptSummary";
            labelOptSummary.Tag = "muted";
            labelOptSummary.Text = "Nessuna analisi eseguita";

            buttonOptRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOptRefresh.FlatStyle = FlatStyle.Flat;
            buttonOptRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            buttonOptRefresh.Location = new Point(684, 13);
            buttonOptRefresh.Name = "buttonOptRefresh";
            buttonOptRefresh.Size = new Size(100, 27);
            buttonOptRefresh.Tag = "ghost";
            buttonOptRefresh.Text = "Aggiorna";
            buttonOptRefresh.UseVisualStyleBackColor = false;
            buttonOptRefresh.Click += buttonOptRefresh_Click;

            buttonOptClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOptClear.FlatStyle = FlatStyle.Flat;
            buttonOptClear.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            buttonOptClear.Location = new Point(792, 13);
            buttonOptClear.Name = "buttonOptClear";
            buttonOptClear.Size = new Size(120, 27);
            buttonOptClear.Tag = "ghost";
            buttonOptClear.Text = "Pulisci elenco";
            buttonOptClear.UseVisualStyleBackColor = false;
            buttonOptClear.Click += buttonOptClear_Click;

            listViewOpt.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewOpt.BorderStyle = BorderStyle.None;
            listViewOpt.Columns.AddRange(new ColumnHeader[] { columnHeaderOptFile, columnHeaderOptFolder, columnHeaderOptFormat, columnHeaderOptSize, columnHeaderOptStatus });
            listViewOpt.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            listViewOpt.FullRowSelect = true;
            listViewOpt.HeaderStyle = ColumnHeaderStyle.Clickable;
            listViewOpt.Location = new Point(16, 52);
            listViewOpt.Name = "listViewOpt";
            listViewOpt.Size = new Size(896, 312);
            listViewOpt.UseCompatibleStateImageBehavior = false;
            listViewOpt.View = View.Details;
            listViewOpt.DoubleClick += listViewOpt_DoubleClick;

            columnHeaderOptFile.Text = "Nome file";
            columnHeaderOptFile.Width = 340;
            columnHeaderOptFolder.Text = "Cartella";
            columnHeaderOptFolder.Width = 150;
            columnHeaderOptFormat.Text = "Formato";
            columnHeaderOptFormat.Width = 70;
            columnHeaderOptSize.Text = "Dimensione";
            columnHeaderOptSize.Width = 90;
            columnHeaderOptStatus.Text = "Esito";
            columnHeaderOptStatus.Width = 240;

            panelOptSpacer3.Dock = DockStyle.Bottom;
            panelOptSpacer3.Height = 12;
            panelOptSpacer3.Name = "panelOptSpacer3";

            //
            // cardOptBottom
            //
            cardOptBottom.Dock = DockStyle.Bottom;
            cardOptBottom.Size = new Size(928, 100);
            cardOptBottom.Name = "cardOptBottom";
            cardOptBottom.Tag = "bar";
            cardOptBottom.Controls.Add(labelOptStatus);
            cardOptBottom.Controls.Add(labelOptMetrics);
            cardOptBottom.Controls.Add(progressBarOpt);
            cardOptBottom.Controls.Add(labelOptPercent);
            cardOptBottom.Controls.Add(buttonOptStart);
            cardOptBottom.Controls.Add(buttonOptCancel);

            labelOptStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelOptStatus.AutoEllipsis = true;
            labelOptStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelOptStatus.Location = new Point(16, 14);
            labelOptStatus.Name = "labelOptStatus";
            labelOptStatus.Size = new Size(540, 18);
            labelOptStatus.Text = "Seleziona una pendrive o una cartella per iniziare.";

            progressBarOpt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBarOpt.Location = new Point(16, 44);
            progressBarOpt.Name = "progressBarOpt";
            progressBarOpt.Size = new Size(486, 8);

            labelOptPercent.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelOptPercent.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            labelOptPercent.Location = new Point(506, 36);
            labelOptPercent.Name = "labelOptPercent";
            labelOptPercent.Size = new Size(50, 24);
            labelOptPercent.Tag = "accent";
            labelOptPercent.Text = "0%";
            labelOptPercent.TextAlign = ContentAlignment.MiddleRight;

            labelOptMetrics.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelOptMetrics.AutoEllipsis = true;
            labelOptMetrics.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            labelOptMetrics.Location = new Point(16, 64);
            labelOptMetrics.Name = "labelOptMetrics";
            labelOptMetrics.Size = new Size(540, 18);
            labelOptMetrics.Tag = "muted";
            labelOptMetrics.Text = "";

            buttonOptStart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOptStart.FlatStyle = FlatStyle.Flat;
            buttonOptStart.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            buttonOptStart.Location = new Point(574, 36);
            buttonOptStart.Name = "buttonOptStart";
            buttonOptStart.Size = new Size(220, 27);
            buttonOptStart.Tag = "primary";
            buttonOptStart.Text = "Avvia ottimizzazione";
            buttonOptStart.UseVisualStyleBackColor = false;
            buttonOptStart.Click += buttonOptStart_Click;

            buttonOptCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOptCancel.Enabled = false;
            buttonOptCancel.FlatStyle = FlatStyle.Flat;
            buttonOptCancel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            buttonOptCancel.Location = new Point(802, 36);
            buttonOptCancel.Name = "buttonOptCancel";
            buttonOptCancel.Size = new Size(110, 27);
            buttonOptCancel.Tag = "secondary";
            buttonOptCancel.Text = "Interrompi";
            buttonOptCancel.UseVisualStyleBackColor = false;
            buttonOptCancel.Click += buttonOptCancel_Click;

            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 780);
            Controls.Add(panelContent);
            Controls.Add(panelSidebar);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(1120, 720);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Music Converter Pro";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;

            panelSidebar.ResumeLayout(false);
            panelSidebar.PerformLayout();
            cardEngine.ResumeLayout(false);
            cardEngine.PerformLayout();
            panelContent.ResumeLayout(false);
            panelDownloaderContainer.ResumeLayout(false);
            panelDlHeader.ResumeLayout(false);
            panelDlHeader.PerformLayout();
            cardInput.ResumeLayout(false);
            cardInput.PerformLayout();
            cardQueue.ResumeLayout(false);
            cardQueue.PerformLayout();
            cardDlBottom.ResumeLayout(false);
            cardDlBottom.PerformLayout();
            panelOptimizerContainer.ResumeLayout(false);
            panelOptHeader.ResumeLayout(false);
            panelOptHeader.PerformLayout();
            cardOptDrive.ResumeLayout(false);
            cardOptDrive.PerformLayout();
            cardOptOptions.ResumeLayout(false);
            cardOptOptions.PerformLayout();
            cardOptQueue.ResumeLayout(false);
            cardOptQueue.PerformLayout();
            cardOptBottom.ResumeLayout(false);
            cardOptBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private NotifyIcon notifyIcon;
        private System.Windows.Forms.Timer clipboardTimer;

        // Barra laterale
        private Panel panelSidebar;
        private Label labelBrand;
        private Label labelBrandSub;
        private Button buttonNavDownloader;
        private Button buttonNavOptimizer;
        private CardPanel cardEngine;
        private Label labelEngineTitle;
        private Label labelEngineState;
        private Panel dotYtdlp;
        private Label labelEngineYtdlp;
        private Panel dotFfmpeg;
        private Label labelEngineFfmpeg;
        private Panel dotJs;
        private Label labelEngineJs;
        private ComboBox comboBoxTheme;
        private Button buttonAbout;

        // Contenuto
        private Panel panelContent;

        // Scaricatore
        private Panel panelDownloaderContainer;
        private Panel panelDlHeader;
        private Label labelDlTitle;
        private Label labelDlSubtitle;
        private ToggleSwitch toggleAutoPaste;
        private CardPanel cardInput;
        private TextBox textBoxUrl;
        private Button buttonPaste;
        private Button buttonAddQueue;
        private Button buttonDownloadDirect;
        private Label labelFormat;
        private ComboBox comboBoxFormat;
        private Label labelBitrate;
        private ComboBox comboBoxBitrate;
        private ToggleSwitch toggleThumbnail;
        private ToggleSwitch toggleMetadata;
        private ToggleSwitch toggleCarAudio;
        private ToggleSwitch toggleCleanNames;
        private Label labelFolderHeader;
        private TextBox textBoxFolder;
        private Button buttonBrowseFolder;
        private Button buttonOpenFolder;
        private Panel panelDlSpacer1;
        private CardPanel cardQueue;
        private Label labelQueueHeader;
        private Label labelQueueCount;
        private Button buttonRemoveSelected;
        private Button btnClearQueue;
        private ListView listViewQueue;
        private ColumnHeader columnHeaderTitle;
        private ColumnHeader columnHeaderFormat;
        private ColumnHeader columnHeaderQuality;
        private ColumnHeader columnHeaderProgress;
        private ColumnHeader columnHeaderStatus;
        private Panel panelDlSpacer2;
        private CardPanel cardDlBottom;
        private Label labelStatusText;
        private Label labelMetricsText;
        private FlatProgressBar progressBar;
        private Label labelPercentText;
        private Button buttonStartQueue;
        private Button buttonCancel;

        // Ottimizzatore
        private Panel panelOptimizerContainer;
        private Panel panelOptHeader;
        private Label labelOptTitle;
        private Label labelOptSubtitle;
        private Button buttonOptScan;
        private CardPanel cardOptDrive;
        private Label labelOptDriveHeader;
        private ComboBox comboBoxOptDrive;
        private TextBox textBoxOptFolder;
        private Button buttonOptBrowse;
        private Button buttonOptOpenFolder;
        private Panel panelOptSpacer1;
        private CardPanel cardOptOptions;
        private Label labelOptSettingsHeader;
        private ToggleSwitch toggleOptNormalize;
        private ToggleSwitch toggleOptFixStandard;
        private ToggleSwitch toggleOptConvertAll;
        private ToggleSwitch toggleOptSkipCompliant;
        private ToggleSwitch toggleOptInPlace;
        private ToggleSwitch toggleOptRemoveDuplicates;
        private ToggleSwitch toggleOptCleanNames;
        private Panel panelOptSpacer2;
        private CardPanel cardOptQueue;
        private Label labelOptQueueHeader;
        private Label labelOptSummary;
        private Button buttonOptClear;
        private Button buttonOptRefresh;
        private ListView listViewOpt;
        private ColumnHeader columnHeaderOptFile;
        private ColumnHeader columnHeaderOptFolder;
        private ColumnHeader columnHeaderOptFormat;
        private ColumnHeader columnHeaderOptSize;
        private ColumnHeader columnHeaderOptStatus;
        private Panel panelOptSpacer3;
        private CardPanel cardOptBottom;
        private Label labelOptStatus;
        private Label labelOptMetrics;
        private FlatProgressBar progressBarOpt;
        private Label labelOptPercent;
        private Button buttonOptStart;
        private Button buttonOptCancel;
    }
}
