using FileSystemViewer.Services;
using FileSystemViewer.Services.Interfaces;
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

            rootFrame.Navigate(typeof(MainPage));
        }
    }
}
