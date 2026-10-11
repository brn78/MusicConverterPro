using System;
using System.Drawing;
using System.Windows.Forms;
using MusicConverterPro.UI;

namespace MusicConverterPro
{
    public enum PlaylistChoice
    {
        Cancel,
        WholePlaylist,
        SingleVideo
    }

    /// <summary>
    /// Finestra mostrata quando un link contiene una playlist: titolo, elenco dei brani,
    /// opzioni di salvataggio e scelta tra tutta la playlist o il solo video.
    /// </summary>
    public sealed class PlaylistDialog : Form
    {
        private readonly Label _title = new Label();
        private readonly Label _summary = new Label();
        private readonly CardPanel _card = new CardPanel();
        private readonly ListBox _list = new ListBox();
        private readonly ToggleSwitch _subfolder = new ToggleSwitch();
        private readonly ToggleSwitch _numbering = new ToggleSwitch();
        private readonly Panel _footer = new Panel();
        private readonly Button _addAll = new Button();
        private readonly Button _single = new Button();
        private readonly Button _cancel = new Button();
        private Theme? _theme;

        public PlaylistChoice Choice { get; private set; } = PlaylistChoice.Cancel;
        public bool CreateSubfolder => _subfolder.Checked;
        public bool NumberFiles => _numbering.Checked;

        public PlaylistDialog(MediaProbe probe, bool canAddSingleVideo, AppSettings settings)
        {
            int count = probe.Entries.Count;

            Text = "Playlist trovata";
            Icon = BrandIcon.AppIcon;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            ClientSize = new Size(640, 540);

            _title.AutoSize = false;
            _title.AutoEllipsis = true;
            _title.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            _title.Location = new Point(22, 18);
            _title.Size = new Size(596, 30);
            _title.Text = probe.Title;

            string duration = DownloadItem.FormatDuration(probe.TotalDurationSeconds);
            _summary.AutoSize = true;
            _summary.Location = new Point(23, 52);
            _summary.Tag = "muted";
            _summary.Text = count == 1 ? "1 brano" : $"{count} brani";
            if (!string.IsNullOrEmpty(duration)) _summary.Text += $"  ·  durata totale {duration}";

            _card.Location = new Point(22, 82);
            _card.Size = new Size(596, 270);
            _card.Padding = new Padding(10);
            _list.BorderStyle = BorderStyle.None;
            _list.Dock = DockStyle.Fill;
            _list.IntegralHeight = false;
            _list.SelectionMode = SelectionMode.None;
            _list.TabStop = false;
            _list.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            _list.BeginUpdate();
            int i = 0;
            foreach (var entry in probe.Entries)
            {
                i++;
                string d = DownloadItem.FormatDuration(entry.DurationSeconds);
                _list.Items.Add(string.IsNullOrEmpty(d) ? $"{i:00}   {entry.Title}" : $"{i:00}   {entry.Title}   ({d})");
            }
            _list.EndUpdate();
            _card.Controls.Add(_list);

            _subfolder.Location = new Point(22, 368);
            _subfolder.Size = new Size(596, 24);
            _subfolder.Checked = settings.PlaylistSubfolder;
            _subfolder.Text = $"Crea la cartella «{probe.Title}» dentro la cartella di salvataggio";

            _numbering.Location = new Point(22, 400);
            _numbering.Size = new Size(596, 24);
            _numbering.Checked = settings.PlaylistNumbering;
            _numbering.Text = "Numera i file (01 - Titolo.mp3) per mantenere l'ordine in autoradio";

            _footer.Location = new Point(22, 444);
            _footer.Size = new Size(596, 70);

            _cancel.Location = new Point(0, 16);
            _cancel.Size = new Size(110, 40);
            _cancel.Tag = "ghost";
            _cancel.Text = "Annulla";
            _cancel.DialogResult = DialogResult.Cancel;
            _cancel.Click += (s, e) => { Choice = PlaylistChoice.Cancel; Close(); };

            _addAll.Location = new Point(596 - 190, 16);
            _addAll.Size = new Size(190, 40);
            _addAll.Tag = "primary";
            _addAll.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            _addAll.Text = count == 1 ? "Aggiungi 1 brano" : $"Aggiungi {count} brani";
            _addAll.Click += (s, e) => { Choice = PlaylistChoice.WholePlaylist; DialogResult = DialogResult.OK; Close(); };

            _single.Location = new Point(596 - 190 - 8 - 170, 16);
            _single.Size = new Size(170, 40);
            _single.Tag = "secondary";
            _single.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            _single.Text = "Solo questo video";
            _single.Visible = canAddSingleVideo;
            _single.Click += (s, e) => { Choice = PlaylistChoice.SingleVideo; DialogResult = DialogResult.OK; Close(); };

            _footer.Controls.Add(_cancel);
            _footer.Controls.Add(_single);
            _footer.Controls.Add(_addAll);

            Controls.Add(_title);
            Controls.Add(_summary);
            Controls.Add(_card);
            Controls.Add(_subfolder);
            Controls.Add(_numbering);
            Controls.Add(_footer);

            AcceptButton = _addAll;
            CancelButton = _cancel;
            Shown += (s, e) => _addAll.Focus();
        }

        public void ApplyTheme(Theme t)
        {
            _theme = t;
            BackColor = t.Window;
            ForeColor = t.Text;
            t.Apply(this);
            _list.BackColor = t.Surface;
            _list.ForeColor = t.Text;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_theme != null)
            {
                NativeTheme.ApplyTitleBar(this, _theme.IsDark);
            }
        }
    }
}
