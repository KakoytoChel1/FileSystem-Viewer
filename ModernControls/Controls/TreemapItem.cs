using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ModernControls.Models;
using System;

namespace ModernControls.Controls
{
    public sealed partial class TreemapItem : Control
    {
        public TreemapItem()
        {
            DefaultStyleKey = typeof(TreemapItem);
        }

        public event EventHandler<TreemapNode> ItemClicked;

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

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            VisualStateManager.GoToState(this, "Normal", true);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (NodeData == null || !NodeData.IsContainer)
                return;

            VisualStateManager.GoToState(this, "Pressed", true);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (NodeData != null && NodeData.IsContainer)
            {
                VisualStateManager.GoToState(this, "Normal", true);
                ItemClicked?.Invoke(this, NodeData);
            }
        }
    }
}
