using FileSystemViewer.ViewModels;
using FileSystemViewer.Services;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using WinUIEx;

namespace FileSystemViewer
{
    public partial class App : Application
    {
        private TrayIcon? icon;
        private Window? _window;

        public IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
        }

        private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            var exeption = e.Exception;

            if (_window != null)
            {
                await DialogManager.ShowContentDialogAsync(_window.Content.XamlRoot,
                    "Error", "Okay", ContentDialogButton.Primary, $"{exeption.Message}");
            }
        }

        private Window GetMainWindow()
        {
            if (_window is not null)
                return _window;
            _window = new MainWindow();
            _window.AppWindow.Closing += (window, args) =>
            {
                args.Cancel = true;
                window.Hide();
            };
            return _window;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            InitializeServices();

            AppState appState = ServiceProvider.GetRequiredService<AppState>();

            Window window = GetMainWindow();
            window.Activate();

            icon = new TrayIcon(1, "Assets/drive.ico", "File viewer");
            icon.IsVisible = true;
            icon.Selected += (s, e) => window.Activate();
            icon.ContextMenu += (w, e) =>
            {
                var flyout = new MenuFlyout();

                flyout.Items.Add(new MenuFlyoutItem() { Text = "Open" });
                ((MenuFlyoutItem)flyout.Items[0]).Click += (s, e) => window.Activate();

                flyout.Items.Add(new MenuFlyoutItem() { Text = "Quit App" });
                ((MenuFlyoutItem)flyout.Items[1]).Click += (s, e) =>
                {
                    var windows = appState.ActiveSubWindows.Values.ToList();

                    foreach (Window subWindow in windows)
                    {
                        subWindow.Close();
                    }
                    window?.Close();
                    icon.Dispose();
                };
                e.Flyout = flyout;
            };
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<AppState>();
            services.AddSingleton<MainPageViewModel>();
            services.AddSingleton<ChartPageViewModel>();

            services.AddSingleton<IDriveUtilsService, DriveUtilsService>();
            services.AddSingleton<IDispatcherQueueProvider, DispatcherQueueProvider>();
            services.AddSingleton<IFileExtentionItemService, FileExtentionItemService>();

            services.AddSingleton(TimeProvider.System);

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

