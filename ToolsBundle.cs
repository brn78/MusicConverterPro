using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MusicConverterPro
{
    /// <summary>
    /// Versione a singolo eseguibile: gli strumenti (yt-dlp, FFmpeg, Deno) e i documenti di licenza
    /// sono incorporati come risorsa "tools.zip". Al primo avvio vengono scompattati in
    /// %LOCALAPPDATA%\MusicConverterPro\runtime\v&lt;versione&gt;-&lt;impronta&gt;; gli avvii successivi
    /// riutilizzano la cartella se il manifesto coincide, altrimenti scompattano di nuovo e
    /// cancellano le versioni precedenti.
    /// </summary>
    public static class ToolsBundle
    {
        public const string ZipResource = "tools.zip";
        public const string ManifestResource = "tools.manifest";
        private const string ManifestFileName = "manifest.txt";

        private static readonly string[] RequiredFiles =
        {
            Path.Combine("tools", "ffmpeg.exe"),
            Path.Combine("tools", "yt-dlp.exe"),
            Path.Combine("tools", "deno.exe")
        };

        private static Assembly Asm => typeof(ToolsBundle).Assembly;

        public static string RuntimeRoot => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MusicConverterPro", "runtime");

        public static bool HasEmbeddedTools => Asm.GetManifestResourceInfo(ZipResource) != null;

        public static string? ReadManifest()
        {
            using var s = Asm.GetManifestResourceStream(ManifestResource);
            if (s == null) return null;
            using var r = new StreamReader(s, Encoding.UTF8);
            return r.ReadToEnd().Trim();
        }

        /// <summary>Cartella di destinazione: dipende dalla versione dell'app e dall'impronta del manifesto.</summary>
        public static string TargetFolder(string manifest)
        {
            string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(manifest))).Substring(0, 10).ToLowerInvariant();
            return Path.Combine(RuntimeRoot, $"v{AppInfo.Version}-{hash}");
        }

        public static bool IsReady(string folder, string manifest)
        {
            try
            {
                string mf = Path.Combine(folder, ManifestFileName);
                if (!File.Exists(mf)) return false;
                if (!string.Equals(File.ReadAllText(mf, Encoding.UTF8).Trim(), manifest, StringComparison.Ordinal)) return false;
                return RequiredFiles.All(f => File.Exists(Path.Combine(folder, f)));
            }
            catch
            {
                return false;
            }
        }

        public static void Extract(string folder, string manifest, IProgress<(int percent, string name)>? progress, CancellationToken ct)
        {
            Directory.CreateDirectory(RuntimeRoot);
            string tmp = folder + ".tmp";
            if (Directory.Exists(tmp)) Directory.Delete(tmp, true);
            Directory.CreateDirectory(tmp);
            string tmpFull = Path.GetFullPath(tmp) + Path.DirectorySeparatorChar;

            using (var stream = Asm.GetManifestResourceStream(ZipResource) ?? throw new FileNotFoundException("Risorsa tools.zip assente"))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                long total = Math.Max(1, zip.Entries.Sum(e => e.Length));
                long done = 0;
                var buffer = new byte[1 << 20];

                foreach (var entry in zip.Entries)
                {
                    ct.ThrowIfCancellationRequested();
                    if (string.IsNullOrEmpty(entry.Name)) continue; // cartella

                    string dest = Path.GetFullPath(Path.Combine(tmp, entry.FullName.Replace('/', Path.DirectorySeparatorChar)));
                    if (!dest.StartsWith(tmpFull, StringComparison.OrdinalIgnoreCase)) continue; // percorso fuori cartella

                    Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                    using var es = entry.Open();
                    using var fs = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length);
                    int n;
                    while ((n = es.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ct.ThrowIfCancellationRequested();
                        fs.Write(buffer, 0, n);
                        done += n;
                        progress?.Report(((int)(done * 100 / total), entry.Name));
                    }
                }
            }

            File.WriteAllText(Path.Combine(tmp, ManifestFileName), manifest, Encoding.UTF8);

            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.Move(tmp, folder);
            progress?.Report((100, "completato"));

            CleanupOldVersions(folder);
        }

        private static void CleanupOldVersions(string keep)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(RuntimeRoot))
                {
                    if (string.Equals(dir, keep, StringComparison.OrdinalIgnoreCase)) continue;
                    try { Directory.Delete(dir, true); } catch { /* in uso da un'altra istanza */ }
                }
            }
            catch { }
        }
    }
}
