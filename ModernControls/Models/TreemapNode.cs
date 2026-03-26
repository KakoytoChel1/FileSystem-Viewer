using System.Collections.Generic;
using Windows.UI;

namespace ModernControls.Models
{
    public class TreemapNode
    {
        public string LabeledName { get; set; }
        public bool IsContainer { get; set; }
        public double Percent { get; set; }
        public Color BackgroundColor { get; set; }
        public List<TreemapNode> Children { get; set; } = new List<TreemapNode>();
    }
}
