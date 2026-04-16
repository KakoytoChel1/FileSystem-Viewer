using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FileSystemViewer.Views.Pages
{
    public sealed partial class TreemapPage : Page
    {
        public MainPageViewModel? MainPageViewModel { get; private set; }

        public TreemapPage()
        {
            InitializeComponent();
            MainPageViewModel = (Application.Current as App)?.ServiceProvider.GetRequiredService<MainPageViewModel>();
        }
    }
}
