using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MusicConverterPro
{
    public class DownloadEngine
    {
        private static readonly Regex PercentRegex = new Regex(@"(\d+(?:\.\d+)?)%", RegexOptions.Compiled);

        public async Task ExecuteDownloadAsync(
            DownloadItem item,
            AppSettings settings,
            Action<DownloadItem, string> onUpdate,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(EngineManager.YtDlpPath) || !File.Exists(EngineManager.YtDlpPath))
            {
                item.State = DownloadState.Errore;
                item.ErrorMessage = "yt-dlp non trovato nella cartella tools";
                onUpdate(item, item.ErrorMessage);
                return;
            }

            string outDir = settings.DownloadFolder;
            if (string.IsNullOrWhiteSpace(outDir) || !Directory.Exists(outDir))
            {
                outDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }
            }

            item.State = DownloadState.Preparazione;
            item.ProgressPercent = 0;
            item.ErrorMessage = "";
            item.ErrorDetails = "";
            onUpdate(item, "Recupero informazioni e preparazione...");

            if (!item.IsFromPlaylist && (item.Title == "In attesa..." || string.IsNullOrWhiteSpace(item.Title) || item.Title == item.Url))
            {
                var fetchedTitle = await EngineManager.FetchTitleAsync(item.Url, cancellationToken);
                if (!string.IsNullOrWhiteSpace(fetchedTitle))
                {
                    item.Title = fetchedTitle;
                    onUpdate(item, $"Trovato: {item.Title}");
                }
            }

            string format = item.Format.ToLowerInvariant();

            // Sottocartella della playlist (nome già ripulito dai caratteri non validi)
            if (!string.IsNullOrWhiteSpace(item.Subfolder))
            {
                try
                {
                    string sub = Path.Combine(outDir, item.Subfolder);
                    Directory.CreateDirectory(sub);
                    outDir = sub;
                }
                catch { }
            }

            // La cartella va in -P e il modello di nome in -o: così un carattere "%" nel nome
            // della playlist non viene interpretato come campo del modello di yt-dlp.
            string prefix = (item.FilePrefix ?? "").Replace("%", "%%");
            string outputTemplate;
            string metadataArgs = "";

            if (settings.CleanFileNames && !string.IsNullOrWhiteSpace(item.Title) && item.Title != item.Url)
            {
                // Nome file pulito come testo letterale e tag titolo/artista coerenti con il nome
                string cleanName = TitleCleaner.Clean(item.Title);
                item.Title = cleanName;
                outputTemplate = prefix + cleanName.Replace("%", "%%") + ".%(ext)s";

                var (artist, track) = TitleCleaner.SplitArtistTitle(cleanName);
                metadataArgs = $"-metadata \\\"title={track}\\\"";
                if (!string.IsNullOrEmpty(artist))
                {
                    metadataArgs += $" -metadata \\\"artist={artist}\\\"";
                }
            }
            else
            {
                outputTemplate = prefix + "%(title)s.%(ext)s";
            }

            var args = new StringBuilder();
            args.Append(EngineManager.CommonArgs());

            args.Append("-f \"bestaudio/best\" ");
            args.Append("--extract-audio ");
            args.Append($"--audio-format {format} ");

            // Bitrate: ha senso solo per i formati con perdita
            if (format == "mp3" || format == "m4a" || format == "opus")
            {
                args.Append($"--audio-quality {item.Bitrate} ");
            }

            // Compatibilità autoradio (44.1 kHz, stereo, ID3v2.3 + ID3v1) e tag puliti,
            // applicati sia all'estrazione sia al passaggio metadati di yt-dlp
            string extractArgs = settings.CarAudioMode && format == "mp3" ? "-ar 44100 -ac 2 -id3v2_version 3 -write_id3v1 1" : "";
            string metaPpArgs = settings.CarAudioMode && format == "mp3" ? "-id3v2_version 3 -write_id3v1 1" : "";
            if (format == "mp3")
            {
                // Marcatore dell'app nei tag: l'ottimizzatore USB riconosce subito cosa è già a norma
                // (il valore non contiene spazi, quindi non servono virgolette)
                string marker = $"-metadata {AudioOptimizer.MarkerKey}={AudioOptimizer.BuildMarker(normalized: false, standard: settings.CarAudioMode)}";
                metadataArgs = (metadataArgs + " " + marker).Trim();
            }
            if (metadataArgs.Length > 0)
            {
                extractArgs = (extractArgs + " " + metadataArgs).Trim();
                metaPpArgs = (metaPpArgs + " " + metadataArgs).Trim();
            }
            if (extractArgs.Length > 0)
            {
                args.Append($"--postprocessor-args \"ExtractAudio:{extractArgs}\" ");
            }
            if (metaPpArgs.Length > 0)
            {
                args.Append($"--postprocessor-args \"Metadata:{metaPpArgs}\" ");
            }

            // WAV non supporta la copertina incorporata: yt-dlp fallirebbe l'intero download
            if (settings.EmbedThumbnail && format != "wav")
            {
                args.Append("--embed-thumbnail ");
            }

            if (settings.EmbedMetadata)
            {
                args.Append("--add-metadata ");
            }

            if (!string.IsNullOrEmpty(EngineManager.FFmpegPath) && File.Exists(EngineManager.FFmpegPath))
            {
                args.Append($"--ffmpeg-location \"{EngineManager.FFmpegPath}\" ");
            }

            args.Append("--windows-filenames ");
            args.Append("--no-playlist ");
            args.Append("--no-warnings ");
            args.Append("--newline ");
            args.Append("--progress-template \"PROGRESS:[download]|%(progress._percent_str)s|%(progress._total_bytes_str)s|%(progress._speed_str)s|%(progress._eta_str)s\" ");
            args.Append($"-P \"{outDir}\" ");
            args.Append($"-o \"{outputTemplate}\" ");
            args.Append($"\"{item.Url}\"");

            var psi = new ProcessStartInfo
            {
                FileName = EngineManager.YtDlpPath,
                Arguments = args.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            var errorOutput = new StringBuilder();

            process.OutputDataReceived += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Data)) return;

                string line = e.Data.Trim();

                if (line.StartsWith("PROGRESS:[download]|"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 5)
                    {
                        string percentRaw = parts[1].Trim();
                        string sizeRaw = parts[2].Trim();
                        string speedRaw = parts[3].Trim();
                        string etaRaw = parts[4].Trim();

                        var match = PercentRegex.Match(percentRaw);
                        if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double pVal))
                        {
                            item.ProgressPercent = (int)Math.Clamp(pVal, 0, 100);
                        }

                        item.State = DownloadState.Download;
                        item.FileSize = string.IsNullOrWhiteSpace(sizeRaw) || sizeRaw == "N/A" ? "-" : sizeRaw;
                        item.Speed = string.IsNullOrWhiteSpace(speedRaw) || speedRaw == "N/A" ? "-" : speedRaw;
                        item.Eta = string.IsNullOrWhiteSpace(etaRaw) || etaRaw == "N/A" ? "-" : etaRaw;

                        onUpdate(item, $"Scaricamento {item.ProgressPercent}% · {item.Speed}");
                    }
                }
                else if (line.Contains("[ExtractAudio]") || line.Contains("[Fixup") || line.Contains("[ThumbnailsConvertor]") || line.Contains("[Metadata]") || line.Contains("[EmbedThumbnail]"))
                {
                    item.State = DownloadState.Conversione;
                    item.ProgressPercent = Math.Max(item.ProgressPercent, 95);
                    onUpdate(item, "Conversione audio in corso...");

                    if (line.StartsWith("[ExtractAudio] Destination: "))
                    {
                        item.OutputFilePath = line.Substring(line.IndexOf(':') + 1).Trim();
                    }
                }
                else if (line.StartsWith("[download] Destination: "))
                {
                    item.OutputFilePath = line.Substring(line.IndexOf(':') + 1).Trim();
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    errorOutput.AppendLine(e.Data);
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                using var reg = cancellationToken.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(true);
                        }
                    }
                    catch { }
                });

                await process.WaitForExitAsync(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    item.State = DownloadState.Annullato;
                    CleanupPartialFiles(outDir);
                    onUpdate(item, "Download annullato dall'utente.");
                    return;
                }

                if (process.ExitCode == 0)
                {
                    item.State = DownloadState.Completato;
                    item.ProgressPercent = 100;
                    onUpdate(item, "Download e conversione completati.");
                }
                else
                {
                    item.State = DownloadState.Errore;
                    item.ErrorDetails = errorOutput.ToString().Trim();
                    item.ErrorMessage = SummarizeError(item.ErrorDetails, process.ExitCode);
                    onUpdate(item, $"Errore: {item.ErrorMessage}");
                }
            }
            catch (OperationCanceledException)
            {
                item.State = DownloadState.Annullato;
                CleanupPartialFiles(outDir);
                onUpdate(item, "Download annullato.");
            }
            catch (Exception ex)
            {
                item.State = DownloadState.Errore;
                item.ErrorMessage = ex.Message;
                item.ErrorDetails = ex.ToString();
                onUpdate(item, $"Eccezione: {ex.Message}");
            }
        }

        /// <summary>Riduce l'output di errore di yt-dlp a una riga leggibile.</summary>
        private static string SummarizeError(string details, int exitCode)
        {
            if (string.IsNullOrWhiteSpace(details))
            {
                return $"processo terminato con codice {exitCode}";
            }

            var lines = details.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                               .Select(l => l.Trim())
                               .Where(l => l.Length > 0)
                               .ToList();

            string? errorLine = lines.LastOrDefault(l => l.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase));
            string msg = errorLine ?? lines.Last();

            if (msg.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
            {
                msg = msg.Substring(6).Trim();
            }

            // Rimuove il prefisso "[estrattore] id:" tipico dei messaggi di yt-dlp
            int colon = msg.IndexOf("]: ", StringComparison.Ordinal);
            if (msg.StartsWith("[") && colon > 0 && colon < 40)
            {
                msg = msg.Substring(colon + 3).Trim();
            }

            if (msg.Contains("403", StringComparison.Ordinal)) msg = "accesso negato (HTTP 403) · riprova o aggiorna yt-dlp";
            else if (msg.Contains("Sign in to confirm your age", StringComparison.OrdinalIgnoreCase)) msg = "video con limiti di età (richiede accesso)";
            else if (msg.Contains("Private video", StringComparison.OrdinalIgnoreCase)) msg = "video privato";
            else if (msg.Contains("Video unavailable", StringComparison.OrdinalIgnoreCase)) msg = "video non disponibile";
            else if (msg.Contains("Unsupported URL", StringComparison.OrdinalIgnoreCase)) msg = "sito o link non supportato";

            return msg.Length > 160 ? msg.Substring(0, 157) + "..." : msg;
        }

        /// <summary>Elimina i file .part / .ytdl lasciati da un download interrotto.</summary>
        private static void CleanupPartialFiles(string outDir)
        {
            try
            {
                foreach (var f in Directory.EnumerateFiles(outDir))
                {
                    if (f.EndsWith(".part", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".ytdl", StringComparison.OrdinalIgnoreCase))
                    {
                        try { File.Delete(f); } catch { }
                    }
                }
            }
            catch { }
        }
    }
}
