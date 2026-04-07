using Windows.Foundation;

namespace ModernControls.Models
{
    public class WrappedTreemapNode(TreemapNode treemapNode, double areaSize)
    {
        public TreemapNode TreemapNode { get; } = treemapNode;
        public double AreaSize { get; } = areaSize;
        public Rect Bounds { get; set; }
    }
}
