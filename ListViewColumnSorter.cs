using System;
using System.Collections;
using System.Windows.Forms;

namespace MusicConverterPro
{
    public class ListViewColumnSorter : IComparer
    {
        public const int QueueProgressColumn = 3;
        public const int OptSizeColumn = 3;

        public int SortColumn { get; set; } = 0;
        public SortOrder Order { get; set; } = SortOrder.None;

        public int Compare(object? x, object? y)
        {
            if (Order == SortOrder.None) return 0;
            if (x is not ListViewItem itemX || y is not ListViewItem itemY)
                return 0;

            int compareResult;

            if (itemX.Tag is AudioItemInfo infoX && itemY.Tag is AudioItemInfo infoY && SortColumn == OptSizeColumn)
            {
                compareResult = infoX.FileSize.CompareTo(infoY.FileSize);
            }
            else if (itemX.Tag is DownloadItem dlX && itemY.Tag is DownloadItem dlY && SortColumn == QueueProgressColumn)
            {
                compareResult = dlX.ProgressPercent.CompareTo(dlY.ProgressPercent);
            }
            else
            {
                string textX = itemX.SubItems.Count > SortColumn ? itemX.SubItems[SortColumn].Text : "";
                string textY = itemY.SubItems.Count > SortColumn ? itemY.SubItems[SortColumn].Text : "";
                compareResult = string.Compare(textX, textY, StringComparison.CurrentCultureIgnoreCase);
            }

            return Order == SortOrder.Descending ? -compareResult : compareResult;
        }
    }
}
