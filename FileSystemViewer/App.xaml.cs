using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using FileSystemViewer.ViewModels.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.Globalization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel.Background;
using Windows.UI;
using Windows.UI.ViewManagement;
using WinUIEx;

namespace FileSystemViewer
{
    public partial class App : Application
    {
        private TrayIcon? trayIcon;
        private Window? _window;
        public Window? MainWindow => _window;

        private AppState? _appState;
        private IConfigurationService<AppSettings>? _configurationService;

        public IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;

            AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
            AppNotificationManager.Default.Register();
        }

        private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            HandleNotificationClick(args.Arguments);
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
                _appState = ServiceProvider.GetRequiredService<AppState>();
                _configurationService = ServiceProvider.GetRequiredService<IConfigurationService<AppSettings>>();
                IBackgroundScannerService backgroundScannerService = ServiceProvider.GetRequiredService<IBackgroundScannerService>();

                ApplicationLanguages.PrimaryLanguageOverride = "uk-UA";

                var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
                if (activatedArgs.Kind == ExtendedActivationKind.AppNotification)
                {
                    var notificationArgs = activatedArgs.Data as AppNotificationActivatedEventArgs;
                    if (notificationArgs != null)
                    {
                        HandleNotificationClick(notificationArgs.Arguments);
                    }
                }
                else
                {
                    trayIcon = new TrayIcon(1, "Assets/appIcon.ico", "File viewer");

                    Window window = GetMainWindow();
                    _appState.SetMainWindowHandle(window.GetWindowHandle());
                    CheckVisualPreferences(window);
                    window.Activate();

                    trayIcon.IsVisible = true;
                    trayIcon.Selected += (s, e) => window.Activate();
                    trayIcon.ContextMenu += (w, e) =>
                    {
                        var flyout = new MenuFlyout();
                        var resourceLoader = new ResourceLoader();

                        flyout.Items.Add(new MenuFlyoutItem() { Text = resourceLoader.GetString("TrayContextMenuOpen") });
                        ((MenuFlyoutItem)flyout.Items[0]).Click += (s, e) => window.Activate();

                        flyout.Items.Add(new MenuFlyoutItem() { Text = resourceLoader.GetString("TrayContextMenuQuite") });
                        ((MenuFlyoutItem)flyout.Items[1]).Click += (s, e) =>
                        {
                            CloseSubWindows();
                            window?.Close();
                            trayIcon.Dispose();
                        };
                        e.Flyout = flyout;
                    };
                }

                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == "FileViewerRecoveryScanTask")
                    {
                        task.Value.Unregister(true);
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "startup_error.log"), ex.ToString());
                Environment.Exit(0);
            }
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
            services.AddSingleton<IVisualManagerService, VisualManagerService>();

            services.AddSingleton(TimeProvider.System);

            ServiceProvider = services.BuildServiceProvider();
        }

        private void HandleNotificationClick(IDictionary<string, string> arguments)
        {
            if (arguments.TryGetValue("reportPath", out string? reportPath))
            {
                if (File.Exists(reportPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = reportPath,
                        UseShellExecute = true
                    });
                }
            }
        }

        private void CheckVisualPreferences(Window window)
        {
            IVisualManagerService visualManager = ServiceProvider.GetRequiredService<IVisualManagerService>();
            visualManager.Initialize(window);

            Color accentColor = _configurationService!.Settings.CustomAccentColor;
            bool isSystemAccentColorUsed = _configurationService.Settings.IsSystemAccentColorUsed;
            AppSettings.ThemeMode themeMode = _configurationService.Settings.AppTheme;

            if (!isSystemAccentColorUsed)
            {
                var color = Color.FromArgb(255, accentColor.R, accentColor.G, accentColor.B);
                visualManager.SetAccentColor(color);
            }
            else
            {
                UISettings uISettings = new();
                visualManager.SetAccentColor(uISettings.GetColorValue(UIColorType.Accent));
            }

            visualManager.SetApplicationTheme(themeMode);
        }
    }
}

