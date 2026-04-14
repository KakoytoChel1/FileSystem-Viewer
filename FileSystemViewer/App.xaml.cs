using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using FileSystemViewer.ViewModels.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinUIEx;

namespace FileSystemViewer
{
    public partial class App : Application
    {
        private TrayIcon? trayIcon;
        private Window? _window;

        private AppState? _appState;
        private IConfigurationService<AppSettings>? _configurationService;
        private IBackgroundScannerService? _backgroundScannerService;

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
                if (_configurationService!.Settings.IsTrayActive)
                {
                    args.Cancel = true;
                    _window.Hide();
                }
                else
                {
                    args.Cancel = false;
                    CloseSubWindows();

                    if (trayIcon == null)
                        return;
                    trayIcon.Dispose();
                }
            };
            return _window;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                InitializeServices();

                string[] cmdArgs = Environment.GetCommandLineArgs();
                _backgroundScannerService = ServiceProvider.GetRequiredService<IBackgroundScannerService>();
                _appState = ServiceProvider.GetRequiredService<AppState>();
                _configurationService = ServiceProvider.GetRequiredService<IConfigurationService<AppSettings>>();

                if (cmdArgs.Contains(BackgroundSchedulerService.ArgumentName))
                {
                    await RunBackgroundTaskAndExit(_backgroundScannerService);
                }
                else
                {
                    trayIcon = new TrayIcon(1, "Assets/drive.ico", "File viewer");
                    Window window = GetMainWindow();
                    _appState.SetMainWindowHandle(window.GetWindowHandle());
                    window.Activate();

                    trayIcon.IsVisible = true;
                    trayIcon.Selected += (s, e) => window.Activate();
                    trayIcon.ContextMenu += (w, e) =>
                    {
                        var flyout = new MenuFlyout();

                        flyout.Items.Add(new MenuFlyoutItem() { Text = "Open" });
                        ((MenuFlyoutItem)flyout.Items[0]).Click += (s, e) => window.Activate();

                        flyout.Items.Add(new MenuFlyoutItem() { Text = "Quit App" });
                        ((MenuFlyoutItem)flyout.Items[1]).Click += (s, e) =>
                        {
                            CloseSubWindows();
                            window?.Close();
                            trayIcon.Dispose();
                        };
                        e.Flyout = flyout;
                    };
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "startup_error.log"), ex.ToString());
            }
        }

        private async Task RunBackgroundTaskAndExit(IBackgroundScannerService backgroundScannerService)
        {
            await backgroundScannerService.ProceedScan().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    string errorImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "cancel.png");
                    NotificationManager.BuildAndShowToastNotification("Scanning was canceled", "Error occured.", errorImagePath);
                }
                Environment.Exit(0);
            });
        }

        private void CloseSubWindows()
        {
            foreach (Window subWindow in _appState!.ActiveSubWindows.Values)
            {
                subWindow.Close();
            }
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<AppState>();
            services.AddSingleton<MainPageViewModel>();
            services.AddSingleton<ChartPageViewModel>();
            services.AddSingleton<SettingsViewModel>();

            services.AddSingleton<IDriveUtilsService, DriveUtilsService>();
            services.AddSingleton<IDispatcherQueueProvider, DispatcherQueueProvider>();
            services.AddSingleton<IFileExtentionItemService, FileExtentionItemService>();
            services.AddSingleton<IConfigurationService<AppSettings>, ConfigurationService<AppSettings>>();
            services.AddSingleton<IBackgroundScannerService, BackgroundScannerService>();
            services.AddSingleton<IBackgroundSchedulerService, BackgroundSchedulerService>();

            services.AddSingleton(TimeProvider.System);

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

