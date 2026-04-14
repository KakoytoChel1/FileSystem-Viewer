using FileSystemViewer.Views.Pages;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace FileSystemViewer
{
    public sealed partial class MainWindow : Window
    {
        public MainPageViewModel? MainPageViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            var coreTitleBar = AppWindow.TitleBar;
            coreTitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

            (Application.Current as App)?.ServiceProvider.GetRequiredService<IDispatcherQueueProvider>().Initialize(this.DispatcherQueue);

            RootFrame.Navigate(typeof(MainPage));
            ChartFrame.Navigate(typeof(ChartPage));

            MainPageViewModel = (Application.Current as App)?.ServiceProvider.GetRequiredService<MainPageViewModel>();

            SettingsFrame.Navigate(typeof(SettingsPage));
        }
    }
}
