using System.Drawing;
using System.Windows.Forms;
using MusicConverterPro.UI;

namespace MusicConverterPro
{
    /// <summary>
    /// Tavolozza dell'interfaccia (scura / chiara) e applicazione ai controlli in base al ruolo
    /// dichiarato nel Tag del controllo: "primary", "secondary", "ghost", "muted", "caption",
    /// "success", "accent", "warning", "sidebar", "bar".
    /// </summary>
    public sealed class Theme
    {
        public bool IsDark { get; init; }

        public Color Window { get; init; }
        public Color Sidebar { get; init; }
        public Color Surface { get; init; }
        public Color Field { get; init; }
        public Color FieldBorder { get; init; }
        public Color Border { get; init; }
        public Color Text { get; init; }
        public Color TextSecondary { get; init; }
        public Color Muted { get; init; }
        public Color Caption { get; init; }

        public Color Accent { get; init; }
        public Color AccentHover { get; init; }
        public Color AccentText { get; init; }
        public Color AccentSoft { get; init; }

        public Color Success { get; init; }
        public Color SuccessSoft { get; init; }
        public Color Warning { get; init; }
        public Color WarningSoft { get; init; }
        public Color WarningSolid { get; init; }
        public Color Danger { get; init; }
        public Color DangerSoft { get; init; }
        public Color Info { get; init; }
        public Color InfoSoft { get; init; }

        public Color Secondary { get; init; }
        public Color SecondaryHover { get; init; }
        public Color Track { get; init; }
        public Color RowAlt { get; init; }
        public Color RowBusy { get; init; }
        public Color RowDuplicate { get; init; }
        public Color Selection { get; init; }

        public static readonly Theme Dark = new Theme
        {
            IsDark = true,
            Window = Color.FromArgb(18, 18, 20),
            Sidebar = Color.FromArgb(24, 24, 27),
            Surface = Color.FromArgb(28, 28, 33),
            Field = Color.FromArgb(35, 35, 41),
            FieldBorder = Color.FromArgb(63, 63, 70),
            Border = Color.FromArgb(39, 39, 42),
            Text = Color.FromArgb(244, 244, 245),
            TextSecondary = Color.FromArgb(212, 212, 216),
            Muted = Color.FromArgb(161, 161, 170),
            Caption = Color.FromArgb(142, 142, 153),
            Accent = Color.FromArgb(225, 29, 72),
            AccentHover = Color.FromArgb(190, 18, 60),
            AccentText = Color.FromArgb(251, 113, 133),
            AccentSoft = Color.FromArgb(43, 26, 32),
            Success = Color.FromArgb(52, 211, 153),
            SuccessSoft = Color.FromArgb(5, 46, 34),
            Warning = Color.FromArgb(251, 191, 36),
            WarningSoft = Color.FromArgb(58, 42, 11),
            WarningSolid = Color.FromArgb(217, 119, 6),
            Danger = Color.FromArgb(251, 113, 133),
            DangerSoft = Color.FromArgb(59, 17, 22),
            Info = Color.FromArgb(125, 211, 252),
            InfoSoft = Color.FromArgb(30, 42, 58),
            Secondary = Color.FromArgb(47, 47, 54),
            SecondaryHover = Color.FromArgb(63, 63, 70),
            Track = Color.FromArgb(47, 47, 54),
            RowAlt = Color.FromArgb(32, 32, 38),
            RowBusy = Color.FromArgb(31, 26, 29),
            RowDuplicate = Color.FromArgb(36, 28, 16),
            Selection = Color.FromArgb(51, 51, 58)
        };

        public static readonly Theme Light = new Theme
        {
            IsDark = false,
            Window = Color.FromArgb(244, 244, 245),
            Sidebar = Color.White,
            Surface = Color.White,
            Field = Color.FromArgb(244, 244, 245),
            FieldBorder = Color.FromArgb(212, 212, 216),
            Border = Color.FromArgb(228, 228, 231),
            Text = Color.FromArgb(24, 24, 27),
            TextSecondary = Color.FromArgb(63, 63, 70),
            Muted = Color.FromArgb(82, 82, 91),
            Caption = Color.FromArgb(107, 107, 118),
            Accent = Color.FromArgb(225, 29, 72),
            AccentHover = Color.FromArgb(190, 18, 60),
            AccentText = Color.FromArgb(190, 18, 60),
            AccentSoft = Color.FromArgb(255, 228, 230),
            Success = Color.FromArgb(4, 120, 87),
            SuccessSoft = Color.FromArgb(209, 250, 229),
            Warning = Color.FromArgb(180, 83, 9),
            WarningSoft = Color.FromArgb(254, 243, 199),
            WarningSolid = Color.FromArgb(217, 119, 6),
            Danger = Color.FromArgb(190, 18, 60),
            DangerSoft = Color.FromArgb(255, 228, 230),
            Info = Color.FromArgb(7, 89, 133),
            InfoSoft = Color.FromArgb(224, 242, 254),
            Secondary = Color.FromArgb(228, 228, 231),
            SecondaryHover = Color.FromArgb(212, 212, 216),
            Track = Color.FromArgb(228, 228, 231),
            RowAlt = Color.FromArgb(250, 250, 250),
            RowBusy = Color.FromArgb(255, 247, 248),
            RowDuplicate = Color.FromArgb(255, 251, 235),
            Selection = Color.FromArgb(228, 228, 231)
        };

        /// <summary>Tema effettivo per l'impostazione "Auto" / "Dark" / "Light".</summary>
        public static Theme Resolve(string mode) => mode switch
        {
            "Dark" => Dark,
            "Light" => Light,
            _ => SystemPrefersDark() ? Dark : Light
        };

        /// <summary>Legge la preferenza "app in tema scuro" di Windows.</summary>
        public static bool SystemPrefersDark()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key?.GetValue("AppsUseLightTheme") is int lightVal)
                {
                    return lightVal == 0;
                }
            }
            catch { }
            return true;
        }

        /// <summary>Applica il tema a tutto l'albero dei controlli a partire da root.</summary>
        public void Apply(Control root)
        {
            foreach (Control c in root.Controls)
            {
                Style(c);
                Apply(c);
            }
        }

        public void Style(Control c)
        {
            string role = c.Tag as string ?? "";

            switch (c)
            {
                case CardPanel card:
                    card.BackColor = role == "sidebar-card" ? Sidebar : Window;
                    card.CardColor = role == "bar" ? Sidebar : Surface;
                    card.BorderColor = Border;
                    break;

                case ToggleSwitch sw:
                    sw.BackColor = sw.Parent is CardPanel parentCard ? parentCard.CardColor
                                 : (sw.Parent?.Tag as string) == "sidebar" ? Sidebar
                                 : Window;
                    sw.OnColor = role == "warning" ? WarningSolid : Accent;
                    sw.OffColor = IsDark ? FieldBorder : Color.FromArgb(161, 161, 170);
                    sw.ForeColor = role == "warning" ? Warning : Text;
                    break;

                case FlatProgressBar pb:
                    pb.TrackColor = Track;
                    pb.FillColor = Accent;
                    break;

                case Label lbl:
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = role switch
                    {
                        "muted" => Muted,
                        "caption" => Caption,
                        "success" => Success,
                        "accent" => AccentText,
                        "warning" => Warning,
                        "secondary" => TextSecondary,
                        _ => Text
                    };
                    break;

                case Button b:
                    StyleButton(b, role);
                    break;

                case TextBox tb:
                    tb.BackColor = Field;
                    tb.ForeColor = Text;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ComboBox cb:
                    cb.BackColor = Field;
                    cb.ForeColor = Text;
                    cb.FlatStyle = FlatStyle.Flat;
                    break;

                case ListView lv:
                    lv.BackColor = Surface;
                    lv.ForeColor = Text;
                    lv.BorderStyle = BorderStyle.None;
                    break;

                case Panel p:
                    if (role == "dot") break; // colorato dal form
                    p.BackColor = role == "sidebar" ? Sidebar : Window;
                    break;
            }
        }

        public void StyleButton(Button b, string role)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
            b.FlatAppearance.BorderSize = 0;

            switch (role)
            {
                case "primary":
                    b.BackColor = Accent;
                    b.ForeColor = Color.White;
                    b.FlatAppearance.MouseOverBackColor = AccentHover;
                    b.FlatAppearance.MouseDownBackColor = AccentHover;
                    break;
                case "ghost":
                    b.BackColor = Surface;
                    b.ForeColor = Text;
                    b.FlatAppearance.BorderSize = 1;
                    b.FlatAppearance.BorderColor = FieldBorder;
                    b.FlatAppearance.MouseOverBackColor = Field;
                    b.FlatAppearance.MouseDownBackColor = Secondary;
                    break;
                case "nav":
                    // gestito dal form (stato attivo/inattivo)
                    break;
                default: // secondary
                    b.BackColor = Secondary;
                    b.ForeColor = Text;
                    b.FlatAppearance.MouseOverBackColor = SecondaryHover;
                    b.FlatAppearance.MouseDownBackColor = SecondaryHover;
                    break;
            }
        }
    }
}
