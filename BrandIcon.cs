using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace MusicConverterPro
{
    /// <summary>
    /// Logo dell'app disegnato a runtime (quadrato arrotondato rosa con onda audio):
    /// la stessa geometria di MusicConverterPro.ico, così finestra, tray e finestra Info sono coerenti.
    /// </summary>
    public static class BrandIcon
    {
        public static readonly Color Background = Color.FromArgb(225, 29, 72);
        private static readonly int[] BarHeights = { 4, 12, 8, 14, 2 }; // su una griglia di 24

        private static Icon? _appIcon;

        public static Icon AppIcon => _appIcon ??= CreateIcon();

        public static Bitmap Render(int size)
        {
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            int radius = Math.Max(2, (int)Math.Round(size * 0.22));
            using (var path = RoundedRect(new RectangleF(0, 0, size, size), radius))
            using (var brush = new SolidBrush(Background))
            {
                g.FillPath(brush, path);
            }

            float box = size * 0.66f;
            float stroke = Math.Max(1f, box * 0.104f);
            using var pen = new Pen(Color.White, stroke)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            float cy = size / 2f;
            for (int i = 0; i < BarHeights.Length; i++)
            {
                float cx = size / 2f + (i - 2) * (box / 6f);
                float h = box * BarHeights[i] / 24f;
                g.DrawLine(pen, cx, cy - h / 2f, cx, cy + h / 2f);
            }

            return bmp;
        }

        private static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Costruisce in memoria un file ICO multi-dimensione (voci PNG) e lo carica come Icon.</summary>
        private static Icon CreateIcon()
        {
            int[] sizes = { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
            var entries = new List<(int size, byte[] png)>();
            foreach (int s in sizes)
            {
                using var bmp = Render(s);
                using var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                entries.Add((s, ms.ToArray()));
            }

            using var ico = new MemoryStream();
            using (var bw = new BinaryWriter(ico, System.Text.Encoding.Default, leaveOpen: true))
            {
                bw.Write((short)0);
                bw.Write((short)1);
                bw.Write((short)entries.Count);

                int offset = 6 + 16 * entries.Count;
                foreach (var (size, png) in entries)
                {
                    bw.Write((byte)(size >= 256 ? 0 : size));
                    bw.Write((byte)(size >= 256 ? 0 : size));
                    bw.Write((byte)0);
                    bw.Write((byte)0);
                    bw.Write((short)1);
                    bw.Write((short)32);
                    bw.Write(png.Length);
                    bw.Write(offset);
                    offset += png.Length;
                }
                foreach (var (_, png) in entries)
                {
                    bw.Write(png);
                }
            }

            ico.Position = 0;
            return new Icon(ico);
        }
    }
}
