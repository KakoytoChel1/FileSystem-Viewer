using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ModernControls.Models;
using System;
using System.Windows.Input;

namespace ModernControls.Controls
{
    [TemplatePart(Name = "PART_OpenMenuItem", Type = typeof(MenuFlyoutItem))]
    public sealed partial class TreemapItem : Control
    {
        private MenuFlyoutItem _openMenuItem;

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

        public static readonly DependencyProperty MenuActionCommandProperty =
            DependencyProperty.Register(
                nameof(MenuActionCommand),
                typeof(ICommand),
                typeof(TreemapItem),
                new PropertyMetadata(null));

        public ICommand MenuActionCommand
        {
            get => (ICommand)GetValue(MenuActionCommandProperty);
            set => SetValue(MenuActionCommandProperty, value);
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

            var pointerPoint = e.GetCurrentPoint(this);

            if (!pointerPoint.Properties.IsLeftButtonPressed)
            {
                return;
            }

            if (NodeData == null || !NodeData.IsContainer)
                return;

            VisualStateManager.GoToState(this, "Pressed", true);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);

            var pointerPoint = e.GetCurrentPoint(this);

            if (pointerPoint.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased)
            {
                return;
            }

            if (NodeData != null && NodeData.IsContainer)
            {
                VisualStateManager.GoToState(this, "Normal", true);
                ItemClicked?.Invoke(this, NodeData);
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_openMenuItem != null)
            {
                _openMenuItem.Click -= OnOpenMenuItemClick;
            }

            _openMenuItem = GetTemplateChild("PART_OpenMenuItem") as MenuFlyoutItem;

            if (_openMenuItem != null)
            {
                _openMenuItem.Click += OnOpenMenuItemClick;
            }
        }

        private void OnOpenMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (MenuActionCommand != null && MenuActionCommand.CanExecute(NodeData))
            {
                MenuActionCommand.Execute(NodeData);
            }
        }
    }
}
