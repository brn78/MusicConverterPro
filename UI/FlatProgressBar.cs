using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MusicConverterPro.UI
{
    /// <summary>
    /// Barra di avanzamento piatta: la ProgressBar di sistema ignora ForeColor con gli stili visivi attivi,
    /// quindi il colore dell'accento non veniva mai mostrato.
    /// </summary>
    public class FlatProgressBar : Control
    {
        private int _value;
        private Color _trackColor = Color.FromArgb(47, 47, 54);
        private Color _fillColor = Color.FromArgb(225, 29, 72);

        public FlatProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false);
            BackColor = Color.Transparent;
            Height = 8;
            TabStop = false;
        }

        public int Value
        {
            get => _value;
            set
            {
                int v = Math.Clamp(value, 0, 100);
                if (v == _value) return;
                _value = v;
                Invalidate();
            }
        }

        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; Invalidate(); }
        }

        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Width <= 0 || Height <= 0) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int radius = Math.Max(1, Height / 2);

            var track = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = CardPanel.RoundedRect(track, radius))
            using (var brush = new SolidBrush(_trackColor))
            {
                g.FillPath(brush, path);
            }

            int fillWidth = (int)Math.Round((Width - 1) * (_value / 100.0));
            if (fillWidth > 0)
            {
                var fill = new Rectangle(0, 0, Math.Max(fillWidth, Height - 1), Height - 1);
                using var path = CardPanel.RoundedRect(fill, radius);
                using var brush = new SolidBrush(_fillColor);
                g.FillPath(brush, path);
            }
        }
    }
}
