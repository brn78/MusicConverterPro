using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MusicConverterPro.UI;

namespace MusicConverterPro
{
    partial class AboutDialog : Form
    {
        private Theme? _theme;

        public AboutDialog()
        {
            InitializeComponent();
            Text = "Informazioni su " + AppInfo.Name;
            Icon = BrandIcon.AppIcon;
            ShowIcon = true;
            logoPictureBox.Image = BrandIcon.Render(128);

            labelProductName.Text = AppInfo.Name;
            labelVersion.Text = $"Versione {AppInfo.Version} (portatile)";
            labelCopyright.Text = $"Copyright © {AppInfo.Year} {AppInfo.Author}";
            labelCompanyName.Text = $"Software libero · {AppInfo.LicenseName} · senza alcuna garanzia";

            textBoxDescription.Text =
                "Scarica, converte e ottimizza la tua musica per autoradio e dispositivi USB.\r\n" +
                "• Auto-Paste: i link copiati dai siti supportati entrano da soli in coda\r\n" +
                "• Estrazione audio in MP3 (fino a 320 kbps), M4A, FLAC, WAV o OPUS\r\n" +
                "• Modalità autoradio: 44,1 kHz, stereo, tag ID3v2.3 + ID3v1\r\n" +
                "• Ottimizzatore USB: analisi, doppioni, conversione e normalizzazione (EBU R128)\r\n" +
                "• Tema chiaro/scuro sincronizzato con Windows\r\n" +
                "\r\n" +
                "LICENZA\r\n" +
                "Questo programma è software libero: puoi ridistribuirlo e modificarlo secondo i termini della " +
                "GNU General Public License pubblicata dalla Free Software Foundation, versione 3 o, a tua scelta, " +
                "una versione successiva. È distribuito nella speranza che sia utile, ma SENZA ALCUNA GARANZIA, " +
                "nemmeno quella implicita di commerciabilità o idoneità a uno scopo particolare. " +
                "Il testo completo è nel file LICENSE accanto al programma.\r\n" +
                "\r\n" +
                "USO RESPONSABILE\r\n" +
                "Usa il programma solo con contenuti di cui possiedi i diritti o che sono espressamente concessi in " +
                "licenza per il download, per uso personale e nel rispetto dei termini di servizio dei siti da cui " +
                "provengono. L'autore non è responsabile dell'uso che viene fatto del programma. " +
                "I nomi dei servizi web sono marchi dei rispettivi proprietari; questo programma non è " +
                "affiliato né approvato da nessuno di essi.\r\n" +
                "\r\n" +
                "COMPONENTI DI TERZE PARTI\r\n" +
                "yt-dlp (Unlicense), FFmpeg (GNU GPL v3, build di gyan.dev), Deno (MIT), " +
                "Newtonsoft.Json (MIT), runtime .NET (MIT). Versioni, licenze complete e indirizzi dei sorgenti " +
                "sono nel file THIRD-PARTY-NOTICES.md e nella cartella licenses.";
            textBoxDescription.SelectionStart = 0;
            textBoxDescription.SelectionLength = 0;
        }

        public void ApplyTheme(Theme t)
        {
            _theme = t;
            BackColor = t.Sidebar;
            ForeColor = t.Text;
            labelProductName.ForeColor = t.Text;
            labelVersion.ForeColor = t.TextSecondary;
            labelCompanyName.ForeColor = t.Success;
            labelCopyright.ForeColor = t.Muted;
            textBoxDescription.BackColor = t.Field;
            textBoxDescription.ForeColor = t.Text;
            panelButtons.BackColor = t.Sidebar;
            foreach (var link in new[] { linkLicense, linkNotices })
            {
                link.BackColor = t.Sidebar;
                link.LinkColor = t.AccentText;
                link.ActiveLinkColor = t.Accent;
                link.VisitedLinkColor = t.AccentText;
            }
            t.StyleButton(okButton, "primary");
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_theme != null)
            {
                NativeTheme.ApplyTitleBar(this, _theme.IsDark);
            }
        }

        private void linkLicense_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e) => OpenDocument(AppInfo.LicenseFile);

        private void linkNotices_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e) => OpenDocument(AppInfo.NoticesFile);

        private void OpenDocument(string fileName)
        {
            string? path = AppInfo.FindDocument(fileName);
            if (path == null)
            {
                MessageBox.Show(this, $"Il file {fileName} non è presente accanto al programma.", AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Blocco note apre qualsiasi file di testo, anche senza associazione per .md o senza estensione
                Process.Start(new ProcessStartInfo("notepad.exe", $"\"{path}\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Impossibile aprire il file: " + ex.Message, AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
