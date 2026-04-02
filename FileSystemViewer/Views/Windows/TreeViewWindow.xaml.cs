using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace FileSystemViewer.Views.Windows
{
    public sealed partial class TreeViewWindow : Window
    {
        public MainPageViewModel? ViewModel { get; }

        public TreeViewWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            ViewModel = (Application.Current as App)?.ServiceProvider.GetRequiredService<MainPageViewModel>();
        }
    }
}
