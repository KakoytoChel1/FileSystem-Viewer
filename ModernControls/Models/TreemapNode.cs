using System.Collections.ObjectModel;
using Windows.UI;

namespace ModernControls.Models
{
    public class TreemapNode
    {
        public string LabeledName { get; set; }
        public double Percent { get; set; }
        public long Size { get; set; }
        public Color BackgroundColor { get; set; }
        public TreemapNode Parent { get; set; }
        public bool IsContainer { get; set; }
        public ObservableCollection<TreemapNode> Children { get; set; } = new ObservableCollection<TreemapNode>();
        public string FullPath { get; set; }
    }
}
