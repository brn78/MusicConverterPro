using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MusicConverterPro
{
    /// <summary>
    /// Trasforma il titolo di un video o il nome di un file in un nome pulito e leggibile:
    /// via "(Official Video)", "(360p)", "[Lyrics]", "(prod. by ...)", "256k", "(1)", hashtag,
    /// emoji, caratteri a larghezza intera, estensioni video rimaste nel nome e caratteri
    /// non ammessi da Windows. Conserva le informazioni di versione ("Remix", "Live", "feat. ...").
    /// </summary>
    public static class TitleCleaner
    {
        private const RegexOptions Opt = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        // Un singolo "pezzo" di rumore (senza numeri puri: quelli valgono solo tra parentesi o dopo altro rumore)
        private const string Noise =
            @"(?:offici(?:al|la)(?:\s*(?:music|lyrics?|hd|hq|4k|full))?\s*(?:video|audio|clip|visuali[sz]er|version|stream)?" +
            @"|music\s*video|video\s*clip|videoclip|video\s*ufficiale|clip\s*officiel|audio\s*ufficiale" +
            @"|lyrics?\s*(?:video|vide)?|with\s*lyrics|con\s*testo|testo|tekst[a-z]*|letra|paroles|sub\s*ita|sub\s*eng|lyrics\s*on\s*screen" +
            @"|visuali[sz]er|audio|video|mv" +
            @"|hd|hq|fhd|uhd|sd|4k|8k|\d{3,4}\s*p|\d{2,3}\s*k(?:bps)?|\d{2,3}\s*kb/s" +
            @"|copy|copia|free\s*download|download" +
            @"|prod(?:\.|uced|otto)?\s*(?:by|da)?\s*[^()\[\]{}|]*" +
            @")";

        private const string NoiseWithCounters = "(?:" + Noise + @"|\d{1,2})";
        private const string Sep = @"[\s\-,/&|:.]+";

        // Parentesi/quadre/graffe che contengono SOLO rumore
        private static readonly Regex NoiseBracket = new Regex(
            @"\s*[\(\[\{]\s*" + NoiseWithCounters + @"(?:" + Sep + NoiseWithCounters + @")*\s*[\)\]\}]", Opt);

        // Rumore in coda a un segmento, fuori dalle parentesi: "Song Official Video HD", "Song 256k", "Song Hd 1"
        private static readonly Regex TrailingNoise = new Regex(
            @"(?:" + Sep + Noise + @")+(?:" + Sep + @"\d{1,2})?\s*$", Opt);

        // Segmento composto solo da rumore: "| Lyrics", "| Official Video"
        private static readonly Regex NoiseOnly = new Regex(
            @"^\s*" + Noise + @"(?:" + Sep + Noise + @")*\s*$", Opt);

        // Estensioni video/audio rimaste dentro il nome ("Brano .mpg", "Brano.mp4")
        private static readonly Regex StrayExtension = new Regex(
            @"\s*\.(?:mpg|mpeg|mp4|m4v|flv|f4v|webm|avi|mkv|wmv|mov|3gp|ts|mp3|m4a|wav|flac|ogg|opus|wma|aac)(?=\s|$|[\(\[\-])", Opt);

        private static readonly Regex Hashtag = new Regex(@"(?<=^|\s)#\w+", Opt);
        private static readonly Regex EmptyBrackets = new Regex(@"[\(\[\{]\s*[\)\]\}]", Opt);
        private static readonly Regex MultiSpace = new Regex(@"\s{2,}", Opt);
        private static readonly Regex DashRuns = new Regex(@"(?:\s*-\s*){2,}", Opt);
        private static readonly Regex SpaceBeforePunct = new Regex(@"\s+([,;!?)\]])", Opt);
        private static readonly Regex SpaceAfterOpen = new Regex(@"([(\[])\s+", Opt);

        private static readonly Dictionary<char, string> CharMap = new Dictionary<char, string>
        {
            ['｜'] = "|", ['│'] = "|", ['¦'] = "|",
            ['：'] = ":", ['／'] = "/", ['＼'] = "\\", ['＂'] = "", ['？'] = "?", ['＊'] = "*", ['＜'] = "<", ['＞'] = ">",
            ['（'] = "(", ['）'] = ")", ['［'] = "[", ['］'] = "]", ['｛'] = "{", ['｝'] = "}",
            ['–'] = "-", ['—'] = "-", ['−'] = "-", ['‐'] = "-", ['‑'] = "-", ['―'] = "-",
            ['‘'] = "'", ['’'] = "'", ['‚'] = "'", ['`'] = "'", ['´'] = "'",
            ['“'] = "", ['”'] = "", ['„'] = "", ['"'] = "",
            ['·'] = "-", ['•'] = "-", ['●'] = "-", ['►'] = "-", ['▶'] = "-", ['＿'] = " ", ['_'] = " ",
            ['…'] = "...", [' '] = " ", ['\t'] = " "
        };

        /// <summary>Nome pulito, senza estensione, valido come nome file su Windows.</summary>
        public static string Clean(string rawTitle)
        {
            if (string.IsNullOrWhiteSpace(rawTitle)) return "Senza titolo";

            string s = rawTitle.Normalize(NormalizationForm.FormC);
            s = MapChars(s);
            s = Hashtag.Replace(s, " ");
            s = StrayExtension.Replace(s, " ");

            // Parentesi di solo rumore: ripetuto finché ce ne sono (anche annidate)
            string prev;
            do
            {
                prev = s;
                s = NoiseBracket.Replace(s, " ");
                s = EmptyBrackets.Replace(s, " ");
            } while (prev != s);

            // Segmenti separati da "|" o "//": via quelli di solo rumore, rumore in coda agli altri
            var segments = Regex.Split(s, @"\s*(?:\||//)\s*")
                                .Select(seg => StripTrailingNoise(seg))
                                .Where(seg => seg.Length > 0)
                                .ToList();

            var kept = segments.Where(seg => !NoiseOnly.IsMatch(seg)).ToList();
            if (kept.Count == 0 && segments.Count > 0) kept.Add(segments[0]);
            s = string.Join(" - ", kept);

            // Rumore in coda ai segmenti separati da " - "; il primo (spesso l'artista) non viene mai svuotato
            var dashParts = s.Split(new[] { " - " }, StringSplitOptions.None).ToList();
            for (int i = dashParts.Count - 1; i >= 0; i--)
            {
                string part = StripTrailingNoise(dashParts[i]);
                if (part.Length == 0 || (i > 0 && NoiseOnly.IsMatch(part)))
                {
                    if (i > 0) dashParts.RemoveAt(i);
                }
                else
                {
                    dashParts[i] = part;
                }
            }
            s = string.Join(" - ", dashParts);

            s = ReplaceInvalidChars(s);
            s = StripSymbols(s);

            s = EmptyBrackets.Replace(s, " ");
            s = MultiSpace.Replace(s, " ");
            s = DashRuns.Replace(s, " - ");
            s = SpaceBeforePunct.Replace(s, "$1");
            s = SpaceAfterOpen.Replace(s, "$1");
            s = s.Trim().Trim('-', '.', ',', ';', ':', '\'', ' ').Trim();

            if (s.Length > 120)
            {
                int cut = s.LastIndexOf(' ', 120);
                s = (cut > 60 ? s.Substring(0, cut) : s.Substring(0, 120)).Trim().TrimEnd('-', '.', ',');
            }

            if (s.Length == 0)
            {
                s = StripSymbols(ReplaceInvalidChars(MapChars(rawTitle))).Trim();
            }

            return s.Length == 0 ? "Senza titolo" : s;
        }

        /// <summary>Se il nome ha la forma "Artista - Titolo" restituisce le due parti, altrimenti (null, nome).</summary>
        public static (string? artist, string title) SplitArtistTitle(string cleanName)
        {
            int idx = cleanName.IndexOf(" - ", StringComparison.Ordinal);
            if (idx <= 0) return (null, cleanName);

            string artist = cleanName.Substring(0, idx).Trim();
            string title = cleanName.Substring(idx + 3).Trim();
            if (artist.Length == 0 || title.Length == 0 || artist.Length > 60) return (null, cleanName);
            return (artist, title);
        }

        private static string StripTrailingNoise(string segment)
        {
            string trimmed = segment.Trim();
            string result = TrailingNoise.Replace(trimmed, "").Trim();
            // Mai lasciare un segmento vuoto se prima conteneva qualcosa di non-rumore
            return result.Length == 0 && !NoiseOnly.IsMatch(trimmed) ? trimmed : result;
        }

        private static string MapChars(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (CharMap.TryGetValue(c, out var rep)) sb.Append(rep);
                else sb.Append(c);
            }
            return sb.ToString();
        }

        private static string ReplaceInvalidChars(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                switch (c)
                {
                    case ':': sb.Append(" -"); break;
                    case '/':
                    case '\\': sb.Append('-'); break;
                    case '"':
                    case '*':
                    case '?':
                    case '<':
                    case '>':
                    case '|': break;
                    default:
                        if (c < ' ') break;
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>Elimina emoji e simboli decorativi; tiene lettere, cifre, punteggiatura e simboli utili (&amp; + $ €).</summary>
        private static string StripSymbols(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                switch (cat)
                {
                    case UnicodeCategory.OtherSymbol:
                    case UnicodeCategory.ModifierSymbol:
                    case UnicodeCategory.Surrogate:
                    case UnicodeCategory.PrivateUse:
                    case UnicodeCategory.Control:
                    case UnicodeCategory.Format:
                    case UnicodeCategory.OtherNotAssigned:
                    case UnicodeCategory.NonSpacingMark when !IsLatinMark(c):
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        // Gli accenti combinanti delle lettere latine restano (NormalizationForm.FormC li ha già uniti quasi tutti)
        private static bool IsLatinMark(char c) => c >= '̀' && c <= 'ͯ';
    }
}
