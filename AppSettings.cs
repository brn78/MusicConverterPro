using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MusicConverterPro
{
    public class AppSettings
    {
        // Scaricatore
        public string DownloadFolder { get; set; } = "";
        public string AudioFormat { get; set; } = "mp3";
        public string AudioBitrate { get; set; } = "320k";
        public bool EmbedThumbnail { get; set; } = true;
        public bool EmbedMetadata { get; set; } = true;
        public bool AutoPaste { get; set; } = true;
        public bool CarAudioMode { get; set; } = true; // 44.1 kHz, stereo, ID3v2.3 + ID3v1
        public bool CleanFileNames { get; set; } = true; // nomi file puliti + tag titolo/artista coerenti

        // Playlist
        public bool PlaylistSubfolder { get; set; } = true;  // una cartella con il nome della playlist
        public bool PlaylistNumbering { get; set; } = true;  // "01 - Titolo.mp3" per mantenere l'ordine in autoradio

        // Ottimizzatore USB
        public bool RemoveDuplicates { get; set; } = false;
        public bool OptNormalize { get; set; } = true;
        public bool OptFixStandard { get; set; } = true;
        public bool OptConvertAll { get; set; } = true;
        public bool OptSkipCompliant { get; set; } = true;
        public bool OptReplaceOriginals { get; set; } = true;
        public bool OptCleanNames { get; set; } = true;      // rinomina i file con nomi puliti

        // Aspetto
        public string ThemeMode { get; set; } = "Auto"; // "Auto", "Dark", "Light"

        // Avanzate (solo da config.json)
        /// <summary>Domini a cui limitare l'Auto-Paste (es. "esempio.org"). Vuoto = qualsiasi link http/https.</summary>
        public List<string> AutoPasteDomains { get; set; } = new List<string>();

        /// <summary>Argomenti aggiuntivi passati a ogni chiamata di yt-dlp (es. opzioni per gli estrattori o i cookie).</summary>
        public string YtDlpExtraArgs { get; set; } = "";

        private static string? _configPath;

        /// <summary>
        /// Percorso di config.json: in %LOCALAPPDATA%\MusicConverterPro, così accanto all'eseguibile
        /// (desktop, Download...) non compare nulla. Uso portatile: se accanto all'exe c'è la cartella
        /// "tools" (la stessa che sostituisce i componenti incorporati) il file resta accanto all'exe.
        /// </summary>
        public static string ConfigPath => _configPath ??= ResolveConfigPath();

        private static string ResolveConfigPath()
        {
            string exeDir = AppInfo.ExeDirectory;
            string local = Path.Combine(exeDir, "config.json");
            if (Directory.Exists(Path.Combine(exeDir, "tools")) && IsWritable(exeDir)) return local;

            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MusicConverterPro");
            try { Directory.CreateDirectory(dir); } catch { }
            string dest = Path.Combine(dir, "config.json");
            MigrateLocalConfig(local, dest);
            return dest;
        }

        /// <summary>
        /// Le versioni precedenti salvavano config.json accanto all'exe: al primo avvio il file viene
        /// spostato nella nuova sede. Se esiste già un file nella nuova sede vale quello accanto all'exe
        /// (è l'ultimo usato), ma le impostazioni avanzate scritte a mano non vengono perse.
        /// </summary>
        private static void MigrateLocalConfig(string local, string dest)
        {
            try
            {
                if (!File.Exists(local)) return;
                if (!File.Exists(dest))
                {
                    File.Move(local, dest);
                    return;
                }

                var fromLocal = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(local));
                if (fromLocal == null) return;

                var fromDest = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(dest));
                if (fromDest != null)
                {
                    if (fromLocal.AutoPasteDomains == null || fromLocal.AutoPasteDomains.Count == 0)
                        fromLocal.AutoPasteDomains = fromDest.AutoPasteDomains;
                    if (string.IsNullOrWhiteSpace(fromLocal.YtDlpExtraArgs))
                        fromLocal.YtDlpExtraArgs = fromDest.YtDlpExtraArgs;
                }

                File.WriteAllText(dest, JsonConvert.SerializeObject(fromLocal, Formatting.Indented));
                File.Delete(local);
            }
            catch { }
        }

        private static bool IsWritable(string dir)
        {
            try
            {
                string probe = Path.Combine(dir, ".scrittura-" + Guid.NewGuid().ToString("N") + ".tmp");
                File.WriteAllText(probe, "");
                File.Delete(probe);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                    if (settings != null)
                    {
                        if (string.IsNullOrWhiteSpace(settings.DownloadFolder) || !Directory.Exists(settings.DownloadFolder))
                        {
                            settings.DownloadFolder = GetDefaultMusicFolder();
                        }
                        return settings;
                    }
                }
            }
            catch { }

            return new AppSettings
            {
                DownloadFolder = GetDefaultMusicFolder()
            };
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }

        private static string GetDefaultMusicFolder()
        {
            string music = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            if (!string.IsNullOrEmpty(music) && Directory.Exists(music))
            {
                return music;
            }
            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (Directory.Exists(downloads))
            {
                return downloads;
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
    }
}
