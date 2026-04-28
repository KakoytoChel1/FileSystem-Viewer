using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using FileSystemViewer.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace FileSystemViewer
{
    public sealed partial class MainWindow : Window
    {
        public MainPageViewModel MainPageViewModel { get; }
        public ReportViewerPageViewModel? ReportViewerPageViewModel { get; private set; }

        private readonly UISettings _uiSettings;
        private bool _hasValidFiles;
        private Border? _currentDragAndDropBorder;

        public IServiceScope WindowScope { get; }

        public MainWindow()
        {
            InitializeComponent();

            WindowScope = (Application.Current as App)?.ServiceProvider.CreateScope()!;

            _uiSettings = new UISettings();
            _uiSettings.ColorValuesChanged += _uiSettings_ColorValuesChanged;

            ExtendsContentIntoTitleBar = true;
            var coreTitleBar = AppWindow.TitleBar;
            coreTitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

            MainPageViewModel = WindowScope.ServiceProvider.GetRequiredService<MainPageViewModel>()!;
            ReportViewerPageViewModel = WindowScope.ServiceProvider.GetRequiredService<ReportViewerPageViewModel>();

            RootFrame.Navigate(typeof(ShellPage), WindowScope.ServiceProvider);
            SettingsFrame.Navigate(typeof(SettingsPage), WindowScope.ServiceProvider);
            ReportViewerFrame.Navigate(typeof(ReportViewerPage), WindowScope.ServiceProvider);
        }

        private void _uiSettings_ColorValuesChanged(UISettings sender, object args)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                HandleColorValuesChanged();

                if (this.Content is FrameworkElement root)
                {
                    MainPageViewModel.VisualManagerService.UpdateTitleBarColors(root.ActualTheme, this);
                }
            });
        }

        private void HandleColorValuesChanged()
        {
            var accent = _uiSettings.GetColorValue(UIColorType.Accent);
            var settings = MainPageViewModel.ConfigurationService.Settings;

            if (settings.IsSystemAccentColorUsed)
            {
                MainPageViewModel.VisualManagerService.SetAccentColor(accent);
            }

            MainPageViewModel.VisualManagerService.SetApplicationTheme(settings.AppTheme);
        }

        private void TitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDragRegion();
        }
        private void UpdateDragRegion()
        {
            var appWindow = this.AppWindow;
            if (appWindow != null && appWindow.TitleBar != null)
            {
                double scaleAdjustment = TitleBar.XamlRoot.RasterizationScale;
                var dragRect = new RectInt32(
                    _X: 0,
                    _Y: 0,
                    _Width: (int)(TitleBar.ActualWidth * scaleAdjustment),
                    _Height: (int)(TitleBar.ActualHeight * scaleAdjustment)
                );
                appWindow.TitleBar.SetDragRectangles(new[] { dragRect });
            }
        }

        private async void rootGrid_DragEnter(object sender, DragEventArgs e)
        {
            _hasValidFiles = false;

            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var deferral = e.GetDeferral();
                try
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    _hasValidFiles = items.Any(item =>
                        item is StorageFile file &&
                        file.FileType.Equals(".json", StringComparison.OrdinalIgnoreCase));

                    _currentDragAndDropBorder = _hasValidFiles ? positiveDrop : criticalDrop;
                    _currentDragAndDropBorder.Visibility = Visibility.Visible;
                }
                finally
                {
                    deferral.Complete();
                }
            }
        }

        private void rootGrid_DragOver(object sender, DragEventArgs e)
        {
            if (_hasValidFiles)
            {
                e.AcceptedOperation = DataPackageOperation.Link;
                e.DragUIOverride.Caption = "Open";
                e.DragUIOverride.IsCaptionVisible = true;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private async void rootGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();

                if (items.Count > 0)
                {
                    ReportViewerPageViewModel!.OpenFileReports(new List<IStorageItem>(items));
                }
                CloseDragAndDropBorder();
            }
        }

        private void rootGrid_DragLeave(object sender, DragEventArgs e)
        {
            CloseDragAndDropBorder();
        }

        private void CloseDragAndDropBorder()
        {
            if (_currentDragAndDropBorder != null)
            {
                _currentDragAndDropBorder.Visibility = Visibility.Collapsed;
                _currentDragAndDropBorder = null;
            }
        }
    }
}
