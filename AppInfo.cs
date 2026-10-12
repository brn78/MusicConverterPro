using System;
using System.IO;

namespace MusicConverterPro
{
    /// <summary>Dati di identità dell'applicazione e percorsi di base.</summary>
    public static class AppInfo
    {
        public const string Name = "Music Converter Pro";
        public const string Version = "2.6.1";
        public const string Author = "packjc";
        public const string Year = "2026";
        public const string LicenseName = "GNU GPL v3 (o successiva)";
        public const string LicenseFile = "LICENSE";
        public const string NoticesFile = "THIRD-PARTY-NOTICES.md";

        private static string? _exeDirectory;

        /// <summary>Cartella in cui si trova l'eseguibile (anche nella versione a singolo file).</summary>
        public static string ExeDirectory
        {
            get
            {
                if (_exeDirectory != null) return _exeDirectory;
                try
                {
                    string? p = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(p))
                    {
                        _exeDirectory = Path.GetDirectoryName(p) ?? AppContext.BaseDirectory;
                        return _exeDirectory;
                    }
                }
                catch { }
                _exeDirectory = AppContext.BaseDirectory;
                return _exeDirectory;
            }
        }

        /// <summary>Cerca un documento (LICENSE, avvisi) accanto all'exe oppure nella cartella dei componenti scompattati.</summary>
        public static string? FindDocument(string fileName)
        {
            foreach (var dir in new[] { ExeDirectory, AppContext.BaseDirectory, EngineManager.ExternalRoot })
            {
                if (string.IsNullOrEmpty(dir)) continue;
                string candidate = Path.Combine(dir, fileName);
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }
    }
}
