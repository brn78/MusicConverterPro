using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MusicConverterPro.UI;

namespace MusicConverterPro
{
    /// <summary>Finestra di avanzamento mostrata mentre i componenti incorporati vengono scompattati.</summary>
    public sealed class ExtractDialog : Form
    {
        private readonly Label _title = new Label();
        private readonly Label _desc = new Label();
        private readonly Label _detail = new Label();
        private readonly FlatProgressBar _bar = new FlatProgressBar();
        private readonly string _folder;
        private readonly string _manifest;
        private Theme? _theme;

        public Exception? Error { get; private set; }

        private ExtractDialog(string folder, string manifest)
        {
            _folder = folder;
            _manifest = manifest;

            Text = AppInfo.Name;
            Icon = BrandIcon.AppIcon;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            ClientSize = new Size(480, 176);

            _title.AutoSize = true;
            _title.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            _title.Location = new Point(24, 22);
            _title.Text = "Preparazione dei componenti";

            _desc.AutoSize = false;
            _desc.Location = new Point(25, 52);
            _desc.Size = new Size(432, 36);
            _desc.Tag = "muted";
            _desc.Text = "Solo al primo avvio di questa versione: yt-dlp, FFmpeg e Deno vengono scompattati in una cartella locale.";

            _bar.Location = new Point(24, 104);
            _bar.Size = new Size(432, 8);

            _detail.AutoSize = false;
            _detail.AutoEllipsis = true;
            _detail.Location = new Point(24, 122);
            _detail.Size = new Size(432, 20);
            _detail.Tag = "caption";
            _detail.Text = "Avvio...";

            Controls.Add(_title);
            Controls.Add(_desc);
            Controls.Add(_bar);
            Controls.Add(_detail);
        }

        private void ApplyTheme(Theme t)
        {
            _theme = t;
            BackColor = t.Window;
            ForeColor = t.Text;
            t.Apply(this);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_theme != null) NativeTheme.ApplyTitleBar(this, _theme.IsDark);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            var progress = new Progress<(int percent, string name)>(p =>
            {
                _bar.Value = p.percent;
                _detail.Text = $"{p.name}  ·  {p.percent}%";
            });

            Task.Run(() => ToolsBundle.Extract(_folder, _manifest, progress, CancellationToken.None))
                .ContinueWith(t =>
                {
                    Error = t.Exception?.GetBaseException();
                    if (!IsDisposed) BeginInvoke(new Action(Close));
                });
        }

        /// <summary>Scompatta mostrando l'avanzamento. Restituisce false (dopo aver avvisato) se fallisce.</summary>
        public static bool Run(Theme theme, string folder, string manifest)
        {
            using var dialog = new ExtractDialog(folder, manifest);
            dialog.ApplyTheme(theme);
            dialog.ShowDialog();

            if (dialog.Error != null)
            {
                MessageBox.Show(
                    "Impossibile preparare i componenti dell'applicazione:\n" + dialog.Error.Message +
                    "\n\nCartella: " + folder,
                    AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
    }
}
