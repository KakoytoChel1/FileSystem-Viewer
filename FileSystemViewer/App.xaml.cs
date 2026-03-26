using FileSystem_Viewer.ViewModels;
using FileSystemViewer.Services;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using FileSystemViewer.ViewModels.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;


namespace FileSystemViewer
{
    public partial class App : Application
    {
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

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            InitializeServices();

            _window = new MainWindow();
            _window.Activate();
        }


        private void InitializeServices()
        {
            var services = new ServiceCollection();

            #region ViewModels

            services.AddSingleton<AppState>();
            services.AddSingleton<MainPageViewModel>();
            services.AddSingleton<ChartPageViewModel>();
            #endregion

            #region Services

            services.AddSingleton<IDriveUtilsService, DriveUtilsService>();
            services.AddSingleton<IDispatcherQueueProvider, DispatcherQueueProvider>();
            services.AddSingleton<IFileExtentionItemService, FileExtentionItemService>();
            #endregion

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

