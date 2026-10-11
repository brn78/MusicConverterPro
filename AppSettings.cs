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

        /// <summary>config.json accanto all'eseguibile (portatile); se la cartella non è scrivibile, in %LOCALAPPDATA%.</summary>
        private static string ConfigPath => _configPath ??= ResolveConfigPath();

        private static string ResolveConfigPath()
        {
            string local = Path.Combine(AppInfo.ExeDirectory, "config.json");
            if (File.Exists(local) || IsWritable(AppInfo.ExeDirectory)) return local;

            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MusicConverterPro");
            try { Directory.CreateDirectory(dir); } catch { }
            return Path.Combine(dir, "config.json");
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
