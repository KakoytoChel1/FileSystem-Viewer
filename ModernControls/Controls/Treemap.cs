using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernControls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace ModernControls.Controls
{
    [TemplatePart(Name = "PART_BackButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    public sealed partial class Treemap : Control
    {
        private Button _backButton;
        private Canvas _canvas;

        private SpaceCalculator _calculator = new SpaceCalculator();

        private DispatcherTimer _resizeTimer;

        public Treemap()
        {
            DefaultStyleKey = typeof(Treemap);
            this.Loaded += Treemap_Loaded;

            _resizeTimer = new DispatcherTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(150);
            _resizeTimer.Tick += OnResizeTimerTick;
        }

        private void Treemap_Loaded(object sender, RoutedEventArgs e)
        {
            RenderTreemap();
        }

        public static readonly DependencyProperty CurrentLevelNameProperty =
            DependencyProperty.Register(
                nameof(CurrentLevelName),
                typeof(string),
                typeof(Treemap),
                new PropertyMetadata("..."));

        public string CurrentLevelName
        {
            get => (string)GetValue(CurrentLevelNameProperty);
            set => SetValue(CurrentLevelNameProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable<TreemapNode>),
                typeof(Treemap),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public IEnumerable<TreemapNode> ItemsSource
        {
            get => (IEnumerable<TreemapNode>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Treemap treemap)
            {
                treemap.RenderTreemap();
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_backButton != null)
                _backButton.Click -= OnBackButtonClicked;

            _backButton = GetTemplateChild("PART_BackButton") as Button;

            if (_backButton != null) 
                _backButton.Click += OnBackButtonClicked;

            if (_canvas != null) _canvas.SizeChanged -= OnCanvasSizeChanged;

            _canvas = GetTemplateChild("PART_Canvas") as Canvas;

            if (_canvas != null) _canvas.SizeChanged += OnCanvasSizeChanged;
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
            //...
        }


        private void RenderTreemap()
        {
            if (_canvas == null || ItemsSource == null || !ItemsSource.Any() || _canvas.ActualWidth <= 0 || _canvas.ActualHeight <= 0)
            {
                _canvas?.Children.Clear();
                return;
            }

            _canvas.Children.Clear();

            double totalScreenArea = _canvas.ActualWidth * _canvas.ActualHeight;

            var wrappedNodes = ItemsSource
                .Select(node =>
                {
                    double realPixelArea = (node.Percent / 100.0) * totalScreenArea;

                    return new WrappedTreemapNode(node, realPixelArea);
                })
                .ToList();

            Rect fullSpace = new Rect(0, 0, _canvas.ActualWidth, _canvas.ActualHeight);

            _calculator.CalculateLevelSpace(wrappedNodes, fullSpace);

            foreach (var wrappedNode in wrappedNodes)
            {
                var itemControl = new TreemapItem
                {
                    NodeData = wrappedNode.TreemapNode,
                    Width = wrappedNode.Bounds.Width,
                    Height = wrappedNode.Bounds.Height
                };

                Canvas.SetLeft(itemControl, wrappedNode.Bounds.X);
                Canvas.SetTop(itemControl, wrappedNode.Bounds.Y);

                _canvas.Children.Add(itemControl);
            }
        }
    }
}
