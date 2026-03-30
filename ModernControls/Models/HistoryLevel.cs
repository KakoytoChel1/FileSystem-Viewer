using System.Collections.Generic;

namespace ModernControls.Models
{
    public class HistoryLevel
    {
        public HistoryLevel() { }

        public HistoryLevel(IEnumerable<TreemapNode> items, string levelName)
        {
            Items = items;
            LevelName = levelName;
        }

        public IEnumerable<TreemapNode> Items { get; set; }
        public string LevelName { get; set; }
    }
}
