using FileSystem_Viewer.ViewModels;
using FileSystemViewer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FileSystem_Viewer.Views.Pages
{
    public sealed partial class ChartPage : Page
    {
        public ChartPageViewModel ViewModel { get; private set; }

        public ChartPage()
        {
            InitializeComponent();
            ViewModel = (Application.Current as App)!.ServiceProvider.GetRequiredService<ChartPageViewModel>();
        }
    }
}
