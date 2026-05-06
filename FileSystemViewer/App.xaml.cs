using FileSystemViewer.Interfaces;
using FileSystemViewer.Models;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Models.Tools;
using FileSystemViewer.Services;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using WinRT.Interop;
using WinUI3Localizer;
using WinUICommunity;
using WinUIEx;

namespace FileSystemViewer
{
    public partial class App : Application, IAppActivationHandler
    {
        private MainWindow? _mainWindow;
        private TrayIcon? _trayIcon;
        public MainWindow? MainWindow => _mainWindow;
        public event Action WindowCreated = null!;
        private List<MainWindow> _mainWindows = new List<MainWindow>();

        private IConfigurationService<AppSettings> _configurationService = null!;
        private IVisualManagerService _visualManagerService = null!;
        private IDispatcherQueueProvider _dispatcherQueueProvider = null!;
        private IDriveUtilsService _driveUtilsService = null!;
        private IDialogService _dialogService = null!;

        public IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;

            AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
            AppNotificationManager.Default.Register();
        }

        private void RegisterFinishing(MainWindow mainWindow)
        {
            if (mainWindow != null)
            {
                mainWindow.Closed += (s, a) => 
                {
                    mainWindow.Content = null;
                    _visualManagerService.MainWindows.Remove(mainWindow);
                    _mainWindows.Remove(mainWindow);
                    
                    if (_mainWindows.Any())
                    {
                        _mainWindow = _mainWindows[0];
                    }
                    else
                    {
                        _mainWindow = null;

                        if (_trayIcon == null)
                            return;
                        _trayIcon.Dispose();
                    }
                    mainWindow.WindowScope.Dispose();
                };
            }
        }

        private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            HandleAppNotificationActivation(args.Arguments);
        }

        private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            var exception = e.Exception;

            if (_mainWindow != null)
            {
                await _dialogService.ShowUnhandledExceptionAsync(exception);
            }
        }

        private MainWindow GetMainWindow(out AppState appState)
        {
            MainWindow mainWindow = new MainWindow();
            var _appState = mainWindow.WindowScope.ServiceProvider.GetRequiredService<AppState>();
            appState = _appState;

            mainWindow.AppWindow.Closing += (window, args) =>
            {
                if (_configurationService!.Settings.IsTrayActive)
                {
                    args.Cancel = true;
                    mainWindow.Hide();
                }
                else
                {
                    args.Cancel = false;
                    CloseSubWindows(_appState);
                }
            };
            return mainWindow;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            InitializeServices();
            await InitializeLocalizer();

            _driveUtilsService = ServiceProvider.GetRequiredService<IDriveUtilsService>();
            _configurationService = ServiceProvider.GetRequiredService<IConfigurationService<AppSettings>>();
            _visualManagerService = ServiceProvider.GetRequiredService<IVisualManagerService>();
            _dispatcherQueueProvider = ServiceProvider.GetRequiredService<IDispatcherQueueProvider>();
            _dialogService = ServiceProvider.GetRequiredService<IDialogService>();
            _dispatcherQueueProvider.Initialize(DispatcherQueue.GetForCurrentThread());
            IBackgroundScannerService backgroundScannerService = ServiceProvider.GetRequiredService<IBackgroundScannerService>();

            await Localizer.Get().SetLanguage(_configurationService.Settings.AppLanguage == AppSettings.Language.English ? "en-GB" : "uk-UA");

            _trayIcon = new TrayIcon(1, "Assets/appIcon.ico", "File viewer");

            _mainWindow = CreateWindow();
            _mainWindows.Add(_mainWindow);
            RegisterFinishing(_mainWindow);

            await CreateContextMenuItems();
        }

        public MainWindow CreateWindow()
        {
            if (_trayIcon == null)
                return null!;

            AppState appState;
            MainWindow window = GetMainWindow(out appState);

            appState.SetMainWindowHandle(window.GetWindowHandle());
            CheckVisualPreferences(window);
            window.Activate();
            WindowCreated?.Invoke();

            _trayIcon.IsVisible = true;
            _trayIcon.Selected += (s, e) => _mainWindow!.Activate();
            _trayIcon.ContextMenu += (w, e) =>
            {
                var flyout = new MenuFlyout();
                var localizer = Localizer.Get();

                flyout.Items.Add(new MenuFlyoutItem() { Text = localizer.GetLocalizedString("TrayContextMenuOpen") });
                ((MenuFlyoutItem)flyout.Items[0]).Click += (s, e) => ActivateAllMainWindows();

                flyout.Items.Add(new MenuFlyoutItem() { Text = localizer.GetLocalizedString("TrayContextMenuQuite") });
                ((MenuFlyoutItem)flyout.Items[1]).Click += (s, e) =>
                {
                    CloseAllMainWindows();
                    _trayIcon.Dispose();
                };
                e.Flyout = flyout;
            };

            return window;
        }

        private void CloseSubWindows(AppState appState)
        {
            foreach (Window subWindow in appState.ActiveSubWindows.Values)
            {
                subWindow.Close();
            }
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();

            services.AddScoped<AppState>();
            services.AddScoped<MainPageViewModel>();
            services.AddScoped<ChartPageViewModel>();
            services.AddScoped<SettingsViewModel>();
            services.AddScoped<ReportViewerPageViewModel>();

            services.AddScoped<IDialogService, DialogService>();
            services.AddScoped<ISubWindowManagerService, SubWindowManagerService>();

            services.AddSingleton<IDriveUtilsService, DriveUtilsService>();
            services.AddSingleton<IDispatcherQueueProvider, DispatcherQueueProvider>();
            services.AddSingleton<IFileExtentionItemService, FileExtentionItemService>();
            services.AddSingleton<IConfigurationService<AppSettings>, ConfigurationService<AppSettings>>();
            services.AddSingleton<IBackgroundScannerService, BackgroundScannerService>();
            services.AddSingleton<IBackgroundSchedulerService, BackgroundSchedulerService>();
            services.AddSingleton<IVisualManagerService, VisualManagerService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IReportStorageService, ReportStorageService>();

            services.AddSingleton(TimeProvider.System);

            ServiceProvider = services.BuildServiceProvider();
        }

        private void CheckVisualPreferences(MainWindow window)
        {
            IVisualManagerService visualManager = ServiceProvider.GetRequiredService<IVisualManagerService>();
            visualManager.MainWindows.Add(window);

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

        private async Task InitializeLocalizer()
        {

            string stringsFolder = Path.Combine(AppContext.BaseDirectory, "Strings");

            ILocalizer localizer = await new LocalizerBuilder()
                .AddStringResourcesFolderForLanguageDictionaries(stringsFolder)
                .SetOptions(options =>
                {
                    options.DefaultLanguage = "en-GB";
                })
                .Build();
        }

        public void HandleRedirectedActivation(AppActivationArguments args)
        {
            _dispatcherQueueProvider!.DispatcherQueue.TryEnqueue(() =>
            {
                var hWnd = WindowNative.GetWindowHandle(MainWindow);
                SetForegroundWindow(hWnd);
                MainWindow!.Activate();
            });  
        }

        // Both files and directories
        public void HandleFileOpenActivation(IReadOnlyList<IStorageItem> items)
        {
            _dispatcherQueueProvider!.DispatcherQueue.TryEnqueue(() =>
            {
                var directories = items.Where(i => i.IsOfType(StorageItemTypes.Folder));
                var files = items.Where(i => i.IsOfType(StorageItemTypes.File));

                ReportViewerPageViewModel reportViewerPageViewModel = MainWindow!.WindowScope.ServiceProvider.GetRequiredService<ReportViewerPageViewModel>();

                if (files.Any())
                {
                    reportViewerPageViewModel.OpenFileReports(files.ToList());
                }
            });
        }

        public void HandleProtocolActivation(IProtocolActivatedEventArgs args)
        {
            _dispatcherQueueProvider!.DispatcherQueue.TryEnqueue(() =>
            {
                Uri activatedUri = args.Uri;
            });
        }

        public async Task HandleCommandLineActivation(string[] arguments, bool isRedirected)
        {
            _dispatcherQueueProvider!.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    if (arguments.Length > 2)
                    {
                        IServiceScope scope = MainWindow!.WindowScope;
                        var command = arguments[1];
                        var path = arguments[2].Trim('"');

                        if (command.Contains("--scan") && !string.IsNullOrWhiteSpace(path))
                        {
                            if (!Directory.Exists(path))
                                return;

                            DirectoryInfo directoryInfo = new DirectoryInfo(path);
                            DirectoryNode directoryNode = _driveUtilsService.CreateDirectoryNode(null, directoryInfo);

                            if (isRedirected)
                            {
                                MainWindow mainWindow = CreateWindow();
                                _mainWindows.Add(mainWindow);
                                RegisterFinishing(mainWindow);
                                scope = mainWindow.WindowScope;

                                mainWindow.Activate();
                            }

                            MainPageViewModel mainPageViewModel = scope.ServiceProvider?.GetRequiredService<MainPageViewModel>()!;
                            mainPageViewModel.DriveNodes.Add(directoryNode);
                            await mainPageViewModel.RequestScanForSelectedTargetAsync(mainPageViewModel.DriveNodes);
                        }
                        else if (command.Contains("--open") && !string.IsNullOrWhiteSpace(path))
                        {
                            if (!File.Exists(path))
                                return;

                            ReportViewerPageViewModel reportViewerPageViewModel = scope.ServiceProvider?.GetRequiredService<ReportViewerPageViewModel>()!;
                            reportViewerPageViewModel.RetrieveAndOpenScanReport(path, true);
                        }
                    }   
                }
                catch(Exception ex)
                {
                    Logger.Log(ex.Message);
                }
            });
        }

        public void HandleStartupActivation(IStartupTaskActivatedEventArgs args)
        {
            _dispatcherQueueProvider!.DispatcherQueue.TryEnqueue(() =>
            {
                //TODO: Something
            });
        }

        public void HandleAppNotificationActivation(IDictionary<string, string> arguments)
        {
            _dispatcherQueueProvider!.DispatcherQueue.TryEnqueue(() =>
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
            }); 
        }

        private void ActivateAllMainWindows()
        {
            foreach (var window in _mainWindows)
            {
                window.Activate();
            }
        }

        private void CloseAllMainWindows()
        {
            List<MainWindow> windows = new List<MainWindow>(_mainWindows);

            foreach (MainWindow window in windows)
            {
                var appState = window.WindowScope.ServiceProvider.GetRequiredService<AppState>();
                window.Close();
                CloseSubWindows(appState);
            }
        }

        private async Task CreateContextMenuItems()
        {
            ContextMenuService menuService = new ContextMenuService();

            // Context menu for directories
            ContextMenuItem folderItem = new ContextMenuItem()
            {
                Title = "Scan with FileSystemViewer",
                Param = @"--scan ""{path}""",
                AcceptDirectoryFlag = (int)(DirectoryMatchFlagEnum.Directory | DirectoryMatchFlagEnum.Background),
                AcceptFileFlag = (int)FileMatchFlagEnum.None,
                Index = 0,
                Enabled = true,
                Icon = Environment.ProcessPath,
                Exe = "fsv.exe"
            };

            // Context menu for report files (.json)
            ContextMenuItem fileItem = new ContextMenuItem()
            {
                Title = "Open in FileSystemViewer",
                Param = @"--open ""{path}""",
                AcceptDirectoryFlag = (int)DirectoryMatchFlagEnum.None,
                AcceptFileFlag = (int)FileMatchFlagEnum.ExtList,
                AcceptExts = AppState.ReportFileExtension,
                Index = 1,
                Enabled = true,
                Icon = Environment.ProcessPath,
                Exe = "fsv.exe"
            };

            await menuService.SaveAsync(folderItem);
            await menuService.SaveAsync(fileItem);
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}

