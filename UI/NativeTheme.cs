using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MusicConverterPro.UI
{
    /// <summary>Barra del titolo scura/chiara tramite DWM (Windows 10 1809+ / Windows 11).</summary>
    public static class NativeTheme
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public static void ApplyTitleBar(Form form, bool dark)
        {
            if (!form.IsHandleCreated) return;
            try
            {
                int value = dark ? 1 : 0;
                int result = DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
                if (result != 0)
                {
                    DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref value, sizeof(int));
                }

                // La cornice si ridisegna solo dopo un cambio di dimensione
                if (form.Visible && form.WindowState == FormWindowState.Normal)
                {
                    form.Width += 1;
                    form.Width -= 1;
                }
            }
            catch
            {
                // Versioni di Windows senza supporto: la barra resta quella di sistema
            }
        }
    }
}
