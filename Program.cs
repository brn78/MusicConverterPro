using System;
using System.IO;
using System.Windows.Forms;

namespace MusicConverterPro
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            if (!PrepareTools()) return;

            Application.Run(new MainForm());
        }

        /// <summary>
        /// Con una cartella "tools" accanto all'exe non fa nulla. Nella versione a singolo eseguibile
        /// scompatta (una sola volta per versione) gli strumenti incorporati e li rende disponibili al motore.
        /// </summary>
        private static bool PrepareTools()
        {
            try
            {
                string localTools = Path.Combine(AppInfo.ExeDirectory, "tools");
                if (File.Exists(Path.Combine(localTools, "ffmpeg.exe")) && File.Exists(Path.Combine(localTools, "yt-dlp.exe")))
                {
                    return true;
                }

                if (!ToolsBundle.HasEmbeddedTools) return true; // versione a cartella: sarà il motore a segnalare i file mancanti

                string? manifest = ToolsBundle.ReadManifest();
                if (string.IsNullOrEmpty(manifest)) return true;

                string folder = ToolsBundle.TargetFolder(manifest);
                if (!ToolsBundle.IsReady(folder, manifest))
                {
                    var settings = AppSettings.Load();
                    if (!ExtractDialog.Run(Theme.Resolve(settings.ThemeMode), folder, manifest))
                    {
                        return false;
                    }
                }

                EngineManager.ExternalRoot = folder;

                // Le versioni precedenti vengono eliminate dopo la scompattazione; se allora una era
                // in uso (o bloccata dall'antivirus), ci si riprova in sottofondo a ogni avvio.
                System.Threading.Tasks.Task.Run(() => ToolsBundle.CleanupOldVersions(folder));
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore durante la preparazione dei componenti: " + ex.Message, AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
