using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernControls.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;

namespace ModernControls.Controls
{
    [TemplatePart(Name = "PART_BackButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    public sealed partial class HierarchicalTreemap : Control
    {
        private Button _backButton;
        private Canvas _canvas;

        private SpaceCalculator _calculator = new SpaceCalculator();
        private DispatcherTimer _resizeTimer;
        private Stack<HistoryLevel> _history = new();

        private ObservableCollection<TreemapNode> _internalItemsSource;

        public HierarchicalTreemap()
        {
            DefaultStyleKey = typeof(HierarchicalTreemap);
            this.Loaded += Treemap_Loaded;

            _resizeTimer = new DispatcherTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(150);
            _resizeTimer.Tick += OnResizeTimerTick;

            _internalItemsSource = new ObservableCollection<TreemapNode>();
        }

        private void Treemap_Loaded(object sender, RoutedEventArgs e)
        {
            RenderTreemap();
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HierarchicalTreemap treemap)
            {
                treemap.OnItemsSourceChangedInternal();
            }
        }

        private void OnItemsSourceChangedInternal()
        {
            _history.Clear();
            this.CurrentLevelName = string.Empty;

            _internalItemsSource.Clear();

            if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
                    _internalItemsSource.Add(item);
                }
            }

            RenderTreemap();
        }
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_backButton != null)
            {
                _backButton.Click -= OnBackButtonClicked;
            }

            _backButton = GetTemplateChild("PART_BackButton") as Button;

            if (_backButton != null)
            {
                _backButton.Click += OnBackButtonClicked;
            }

            if (_canvas != null)
            {
                _canvas.SizeChanged -= OnCanvasSizeChanged;
            }

            _canvas = GetTemplateChild("PART_Canvas") as Canvas;

            if (_canvas != null)
            {
                _canvas.SizeChanged += OnCanvasSizeChanged;
            }
        }

        private void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }
        private void OnResizeTimerTick(object sender, object e)
        {
            _resizeTimer.Stop();
            RenderTreemap();
        }

        private void OnBackButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_history.Any())
            {
                var previousState = _history.Pop();

                this.CurrentLevelName = previousState.LevelName;

                _internalItemsSource.Clear();
                foreach (var item in previousState.Items)
                {
                    _internalItemsSource.Add(item);
                }

                RenderTreemap();
            }
        }

        private void OnTreemapItemClicked(object sender, TreemapNode clickedNode)
        {
            if (clickedNode.Children == null || !clickedNode.Children.Any())
                return;

            _history.Push(new HistoryLevel() { Items = new ObservableCollection<TreemapNode>(_internalItemsSource), LevelName = this.CurrentLevelName});

            this.CurrentLevelName = $"{this.CurrentLevelName}{clickedNode.LabeledName}{this.Separator}";

            _internalItemsSource.Clear();
            foreach (var item in clickedNode.Children)
            {
                _internalItemsSource.Add(item);
            }

            RenderTreemap();
        }

        public static readonly DependencyProperty CurrentLevelNameProperty =
            DependencyProperty.Register(
                nameof(CurrentLevelName),
                typeof(string),
                typeof(HierarchicalTreemap),
                new PropertyMetadata(string.Empty));

        public string CurrentLevelName
        {
            get => (string)GetValue(CurrentLevelNameProperty);
            set => SetValue(CurrentLevelNameProperty, value);
        }

        public static readonly DependencyProperty SeparatorProperty =
            DependencyProperty.Register(
                nameof(Separator),
                typeof(string),
                typeof(HierarchicalTreemap),
                new PropertyMetadata("/"));

        public string Separator
        {
            get => (string)GetValue(SeparatorProperty);
            set => SetValue(SeparatorProperty, value);
        }

        public static readonly DependencyProperty GlyphIconProperty =
            DependencyProperty.Register(
                nameof(GlyphIcon),
                typeof(string),
                typeof(HierarchicalTreemap),
                new PropertyMetadata("\uE76C"));

        public string GlyphIcon
        {
            get => (string)GetValue(GlyphIconProperty);
            set => SetValue(GlyphIconProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(ObservableCollection<TreemapNode>),
                typeof(HierarchicalTreemap),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public ObservableCollection<TreemapNode> ItemsSource
        {
            get => (ObservableCollection<TreemapNode>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty MenuActionCommandProperty =
            DependencyProperty.Register(
                nameof(MenuActionCommand),
                typeof(ICommand),
                typeof(HierarchicalTreemap),
                new PropertyMetadata(null));

        public ICommand MenuActionCommand
        {
            get => (ICommand)GetValue(MenuActionCommandProperty);
            set => SetValue(MenuActionCommandProperty, value);
        }

        private void RenderTreemap()
        {
            if (_canvas == null || _internalItemsSource == null || !_internalItemsSource.Any() || _canvas.ActualWidth <= 0 || _canvas.ActualHeight <= 0)
            {
                _canvas?.Children.Clear();
                return;
            }
            _canvas.Children.Clear();

            double totalScreenArea = _canvas.ActualWidth * _canvas.ActualHeight;

            // Calculate percentages for root levels (because they don't have them)
            var nodesToRender = _internalItemsSource.ToList();
            var validNodes = nodesToRender.Where(n => n.Percent > 0).ToList();

            // If no valid percentages, calculate them
            if (validNodes.Count < nodesToRender.Count)
            {
                long totalSize = nodesToRender.Sum(n => n.Size);
                if (totalSize > 0)
                {
                    foreach (var node in nodesToRender)
                    {
                        node.Percent = (double)node.Size / totalSize * 100;
                    }
                    validNodes = nodesToRender;
                }
            }

            var wrappedNodes = nodesToRender
                .Select(node =>
                {
                    double realPixelArea = (node.Percent / 100.0) * totalScreenArea;
                    realPixelArea = Math.Max(realPixelArea, 1);

                    return new WrappedTreemapNode(node, realPixelArea);
                })
                .ToList();

            Rect fullSpace = new Rect(0, 0, _canvas.ActualWidth, _canvas.ActualHeight);

            _calculator.CalculateLevelSpace(wrappedNodes, fullSpace);

            foreach (var wrappedNode in wrappedNodes)
            {
                // Validate bounds before creating control
                if (wrappedNode.Bounds.Width <= 0 || wrappedNode.Bounds.Height <= 0)
                    continue;

                var itemControl = new TreemapItem
                {
                    NodeData = wrappedNode.TreemapNode,
                    Width = wrappedNode.Bounds.Width,
                    Height = wrappedNode.Bounds.Height,
                    MenuActionCommand = this.MenuActionCommand
                };

                itemControl.ItemClicked += OnTreemapItemClicked;

                Canvas.SetLeft(itemControl, wrappedNode.Bounds.X);
                Canvas.SetTop(itemControl, wrappedNode.Bounds.Y);

                _canvas.Children.Add(itemControl);
            }
        }
    }
}
