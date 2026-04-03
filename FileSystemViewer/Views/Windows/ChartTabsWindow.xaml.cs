using FileSystem_Viewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace FileSystemViewer.Views.Windows
{
    public sealed partial class ChartTabsWindow : Window
    {
        public ChartPageViewModel? ViewModel { get; }

        public ChartTabsWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            ViewModel = (Application.Current as App)?.ServiceProvider.GetRequiredService<ChartPageViewModel>();
        }
    }
}
