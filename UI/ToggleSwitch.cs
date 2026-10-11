using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MusicConverterPro.UI
{
    /// <summary>
    /// CheckBox disegnata come interruttore (pillola + pomello) con etichetta a destra.
    /// Si comporta esattamente come una CheckBox: Checked, CheckedChanged, tastiera, focus.
    /// Lo sfondo è opaco (BackColor): il tema lo imposta uguale al contenitore.
    /// </summary>
    public class ToggleSwitch : CheckBox
    {
        private const int TrackWidth = 36;
        private const int TrackHeight = 20;
        private const int Gap = 10;

        private Color _onColor = Color.FromArgb(225, 29, 72);
        private Color _offColor = Color.FromArgb(63, 63, 70);
        private Color _knobColor = Color.White;
        private bool _hover;

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoSize = false;
            Cursor = Cursors.Hand;
            Size = new Size(200, 24);
            TextAlign = ContentAlignment.MiddleLeft;
        }

        public Color OnColor
        {
            get => _onColor;
            set { _onColor = value; Invalidate(); }
        }

        public Color OffColor
        {
            get => _offColor;
            set { _offColor = value; Invalidate(); }
        }

        public Color KnobColor
        {
            get => _knobColor;
            set { _knobColor = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            // ButtonBase è "Opaque": lo sfondo va dipinto qui, altrimenti restano i residui del frame precedente.
            if (BackColor.A == 255)
            {
                g.Clear(BackColor);
            }
            else
            {
                ButtonRenderer.DrawParentBackground(g, ClientRectangle, this);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;

            int y = (Height - TrackHeight) / 2;
            var track = new Rectangle(0, y, TrackWidth, TrackHeight);

            Color trackColor = Checked ? _onColor : _offColor;
            if (!Enabled) trackColor = Color.FromArgb(120, trackColor);
            else if (_hover) trackColor = ControlPaint.Light(trackColor, 0.15f);

            using (var path = CardPanel.RoundedRect(track, TrackHeight / 2))
            using (var brush = new SolidBrush(trackColor))
            {
                g.FillPath(brush, path);
            }

            int knobX = Checked ? TrackWidth - 18 : 2;
            using (var brush = new SolidBrush(Enabled ? _knobColor : Color.FromArgb(160, _knobColor)))
            {
                g.FillEllipse(brush, knobX, y + 2, 16, 16);
            }

            g.SmoothingMode = SmoothingMode.Default;

            var textRect = new Rectangle(TrackWidth + Gap, 0, Math.Max(0, Width - TrackWidth - Gap), Height);
            Color textColor = Enabled ? ForeColor : Color.FromArgb(140, ForeColor);
            TextRenderer.DrawText(g, Text, Font, textRect, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

            if (Focused && ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(g, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }
    }
}
