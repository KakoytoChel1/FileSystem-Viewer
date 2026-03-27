using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernControls.Models;

namespace ModernControls.Controls
{
    public sealed partial class TreemapItem : Control
    {
        public TreemapItem()
        {
            DefaultStyleKey = typeof(TreemapItem);
        }

        public static readonly DependencyProperty NodeDataProperty =
            DependencyProperty.Register(
                nameof(NodeData),
                typeof(TreemapNode),
                typeof(TreemapItem),
                new PropertyMetadata(null));

        public TreemapNode NodeData
        {
            get => (TreemapNode)GetValue(NodeDataProperty);
            set => SetValue(NodeDataProperty, value);
        }
    }
}
