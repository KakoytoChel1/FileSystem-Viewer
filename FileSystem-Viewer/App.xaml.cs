using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using FileSystem_Viewer.Views.Pages;
using FileSystem_Viewer.ViewModels;


namespace FileSystem_Viewer
{
    public partial class App : Application
    {
        private Window? _window;

        public IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
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

            services.AddSingleton<MainPageViewModel>();
            #endregion

            #region Services


            #endregion

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

