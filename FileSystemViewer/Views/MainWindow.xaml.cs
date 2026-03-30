using FileSystem_Viewer.Views.Pages;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using FileSystemViewer.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace FileSystemViewer
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            (Application.Current as App)?.ServiceProvider.GetRequiredService<IDispatcherQueueProvider>().Initialize(this.DispatcherQueue);

            RootFrame.Navigate(typeof(MainPage));
            ChartFrame.Navigate(typeof(ChartPage));

            var mainPageViewModel = (Application.Current as App)?.ServiceProvider.GetRequiredService<MainPageViewModel>();

            hierarchicalTreemap.ItemsSource = mainPageViewModel?.TreemapNodes;
        }
    }
}
