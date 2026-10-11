using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using MusicConverterPro.UI;

namespace MusicConverterPro
{
    public partial class MainForm : Form
    {
        private readonly AppSettings _settings;
        private readonly DownloadEngine _engine = new DownloadEngine();
        private readonly AudioOptimizer _optimizer = new AudioOptimizer();
        private readonly List<DownloadItem> _items = new List<DownloadItem>();
        private List<AudioItemInfo> _scannedItems = new List<AudioItemInfo>();

        private readonly ListViewColumnSorter _queueSorter = new ListViewColumnSorter();
        private readonly ListViewColumnSorter _optSorter = new ListViewColumnSorter();

        private CancellationTokenSource? _cts;
        private CancellationTokenSource? _optCts;
        private bool _isDownloading;
        private bool _isOptimizing;
        private bool _isScanning;
        private bool _loadingSettings;
        private bool _suppressDriveEvents;
        private bool _engineReady;
        private bool _engineYt;
        private bool _engineFf;
        private bool _engineJs;
        private string _lastAutoPastedUrl = "";
        private string _lastScannedFolder = "";
        private readonly CancellationTokenSource _probeCts = new CancellationTokenSource();
        private bool _isProbing;

        private Theme _theme = Theme.Dark;
        private readonly List<ListViewHeaderPainter> _headerPainters = new List<ListViewHeaderPainter>();
        private readonly Font _headerFont = new Font("Segoe UI", 8F, FontStyle.Bold);
        private readonly Font _pillFont = new Font("Segoe UI", 8.25F, FontStyle.Bold);
        private readonly Font _smallFont = new Font("Segoe UI", 8.25F, FontStyle.Regular);
        private readonly Font _rowTitleFont = new Font("Segoe UI", 9F, FontStyle.Bold);
        private readonly Font _rowSubFont = new Font("Segoe UI", 7.5F, FontStyle.Regular);
        private readonly Font _navBold = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        private readonly Font _navRegular = new Font("Segoe UI", 9.75F, FontStyle.Regular);

        private sealed class DriveEntry
        {
            public DriveInfo? Drive { get; }
            public string Root { get; }
            public bool IsCustom => Drive == null;

            public DriveEntry(DriveInfo? drive)
            {
                Drive = drive;
                Root = drive?.RootDirectory.FullName ?? "";
            }

            public override string ToString()
            {
                if (Drive == null) return "Altra cartella...";
                string label;
                string free;
                try
                {
                    label = string.IsNullOrWhiteSpace(Drive.VolumeLabel) ? "USB" : Drive.VolumeLabel;
                    free = FormatSize(Drive.AvailableFreeSpace) + " liberi";
                }
                catch
                {
                    label = "USB";
                    free = "";
                }
                return $"{Root.TrimEnd('\\')}  {label}  ·  {free}".Trim();
            }
        }

        public MainForm()
        {
            InitializeComponent();
            Icon = BrandIcon.AppIcon;
            _settings = AppSettings.Load();
            EngineManager.ExtraArgs = _settings.YtDlpExtraArgs;

            SetupListView(listViewQueue);
            SetupListView(listViewOpt);

            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        private void SetupListView(ListView lv)
        {
            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(lv, true);
            lv.OwnerDraw = true;
            lv.GridLines = false;
            lv.HideSelection = false;
            lv.SmallImageList = new ImageList { ImageSize = new Size(1, RowHeight) }; // altezza riga
            _headerPainters.Add(new ListViewHeaderPainter(lv));
            lv.DrawColumnHeader += ListView_DrawColumnHeader;
            lv.DrawItem += ListView_DrawItem;
            lv.DrawSubItem += ListView_DrawSubItem;
            lv.ColumnClick += ListView_ColumnClick;
            lv.Resize += (s, e) =>
            {
                if (lv == listViewQueue) AutoResizeQueueColumns();
                else AutoResizeOptColumns();
            };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeEngineStatus();

            try
            {
                notifyIcon.Icon = Icon;
                notifyIcon.Visible = true;
            }
            catch { }

            _loadingSettings = true;
            textBoxFolder.Text = _settings.DownloadFolder;
            toggleThumbnail.Checked = _settings.EmbedThumbnail;
            toggleMetadata.Checked = _settings.EmbedMetadata;
            toggleCarAudio.Checked = _settings.CarAudioMode;
            toggleCleanNames.Checked = _settings.CleanFileNames;
            toggleAutoPaste.Checked = _settings.AutoPaste;
            toggleOptCleanNames.Checked = _settings.OptCleanNames;
            toggleOptRemoveDuplicates.Checked = _settings.RemoveDuplicates;
            toggleOptNormalize.Checked = _settings.OptNormalize;
            toggleOptFixStandard.Checked = _settings.OptFixStandard;
            toggleOptConvertAll.Checked = _settings.OptConvertAll;
            toggleOptSkipCompliant.Checked = _settings.OptSkipCompliant;
            toggleOptInPlace.Checked = _settings.OptReplaceOriginals;
            SelectFormat(_settings.AudioFormat);
            SelectBitrate(_settings.AudioBitrate);
            comboBoxTheme.SelectedIndex = _settings.ThemeMode switch { "Dark" => 1, "Light" => 2, _ => 0 };
            _loadingSettings = false;

            labelBrand.Text = AppInfo.Name;
            labelBrandSub.Text = $"v{AppInfo.Version} · scarica, converti, ottimizza";
            labelQueueCount.Left = labelQueueHeader.Right + 10;
            labelOptSummary.Left = labelOptQueueHeader.Right + 10;

            ApplyEffectiveTheme();
            ShowDownloader();
            UpdateQueueHeader();
            AutoResizeQueueColumns();
            AutoResizeOptColumns();

            clipboardTimer.Enabled = _settings.AutoPaste;
            PopulateDrives(selectFirstRemovable: true);
        }

        #region Motore

        private void InitializeEngineStatus()
        {
            _engineReady = EngineManager.Initialize(out string msg);
            _engineYt = !string.IsNullOrEmpty(EngineManager.YtDlpPath) && File.Exists(EngineManager.YtDlpPath);
            _engineFf = !string.IsNullOrEmpty(EngineManager.FFmpegPath) && File.Exists(EngineManager.FFmpegPath);
            bool deno = !string.IsNullOrEmpty(EngineManager.DenoPath) && File.Exists(EngineManager.DenoPath);
            bool node = !string.IsNullOrEmpty(EngineManager.NodePath) && File.Exists(EngineManager.NodePath);
            _engineJs = deno || node;

            labelEngineJs.Text = deno ? "Deno (sfide JS)" : node ? "Node.js (sfide JS)" : "Runtime JS assente";
            labelEngineState.Text = _engineReady ? "Pronto" : "Problema";
            labelEngineState.Tag = _engineReady ? "success" : "warning";

            if (!_engineReady)
            {
                MessageBox.Show(msg, "Verifica componenti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateEngineStatusColors()
        {
            labelEngineState.ForeColor = _engineReady ? _theme.Success : _theme.Warning;
            StyleDot(dotYtdlp, _engineYt);
            StyleDot(dotFfmpeg, _engineFf);
            StyleDot(dotJs, _engineJs);
        }

        private void StyleDot(Panel dot, bool ok)
        {
            dot.BackColor = ok ? _theme.Success : _theme.Danger;
            using var path = new GraphicsPath();
            path.AddEllipse(0, 0, dot.Width, dot.Height);
            dot.Region = new Region(path);
        }

        #endregion

        #region Tema e stile

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (_settings.ThemeMode == "Auto" && !IsDisposed)
            {
                BeginInvoke(new Action(ApplyEffectiveTheme));
            }
        }

        private void ApplyEffectiveTheme()
        {
            _theme = Theme.Resolve(_settings.ThemeMode);

            SuspendLayout();
            BackColor = _theme.Window;
            ForeColor = _theme.Text;
            _theme.Apply(this);
            NativeTheme.ApplyTitleBar(this, _theme.IsDark);
            foreach (var painter in _headerPainters)
            {
                painter.BackColor = _theme.Surface;
                painter.BorderColor = _theme.Border;
            }
            UpdateNavButtons();
            UpdateEngineStatusColors();
            UpdateCancelButtons();
            StylePrimary(buttonStartQueue, buttonStartQueue.Enabled);
            StylePrimary(buttonOptStart, buttonOptStart.Enabled);
            ResumeLayout();
            Invalidate(true);
        }

        private void StyleNav(Button b, bool active)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
            b.Font = active ? _navBold : _navRegular;
            b.BackColor = active ? _theme.AccentSoft : _theme.Sidebar;
            b.ForeColor = active ? _theme.AccentText : _theme.TextSecondary;
            b.FlatAppearance.MouseOverBackColor = active ? _theme.AccentSoft : _theme.Surface;
            b.FlatAppearance.MouseDownBackColor = _theme.AccentSoft;
        }

        private void UpdateNavButtons()
        {
            StyleNav(buttonNavDownloader, panelDownloaderContainer.Visible);
            StyleNav(buttonNavOptimizer, panelOptimizerContainer.Visible);
        }

        private void StylePrimary(Button b, bool enabled)
        {
            b.Enabled = enabled;
            if (enabled)
            {
                _theme.StyleButton(b, "primary");
            }
            else
            {
                b.BackColor = _theme.AccentSoft;
                b.ForeColor = _theme.AccentText;
            }
        }

        private void StyleCancel(Button b, bool active)
        {
            b.Enabled = active;
            if (active)
            {
                _theme.StyleButton(b, "secondary");
                b.BackColor = _theme.DangerSoft;
                b.ForeColor = _theme.Danger;
                b.FlatAppearance.MouseOverBackColor = _theme.Accent;
                b.FlatAppearance.MouseDownBackColor = _theme.AccentHover;
            }
            else
            {
                _theme.StyleButton(b, "secondary");
            }
        }

        private void UpdateCancelButtons()
        {
            StyleCancel(buttonCancel, _isDownloading);
            StyleCancel(buttonOptCancel, _isOptimizing);
        }

        private void comboBoxTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
            _settings.ThemeMode = comboBoxTheme.SelectedIndex switch
            {
                1 => "Dark",
                2 => "Light",
                _ => "Auto"
            };
            _settings.Save();
            ApplyEffectiveTheme();
        }

        #endregion

        #region Tabelle (disegno personalizzato)

        private void ListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            var lv = (ListView)sender!;
            using (var bg = new SolidBrush(_theme.Surface))
            {
                e.Graphics.FillRectangle(bg, e.Bounds);
            }
            using (var pen = new Pen(_theme.Border))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            var r = e.Bounds;
            r.Inflate(-8, 0);
            bool right = lv == listViewOpt && e.ColumnIndex == 3;
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix |
                        (right ? TextFormatFlags.Right : TextFormatFlags.Left);
            TextRenderer.DrawText(e.Graphics, (e.Header?.Text ?? "").ToUpperInvariant(), _headerFont, r, _theme.Caption, flags);
        }

        private void ListView_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            // Tutto il disegno avviene cella per cella in DrawSubItem.
        }

        private void ListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            var lv = (ListView)sender!;
            var g = e.Graphics;
            if (e.Item == null) return;

            var bounds = e.Bounds;
            if (e.ColumnIndex == lv.Columns.Count - 1 && bounds.Right < lv.ClientSize.Width)
            {
                bounds.Width = lv.ClientSize.Width - bounds.Left;
            }

            using (var b = new SolidBrush(RowBackground(e.Item, e.ItemIndex)))
            {
                g.FillRectangle(b, bounds);
            }
            using (var p = new Pen(_theme.Border))
            {
                g.DrawLine(p, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
            }

            var cell = e.Bounds;
            cell.Inflate(-8, 0);
            if (cell.Width <= 4) return;

            const TextFormatFlags baseFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

            if (e.Item.Tag is DownloadItem d)
            {
                switch (e.ColumnIndex)
                {
                    case 0:
                        DrawTitleCell(g, cell, d.Title, d.Subtitle);
                        return;
                    case 1:
                        DrawTag(g, cell, d.Format.ToUpperInvariant());
                        return;
                    case 3:
                        DrawProgressCell(g, cell, d.ProgressPercent, ProgressColor(d.State));
                        return;
                    case 4:
                        var (bg, fg) = PillColors(d.State);
                        DrawPill(g, cell, d.GetStatusDisplay(), bg, fg);
                        return;
                }
            }
            else if (e.Item.Tag is AudioItemInfo a)
            {
                switch (e.ColumnIndex)
                {
                    case 2:
                        DrawTag(g, cell, a.FormatLabel);
                        return;
                    case 3:
                        TextRenderer.DrawText(g, e.SubItem?.Text ?? "", lv.Font, cell, _theme.TextSecondary, baseFlags | TextFormatFlags.Right);
                        return;
                    case 4:
                        var (bg, fg) = PillColors(a.Kind);
                        DrawPill(g, cell, a.Status, bg, fg);
                        return;
                }
            }

            bool first = e.ColumnIndex == 0;
            TextRenderer.DrawText(g, e.SubItem?.Text ?? "", first ? _rowTitleFont : lv.Font, cell,
                first ? _theme.Text : _theme.TextSecondary, baseFlags | TextFormatFlags.Left);
        }

        private Color RowBackground(ListViewItem item, int index)
        {
            if (item.Selected) return _theme.Selection;
            if (item.Tag is AudioItemInfo a)
            {
                if (a.Kind == AudioItemKind.Busy) return _theme.RowBusy;
                if (a.IsDuplicate && a.Kind != AudioItemKind.Deleted) return _theme.RowDuplicate;
            }
            if (item.Tag is DownloadItem d && d.IsBusy) return _theme.RowBusy;
            return index % 2 == 0 ? _theme.Surface : _theme.RowAlt;
        }

        private Color ProgressColor(DownloadState s) => s switch
        {
            DownloadState.Completato => _theme.Success,
            DownloadState.Errore => _theme.Danger,
            DownloadState.InCoda => _theme.FieldBorder,
            DownloadState.Annullato => _theme.FieldBorder,
            _ => _theme.Accent
        };

        private (Color bg, Color fg) PillColors(DownloadState s) => s switch
        {
            DownloadState.Completato => (_theme.SuccessSoft, _theme.Success),
            DownloadState.Errore => (_theme.DangerSoft, _theme.Danger),
            DownloadState.Annullato => (_theme.Secondary, _theme.Muted),
            DownloadState.InCoda => (_theme.Secondary, _theme.TextSecondary),
            _ => (_theme.AccentSoft, _theme.AccentText)
        };

        private (Color bg, Color fg) PillColors(AudioItemKind k) => k switch
        {
            AudioItemKind.Duplicate => (_theme.WarningSoft, _theme.Warning),
            AudioItemKind.Deleted => (_theme.WarningSoft, _theme.Warning),
            AudioItemKind.Master => (_theme.SuccessSoft, _theme.Success),
            AudioItemKind.Done => (_theme.SuccessSoft, _theme.Success),
            AudioItemKind.ToConvert => (_theme.InfoSoft, _theme.Info),
            AudioItemKind.Busy => (_theme.AccentSoft, _theme.AccentText),
            AudioItemKind.Error => (_theme.DangerSoft, _theme.Danger),
            AudioItemKind.Skipped => (_theme.Secondary, _theme.Muted),
            _ => (_theme.Secondary, _theme.TextSecondary)
        };

        /// <summary>Titolo su una riga, oppure titolo + riga secondaria (playlist/sito e durata).</summary>
        private void DrawTitleCell(Graphics g, Rectangle cell, string title, string subtitle)
        {
            const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            if (string.IsNullOrEmpty(subtitle))
            {
                TextRenderer.DrawText(g, title, _rowTitleFont, cell, _theme.Text, flags | TextFormatFlags.VerticalCenter);
                return;
            }

            var top = new Rectangle(cell.Left, cell.Top + 3, cell.Width, 17);
            var bottom = new Rectangle(cell.Left, cell.Top + 20, cell.Width, 14);
            TextRenderer.DrawText(g, title, _rowTitleFont, top, _theme.Text, flags | TextFormatFlags.Top);
            TextRenderer.DrawText(g, subtitle, _rowSubFont, bottom, _theme.Caption, flags | TextFormatFlags.Top);
        }

        private void DrawPill(Graphics g, Rectangle cell, string text, Color bg, Color fg)
        {
            var size = TextRenderer.MeasureText(g, text, _pillFont, new Size(int.MaxValue, cell.Height), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            int h = 22;
            int w = Math.Min(cell.Width, size.Width + 18);
            var rect = new Rectangle(cell.Left, cell.Top + (cell.Height - h) / 2, w, h);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CardPanel.RoundedRect(rect, 11))
            using (var brush = new SolidBrush(bg))
            {
                g.FillPath(brush, path);
            }
            g.SmoothingMode = SmoothingMode.Default;

            var tr = rect;
            tr.Inflate(-9, 0);
            TextRenderer.DrawText(g, text, _pillFont, tr, fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        }

        private void DrawTag(Graphics g, Rectangle cell, string text)
        {
            var size = TextRenderer.MeasureText(g, text, _pillFont, new Size(int.MaxValue, cell.Height), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            int h = 20;
            int w = Math.Min(cell.Width, size.Width + 14);
            var rect = new Rectangle(cell.Left, cell.Top + (cell.Height - h) / 2, w, h);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CardPanel.RoundedRect(rect, 5))
            using (var brush = new SolidBrush(_theme.Secondary))
            {
                g.FillPath(brush, path);
            }
            g.SmoothingMode = SmoothingMode.Default;

            TextRenderer.DrawText(g, text, _pillFont, rect, _theme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        }

        private void DrawProgressCell(Graphics g, Rectangle cell, int pct, Color fill)
        {
            const int pctW = 38;
            const int barH = 6;
            int barW = cell.Width - pctW - 6;
            if (barW < 10) return;

            var track = new Rectangle(cell.Left, cell.Top + (cell.Height - barH) / 2, barW, barH);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CardPanel.RoundedRect(track, 3))
            using (var brush = new SolidBrush(_theme.Track))
            {
                g.FillPath(brush, path);
            }

            int fw = (int)Math.Round(barW * Math.Clamp(pct, 0, 100) / 100.0);
            if (fw > 0)
            {
                var fr = new Rectangle(track.Left, track.Top, Math.Max(fw, barH), barH);
                using var path = CardPanel.RoundedRect(fr, 3);
                using var brush = new SolidBrush(fill);
                g.FillPath(brush, path);
            }
            g.SmoothingMode = SmoothingMode.Default;

            TextRenderer.DrawText(g, pct + "%", _smallFont, new Rectangle(track.Right + 6, cell.Top, pctW, cell.Height), _theme.Muted,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        private void ListView_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            var lv = (ListView)sender!;
            var sorter = lv == listViewQueue ? _queueSorter : _optSorter;

            if (lv.ListViewItemSorter == null)
            {
                lv.ListViewItemSorter = sorter;
            }

            if (e.Column == sorter.SortColumn)
            {
                sorter.Order = sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                sorter.SortColumn = e.Column;
                sorter.Order = SortOrder.Ascending;
            }

            lv.BeginUpdate();
            lv.Sort();
            lv.EndUpdate();
            lv.Invalidate();
        }

        /// <summary>Larghezza utile per le colonne, al netto della barra di scorrimento verticale
        /// (anche quando deve ancora comparire) per non far mai apparire quella orizzontale.</summary>
        private const int RowHeight = 36;

        private static int UsableWidth(ListView lv, int? expectedItems = null)
        {
            const int headerHeight = 28;
            int count = expectedItems ?? lv.Items.Count;
            bool needsVScroll = count * RowHeight + headerHeight > lv.Height;
            int width = lv.Width - 4;
            if (needsVScroll) width -= SystemInformation.VerticalScrollBarWidth;
            return width;
        }

        private void AutoResizeQueueColumns() => AutoResizeQueueColumns(null);

        private void AutoResizeQueueColumns(int? expectedItems)
        {
            if (listViewQueue.Columns.Count < 5) return;
            int available = UsableWidth(listViewQueue, expectedItems);
            if (available < 400) return;

            int formatWidth = 84;
            int qualityWidth = 90;
            int progressWidth = 150;
            int statusWidth = Math.Max(170, (int)(available * 0.22));
            int titleWidth = Math.Max(200, available - (formatWidth + qualityWidth + progressWidth + statusWidth));

            listViewQueue.Columns[0].Width = titleWidth;
            listViewQueue.Columns[1].Width = formatWidth;
            listViewQueue.Columns[2].Width = qualityWidth;
            listViewQueue.Columns[3].Width = progressWidth;
            listViewQueue.Columns[4].Width = statusWidth;
        }

        private void AutoResizeOptColumns() => AutoResizeOptColumns(null);

        private void AutoResizeOptColumns(int? expectedItems)
        {
            if (listViewOpt.Columns.Count < 5) return;
            int available = UsableWidth(listViewOpt, expectedItems);
            if (available < 400) return;

            int folderWidth = Math.Max(120, (int)(available * 0.15));
            int formatWidth = 84;
            int sizeWidth = 100;
            int statusWidth = Math.Max(240, (int)(available * 0.30));
            int fileWidth = Math.Max(200, available - (folderWidth + formatWidth + sizeWidth + statusWidth));

            listViewOpt.Columns[0].Width = fileWidth;
            listViewOpt.Columns[1].Width = folderWidth;
            listViewOpt.Columns[2].Width = formatWidth;
            listViewOpt.Columns[3].Width = sizeWidth;
            listViewOpt.Columns[4].Width = statusWidth;
        }

        private static ListViewItem? FindRow(ListView lv, object tag)
        {
            foreach (ListViewItem row in lv.Items)
            {
                if (ReferenceEquals(row.Tag, tag)) return row;
            }
            return null;
        }

        #endregion

        #region Navigazione

        private void ShowDownloader()
        {
            panelDownloaderContainer.Visible = true;
            panelOptimizerContainer.Visible = false;
            UpdateNavButtons();
            BeginInvoke(new Action(AutoResizeQueueColumns));
        }

        private void ShowOptimizer()
        {
            panelDownloaderContainer.Visible = false;
            panelOptimizerContainer.Visible = true;
            UpdateNavButtons();
            BeginInvoke(new Action(AutoResizeOptColumns));
        }

        private void buttonTabDownloader_Click(object sender, EventArgs e) => ShowDownloader();

        private void buttonTabOptimizer_Click(object sender, EventArgs e) => ShowOptimizer();

        #endregion

        #region Auto-Paste e notifiche

        private void checkBoxAutoPaste_CheckedChanged(object sender, EventArgs e)
        {
            clipboardTimer.Enabled = toggleAutoPaste.Checked;
            if (_loadingSettings) return;
            _settings.AutoPaste = toggleAutoPaste.Checked;
            _settings.Save();
        }

        private void clipboardTimer_Tick(object sender, EventArgs e)
        {
            if (!toggleAutoPaste.Checked || IsDisposed || _isProbing) return;

            try
            {
                if (!Clipboard.ContainsText()) return;

                string text = Clipboard.GetText().Trim();
                if (string.IsNullOrEmpty(text) || text == _lastAutoPastedUrl) return;

                // Solo link http/https e, se configurato in config.json, solo dei domini elencati:
                // un testo qualsiasi copiato non deve finire in coda né svuotare gli appunti.
                if (!IsAutoPasteUrl(text)) return;

                _lastAutoPastedUrl = text;
                if (IsAlreadyQueued(text)) return;

                try { Clipboard.Clear(); } catch { }
                _ = AddFromClipboardAsync(text);
            }
            catch { }
        }

        /// <summary>Auto-Paste: nessuna finestra di dialogo. Una playlist pura entra tutta,
        /// un video dentro una playlist entra da solo.</summary>
        private async Task AddFromClipboardAsync(string url)
        {
            var probe = await ProbeWithStatusAsync(url);
            if (IsDisposed) return;

            int added;
            string what;

            if (probe != null && probe.IsPlaylist && probe.Entries.Count > 0)
            {
                var single = await ProbeSingleInPlaylistAsync(url, probe);
                if (IsDisposed) return;
                if (single != null)
                {
                    what = single.Title;
                    added = AddSingle(url, single.Title, single.DurationSeconds) ? 1 : 0;
                    labelStatusText.Text = $"Aggiunto dagli appunti: {Shorten(what, 60)}  ·  il link contiene anche una playlist: usa \"Aggiungi in coda\" per averla tutta.";
                }
                else
                {
                    added = AddPlaylist(probe, _settings.PlaylistSubfolder, _settings.PlaylistNumbering);
                    what = $"Playlist «{probe.Title}»: {added} brani";
                }
            }
            else
            {
                what = probe?.Title ?? url;
                added = AddSingle(url, probe?.Title, probe?.DurationSeconds) ? 1 : 0;
            }

            if (added > 0)
            {
                System.Media.SystemSounds.Asterisk.Play();
                notifyIcon.ShowBalloonTip(3500, AppInfo.Name, $"Aggiunto alla coda:\n{Shorten(what, 80)}", ToolTipIcon.Info);
            }
        }

        private static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && uri.Host.Contains('.');
            }
            return false;
        }

        /// <summary>Link accettato dall'Auto-Paste: http/https e, se la lista in config.json non è vuota, uno dei domini elencati.</summary>
        private bool IsAutoPasteUrl(string text)
        {
            if (!IsValidUrl(text)) return false;
            if (_settings.AutoPasteDomains == null || _settings.AutoPasteDomains.Count == 0) return true;
            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri)) return false;

            string host = uri.Host.ToLowerInvariant();
            foreach (var domain in _settings.AutoPasteDomains)
            {
                string d = (domain ?? "").Trim().TrimStart('.').ToLowerInvariant();
                if (d.Length == 0) continue;
                if (host == d || host.EndsWith("." + d, StringComparison.Ordinal)) return true;
            }
            return false;
        }

        private void notifyIcon_BalloonTipClicked(object sender, EventArgs e) => RestoreWindow();

        private void notifyIcon_DoubleClick(object sender, EventArgs e) => RestoreWindow();

        private void RestoreWindow()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            Show();
            Activate();
        }

        #endregion

        #region Scaricatore

        private void textBoxUrl_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                buttonAddQueue_Click(sender!, EventArgs.Empty);
            }
        }

        private void buttonPaste_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText().Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        textBoxUrl.Text = text;
                        textBoxUrl.Focus();
                        textBoxUrl.SelectAll();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossibile leggere dagli appunti: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonBrowseFolder_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Seleziona la cartella di salvataggio per i file audio",
                UseDescriptionForTitle = true
            };

            if (Directory.Exists(textBoxFolder.Text))
            {
                fbd.SelectedPath = textBoxFolder.Text;
            }

            if (fbd.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                textBoxFolder.Text = fbd.SelectedPath;
                _settings.DownloadFolder = fbd.SelectedPath;
                _settings.Save();
            }
        }

        private void buttonOpenFolder_Click(object sender, EventArgs e) => OpenFolder(textBoxFolder.Text);

        private static void OpenFolder(string folder)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(folder)) return;
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossibile aprire la cartella: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectFormat(string format)
        {
            for (int i = 0; i < comboBoxFormat.Items.Count; i++)
            {
                if (string.Equals(comboBoxFormat.Items[i]?.ToString(), format, StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxFormat.SelectedIndex = i;
                    return;
                }
            }
            comboBoxFormat.SelectedIndex = 0;
        }

        private void SelectBitrate(string bitrate)
        {
            string digits = new string((bitrate ?? "").Where(char.IsDigit).ToArray());
            for (int i = 0; i < comboBoxBitrate.Items.Count; i++)
            {
                string text = comboBoxBitrate.Items[i]?.ToString() ?? "";
                if (digits.Length > 0 && text.StartsWith(digits, StringComparison.Ordinal))
                {
                    comboBoxBitrate.SelectedIndex = i;
                    return;
                }
            }
            comboBoxBitrate.SelectedIndex = 0;
        }

        private string GetSelectedFormat() => (comboBoxFormat.SelectedItem?.ToString() ?? "mp3").ToLowerInvariant();

        private string GetSelectedBitrate()
        {
            string raw = comboBoxBitrate.SelectedItem?.ToString() ?? "320";
            string digits = new string(raw.TakeWhile(char.IsDigit).ToArray());
            return (digits.Length > 0 ? digits : "320") + "k";
        }

        private static string QualityLabel(DownloadItem item)
        {
            if (item.Format == "flac" || item.Format == "wav") return "lossless";
            return item.Bitrate.TrimEnd('k') + " kbps";
        }

        private void comboBoxFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            string format = GetSelectedFormat();
            comboBoxBitrate.Enabled = format != "flac" && format != "wav";
            if (_loadingSettings) return;
            _settings.AudioFormat = format;
            _settings.Save();
        }

        private void comboBoxBitrate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
            _settings.AudioBitrate = GetSelectedBitrate();
            _settings.Save();
        }

        private void checkBoxThumbnail_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
            _settings.EmbedThumbnail = toggleThumbnail.Checked;
            _settings.Save();
        }

        private void checkBoxMetadata_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
            _settings.EmbedMetadata = toggleMetadata.Checked;
            _settings.Save();
        }

        private void checkBoxCarAudio_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
            _settings.CarAudioMode = toggleCarAudio.Checked;
            _settings.Save();
        }

        private void checkBoxCleanNames_CheckedChanged(object? sender, EventArgs e)
        {
            if (_loadingSettings) return;
            _settings.CleanFileNames = toggleCleanNames.Checked;
            _settings.Save();
        }

        private async void buttonAddQueue_Click(object sender, EventArgs e)
        {
            string url = textBoxUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                labelStatusText.Text = "Incolla prima un link.";
                return;
            }
            await AddUrlInteractiveAsync(url, startAfter: false);
        }

        private async void buttonDownloadDirect_Click(object sender, EventArgs e)
        {
            string url = textBoxUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                if (_items.Count > 0 && !_isDownloading)
                {
                    _ = StartQueueDownloadAsync();
                    return;
                }
                MessageBox.Show("Incolla un link valido prima di avviare il download.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await AddUrlInteractiveAsync(url, startAfter: true);
        }

        /// <summary>
        /// Aggiunta manuale: legge il link con yt-dlp; se è una playlist chiede cosa fare,
        /// altrimenti aggiunge il singolo brano. Restituisce quanti elementi sono entrati in coda.
        /// </summary>
        private async Task<int> AddUrlInteractiveAsync(string url, bool startAfter)
        {
            url = url.Trim();
            if (!IsValidUrl(url))
            {
                labelStatusText.Text = "Il testo incollato non è un link valido (serve http:// o https://).";
                return 0;
            }
            if (IsAlreadyQueued(url))
            {
                labelStatusText.Text = "Questo link è già in coda.";
                return 0;
            }
            if (_isProbing)
            {
                labelStatusText.Text = "Attendi: sto ancora leggendo il link precedente.";
                return 0;
            }

            var probe = await ProbeWithStatusAsync(url);
            if (IsDisposed) return 0;

            int added;
            if (probe != null && probe.IsPlaylist)
            {
                if (probe.Entries.Count == 0)
                {
                    labelStatusText.Text = "La playlist è vuota o non accessibile.";
                    return 0;
                }

                var single = await ProbeSingleInPlaylistAsync(url, probe);
                if (IsDisposed) return 0;

                using var dialog = new PlaylistDialog(probe, single != null, _settings);
                dialog.ApplyTheme(_theme);
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Choice == PlaylistChoice.Cancel)
                {
                    labelStatusText.Text = "Aggiunta annullata.";
                    return 0;
                }

                if (dialog.Choice == PlaylistChoice.SingleVideo)
                {
                    added = AddSingle(url, single?.Title, single?.DurationSeconds) ? 1 : 0;
                }
                else
                {
                    _settings.PlaylistSubfolder = dialog.CreateSubfolder;
                    _settings.PlaylistNumbering = dialog.NumberFiles;
                    _settings.Save();
                    added = AddPlaylist(probe, dialog.CreateSubfolder, dialog.NumberFiles);
                }
            }
            else
            {
                added = AddSingle(url, probe?.Title, probe?.DurationSeconds) ? 1 : 0;
            }

            if (added > 0)
            {
                textBoxUrl.Clear();
                if (startAfter && !_isDownloading)
                {
                    _ = StartQueueDownloadAsync();
                }
            }
            return added;
        }

        private async Task<MediaProbe?> ProbeWithStatusAsync(string url)
        {
            _isProbing = true;
            buttonAddQueue.Enabled = false;
            StylePrimary(buttonDownloadDirect, false);
            labelStatusText.Text = "Lettura del link in corso...";
            try
            {
                return await EngineManager.ProbeAsync(url, _probeCts.Token);
            }
            catch
            {
                return null;
            }
            finally
            {
                _isProbing = false;
                if (!IsDisposed)
                {
                    buttonAddQueue.Enabled = true;
                    StylePrimary(buttonDownloadDirect, true);
                }
            }
        }

        /// <summary>
        /// Se il link indica anche un singolo elemento dentro la playlist, yt-dlp con --no-playlist
        /// restituisce quel solo elemento: in tal caso lo descriviamo, altrimenti null.
        /// </summary>
        private async Task<MediaEntry?> ProbeSingleInPlaylistAsync(string url, MediaProbe playlist)
        {
            try
            {
                labelStatusText.Text = "Lettura del link in corso...";
                var single = await EngineManager.ProbeAsync(url, _probeCts.Token, noPlaylist: true);
                if (single == null || single.IsPlaylist) return null;
                if (string.IsNullOrEmpty(single.Id) || single.Id == playlist.Id) return null;
                return new MediaEntry { Url = url, Title = single.Title, Id = single.Id, DurationSeconds = single.DurationSeconds };
            }
            catch
            {
                return null;
            }
        }

        private bool IsAlreadyQueued(string url) => _items.Exists(i => i.Url.Equals(url, StringComparison.OrdinalIgnoreCase));

        private DownloadItem NewItem(string url, string? title, double? duration) => new DownloadItem
        {
            Url = url,
            Title = string.IsNullOrWhiteSpace(title) ? url : (_settings.CleanFileNames ? TitleCleaner.Clean(title!) : title!),
            Format = GetSelectedFormat(),
            Bitrate = GetSelectedBitrate(),
            State = DownloadState.InCoda,
            DurationSeconds = duration
        };

        private bool AddSingle(string url, string? title, double? duration)
        {
            if (IsAlreadyQueued(url)) return false;
            var item = NewItem(url, title, duration);
            AppendRows(new[] { item });
            labelStatusText.Text = $"Aggiunto alla coda: {Shorten(item.Title, 90)}";
            if (string.IsNullOrWhiteSpace(title))
            {
                FetchTitleInBackground(item);
            }
            return true;
        }

        private int AddPlaylist(MediaProbe probe, bool subfolder, bool numbering)
        {
            string folder = subfolder ? (_settings.CleanFileNames ? TitleCleaner.Clean(probe.Title) : SanitizeFileName(probe.Title)) : "";
            int count = probe.Entries.Count;
            var items = new List<DownloadItem>();
            int index = 0;

            foreach (var entry in probe.Entries)
            {
                index++;
                if (IsAlreadyQueued(entry.Url)) continue;

                var item = NewItem(entry.Url, entry.Title, entry.DurationSeconds);
                item.PlaylistTitle = probe.Title;
                item.PlaylistIndex = index;
                item.PlaylistCount = count;
                item.Subfolder = folder;
                item.FilePrefix = numbering ? (count >= 100 ? $"{index:000} - " : $"{index:00} - ") : "";
                items.Add(item);
            }

            AppendRows(items);
            labelStatusText.Text = items.Count == count
                ? $"Playlist «{probe.Title}»: {count} brani aggiunti alla coda."
                : $"Playlist «{probe.Title}»: {items.Count} brani aggiunti ({count - items.Count} erano già in coda).";
            return items.Count;
        }

        private void AppendRows(IEnumerable<DownloadItem> items)
        {
            var list = items.ToList();
            listViewQueue.BeginUpdate();
            // Colonne dimensionate PRIMA di inserire le righe, così la barra orizzontale non compare mai
            AutoResizeQueueColumns(_items.Count + list.Count);
            foreach (var item in list)
            {
                _items.Add(item);
                var lvi = new ListViewItem(item.Title) { Tag = item };
                lvi.SubItems.Add(item.Format.ToUpperInvariant());
                lvi.SubItems.Add(QualityLabel(item));
                lvi.SubItems.Add("0%");
                lvi.SubItems.Add(item.GetStatusDisplay());
                listViewQueue.Items.Add(lvi);
            }
            listViewQueue.EndUpdate();
            BeginInvoke(new Action(AutoResizeQueueColumns));
            UpdateQueueHeader();
        }

        private void FetchTitleInBackground(DownloadItem item)
        {
            Task.Run(async () =>
            {
                try
                {
                    string? title = await EngineManager.FetchTitleAsync(item.Url, _probeCts.Token);
                    if (string.IsNullOrEmpty(title) || IsDisposed) return;
                    if (_settings.CleanFileNames) title = TitleCleaner.Clean(title);
                    item.Title = title;
                    BeginInvoke(new Action(() =>
                    {
                        if (IsDisposed) return;
                        var row = FindRow(listViewQueue, item);
                        if (row != null)
                        {
                            row.Text = title;
                            listViewQueue.Invalidate(row.Bounds);
                        }
                    }));
                }
                catch { }
            });
        }

        /// <summary>Nome di cartella valido su Windows, senza "%" (campo speciale per yt-dlp) e non troppo lungo.</summary>
        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                sb.Append(Array.IndexOf(invalid, c) >= 0 || c == '%' ? ' ' : c);
            }
            string clean = Regex.Replace(sb.ToString(), @"\s+", " ").Trim().TrimEnd('.');
            if (clean.Length > 80) clean = clean.Substring(0, 80).Trim();
            return string.IsNullOrEmpty(clean) ? "Playlist" : clean;
        }

        private void UpdateQueueHeader()
        {
            int total = _items.Count;
            if (total == 0)
            {
                labelQueueCount.Text = "nessun elemento";
                return;
            }
            int done = _items.Count(i => i.State == DownloadState.Completato);
            int errors = _items.Count(i => i.State == DownloadState.Errore);
            string text = $"{total} {(total == 1 ? "elemento" : "elementi")}";
            if (done > 0) text += $"  ·  {done} completati";
            if (errors > 0) text += $"  ·  {errors} con errori";
            labelQueueCount.Text = text;
        }

        private void listViewQueue_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                buttonRemoveSelected_Click(sender!, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void buttonRemoveSelected_Click(object sender, EventArgs e)
        {
            var selected = listViewQueue.SelectedItems.Cast<ListViewItem>().ToList();
            if (selected.Count == 0)
            {
                labelStatusText.Text = "Seleziona almeno una riga da rimuovere.";
                return;
            }

            int skipped = 0;
            foreach (var lvi in selected)
            {
                if (lvi.Tag is not DownloadItem item) continue;
                if (item.IsBusy)
                {
                    skipped++;
                    continue;
                }
                _items.Remove(item);
                listViewQueue.Items.Remove(lvi);
            }

            UpdateQueueHeader();
            listViewQueue.Invalidate();
            labelStatusText.Text = skipped > 0
                ? "Gli elementi in download non possono essere rimossi: usa Annulla."
                : "Elementi rimossi dalla coda.";
        }

        private void btnClearQueue_Click(object sender, EventArgs e)
        {
            if (_isDownloading)
            {
                MessageBox.Show("Impossibile svuotare la coda durante il download. Premi prima Annulla.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _items.Clear();
            listViewQueue.Items.Clear();
            UpdateQueueHeader();
            progressBar.Value = 0;
            labelPercentText.Text = "0%";
            labelStatusText.Text = "Coda svuotata.";
            labelMetricsText.Text = "Velocità -  ·  rimanente -";
        }

        private async void buttonStartQueue_Click(object sender, EventArgs e)
        {
            await StartQueueDownloadAsync();
        }

        private async Task StartQueueDownloadAsync()
        {
            if (_isDownloading) return;

            if (!_engineReady)
            {
                MessageBox.Show("Il motore di download non è disponibile: controlla la cartella tools (yt-dlp.exe e ffmpeg.exe).", "Motore non pronto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_items.Count == 0)
            {
                labelStatusText.Text = "La coda è vuota: incolla prima un link.";
                return;
            }

            _isDownloading = true;
            _cts = new CancellationTokenSource();
            StylePrimary(buttonStartQueue, false);
            UpdateCancelButtons();

            int completedCount = 0;
            int failedCount = 0;

            try
            {
                var snapshot = _items.ToList();
                int total = snapshot.Count;

                for (int i = 0; i < snapshot.Count; i++)
                {
                    var item = snapshot[i];
                    if (!_items.Contains(item)) continue;
                    if (item.State == DownloadState.Completato) continue;
                    if (_cts.IsCancellationRequested) break;

                    int position = i + 1;
                    var lvi = FindRow(listViewQueue, item);
                    lvi?.EnsureVisible();

                    labelStatusText.Text = $"{position} di {total}  ·  {item.Title}";
                    labelMetricsText.Text = "Preparazione...";
                    progressBar.Value = 0;
                    labelPercentText.Text = "0%";

                    await _engine.ExecuteDownloadAsync(
                        item,
                        _settings,
                        (currItem, statusMsg) =>
                        {
                            if (IsDisposed) return;
                            BeginInvoke(new Action(() =>
                            {
                                if (IsDisposed) return;
                                labelStatusText.Text = $"{position} di {total}  ·  {currItem.Title}";
                                labelMetricsText.Text = $"{statusMsg}  ·  rimanente {currItem.Eta}  ·  {currItem.FileSize}";
                                progressBar.Value = currItem.ProgressPercent;
                                labelPercentText.Text = $"{currItem.ProgressPercent}%";

                                if (lvi != null)
                                {
                                    lvi.Text = currItem.Title;
                                    lvi.SubItems[3].Text = $"{currItem.ProgressPercent}%";
                                    lvi.SubItems[4].Text = currItem.GetStatusDisplay();
                                    listViewQueue.Invalidate(lvi.Bounds);
                                }
                                UpdateQueueHeader();
                            }));
                        },
                        _cts.Token);

                    if (item.State == DownloadState.Completato) completedCount++;
                    else if (item.State == DownloadState.Errore) failedCount++;

                    if (_cts.IsCancellationRequested) break;
                }
            }
            finally
            {
                _isDownloading = false;
                StylePrimary(buttonStartQueue, true);
                UpdateCancelButtons();
                listViewQueue.Invalidate();
                UpdateQueueHeader();

                if (_cts != null && _cts.IsCancellationRequested)
                {
                    labelStatusText.Text = "Download interrotto.";
                    labelMetricsText.Text = completedCount > 0 ? $"{completedCount} file completati prima dell'interruzione" : "";
                }
                else
                {
                    progressBar.Value = 100;
                    labelPercentText.Text = "100%";
                    string summary = completedCount == 1 ? "1 file pronto" : $"{completedCount} file pronti";
                    if (failedCount > 0) summary += $"  ·  {failedCount} con errori (doppio clic sulla riga per i dettagli)";
                    labelStatusText.Text = completedCount > 0 ? "Download completati." : "Nessun file scaricato.";
                    labelMetricsText.Text = summary + $"  ·  cartella: {_settings.DownloadFolder}";

                    if (completedCount > 0)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        notifyIcon.ShowBalloonTip(4000, "Download completati", $"{summary} in {_settings.DownloadFolder}", ToolTipIcon.Info);
                    }
                }

                _cts?.Dispose();
                _cts = null;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (_isDownloading)
            {
                labelStatusText.Text = "Interruzione in corso...";
                _cts?.Cancel();
            }
        }

        private void listViewQueue_DoubleClick(object sender, EventArgs e)
        {
            if (listViewQueue.SelectedItems.Count == 0) return;
            if (listViewQueue.SelectedItems[0].Tag is not DownloadItem item) return;

            if (item.State == DownloadState.Errore)
            {
                string details = string.IsNullOrWhiteSpace(item.ErrorDetails) ? item.ErrorMessage : item.ErrorDetails;
                MessageBox.Show(this, $"{item.Title}\n{item.Url}\n\n{details}", "Dettagli errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!string.IsNullOrEmpty(item.OutputFilePath) && File.Exists(item.OutputFilePath))
            {
                Process.Start("explorer.exe", $"/select,\"{item.OutputFilePath}\"");
            }
            else
            {
                OpenFolder(textBoxFolder.Text);
            }
        }

        #endregion

        #region Ottimizza USB

        private void PopulateDrives(bool selectFirstRemovable)
        {
            _suppressDriveEvents = true;
            try
            {
                string? currentRoot = (comboBoxOptDrive.SelectedItem as DriveEntry)?.Root;
                comboBoxOptDrive.Items.Clear();

                foreach (var d in DriveInfo.GetDrives())
                {
                    try
                    {
                        if (d.DriveType == DriveType.Removable && d.IsReady)
                        {
                            comboBoxOptDrive.Items.Add(new DriveEntry(d));
                        }
                    }
                    catch { }
                }
                comboBoxOptDrive.Items.Add(new DriveEntry(null));

                int removableCount = comboBoxOptDrive.Items.Count - 1;

                if (selectFirstRemovable && removableCount > 0 && comboBoxOptDrive.Items[0] is DriveEntry first)
                {
                    comboBoxOptDrive.SelectedIndex = 0;
                    textBoxOptFolder.Text = first.Root;
                    return;
                }

                if (!string.IsNullOrEmpty(currentRoot))
                {
                    for (int i = 0; i < removableCount; i++)
                    {
                        if (comboBoxOptDrive.Items[i] is DriveEntry entry && entry.Root == currentRoot)
                        {
                            comboBoxOptDrive.SelectedIndex = i;
                            return;
                        }
                    }
                }

                comboBoxOptDrive.SelectedIndex = comboBoxOptDrive.Items.Count - 1;
                if (string.IsNullOrWhiteSpace(textBoxOptFolder.Text))
                {
                    textBoxOptFolder.Text = _settings.DownloadFolder;
                }
            }
            finally
            {
                _suppressDriveEvents = false;
            }
        }

        private void comboBoxOptDrive_DropDown(object? sender, EventArgs e) => PopulateDrives(selectFirstRemovable: false);

        private void comboBoxOptDrive_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressDriveEvents) return;
            if (comboBoxOptDrive.SelectedItem is not DriveEntry entry) return;

            if (entry.IsCustom)
            {
                buttonOptBrowse_Click(sender!, e);
                return;
            }

            textBoxOptFolder.Text = entry.Root;
            _ = RunScanAndAnalyzeAsync(entry.Root);
        }

        private void checkBoxOptRemoveDuplicates_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
            _settings.RemoveDuplicates = toggleOptRemoveDuplicates.Checked;
            _settings.Save();
        }

        private void optToggle_CheckedChanged(object? sender, EventArgs e)
        {
            if (_loadingSettings) return;
            _settings.OptNormalize = toggleOptNormalize.Checked;
            _settings.OptFixStandard = toggleOptFixStandard.Checked;
            _settings.OptConvertAll = toggleOptConvertAll.Checked;
            _settings.OptSkipCompliant = toggleOptSkipCompliant.Checked;
            _settings.OptReplaceOriginals = toggleOptInPlace.Checked;
            _settings.OptCleanNames = toggleOptCleanNames.Checked;
            _settings.Save();
        }

        private void textBoxOptFolder_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                buttonOptScan_Click(sender!, EventArgs.Empty);
            }
        }

        private void buttonOptBrowse_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Seleziona la pendrive USB o la cartella con i file audio da ottimizzare",
                UseDescriptionForTitle = true
            };

            if (Directory.Exists(textBoxOptFolder.Text))
            {
                fbd.SelectedPath = textBoxOptFolder.Text;
            }

            if (fbd.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                textBoxOptFolder.Text = fbd.SelectedPath;

                _suppressDriveEvents = true;
                try
                {
                    comboBoxOptDrive.SelectedIndex = comboBoxOptDrive.Items.Count - 1;
                }
                finally
                {
                    _suppressDriveEvents = false;
                }

                _ = RunScanAndAnalyzeAsync(fbd.SelectedPath);
            }
        }

        private void buttonOptScan_Click(object sender, EventArgs e)
        {
            string target = textBoxOptFolder.Text;
            if (string.IsNullOrWhiteSpace(target) || !Directory.Exists(target))
            {
                MessageBox.Show("Seleziona una cartella o una pendrive valida prima di avviare l'analisi.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _ = RunScanAndAnalyzeAsync(target);
        }

        private void buttonOptOpenFolder_Click(object sender, EventArgs e) => OpenFolder(textBoxOptFolder.Text);

        private async Task RunScanAndAnalyzeAsync(string folderPath)
        {
            if (_isScanning || _isOptimizing) return;

            _isScanning = true;
            buttonOptScan.Enabled = false;
            StylePrimary(buttonOptStart, false);
            labelOptStatus.Text = $"Analisi di {folderPath} in corso...";
            labelOptMetrics.Text = "Ricerca dei file audio e dei doppioni";
            labelOptSummary.Text = "analisi in corso...";
            progressBarOpt.Value = 0;
            labelOptPercent.Text = "0%";
            listViewOpt.Items.Clear();

            try
            {
                bool cleanNames = toggleOptCleanNames.Checked;
                var items = await Task.Run(() => _optimizer.ScanAndAnalyzeFolder(folderPath, cleanNames));
                if (IsDisposed) return;

                _scannedItems = items;
                _lastScannedFolder = folderPath;
                FillOptList();

                int total = items.Count;
                int duplicates = items.Count(i => i.IsDuplicate);
                int toConvert = items.Count(i => i.Kind == AudioItemKind.ToConvert);
                int toRename = items.Count(i => !i.IsDuplicate && i.NeedsRename);
                long bytes = items.Sum(i => i.FileSize);

                string summary = total == 0 ? "nessun file audio trovato" : $"{total} file  ·  {FormatSize(bytes)}";
                if (duplicates > 0) summary += $"  ·  {duplicates} doppioni";
                if (toConvert > 0) summary += $"  ·  {toConvert} da convertire";
                if (toRename > 0) summary += $"  ·  {toRename} da rinominare";
                labelOptSummary.Text = summary;

                if (total == 0)
                {
                    labelOptStatus.Text = "Nessun file audio (.mp3, .m4a, .flac, .wav, .ogg, .opus, .wma) nella cartella selezionata.";
                    labelOptMetrics.Text = "";
                }
                else if (duplicates > 0)
                {
                    labelOptStatus.Text = $"Analisi completata: {total} file, di cui {duplicates} doppioni.";
                    labelOptMetrics.Text = "I doppioni sono evidenziati. Attiva \"Elimina i doppioni\" per rimuoverli durante l'ottimizzazione.";
                    System.Media.SystemSounds.Beep.Play();
                }
                else
                {
                    labelOptStatus.Text = $"Analisi completata: {total} file, nessun doppione.";
                    labelOptMetrics.Text = "Pronto per l'ottimizzazione.";
                }
            }
            catch (Exception ex)
            {
                labelOptStatus.Text = "Errore durante l'analisi: " + ex.Message;
                labelOptSummary.Text = "analisi fallita";
            }
            finally
            {
                _isScanning = false;
                buttonOptScan.Enabled = true;
                StylePrimary(buttonOptStart, true);
            }
        }

        private void FillOptList()
        {
            listViewOpt.BeginUpdate();
            listViewOpt.Items.Clear();
            listViewOpt.ListViewItemSorter = null;

            // Colonne dimensionate PRIMA di inserire le righe, così la barra orizzontale non compare mai
            AutoResizeOptColumns(_scannedItems.Count);

            foreach (var item in _scannedItems)
            {
                var lvi = new ListViewItem(item.FileName) { Tag = item };
                lvi.SubItems.Add(item.Folder);
                lvi.SubItems.Add(item.FormatLabel);
                lvi.SubItems.Add(FormatSize(item.FileSize));
                lvi.SubItems.Add(item.Status);
                listViewOpt.Items.Add(lvi);
            }

            listViewOpt.EndUpdate();
            // Differito: la barra di scorrimento verticale compare solo dopo il layout
            BeginInvoke(new Action(AutoResizeOptColumns));
        }

        private void buttonOptClear_Click(object sender, EventArgs e)
        {
            if (_isOptimizing || _isScanning) return;
            _scannedItems.Clear();
            _lastScannedFolder = "";
            listViewOpt.Items.Clear();
            labelOptSummary.Text = "Nessuna analisi eseguita";
            progressBarOpt.Value = 0;
            labelOptPercent.Text = "0%";
            labelOptStatus.Text = "Elenco pulito.";
            labelOptMetrics.Text = "";
        }

        private async void buttonOptStart_Click(object sender, EventArgs e)
        {
            if (_isOptimizing || _isScanning) return;

            if (!_engineFf)
            {
                MessageBox.Show("FFmpeg non è disponibile: controlla la cartella tools.", "Motore non pronto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string target = textBoxOptFolder.Text;
            if (string.IsNullOrWhiteSpace(target) || !Directory.Exists(target))
            {
                MessageBox.Show("Seleziona una cartella o una pendrive USB valida.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_scannedItems.Count == 0 || !string.Equals(_lastScannedFolder, target, StringComparison.OrdinalIgnoreCase))
            {
                await RunScanAndAnalyzeAsync(target);
                if (_scannedItems.Count == 0)
                {
                    MessageBox.Show("Nessun file audio trovato nella cartella selezionata.", "Nessun file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            var options = new OptimizeOptions
            {
                RemoveDuplicates = toggleOptRemoveDuplicates.Checked,
                Normalize = toggleOptNormalize.Checked,
                FixStandard = toggleOptFixStandard.Checked,
                ConvertAll = toggleOptConvertAll.Checked,
                SkipCompliant = toggleOptSkipCompliant.Checked,
                ReplaceOriginals = toggleOptInPlace.Checked,
                CleanNames = toggleOptCleanNames.Checked
            };

            int dupCount = _scannedItems.Count(i => i.IsDuplicate);
            int convertCount = _scannedItems.Count(i => !i.IsDuplicate && i.Extension != ".mp3");

            var lines = new List<string>();
            if (options.RemoveDuplicates && dupCount > 0)
                lines.Add($"• {dupCount} doppioni verranno eliminati definitivamente (resta la copia migliore).");
            if (options.ReplaceOriginals && options.ConvertAll && convertCount > 0)
                lines.Add($"• {convertCount} file non MP3 verranno convertiti e gli originali sostituiti.");
            if (options.ReplaceOriginals && (options.Normalize || options.FixStandard))
                lines.Add("• Gli MP3 ricodificati sostituiranno i file originali sulla pendrive.");

            if (lines.Count > 0)
            {
                var ask = MessageBox.Show(
                    "Prima di procedere:\n\n" + string.Join("\n", lines) + "\n\nVuoi continuare?",
                    "Conferma ottimizzazione",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (ask != DialogResult.Yes) return;
            }

            _isOptimizing = true;
            _optCts = new CancellationTokenSource();
            StylePrimary(buttonOptStart, false);
            buttonOptScan.Enabled = false;
            UpdateCancelButtons();

            try
            {
                var result = await _optimizer.OptimizeItemsAsync(
                    _scannedItems,
                    options,
                    info =>
                    {
                        if (IsDisposed) return;
                        BeginInvoke(new Action(() =>
                        {
                            if (IsDisposed) return;
                            labelOptStatus.Text = $"{info.CurrentIndex} di {info.TotalFiles}  ·  {info.FileName}";
                            labelOptMetrics.Text = info.Status;
                            progressBarOpt.Value = info.Percent;
                            labelOptPercent.Text = $"{info.Percent}%";

                            if (info.Item != null)
                            {
                                var row = FindRow(listViewOpt, info.Item);
                                if (row != null)
                                {
                                    row.Text = info.Item.FileName;
                                    row.SubItems[2].Text = info.Item.FormatLabel;
                                    row.SubItems[4].Text = info.Status;
                                    row.EnsureVisible();
                                    listViewOpt.Invalidate(row.Bounds);
                                }
                            }
                        }));
                    },
                    _optCts.Token);

                progressBarOpt.Value = 100;
                labelOptPercent.Text = "100%";
                labelOptStatus.Text = "Ottimizzazione completata.";
                string summary = $"{result.Optimized} ottimizzati  ·  {result.Skipped} invariati  ·  {result.DuplicatesRemoved} doppioni eliminati";
                if (result.Renamed > 0) summary += $"  ·  {result.Renamed} rinominati";
                if (result.Errors > 0) summary += $"  ·  {result.Errors} errori";
                labelOptMetrics.Text = summary;
                labelOptSummary.Text = summary;

                System.Media.SystemSounds.Asterisk.Play();
                notifyIcon.ShowBalloonTip(4500, "Ottimizzazione terminata", $"{result.Optimized} tracce pronte per l'autoradio.\nDoppioni eliminati: {result.DuplicatesRemoved}", ToolTipIcon.Info);
            }
            catch (OperationCanceledException)
            {
                labelOptStatus.Text = "Ottimizzazione interrotta.";
                labelOptMetrics.Text = "I file già elaborati sono stati conservati.";
            }
            catch (Exception ex)
            {
                labelOptStatus.Text = "Errore durante l'ottimizzazione.";
                labelOptMetrics.Text = ex.Message;
                MessageBox.Show("Errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isOptimizing = false;
                StylePrimary(buttonOptStart, true);
                buttonOptScan.Enabled = true;
                UpdateCancelButtons();
                listViewOpt.Invalidate();
                _optCts?.Dispose();
                _optCts = null;
            }
        }

        private void buttonOptCancel_Click(object sender, EventArgs e)
        {
            if (_isOptimizing)
            {
                labelOptStatus.Text = "Interruzione in corso...";
                _optCts?.Cancel();
            }
        }

        private void listViewOpt_DoubleClick(object? sender, EventArgs e)
        {
            if (listViewOpt.SelectedItems.Count == 0) return;
            if (listViewOpt.SelectedItems[0].Tag is not AudioItemInfo item) return;

            try
            {
                if (File.Exists(item.FilePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{item.FilePath}\"");
                }
                else
                {
                    OpenFolder(Path.GetDirectoryName(item.FilePath) ?? textBoxOptFolder.Text);
                }
            }
            catch { }
        }

        #endregion

        #region Varie

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024 * 1024):0.0} GB";
            if (bytes >= 1024L * 1024) return $"{bytes / (1024.0 * 1024):0.0} MB";
            if (bytes >= 1024) return $"{bytes / 1024.0:0} KB";
            return $"{bytes} B";
        }

        private static string Shorten(string text, int max)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= max) return text ?? "";
            return text.Substring(0, max - 1) + "…";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isDownloading || _isOptimizing)
            {
                var dialog = MessageBox.Show("Ci sono operazioni in corso. Vuoi davvero uscire e interromperle?", "Uscita", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialog == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                _cts?.Cancel();
                _optCts?.Cancel();
            }

            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
            try { _probeCts.Cancel(); } catch { }
            _settings.Save();
            notifyIcon.Visible = false;
        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            using var about = new AboutDialog();
            about.ApplyTheme(_theme);
            about.ShowDialog(this);
        }

        #endregion
    }
}
