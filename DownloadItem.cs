using System;

namespace MusicConverterPro
{
    public enum DownloadState
    {
        InCoda,
        Preparazione,
        Download,
        Conversione,
        Completato,
        Errore,
        Annullato
    }

    public class DownloadItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Url { get; set; } = "";
        public string Title { get; set; } = "In attesa...";
        public string Format { get; set; } = "mp3";
        public string Bitrate { get; set; } = "320k";
        public DownloadState State { get; set; } = DownloadState.InCoda;
        public int ProgressPercent { get; set; } = 0;
        public string Speed { get; set; } = "-";
        public string Eta { get; set; } = "-";
        public string FileSize { get; set; } = "-";
        public double? DurationSeconds { get; set; }

        /// <summary>Playlist di provenienza (vuoto per i singoli video).</summary>
        public string PlaylistTitle { get; set; } = "";
        public int PlaylistIndex { get; set; }
        public int PlaylistCount { get; set; }

        /// <summary>Sottocartella (relativa alla cartella di salvataggio) e prefisso del nome file, es. "03 - ".</summary>
        public string Subfolder { get; set; } = "";
        public string FilePrefix { get; set; } = "";

        /// <summary>Messaggio breve mostrato nell'interfaccia.</summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>Output completo di yt-dlp in caso di errore (per la diagnostica).</summary>
        public string ErrorDetails { get; set; } = "";

        public string OutputFilePath { get; set; } = "";

        public bool IsBusy => State == DownloadState.Preparazione || State == DownloadState.Download || State == DownloadState.Conversione;

        public bool IsFromPlaylist => !string.IsNullOrEmpty(PlaylistTitle);

        /// <summary>Riga secondaria nella colonna titolo: playlist e posizione, oppure sito e durata.</summary>
        public string Subtitle
        {
            get
            {
                string duration = FormatDuration(DurationSeconds);
                if (IsFromPlaylist)
                {
                    string pos = PlaylistCount > 0 ? $"{PlaylistIndex:00}/{PlaylistCount}" : $"{PlaylistIndex:00}";
                    return string.IsNullOrEmpty(duration) ? $"{PlaylistTitle} · {pos}" : $"{PlaylistTitle} · {pos} · {duration}";
                }

                string host = "";
                if (Uri.TryCreate(Url, UriKind.Absolute, out var uri))
                {
                    host = uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host.Substring(4) : uri.Host;
                }
                if (string.IsNullOrEmpty(host)) return duration;
                return string.IsNullOrEmpty(duration) ? host : $"{host} · {duration}";
            }
        }

        public static string FormatDuration(double? seconds)
        {
            if (seconds == null || seconds <= 0) return "";
            var t = TimeSpan.FromSeconds(seconds.Value);
            return t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes}:{t.Seconds:00}";
        }

        public string GetStatusDisplay()
        {
            return State switch
            {
                DownloadState.InCoda => "In coda",
                DownloadState.Preparazione => "Preparazione",
                DownloadState.Download => "Scaricamento",
                DownloadState.Conversione => "Conversione audio",
                DownloadState.Completato => "Completato",
                DownloadState.Errore => string.IsNullOrWhiteSpace(ErrorMessage) ? "Errore" : $"Errore · {ErrorMessage}",
                DownloadState.Annullato => "Annullato",
                _ => "-"
            };
        }
    }
}
