using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MusicConverterPro
{
    /// <summary>Una voce di playlist (o il singolo video) restituita da yt-dlp.</summary>
    public sealed class MediaEntry
    {
        public string Url { get; set; } = "";
        public string Title { get; set; } = "";
        public string Id { get; set; } = "";
        public double? DurationSeconds { get; set; }
    }

    /// <summary>Risultato della lettura di un link: singolo video oppure playlist con le sue voci.</summary>
    public sealed class MediaProbe
    {
        public bool IsPlaylist { get; set; }
        public string Title { get; set; } = "";
        public string Id { get; set; } = "";
        public double? DurationSeconds { get; set; }
        public string Extractor { get; set; } = "";
        public List<MediaEntry> Entries { get; } = new List<MediaEntry>();

        public double TotalDurationSeconds
        {
            get
            {
                double total = 0;
                foreach (var e in Entries) total += e.DurationSeconds ?? 0;
                return total;
            }
        }
    }

    public static class EngineManager
    {
        public static string? YtDlpPath { get; private set; }
        public static string? FFmpegPath { get; private set; }
        public static string? DenoPath { get; private set; }
        public static string? NodePath { get; private set; }

        /// <summary>Cartella dei componenti scompattati dalla versione a singolo eseguibile (contiene "tools").</summary>
        public static string? ExternalRoot { get; set; }

        private static string ExternalTool(string name) =>
            string.IsNullOrEmpty(ExternalRoot) ? Path.Combine("?", name) : Path.Combine(ExternalRoot, "tools", name);

        public static bool Initialize(out string message)
        {
            string baseDir = AppInfo.ExeDirectory;

            string[] possibleYtDlp = new[]
            {
                Path.Combine(baseDir, "tools", "yt-dlp.exe"),
                ExternalTool("yt-dlp.exe"),
                Path.Combine(baseDir, "yt-dlp.exe"),
                Path.Combine(baseDir, "..", "..", "..", "tools", "yt-dlp.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "yt-dlp", "yt-dlp.exe")
            };

            foreach (var p in possibleYtDlp)
            {
                if (File.Exists(p))
                {
                    YtDlpPath = Path.GetFullPath(p);
                    break;
                }
            }

            if (string.IsNullOrEmpty(YtDlpPath))
            {
                YtDlpPath = FindInPath("yt-dlp.exe");
            }

            string[] possibleFFmpeg = new[]
            {
                Path.Combine(baseDir, "tools", "ffmpeg.exe"),
                ExternalTool("ffmpeg.exe"),
                Path.Combine(baseDir, "ffmpeg.exe"),
                Path.Combine(baseDir, "..", "..", "..", "tools", "ffmpeg.exe")
            };

            foreach (var p in possibleFFmpeg)
            {
                if (File.Exists(p))
                {
                    FFmpegPath = Path.GetFullPath(p);
                    break;
                }
            }

            if (string.IsNullOrEmpty(FFmpegPath))
            {
                FFmpegPath = FindInPath("ffmpeg.exe");
            }

            string[] possibleDeno = new[]
            {
                Path.Combine(baseDir, "tools", "deno.exe"),
                ExternalTool("deno.exe"),
                Path.Combine(baseDir, "deno.exe"),
                Path.Combine(baseDir, "..", "..", "..", "tools", "deno.exe")
            };

            foreach (var p in possibleDeno)
            {
                if (File.Exists(p))
                {
                    DenoPath = Path.GetFullPath(p);
                    break;
                }
            }
            if (string.IsNullOrEmpty(DenoPath))
            {
                DenoPath = FindInPath("deno.exe");
            }

            string[] possibleNode = new[]
            {
                Path.Combine(baseDir, "tools", "node.exe"),
                ExternalTool("node.exe"),
                Path.Combine(baseDir, "node.exe"),
                Path.Combine(baseDir, "..", "..", "..", "tools", "node.exe")
            };

            foreach (var p in possibleNode)
            {
                if (File.Exists(p))
                {
                    NodePath = Path.GetFullPath(p);
                    break;
                }
            }
            if (string.IsNullOrEmpty(NodePath))
            {
                NodePath = FindInPath("node.exe");
            }

            if (string.IsNullOrEmpty(YtDlpPath) || !File.Exists(YtDlpPath))
            {
                message = "Motore di download (yt-dlp) non trovato. Verifica la cartella 'tools'.";
                return false;
            }

            if (string.IsNullOrEmpty(FFmpegPath) || !File.Exists(FFmpegPath))
            {
                message = "Encoder audio (FFmpeg) non trovato. Verifica la cartella 'tools'.";
                return false;
            }

            string jsEngine = !string.IsNullOrEmpty(DenoPath) ? "Deno JS" : (!string.IsNullOrEmpty(NodePath) ? "Node JS" : "Nativo");
            message = $"Motore pronto (yt-dlp + FFmpeg + {jsEngine})";
            return true;
        }

        private static string? FindInPath(string filename)
        {
            string? envPath = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(envPath)) return null;

            foreach (var dir in envPath.Split(Path.PathSeparator))
            {
                try
                {
                    string candidate = Path.Combine(dir.Trim(), filename);
                    if (File.Exists(candidate))
                    {
                        return Path.GetFullPath(candidate);
                    }
                }
                catch { }
            }
            return null;
        }

        /// <summary>Argomenti aggiuntivi per yt-dlp letti da config.json (YtDlpExtraArgs).</summary>
        public static string ExtraArgs { get; set; } = "";

        /// <summary>Argomenti comuni a ogni chiamata: runtime JS per le sfide dei player e argomenti dell'utente.</summary>
        public static string CommonArgs()
        {
            string args = "";
            if (!string.IsNullOrEmpty(DenoPath) && File.Exists(DenoPath))
            {
                args += $"--js-runtimes \"deno:{DenoPath}\" ";
            }
            else if (!string.IsNullOrEmpty(NodePath) && File.Exists(NodePath))
            {
                args += $"--js-runtimes \"node:{NodePath}\" ";
            }
            if (!string.IsNullOrWhiteSpace(ExtraArgs))
            {
                args += ExtraArgs.Trim() + " ";
            }
            return args;
        }

        public static async Task<string?> FetchTitleAsync(string url, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(YtDlpPath)) return null;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = YtDlpPath,
                    Arguments = "--no-playlist --no-warnings --print \"%(title)s\" " + CommonArgs() + $"\"{url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = new Process { StartInfo = psi };
                proc.Start();

                var outputTask = proc.StandardOutput.ReadToEndAsync(ct);
                var errorTask = proc.StandardError.ReadToEndAsync(ct);
                await proc.WaitForExitAsync(ct);
                await errorTask;

                string title = (await outputTask).Trim();
                return string.IsNullOrWhiteSpace(title) ? null : title;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Legge un link con yt-dlp in modalità "flat": restituisce il titolo del singolo video
        /// oppure la playlist con tutte le sue voci, senza scaricare nulla. Null se la lettura fallisce.
        /// </summary>
        public static async Task<MediaProbe?> ProbeAsync(string url, CancellationToken ct, bool noPlaylist = false)
        {
            if (string.IsNullOrEmpty(YtDlpPath)) return null;

            try
            {
                string mode = noPlaylist ? "--no-playlist " : "--flat-playlist ";
                var psi = new ProcessStartInfo
                {
                    FileName = YtDlpPath,
                    Arguments = mode + "--no-warnings --ignore-errors -J " + CommonArgs() + $"\"{url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = new Process { StartInfo = psi };
                proc.Start();

                using var reg = ct.Register(() =>
                {
                    try { if (!proc.HasExited) proc.Kill(true); } catch { }
                });

                var outputTask = proc.StandardOutput.ReadToEndAsync(ct);
                var errorTask = proc.StandardError.ReadToEndAsync(ct);
                await proc.WaitForExitAsync(ct);
                string json = await outputTask;
                await errorTask;

                if (string.IsNullOrWhiteSpace(json)) return null;
                return Parse(json.Trim());
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }

        private static MediaProbe? Parse(string json)
        {
            var root = JObject.Parse(json);
            var probe = new MediaProbe
            {
                Extractor = root.Value<string>("extractor_key") ?? root.Value<string>("extractor") ?? ""
            };

            string type = root.Value<string>("_type") ?? "video";

            if (type == "playlist" || type == "multi_video")
            {
                probe.IsPlaylist = true;
                probe.Title = root.Value<string>("title") ?? "Playlist";
                probe.Id = root.Value<string>("id") ?? "";

                if (root["entries"] is JArray entries)
                {
                    foreach (var token in entries)
                    {
                        if (token is not JObject e) continue;

                        // Senza titolo = video privato/rimosso non segnalato: lo saltiamo
                        string title = e.Value<string>("title") ?? "";
                        if (string.IsNullOrWhiteSpace(title) || IsUnavailable(title)) continue;

                        string id = e.Value<string>("id") ?? "";
                        string entryUrl = e.Value<string>("url") ?? e.Value<string>("webpage_url") ?? "";
                        if (string.IsNullOrEmpty(entryUrl)) continue;

                        probe.Entries.Add(new MediaEntry
                        {
                            Url = entryUrl,
                            Title = string.IsNullOrWhiteSpace(title) ? entryUrl : title,
                            Id = id,
                            DurationSeconds = e.Value<double?>("duration")
                        });
                    }
                }
                return probe;
            }

            probe.IsPlaylist = false;
            probe.Title = root.Value<string>("title") ?? "";
            probe.Id = root.Value<string>("id") ?? "";
            probe.DurationSeconds = root.Value<double?>("duration");
            return probe;
        }

        private static bool IsUnavailable(string title)
        {
            string t = title.Trim();
            return t.Equals("[Private video]", StringComparison.OrdinalIgnoreCase) ||
                   t.Equals("[Deleted video]", StringComparison.OrdinalIgnoreCase) ||
                   t.Equals("[Unavailable video]", StringComparison.OrdinalIgnoreCase);
        }
    }
}
