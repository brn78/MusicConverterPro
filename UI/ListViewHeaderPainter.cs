using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MusicConverterPro.UI
{
    /// <summary>
    /// Il controllo intestazione della ListView dipinge con i colori di sistema lo spazio
    /// a destra dell'ultima colonna (e sopra la barra di scorrimento): qui lo copriamo
    /// con il colore del tema dopo ogni WM_PAINT.
    /// </summary>
    public sealed class ListViewHeaderPainter : NativeWindow
    {
        private const int WM_PAINT = 0x000F;
        private const int LVM_GETHEADER = 0x1000 + 31;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private readonly ListView _listView;

        public Color BackColor { get; set; } = Color.FromArgb(28, 28, 33);
        public Color BorderColor { get; set; } = Color.FromArgb(39, 39, 42);

        public ListViewHeaderPainter(ListView listView)
        {
            _listView = listView;
            listView.HandleCreated += (s, e) => Attach();
            listView.HandleDestroyed += (s, e) => ReleaseHandle();
            if (listView.IsHandleCreated) Attach();
        }

        private void Attach()
        {
            IntPtr header = SendMessage(_listView.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            if (header == IntPtr.Zero || header == Handle) return;
            if (Handle != IntPtr.Zero) ReleaseHandle();
            AssignHandle(header);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT)
            {
                PaintRemainder();
            }
        }

        private void PaintRemainder()
        {
            try
            {
                if (Handle == IntPtr.Zero || !GetClientRect(Handle, out var rc)) return;

                int used = 0;
                foreach (ColumnHeader c in _listView.Columns) used += c.Width;

                int width = rc.Right - rc.Left;
                int height = rc.Bottom - rc.Top;
                if (used >= width || height <= 0) return;

                using var g = Graphics.FromHwnd(Handle);
                using var brush = new SolidBrush(BackColor);
                g.FillRectangle(brush, used, 0, width - used, height);
                using var pen = new Pen(BorderColor);
                g.DrawLine(pen, used, height - 1, width, height - 1);
            }
            catch
            {
                // puramente estetico: mai far fallire il disegno
            }
        }
    }
}
