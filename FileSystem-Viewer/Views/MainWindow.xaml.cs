using FileSystem_Viewer.Services;
using FileSystem_Viewer.Services.IServices;
using FileSystem_Viewer.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace FileSystem_Viewer
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            (Application.Current as App)?.ServiceProvider.GetRequiredService<IDispatcherQueueProvider>().Initialize(this.DispatcherQueue);

            rootFrame.Navigate(typeof(MainPage));
        }
    }
}
