using System.Collections.ObjectModel;

namespace ModernControls.Models
{
    public class HistoryLevel
    {
        public required ObservableCollection<TreemapNode> Items { get; set; }
        public required string LevelName { get; set; }
    }
}
