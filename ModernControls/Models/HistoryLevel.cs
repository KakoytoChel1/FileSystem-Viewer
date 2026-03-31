using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModernControls.Models
{
    public class HistoryLevel
    {
        public HistoryLevel() { }

        public HistoryLevel(ObservableCollection<TreemapNode> items, string levelName)
        {
            Items = new ObservableCollection<TreemapNode>(items);
            LevelName = levelName;
        }

        public ObservableCollection<TreemapNode> Items { get; set; }
        public string LevelName { get; set; }
    }
}
