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
            MainPageViewModel = (Application.Current as App)?.ServiceProvider.GetRequiredService<MainPageViewModel>();

            var template = (DataTemplate)Application.Current.Resources["FullScreenStateTemplate"];
            var content = template.LoadContent() as FrameworkElement;
            RootFrame.Content = content;
        }

        private void mainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {

        }
    }
}
