using System;
using System.IO;
using System.Text;

namespace MusicConverterPro
{
    /// <summary>
    /// Lettura minima dei tag ID3v2 senza lanciare FFmpeg: versione dell'intestazione e frame
    /// TXXX "utente" (quelli in cui l'app scrive il marcatore di ottimizzazione). Legge solo
    /// quello che serve e salta i frame grandi (copertine) con un seek, quindi costa pochi
    /// millisecondi anche su una pendrive lenta.
    /// </summary>
    public static class Id3Tags
    {
        /// <summary>Versione principale del tag ID3v2 (2, 3, 4) oppure null se il file non ne ha uno.</summary>
        public static int? ReadMajorVersion(string filePath)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
                var header = new byte[10];
                if (ReadFully(fs, header, 10) < 10) return null;
                if (header[0] == (byte)'I' && header[1] == (byte)'D' && header[2] == (byte)'3') return header[3];
            }
            catch { }
            return null;
        }

        /// <summary>Valore del frame TXXX con la descrizione indicata (confronto senza maiuscole), oppure null.</summary>
        public static string? ReadUserText(string filePath, string description)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
                var header = new byte[10];
                if (ReadFully(fs, header, 10) < 10) return null;
                if (header[0] != (byte)'I' || header[1] != (byte)'D' || header[2] != (byte)'3') return null;

                int major = header[3];
                if (major != 3 && major != 4) return null;

                byte flags = header[5];
                if ((flags & 0x80) != 0) return null; // tag "unsynchronised": raro, non gestito

                long end = 10 + SyncSafe(header, 6);

                if ((flags & 0x40) != 0)
                {
                    // Intestazione estesa: in 2.3 la dimensione esclude i 4 byte del campo, in 2.4 li include
                    var ext = new byte[4];
                    if (ReadFully(fs, ext, 4) < 4) return null;
                    long extSize = major == 4 ? SyncSafe(ext, 0) - 4 : BigEndian(ext, 0);
                    if (extSize < 0) return null;
                    fs.Seek(extSize, SeekOrigin.Current);
                }

                var fh = new byte[10];
                while (fs.Position + 10 <= end)
                {
                    if (ReadFully(fs, fh, 10) < 10) return null;
                    if (fh[0] == 0) return null; // riempimento: fine dei frame

                    int size = major == 4 ? SyncSafe(fh, 4) : BigEndian(fh, 4);
                    if (size < 0 || fs.Position + size > end) return null;

                    bool isTxxx = fh[0] == (byte)'T' && fh[1] == (byte)'X' && fh[2] == (byte)'X' && fh[3] == (byte)'X';
                    bool plainFrame = fh[9] == 0; // niente compressione, cifratura, unsync o indicatore di lunghezza

                    if (isTxxx && plainFrame && size > 1 && size <= 64 * 1024)
                    {
                        var data = new byte[size];
                        if (ReadFully(fs, data, size) < size) return null;
                        var (desc, value) = ParseUserText(data);
                        if (desc != null && string.Equals(desc, description, StringComparison.OrdinalIgnoreCase))
                        {
                            return value;
                        }
                    }
                    else
                    {
                        fs.Seek(size, SeekOrigin.Current);
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>Frame TXXX: byte di codifica, descrizione terminata da zero, valore.</summary>
        private static (string? desc, string value) ParseUserText(byte[] data)
        {
            int enc = data[0];
            int pos = 1;

            string desc = ReadString(data, ref pos, enc, terminated: true);
            string value = ReadString(data, ref pos, enc, terminated: false);
            return (desc, value);
        }

        private static string ReadString(byte[] data, ref int pos, int enc, bool terminated)
        {
            if (pos >= data.Length) return "";

            bool wide = enc == 1 || enc == 2;
            Encoding encoding;
            if (enc == 0) encoding = Encoding.Latin1;
            else if (enc == 3) encoding = Encoding.UTF8;
            else encoding = enc == 2 ? Encoding.BigEndianUnicode : Encoding.Unicode;

            int start = pos;
            if (wide && pos + 1 < data.Length)
            {
                // Ogni stringa UTF-16 può avere il proprio BOM
                if (data[pos] == 0xFF && data[pos + 1] == 0xFE) { encoding = Encoding.Unicode; start = pos + 2; }
                else if (data[pos] == 0xFE && data[pos + 1] == 0xFF) { encoding = Encoding.BigEndianUnicode; start = pos + 2; }
            }

            int endExclusive = data.Length;
            int next = data.Length;
            if (terminated)
            {
                if (wide)
                {
                    for (int i = start; i + 1 < data.Length; i += 2)
                    {
                        if (data[i] == 0 && data[i + 1] == 0) { endExclusive = i; next = i + 2; break; }
                    }
                }
                else
                {
                    int zero = Array.IndexOf(data, (byte)0, start);
                    if (zero >= 0) { endExclusive = zero; next = zero + 1; }
                }
            }
            else
            {
                // Valore finale: eventuali zeri di coda vengono ignorati
                int e = data.Length;
                if (wide) { while (e - 2 >= start && data[e - 1] == 0 && data[e - 2] == 0) e -= 2; }
                else { while (e - 1 >= start && data[e - 1] == 0) e--; }
                endExclusive = e;
            }

            pos = next;
            if (endExclusive <= start) return "";
            return encoding.GetString(data, start, endExclusive - start);
        }

        private static int SyncSafe(byte[] b, int offset) =>
            ((b[offset] & 0x7F) << 21) | ((b[offset + 1] & 0x7F) << 14) | ((b[offset + 2] & 0x7F) << 7) | (b[offset + 3] & 0x7F);

        private static int BigEndian(byte[] b, int offset) =>
            (b[offset] << 24) | (b[offset + 1] << 16) | (b[offset + 2] << 8) | b[offset + 3];

        private static int ReadFully(Stream s, byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int n = s.Read(buffer, total, count - total);
                if (n <= 0) break;
                total += n;
            }
            return total;
        }
    }
}
