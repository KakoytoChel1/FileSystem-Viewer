using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using FileSystemViewer.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using Windows.UI.ViewManagement;

namespace FileSystemViewer
{
    public sealed partial class MainWindow : Window
    {
        public MainPageViewModel MainPageViewModel { get; }
        private readonly UISettings _uiSettings;

        public MainWindow()
        {
            InitializeComponent();

            _uiSettings = new UISettings();
            _uiSettings.ColorValuesChanged += _uiSettings_ColorValuesChanged;

            ExtendsContentIntoTitleBar = true;
            var coreTitleBar = AppWindow.TitleBar;
            coreTitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

            (Application.Current as App)?.ServiceProvider.GetRequiredService<IDispatcherQueueProvider>().Initialize(this.DispatcherQueue);
            MainPageViewModel = (Application.Current as App)?.ServiceProvider.GetRequiredService<MainPageViewModel>()!;

            RootFrame.Navigate(typeof(ShellPage));
            SettingsFrame.Navigate(typeof(SettingsPage));
        }

        private void _uiSettings_ColorValuesChanged(UISettings sender, object args)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                HandleColorValuesChanged();

                if (this.Content is FrameworkElement root)
                {
                    MainPageViewModel.VisualManagerService.UpdateTitleBarColors(root.ActualTheme);
                }
            });
        }

        private void HandleColorValuesChanged()
        {
            var accent = _uiSettings.GetColorValue(UIColorType.Accent);
            var settings = MainPageViewModel.ConfigurationService.Settings;

            if (settings.AccentColor == null || settings.AccentColor.Count != 3)
            {
                MainPageViewModel.VisualManagerService.SetAccentColor(accent);
            }

            MainPageViewModel.VisualManagerService.SetApplicationTheme(settings.AppTheme);
        }
    }
}
