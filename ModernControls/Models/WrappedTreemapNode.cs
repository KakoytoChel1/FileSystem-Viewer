using Windows.Foundation;

namespace ModernControls.Models
{
    public class WrappedTreemapNode
    {
        public WrappedTreemapNode(TreemapNode treemapNode, double areaSize)
        {
            TreemapNode = treemapNode;
            AreaSize = areaSize;
        }

        public TreemapNode TreemapNode { get; }
        public double AreaSize { get; }
        public Rect Bounds { get; set; }
    }
}
