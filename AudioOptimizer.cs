using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MusicConverterPro
{
    public enum AudioItemKind
    {
        Pending,
        Master,
        Duplicate,
        ToConvert,
        Compliant,
        Busy,
        Done,
        Skipped,
        Deleted,
        Error
    }

    public class AudioItemInfo
    {
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public string Folder { get; set; } = "";       // cartella relativa alla radice analizzata
        public string Extension { get; set; } = "";    // ".mp3", ".flac", ...
        public long FileSize { get; set; }
        public string NormalizedKey { get; set; } = "";
        public bool IsDuplicate { get; set; }
        public string? DuplicateOf { get; set; }
        public AudioItemKind Kind { get; set; } = AudioItemKind.Pending;
        public string Status { get; set; } = "Da verificare";

        /// <summary>Nome pulito (senza estensione) che il file assumerà con "Rinomina con nomi puliti".</summary>
        public string CleanName { get; set; } = "";

        /// <summary>Marcatore scritto nei tag dall'app quando il file è stato ottimizzato (null = mai elaborato).</summary>
        public string? OptimizeMarker { get; set; }
        public bool NeedsRename => !string.IsNullOrEmpty(CleanName) &&
                                   !string.Equals(CleanName, Path.GetFileNameWithoutExtension(FileName), StringComparison.Ordinal);

        public string FormatLabel => Extension.TrimStart('.').ToUpperInvariant();
    }

    public class OptimizeOptions
    {
        public bool RemoveDuplicates { get; set; }
        public bool Normalize { get; set; } = true;
        public bool FixStandard { get; set; } = true;      // 44.1 kHz, stereo, ID3v2.3 + ID3v1
        public bool ConvertAll { get; set; } = true;       // formati diversi da MP3 -> MP3 320 kbps
        public bool SkipCompliant { get; set; } = true;    // non ricodificare gli MP3 già conformi
        public bool ReplaceOriginals { get; set; } = true; // altrimenti i file vanno in "Ottimizzati"
        public bool CleanNames { get; set; } = true;       // rinomina con nomi puliti e tag titolo/artista
    }

    public class OptimizeProgressInfo
    {
        public AudioItemInfo? Item { get; set; }
        public int CurrentIndex { get; set; }
        public int TotalFiles { get; set; }
        public string FileName { get; set; } = "";
        public string Status { get; set; } = "";
        public int Percent => TotalFiles > 0 ? (int)((double)CurrentIndex / TotalFiles * 100) : 0;
    }

    public class OptimizeResult
    {
        public int Optimized { get; set; }
        public int Skipped { get; set; }
        public int DuplicatesRemoved { get; set; }
        public int Renamed { get; set; }
        public int Errors { get; set; }
    }

    public class AudioOptimizer
    {
        public const string OutputSubfolder = "Ottimizzati";
        private const string TempMarker = ".mcp_tmp_";

        /// <summary>
        /// Nome del tag (frame ID3 TXXX) in cui l'app annota cosa ha già fatto al file: così alla
        /// successiva analisi il file risulta subito "già ottimizzato" senza riaprirlo con FFmpeg
        /// né ricodificarlo. Invisibile per autoradio e lettori.
        /// </summary>
        public const string MarkerKey = "MusicConverterPro";

        /// <summary>Valore del marcatore: versione dell'app e cosa è stato applicato (normalizzazione, standard autoradio).</summary>
        public static string BuildMarker(bool normalized, bool standard)
        {
            var sb = new StringBuilder("mcp=" + AppInfo.Version);
            if (normalized) sb.Append(";norm=r128-14");
            if (standard) sb.Append(";std=44100-stereo-id3v23");
            return sb.ToString();
        }

        private static bool MarkerHas(string? marker, string token)
        {
            if (string.IsNullOrEmpty(marker)) return false;
            foreach (var part in marker.Split(';'))
            {
                string p = part.Trim();
                if (p.Equals(token, StringComparison.OrdinalIgnoreCase) ||
                    p.StartsWith(token + "=", StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        /// <summary>Il marcatore copre tutto ciò che le opzioni correnti chiederebbero di fare.</summary>
        public static bool MarkerSatisfies(string? marker, OptimizeOptions options) =>
            !string.IsNullOrEmpty(marker) &&
            (!options.Normalize || MarkerHas(marker, "norm")) &&
            (!options.FixStandard || MarkerHas(marker, "std"));

        /// <summary>Descrizione breve del marcatore (deve stare nella colonna "Esito"), es. "R128, 44,1 kHz".</summary>
        public static string DescribeMarker(string? marker)
        {
            var parts = new List<string>();
            if (MarkerHas(marker, "norm")) parts.Add("R128");
            if (MarkerHas(marker, "std")) parts.Add("44,1 kHz");
            return parts.Count == 0 ? "" : string.Join(", ", parts);
        }

        private static string OptimizedStatus(string? marker)
        {
            string d = DescribeMarker(marker);
            return d.Length == 0 ? "Già ottimizzato" : $"Già ottimizzato ({d})";
        }

        private static readonly HashSet<string> AudioExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".m4a", ".aac", ".flac", ".wav", ".ogg", ".opus", ".wma"
        };

        private static readonly Regex ParenthesesContentRegex = new Regex(@"\s*[\(\[\{][^\)\]\}]*[\)\]\}]", RegexOptions.Compiled);

        private static readonly Regex QualityAndTagRegex = new Regex(
            @"\b(official\s*(music\s*)?video|official\s*audio|lyrics?\s*(video)?|music\s*video|audio|video|clip|remaster(ed)?)\b|" +
            @"\b(prod\.?\s*by\s*[a-z0-9_\s]+)\b|" +
            @"\b(ft\.?|feat\.?)\s*[a-z0-9_\s]+|" +
            @"\b(hd|4k|2k|1080p|720p|480p|360p|240p)\b|" +
            @"\b(320k|256k|192k|128k)\b|" +
            @"[-_~""'“”‘’]",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex NonAlphanumericRegex = new Regex(@"[^a-z0-9]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex StreamInfoRegex = new Regex(
            @"Audio:\s*(?<codec>[a-z0-9]+)[^,]*,\s*(?<rate>\d+)\s*Hz,\s*(?<ch>stereo|mono|[^,]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Chiave di confronto per i doppioni: prima il nome viene ripulito come farebbe la rinomina
        /// ("Amarda - Valium Hd 1" e "Amarda - Valium Hd" diventano entrambi "Amarda - Valium"),
        /// poi restano solo lettere e cifre.
        /// </summary>
        public static string NormalizeAudioTitle(string filename)
        {
            try
            {
                string baseName = TitleCleaner.Clean(Path.GetFileNameWithoutExtension(filename)).ToLowerInvariant();

                string noParentheses = ParenthesesContentRegex.Replace(baseName, " ");
                string noTags = QualityAndTagRegex.Replace(noParentheses, " ");
                string alphanumeric = NonAlphanumericRegex.Replace(noTags, "");

                if (string.IsNullOrWhiteSpace(alphanumeric))
                {
                    alphanumeric = NonAlphanumericRegex.Replace(baseName, "");
                }

                return alphanumeric.Trim();
            }
            catch
            {
                return NonAlphanumericRegex.Replace(filename.ToLowerInvariant(), "").Trim();
            }
        }

        public List<AudioItemInfo> ScanAndAnalyzeFolder(string folderPath, bool cleanNames = false)
        {
            var results = new List<AudioItemInfo>();
            if (!Directory.Exists(folderPath)) return results;

            string root = Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar);
            var files = new List<string>();
            CollectAudioFiles(root, files);

            foreach (var f in files)
            {
                try
                {
                    var fi = new FileInfo(f);
                    string fileName = fi.Name;
                    string dir = fi.DirectoryName ?? root;
                    string relative = dir.Length > root.Length ? dir.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar) : "";
                    string ext = fi.Extension.ToLowerInvariant();

                    results.Add(new AudioItemInfo
                    {
                        FilePath = f,
                        FileName = fileName,
                        Folder = string.IsNullOrEmpty(relative) ? "\\" : relative,
                        Extension = ext,
                        FileSize = fi.Length,
                        NormalizedKey = NormalizeAudioTitle(fileName),
                        CleanName = cleanNames ? TitleCleaner.Clean(Path.GetFileNameWithoutExtension(fileName)) : "",
                        OptimizeMarker = ext == ".mp3" ? Id3Tags.ReadUserText(f, MarkerKey) : null
                    });
                }
                catch { }
            }

            results.Sort((a, b) =>
            {
                int c = string.Compare(a.Folder, b.Folder, StringComparison.CurrentCultureIgnoreCase);
                return c != 0 ? c : string.Compare(a.FileName, b.FileName, StringComparison.CurrentCultureIgnoreCase);
            });

            // Livello 1: stesso titolo normalizzato ("Brano.mp3" vs "BRANO (Official Video) (360p).mp3")
            var byKey = results.Where(r => !string.IsNullOrEmpty(r.NormalizedKey))
                               .GroupBy(r => r.NormalizedKey)
                               .Where(g => g.Count() > 1);

            foreach (var group in byKey)
            {
                // Copia principale: nome senza rumore, già pulito, preferibilmente MP3, poi la più grande
                var master = group.OrderBy(x => HasRipNoiseInName(x.FileName))
                                  .ThenBy(x => x.NeedsRename ? 1 : 0)
                                  .ThenByDescending(x => x.Extension == ".mp3")
                                  .ThenByDescending(x => x.FileSize)
                                  .First();

                foreach (var item in group)
                {
                    if (item != master)
                    {
                        item.IsDuplicate = true;
                        item.DuplicateOf = master.FileName;
                        item.Kind = AudioItemKind.Duplicate;
                        item.Status = $"Doppione di: {master.FileName}";
                    }
                    else
                    {
                        item.Kind = AudioItemKind.Master;
                        item.Status = "Copia principale (conservata)";
                    }
                }
            }

            // Livello 2: stessa dimensione esatta tra i file non ancora marcati. Due brani diversi possono
            // avere la stessa dimensione (CBR 320 kbps, stessa durata): si confronta anche la coda del file,
            // così vengono marcate solo le copie binarie identiche.
            var bySize = results.Where(r => !r.IsDuplicate && r.FileSize > 100 * 1024)
                                .GroupBy(r => r.FileSize)
                                .Where(g => g.Count() > 1);

            foreach (var group in bySize)
            {
                var master = group.First();
                byte[]? masterTail = ReadTail(master.FilePath);
                if (masterTail == null) continue;

                foreach (var item in group.Skip(1))
                {
                    byte[]? tail = ReadTail(item.FilePath);
                    if (tail == null || !tail.AsSpan().SequenceEqual(masterTail)) continue;

                    item.IsDuplicate = true;
                    item.DuplicateOf = master.FileName;
                    item.Kind = AudioItemKind.Duplicate;
                    item.Status = $"Copia identica di: {master.FileName}";
                }
            }

            foreach (var item in results.Where(r => r.Kind == AudioItemKind.Pending))
            {
                if (item.Extension != ".mp3")
                {
                    item.Kind = AudioItemKind.ToConvert;
                    item.Status = $"Da convertire in MP3 ({item.FormatLabel})";
                }
                else if (item.OptimizeMarker != null)
                {
                    item.Kind = AudioItemKind.Compliant;
                    item.Status = OptimizedStatus(item.OptimizeMarker);
                }
                else
                {
                    item.Status = "Da verificare";
                }

                if (item.NeedsRename)
                {
                    item.Status += $" · nuovo nome: {item.CleanName}";
                }
            }

            return results;
        }

        /// <summary>Raccoglie i file audio in modo ricorsivo ignorando le cartelle non accessibili,
        /// i file temporanei dell'app e la cartella di output "Ottimizzati".</summary>
        private static void CollectAudioFiles(string dir, List<string> files)
        {
            IEnumerable<string> entries;
            try
            {
                entries = Directory.EnumerateFiles(dir);
            }
            catch
            {
                return;
            }

            foreach (var f in entries)
            {
                string name = Path.GetFileName(f);

                // File temporanei di una conversione interrotta: sono nostri, li eliminiamo
                if (name.Contains(TempMarker, StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("_mcp_opt_", StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(f); } catch { }
                    continue;
                }

                if (AudioExtensions.Contains(Path.GetExtension(f)))
                {
                    files.Add(f);
                }
            }

            IEnumerable<string> subdirs;
            try
            {
                subdirs = Directory.EnumerateDirectories(dir);
            }
            catch
            {
                return;
            }

            foreach (var sub in subdirs)
            {
                string subName = Path.GetFileName(sub);
                if (string.Equals(subName, OutputSubfolder, StringComparison.OrdinalIgnoreCase)) continue;
                if (subName.StartsWith("System Volume Information", StringComparison.OrdinalIgnoreCase)) continue;
                if (subName.StartsWith("$RECYCLE", StringComparison.OrdinalIgnoreCase)) continue;
                CollectAudioFiles(sub, files);
            }
        }

        /// <summary>Ultimi 64 KB del file (dove stanno gli ultimi frame audio, non i tag): null se illeggibile.</summary>
        private static byte[]? ReadTail(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
                int len = (int)Math.Min(64 * 1024, fs.Length);
                fs.Seek(-len, SeekOrigin.End);
                var buf = new byte[len];
                int total = 0;
                while (total < len)
                {
                    int n = fs.Read(buf, total, len - total);
                    if (n <= 0) break;
                    total += n;
                }
                return total == len ? buf : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool HasRipNoiseInName(string name)
        {
            string lower = name.ToLowerInvariant();
            return lower.Contains("360p") || lower.Contains("240p") || lower.Contains("480p") ||
                   lower.Contains("(1)") || lower.Contains("(2)") || lower.Contains("_1.") ||
                   lower.Contains("copy") || lower.Contains("copia");
        }

        public async Task<OptimizeResult> OptimizeItemsAsync(
            List<AudioItemInfo> items,
            OptimizeOptions options,
            Action<OptimizeProgressInfo> onProgress,
            CancellationToken ct)
        {
            if (string.IsNullOrEmpty(EngineManager.FFmpegPath) || !File.Exists(EngineManager.FFmpegPath))
            {
                throw new FileNotFoundException("FFmpeg non trovato. Verifica la cartella tools.");
            }

            var result = new OptimizeResult();

            for (int i = 0; i < items.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var item = items[i];

                void Report(string status)
                {
                    item.Status = status;
                    onProgress(new OptimizeProgressInfo
                    {
                        Item = item,
                        CurrentIndex = i + 1,
                        TotalFiles = items.Count,
                        FileName = item.FileName,
                        Status = status
                    });
                }

                if (!File.Exists(item.FilePath))
                {
                    item.Kind = AudioItemKind.Skipped;
                    Report("File non più presente");
                    result.Skipped++;
                    continue;
                }

                // Doppione da eliminare
                if (item.IsDuplicate && options.RemoveDuplicates)
                {
                    item.Kind = AudioItemKind.Busy;
                    Report("Eliminazione doppione...");
                    try
                    {
                        File.Delete(item.FilePath);
                        result.DuplicatesRemoved++;
                        item.Kind = AudioItemKind.Deleted;
                        Report("Doppione eliminato");
                    }
                    catch (Exception ex)
                    {
                        item.Kind = AudioItemKind.Error;
                        result.Errors++;
                        Report("Errore eliminazione: " + ex.Message);
                    }
                    continue;
                }

                bool isMp3 = item.Extension == ".mp3";

                if (!isMp3 && !options.ConvertAll)
                {
                    item.Kind = AudioItemKind.Skipped;
                    string note = ApplyCleanName(item, item.FilePath, options, result);
                    Report($"Formato {item.FormatLabel} mantenuto (conversione disattivata)" + note);
                    result.Skipped++;
                    continue;
                }

                if (isMp3 && !options.Normalize && !options.FixStandard)
                {
                    item.Kind = AudioItemKind.Skipped;
                    string note = ApplyCleanName(item, item.FilePath, options, result);
                    Report("Nessuna modifica richiesta" + note);
                    result.Skipped++;
                    continue;
                }

                item.Kind = AudioItemKind.Busy;
                Report(isMp3 ? "Verifica..." : $"Conversione {item.FormatLabel} → MP3...");

                if (options.SkipCompliant)
                {
                    // Il file porta il marcatore dell'app con tutto ciò che le opzioni richiedono:
                    // nessuna ricodifica (e nessuna perdita di qualità a ogni passaggio).
                    if (isMp3)
                    {
                        string? marker = item.OptimizeMarker ?? Id3Tags.ReadUserText(item.FilePath, MarkerKey);
                        if (MarkerSatisfies(marker, options))
                        {
                            item.OptimizeMarker = marker;
                            item.Kind = AudioItemKind.Compliant;
                            string note = ApplyCleanName(item, item.FilePath, options, result);
                            Report(OptimizedStatus(marker) + note);
                            result.Skipped++;
                            continue;
                        }
                    }

                    // Senza "Sostituisci gli originali" la copia ottimizzata sta in "Ottimizzati": se c'è già, basta
                    if (!options.ReplaceOriginals && FindExistingOutput(item, options) != null)
                    {
                        item.Kind = AudioItemKind.Compliant;
                        Report($"Già ottimizzato in {OutputSubfolder}\\");
                        result.Skipped++;
                        continue;
                    }

                    // MP3 mai elaborati dall'app: senza normalizzazione basta che siano già a 44,1 kHz stereo ID3v2.3
                    if (isMp3 && !options.Normalize)
                    {
                        bool compliant = await IsCompliantAsync(item.FilePath, options.FixStandard, ct);
                        if (compliant)
                        {
                            item.Kind = AudioItemKind.Compliant;
                            string note = ApplyCleanName(item, item.FilePath, options, result);
                            Report("Già conforme · nessuna modifica" + note);
                            result.Skipped++;
                            continue;
                        }
                    }
                }

                Report(isMp3 ? (options.Normalize ? "Normalizzazione e ricodifica..." : "Ricodifica 44,1 kHz...") : $"Conversione {item.FormatLabel} → MP3...");

                string? output = await OptimizeSingleFileAsync(item.FilePath, options, ct);

                if (output != null)
                {
                    result.Optimized++;
                    item.Kind = AudioItemKind.Done;
                    item.OptimizeMarker = BuildMarker(options.Normalize, options.FixStandard);
                    string where = options.ReplaceOriginals ? "" : $" → {OutputSubfolder}\\";
                    string note = ApplyCleanName(item, output, options, result);
                    Report((item.IsDuplicate ? "Ottimizzato (doppione mantenuto)" : "Ottimizzato per autoradio") + where + note);
                }
                else
                {
                    result.Errors++;
                    item.Kind = AudioItemKind.Error;
                    Report("Errore durante la conversione");
                }
            }

            return result;
        }

        /// <summary>
        /// Rinomina il file con il nome pulito (se diverso) evitando collisioni; aggiorna l'elemento.
        /// Restituisce la nota da aggiungere allo stato ("" se non rinominato).
        /// </summary>
        private static string ApplyCleanName(AudioItemInfo item, string currentPath, OptimizeOptions options, OptimizeResult result)
        {
            try
            {
                if (!File.Exists(currentPath)) return "";

                string dir = Path.GetDirectoryName(currentPath)!;
                string ext = Path.GetExtension(currentPath);
                string baseName = Path.GetFileNameWithoutExtension(currentPath);

                // L'elemento segue il file risultante (l'originale rinominato, oppure l'MP3 che lo ha sostituito).
                // Con "Sostituisci gli originali" spento la copia sta in "Ottimizzati" e l'elemento resta l'originale.
                bool isOriginal = string.Equals(currentPath, item.FilePath, StringComparison.OrdinalIgnoreCase);
                bool followOutput = isOriginal || options.ReplaceOriginals;

                if (!options.CleanNames)
                {
                    if (followOutput) UpdateItemPath(item, currentPath);
                    return "";
                }

                string cleanBase = TitleCleaner.Clean(baseName);
                if (string.Equals(cleanBase, baseName, StringComparison.Ordinal))
                {
                    if (followOutput) UpdateItemPath(item, currentPath);
                    return "";
                }

                string target = Path.Combine(dir, cleanBase + ext);
                int n = 2;
                while (File.Exists(target) && !string.Equals(target, currentPath, StringComparison.OrdinalIgnoreCase))
                {
                    target = Path.Combine(dir, $"{cleanBase} ({n}){ext}");
                    n++;
                }

                if (string.Equals(target, currentPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Solo maiuscole/minuscole diverse: su Windows serve un passaggio intermedio
                    string tmp = Path.Combine(dir, cleanBase + TempMarker + "ren" + ext);
                    File.Move(currentPath, tmp);
                    File.Move(tmp, target);
                }
                else
                {
                    File.Move(currentPath, target);
                }

                result.Renamed++;
                if (followOutput) UpdateItemPath(item, target);
                return $" · rinominato: {Path.GetFileName(target)}";
            }
            catch
            {
                return " · rinomina non riuscita";
            }
        }

        private static void UpdateItemPath(AudioItemInfo item, string path)
        {
            item.FilePath = path;
            item.FileName = Path.GetFileName(path);
            item.Extension = Path.GetExtension(path).ToLowerInvariant();
        }

        /// <summary>
        /// Copia già ottimizzata nella sottocartella "Ottimizzati" (nome originale o nome pulito)
        /// che porta un marcatore sufficiente per le opzioni correnti; altrimenti null.
        /// </summary>
        private static string? FindExistingOutput(AudioItemInfo item, OptimizeOptions options)
        {
            try
            {
                string outDir = Path.Combine(Path.GetDirectoryName(item.FilePath)!, OutputSubfolder);
                if (!Directory.Exists(outDir)) return null;

                string baseName = Path.GetFileNameWithoutExtension(item.FilePath);
                var candidates = new List<string> { baseName };
                if (options.CleanNames) candidates.Add(TitleCleaner.Clean(baseName));

                foreach (var name in candidates.Distinct(StringComparer.Ordinal))
                {
                    string p = Path.Combine(outDir, name + ".mp3");
                    if (File.Exists(p) && MarkerSatisfies(Id3Tags.ReadUserText(p, MarkerKey), options)) return p;
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Un MP3 è considerato conforme se è a 44.1 kHz stereo e, se richiesto lo standard tag,
        /// non ha un'intestazione ID3v2 di versione diversa dalla 2.3.
        /// </summary>
        private static async Task<bool> IsCompliantAsync(string filePath, bool checkTags, CancellationToken ct)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = EngineManager.FFmpegPath,
                    Arguments = $"-hide_banner -i \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = new Process { StartInfo = psi };
                proc.Start();
                var stderrTask = proc.StandardError.ReadToEndAsync(ct);
                var stdoutTask = proc.StandardOutput.ReadToEndAsync(ct);
                await proc.WaitForExitAsync(ct);
                string info = await stderrTask;
                await stdoutTask;

                var m = StreamInfoRegex.Match(info);
                if (!m.Success) return false;

                bool isMp3 = m.Groups["codec"].Value.Equals("mp3", StringComparison.OrdinalIgnoreCase);
                bool rateOk = m.Groups["rate"].Value == "44100";
                bool stereo = m.Groups["ch"].Value.Trim().Equals("stereo", StringComparison.OrdinalIgnoreCase);
                if (!isMp3 || !rateOk || !stereo) return false;

                if (checkTags)
                {
                    int? id3Major = Id3Tags.ReadMajorVersion(filePath);
                    if (id3Major.HasValue && id3Major.Value != 3) return false;
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Ricodifica un file in MP3 320 kbps. Restituisce il percorso finale oppure null in caso di errore.</summary>
        private static async Task<string?> OptimizeSingleFileAsync(string filePath, OptimizeOptions options, CancellationToken ct)
        {
            if (!File.Exists(filePath)) return null;

            string dir = Path.GetDirectoryName(filePath)!;
            string baseName = Path.GetFileNameWithoutExtension(filePath);
            string tempOut = Path.Combine(dir, $"{baseName}{TempMarker}{Guid.NewGuid():N}.mp3");

            var args = new StringBuilder();
            args.Append($"-hide_banner -loglevel error -y -i \"{filePath}\" ");
            args.Append("-map 0:a:0 -map 0:v? -c:v copy -map_metadata 0 ");
            args.Append("-c:a libmp3lame -b:a 320k ");

            if (options.FixStandard)
            {
                args.Append("-ar 44100 -ac 2 ");
            }

            if (options.Normalize)
            {
                args.Append("-af \"loudnorm=I=-14:TP=-1.5:LRA=11\" ");
            }

            if (options.FixStandard)
            {
                args.Append("-id3v2_version 3 -write_id3v1 1 ");
            }

            // Marcatore nei tag: alla prossima analisi il file risulta subito "già ottimizzato"
            args.Append($"-metadata \"{MarkerKey}={BuildMarker(options.Normalize, options.FixStandard)}\" ");

            if (options.CleanNames)
            {
                // Tag titolo/artista coerenti con il nome pulito
                var (artist, track) = TitleCleaner.SplitArtistTitle(TitleCleaner.Clean(baseName));
                args.Append($"-metadata \"title={track}\" ");
                if (!string.IsNullOrEmpty(artist))
                {
                    args.Append($"-metadata \"artist={artist}\" ");
                }
            }

            args.Append($"\"{tempOut}\"");

            var psi = new ProcessStartInfo
            {
                FileName = EngineManager.FFmpegPath,
                Arguments = args.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi };

            try
            {
                proc.Start();
                var errTask = proc.StandardError.ReadToEndAsync(ct);
                var outTask = proc.StandardOutput.ReadToEndAsync(ct);

                using var reg = ct.Register(() =>
                {
                    try { if (!proc.HasExited) proc.Kill(true); } catch { }
                });

                await proc.WaitForExitAsync(ct);
                await errTask;
                await outTask;

                if (proc.ExitCode != 0 || !File.Exists(tempOut))
                {
                    TryDelete(tempOut);
                    return null;
                }

                string finalPath;
                if (options.ReplaceOriginals)
                {
                    finalPath = Path.Combine(dir, baseName + ".mp3");
                    bool sameFile = string.Equals(finalPath, filePath, StringComparison.OrdinalIgnoreCase);
                    if (!sameFile && File.Exists(finalPath))
                    {
                        // esiste già un MP3 con lo stesso nome: non lo sovrascriviamo
                        finalPath = Path.Combine(dir, baseName + " (ottimizzato).mp3");
                    }
                    File.Delete(filePath);
                    if (File.Exists(finalPath)) File.Delete(finalPath);
                    File.Move(tempOut, finalPath);
                }
                else
                {
                    string outDir = Path.Combine(dir, OutputSubfolder);
                    Directory.CreateDirectory(outDir);
                    finalPath = Path.Combine(outDir, baseName + ".mp3");
                    if (File.Exists(finalPath)) File.Delete(finalPath);
                    File.Move(tempOut, finalPath);
                }

                return finalPath;
            }
            catch (OperationCanceledException)
            {
                // FFmpeg tiene il file aperto per qualche istante dopo il Kill: aspettiamo prima di cancellare
                try { proc.WaitForExit(5000); } catch { }
                TryDelete(tempOut);
                throw;
            }
            catch
            {
                try { if (!proc.HasExited) proc.Kill(true); proc.WaitForExit(5000); } catch { }
                TryDelete(tempOut);
                return null;
            }
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }
    }
}
